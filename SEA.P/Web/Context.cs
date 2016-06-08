using Microsoft.AspNet.SignalR;
using Sandbox;
using Sandbox.ModAPI;
using SEA.P.Models.Enum;
using SEA.P.Web.Models;
using SQLite.Net;
using SQLite.Net.Async;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//using WindowsInput;

namespace SEA.P.Web
{
    public class StorageManager : IDisposable
    {
        private bool disposed = false;
        private const string filePathTemplate = @"\storage.s3db";
        private string filePath;
        private static SQLiteConnectionWithLock connection;
        private static SQLiteAsyncConnection db;
        private static AsyncLock _lockCascadeDelete;
        private TableWorlds worlds;
        private TableGrids grids;
        private TableControls controls;
        private TableControlSettings controlSettings;
        private TableUserData userData;

        public TableWorlds Worlds => worlds;
        public TableGrids Grids => grids;
        public TableControls Controls => controls;
        public TableControlSettings ControlSettings => controlSettings;
        public TableUserData UserData => userData;

        public StorageManager()
        {
            filePath = Environment.CurrentDirectory + filePathTemplate;
            try
            {
                connection = new SQLiteConnectionWithLock(new SQLite.Net.Platform.Win32.SQLitePlatformWin32(), new SQLiteConnectionString(filePath, false));
                db = new SQLiteAsyncConnection(() => { return connection; });
                CreateAndSeedAsync();
            }
            catch
            {
                connection = null;
                db = null;
            }

            worlds = new TableWorlds();
            grids = new TableGrids();
            controls = new TableControls();
            controlSettings = new TableControlSettings();
            userData = new TableUserData();
        }
        private async void CreateAndSeedAsync()
        {
            if (db == null) return;

            _lockCascadeDelete = new AsyncLock();

            await db.CreateTablesAsync<World, Grid, Control, ControlSettings, UserData>();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                if (connection != null)
                    connection.Dispose();
                db = null;
                disposed = true;
            }
        }

        public class TableWorlds
        {
            private readonly AsyncLock _lockModify = new AsyncLock();

            public async Task<List<World>> GetAllAsync()
            {
                try
                {
                    var list = await db.Table<World>().ToListAsync();
                    return list.OrderBy(x => x.Title, Utilities.NaturalNumericComparer).ToList();
                }
                catch
                {
                    return null;
                }
            }
            public async Task<World> GetAsync(int id)
            {
                if (id > 0)
                    try
                    {
                        return await db.Table<World>().Where(x => x.Id == id).FirstOrDefaultAsync();
                    }
                    catch { }

                return null;
            }
            public async Task<List<World>> GetWhereAsync(long extId)
            {
                if (extId > 0)
                    try
                    {
                        var worldList = await db.Table<World>().Where(x => x.ExtId == extId).ToListAsync();
                        return worldList == null || worldList.Count == 0 ? null : worldList;
                    }
                    catch { }

                return null;
            }
            public async Task<int> AddAsync(World world)
            {
                if (world != null && world.isNew && world.isValid)
                    using (await _lockModify.LockAsync())
                        try
                        {
                            if (await db.Table<World>().Where(x => x.Title == world.Title).CountAsync() > 0)
                                return (int)ErrorCode.Exist;

                            return await db.InsertAsync(world) > 0 ? world.Id : (int)ErrorCode.InternalError;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;//Error
            }
            public async Task<List<World>> AddAsync(List<World> worldList)
            {
                if (worldList != null && worldList.Count > 0 && worldList.All(x => x.isNew && x.isValid))
                    using (await _lockModify.LockAsync())
                        try
                        {
                            string[] _titleList = new string[worldList.Count];
                            int couter = 0;
                            foreach (var world in worldList)
                            {
                                world.Title = world.Title.Trim();
                                _titleList[couter++] = world.Title;
                            }
                            if (await db.Table<World>().Where(x => _titleList.Contains(x.Title)).CountAsync() > 0)
                                return null;

                            return await db.InsertAllAsync(worldList) > 0 ? worldList : null;
                        }
                        catch { }

                return null;//Error
            }
            public async Task<int> UpdateAsync(World world)
            {
                if (world != null && !world.isNew && world.isValid)
                    using (await _lockModify.LockAsync())
                        try
                        {
                            if (await db.Table<World>().Where(x => x.Id == world.Id).CountAsync() == 0)
                                return (int)ErrorCode.NotExist;//Not exist

                            if (await db.Table<World>().Where(x => x.Title == world.Title && x.Id != world.Id).CountAsync() > 0)
                                return (int)ErrorCode.Exist;//Exist

                            return await db.UpdateAsync(world) > 0 ? world.Id : (int)ErrorCode.InternalError;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;
            }
            public async Task<int> DeleteAsync(int id)
            {
                if (id > 0)
                    using (await _lockCascadeDelete.LockAsync())
                        try
                        {
                            World _world;
                            try
                            {
                                _world = await db.GetWithChildrenAsync<World>(id, true);
                            }
                            catch
                            {
                                return (int)ErrorCode.Invalid;
                            }

                            await db.DeleteAsync(_world, true);
                            return id;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;
            }
        }
        public class TableGrids
        {
            private readonly AsyncLock _lockModify = new AsyncLock();

            public async Task<List<Grid>> GetAllAsync(int pId)
            {
                if (pId > 0)
                    try
                    {
                        var list = await db.Table<Grid>().Where(x => x.WorldId == pId).ToListAsync();
                        return list.OrderBy(x => x.Title, Utilities.NaturalNumericComparer).ToList();
                    }
                    catch { }

                return null;
            }
            public async Task<Grid> GetAsync(int id)
            {
                if (id > 0)
                    try
                    {
                        return await db.Table<Grid>().Where(x => x.Id == id).FirstOrDefaultAsync();
                    }
                    catch { }

                return null;
            }
            public async Task<List<Grid>> GetWhereAsync(int pId, long extId)
            {
                if (extId > 0)
                    try
                    {
                        if (pId > 0)
                            return await db.Table<Grid>().Where(x => x.WorldId == pId && x.ExtId == extId).ToListAsync();
                        else if (pId == 0)
                            return await db.Table<Grid>().Where(x => x.ExtId == extId).ToListAsync();
                    }
                    catch { }

                return null;
            }
            public async Task<int> AddAsync(Grid grid)
            {
                if (grid != null && grid.isNew && grid.isValid)
                    using (await _lockModify.LockAsync())
                        try
                        {
                            if (await db.Table<Grid>().Where(x => x.Title == grid.Title).CountAsync() > 0)
                                return (int)ErrorCode.Exist;

                            return await db.InsertAsync(grid) > 0 ? grid.Id : (int)ErrorCode.InternalError;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;//Error
            }
            public async Task<List<Grid>> AddAsync(List<Grid> gridList)
            {
                if (gridList != null && gridList.Count > 0 && gridList.All(x => x.isNew && x.isValid))
                    using (await _lockModify.LockAsync())
                        try
                        {
                            string[] _titleList = new string[gridList.Count];
                            var couter = 0;
                            var pId = gridList[0].WorldId;
                            foreach (var grid in gridList)
                            {
                                if (pId != grid.WorldId)
                                    return null;
                                grid.Title = grid.Title.Trim();
                                _titleList[couter++] = grid.Title;
                            }
                            if (await db.Table<Grid>().Where(x => x.Id == pId && _titleList.Contains(x.Title)).CountAsync() > 0)
                                return null;

                            return await db.InsertAllAsync(gridList) > 0 ? gridList : null;
                        }
                        catch { }

                return null;//Error
            }
            public async Task<int> UpdateAsync(Grid grid)
            {
                if (grid != null && !grid.isNew && grid.isValid)
                    using (await _lockModify.LockAsync())
                        try
                        {
                            if (await db.Table<Grid>().Where(x => x.Id == grid.Id).CountAsync() == 0)
                                return (int)ErrorCode.NotExist;//Not exist

                            if (await db.Table<Grid>().Where(x => x.Title == grid.Title && x.Id != grid.Id).CountAsync() > 0)
                                return (int)ErrorCode.Exist;//Exist

                            return await db.UpdateAsync(grid) > 0 ? grid.Id : (int)ErrorCode.InternalError;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;
            }
            public async Task<int> DeleteAsync(int id)
            {
                if (id > 0)
                    using (await _lockCascadeDelete.LockAsync())
                        try
                        {
                            Grid _grid;
                            try
                            {
                                _grid = await db.GetWithChildrenAsync<Grid>(id, true);
                            }
                            catch
                            {
                                return (int)ErrorCode.Invalid;
                            }

                            await db.DeleteAsync(_grid, true);
                            return id;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;
            }
        }
        public class TableControls
        {
            private readonly AsyncLock _lockModify = new AsyncLock();

            public async Task<List<Control>> GetAllAsync(int pId)
            {
                if (pId > 0)
                    try
                    {
                        return await db.Table<Control>().Where(x => x.GridId == pId).OrderBy(x => x.Title).ToListAsync();
                    }
                    catch { }

                return null;
            }
            public async Task<Control> GetAsync(int id)
            {
                if (id > 0)
                    try
                    {
                        return await db.Table<Control>().Where(x => x.Id == id).FirstOrDefaultAsync();
                    }
                    catch { }

                return null;
            }
            public async Task<List<Control>> GetWhereAsync(int pId, string extId)
            {
                if (!string.IsNullOrWhiteSpace(extId))
                    try
                    {
                        if (pId > 0)
                            return await db.Table<Control>().Where(x => x.GridId == pId && x.ExtId == extId).ToListAsync();
                        else if (pId == 0)
                            return await db.Table<Control>().Where(x => x.ExtId == extId).ToListAsync();
                    }
                    catch { }

                return null;
            }
            public async Task<int> AddAsync(Control control)
            {
                if (control != null && control.isNew && control.isValid)
                    using (await _lockModify.LockAsync())
                        try
                        {
                            if (await db.Table<Control>().Where(x => x.GridId == control.GridId && x.Title == control.Title).CountAsync() > 0)
                                return (int)ErrorCode.Exist;

                            return await db.InsertAsync(control) > 0 ? control.Id : (int)ErrorCode.InternalError;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;//Error
            }
            public async Task<List<Control>> AddAsync(List<Control> controlList)
            {
                if (controlList != null && controlList.Count > 0 && controlList.All(x => x.isNew && x.isValid))
                    using (await _lockModify.LockAsync())
                        try
                        {
                            string[] _titleList = new string[controlList.Count];
                            var couter = 0;
                            var pId = controlList[0].GridId;
                            foreach (var grid in controlList)
                            {
                                if (pId != grid.GridId)
                                    return null;
                                grid.Title = grid.Title.Trim();
                                _titleList[couter++] = grid.Title;
                            }
                            if (await db.Table<Control>().Where(x => x.GridId == pId && _titleList.Contains(x.Title)).CountAsync() > 0)
                                return null;

                            return await db.InsertAllAsync(controlList) > 0 ? controlList : null;
                        }
                        catch { }

                return null;//Error
            }
            public async Task<int> UpdateAsync(Control control)
            {
                if (control != null && !control.isNew && control.isValid)
                    using (await _lockModify.LockAsync())
                        try
                        {
                            if (await db.Table<Control>().Where(x => x.Id == control.Id).CountAsync() == 0)
                                return (int)ErrorCode.NotExist;//Not exist

                            if (await db.Table<Control>().Where(x => x.GridId == control.GridId && x.Title == control.Title && x.Id != control.Id).CountAsync() > 0)
                                return (int)ErrorCode.Exist;//Exist

                            return await db.UpdateAsync(control) > 0 ? control.Id : (int)ErrorCode.InternalError;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;
            }
            public async Task<int> DeleteAsync(int id)
            {
                if (id > 0)
                    using (await _lockCascadeDelete.LockAsync())
                        try
                        {
                            Control _control;
                            try
                            {
                                _control = await db.GetWithChildrenAsync<Control>(id, true);
                            }
                            catch
                            {
                                return (int)ErrorCode.Invalid;
                            }

                            await db.DeleteAsync(_control, true);
                            return id;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;
            }
        }
        public class TableControlSettings
        {
            private readonly AsyncLock _lockModify = new AsyncLock();

            private class PKey
            {
                public string Key { get; set; }
            }
            public async Task<List<string>> KeysAsync(int pId)
            {
                if (pId > 0)
                    try
                    {
                        var list = await db.QueryAsync<PKey>("SELECT Key FROM ControlsSettings WHERE pId = '?'", pId);
                        return list.Select(x => x.Key).ToList<string>();
                    }
                    catch { }

                return null;
            }
            public async Task<int> SaveAsync(ControlSettings controlSettings)
            {
                if (controlSettings != null && controlSettings.isValid)
                    using (await _lockModify.LockAsync())
                        try
                        {
                            var _controlSettings = await db.Table<ControlSettings>().Where(x => x.ControlId == controlSettings.ControlId && x.Key == controlSettings.Key).FirstOrDefaultAsync();
                            if (_controlSettings == null)
                                return await db.InsertAsync(controlSettings) > 0 ? controlSettings.Id : (int)ErrorCode.InternalError;
                            else
                            {
                                controlSettings.Id = _controlSettings.Id;
                                return await db.UpdateAsync(controlSettings) > 0 ? controlSettings.Id : (int)ErrorCode.InternalError;
                            }
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;//Error
            }
            public async Task<string> LoadAsync(int pId, string key)
            {
                if (pId > 0 && !string.IsNullOrWhiteSpace(key))
                    try
                    {
                        var controlSettings = await db.Table<ControlSettings>().Where(x => x.ControlId == pId && x.Key == key).FirstOrDefaultAsync();
                        if (controlSettings != null)
                            return controlSettings.Value;
                    }
                    catch { }

                return null;//Error
            }
            public async Task<int> DeleteAsync(int pId, string key)
            {
                if (pId > 0 && !string.IsNullOrWhiteSpace(key))
                    using (await _lockCascadeDelete.LockAsync())
                        try
                        {
                            ControlSettings _controlSettings;
                            try
                            {
                                _controlSettings = await db.GetAsync<ControlSettings>(x => x.ControlId == pId && x.Key == key);
                            }
                            catch
                            {
                                return (int)ErrorCode.Invalid;
                            }

                            if (_controlSettings != null)
                            {
                                await db.DeleteAsync(_controlSettings, true);
                                return 1;
                            }
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;
            }
        }
        public class TableUserData
        {
            private readonly AsyncLock _lockModify = new AsyncLock();

            private class Key
            {
                public string Id { get; set; }
            }
            public async Task<List<string>> KeysAsync()
            {
                try
                {
                    var list = await db.QueryAsync<Key>("SELECT Id FROM UserData");
                    return list.Select(x => x.Id).ToList<string>();
                }
                catch { }

                return null;
            }
            public async Task<int> SaveAsync(string id, string value)
            {
                if (id != null && !string.IsNullOrWhiteSpace(id))
                    using (await _lockModify.LockAsync())
                        try
                        {
                            return await db.InsertOrReplaceAsync(new UserData() { Id = id, Value = value }) > 0 ? 1 : (int)ErrorCode.InternalError;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;//Error
            }
            public async Task<int> SaveAsync(UserData userData)
            {
                if (userData != null && userData.isValid)
                    using (await _lockModify.LockAsync())
                        try
                        {
                            return await db.InsertOrReplaceAsync(userData) > 0 ? 1 : (int)ErrorCode.InternalError;
                        }
                        catch { }
                else
                    return (int)ErrorCode.Invalid;

                return (int)ErrorCode.InternalError;//Error
            }
            public async Task<string> LoadAsync(string id)
            {
                if (!string.IsNullOrWhiteSpace(id))
                    try
                    {
                        var userData = await db.Table<UserData>().Where(x => x.Id == id).FirstOrDefaultAsync();
                        if (userData != null)
                            return userData.Value;
                    }
                    catch { }

                return null;//Error
            }
            public async Task<int> DeleteAsync(string id)
            {
                if (string.IsNullOrWhiteSpace(id))
                    return (int)ErrorCode.Invalid;
                else
                    using (await _lockModify.LockAsync())
                        try
                        {
                            if (await db.Table<UserData>().Where(x => x.Id == id).CountAsync() == 0)
                                return (int)ErrorCode.NotExist;

                            return await db.DeleteAsync<UserData>(id);
                        }
                        catch { }

                return (int)ErrorCode.InternalError;
            }
        }
    }

    public class HUBContext : IDisposable
    {
        private IHubContext seaHubContext;
        private TimeSpan executeTimeout = TimeSpan.FromSeconds(2);
        private System.Timers.Timer reconnectionTimer;
        private bool disposed = false;
        private bool sea_gm_online = false;
        private Func<uint, string, string> externalDoHandler;
        private Action<uint, string, IList<string>> internalDoHandler;
        //private InputSimulator inputSimulator;

        private StorageManager storageManager;
        public StorageManager Storage => storageManager;

        public HUBContext()
        {
            seaHubContext = GlobalHost.ConnectionManager.GetHubContext<Hubs.seaHub>();
            storageManager = new StorageManager();
            reconnectionTimer = new System.Timers.Timer(1000);
            reconnectionTimer.Elapsed += Reconnecting;
            reconnectionTimer.AutoReset = true;
            reconnectionTimer.Enabled = true;

            //inputSimulator = new InputSimulator();
        }

        public GameSessionStatus ConnectToGameSession()
        {
            if (sea_gm_online)
                return GameSessionStatus.Online;

            if (MyAPIGateway.Session == null || !MyAPIUtilities.Static.Variables.ContainsKey("SEA.GM-Init"))
                return GameSessionStatus.Offline;

            externalDoHandler = MyAPIUtilities.Static.Variables["SEA.GM-DoHandler"] as Func<uint, string, string>;
            MyAPIUtilities.Static.Variables.Remove("SEA.GM-DoHandler");
            // - - - - - - - - - - - - - - - -

            MyAPIUtilities.Static.Variables["SEA.P-DoHandler"] = internalDoHandler = new Action<uint, string, IList<string>>(DoIn);

            var result = externalDoHandler(0, "\"connect\"");
            if (string.IsNullOrEmpty(result) || result != "true")
            {
                MySandboxGame.Log.WriteLineAndConsole("S.E.A: \"HUBContext.ConnectToGameSession()\" initialization error externalDoHandler. Result is: " + result);
                return GameSessionStatus.Offline;
            }
            // - - - - - - - - - - - - - - - -

            sea_gm_online = true;
            return GameSessionStatus.Online;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                if (storageManager != null)
                {
                    storageManager.Dispose();
                }
                disposed = true;
            }
        }

        private void DoIn(uint id, string value, IList<string> userIds)
        {
            if (id == 0)
                switch (value)
                {
                    case "\"disconnect\"":

                        sea_gm_online = false;
                        reconnectionTimer.Enabled = true;
                        break;
                }

            if (userIds == null || userIds.Count == 0)
                seaHubContext.Clients.All.doAsync(id, value);
            else
                seaHubContext.Clients.Users(userIds).doAsync(id, value);
        }

        public string DoOut(Command command, out ExecuteCode executeCode)
        {
            if (!sea_gm_online || !MyAPIUtilities.Static.Variables.ContainsKey("SEA.GM-Init"))
            {
                sea_gm_online = false;
                if (!reconnectionTimer.Enabled)
                    reconnectionTimer.Enabled = true;

                executeCode = ExecuteCode.NotInit;
                return null;
            }
            executeCode = ExecuteCode.Success;

            string result = null;
            AutoResetEvent waitHandler = new AutoResetEvent(false);
            MyAPIGateway.Utilities.InvokeOnGameThread(delegate ()
            {
                result = externalDoHandler(command.Id, command.Value);
                waitHandler.Set();
            });
            waitHandler.WaitOne(executeTimeout);
            waitHandler.Close();

            return result;
        }

        private void Reconnecting(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (ConnectToGameSession() == GameSessionStatus.Online)
            {
                MySandboxGame.Log.WriteLineAndConsole("S.E.A: Reconnecting");
                reconnectionTimer.Enabled = false;

                seaHubContext.Clients.All.onConnectToGameSession(GameSessionStatus.Online);
            }
        }

        /*public void KeyPress(string key)
        {
            inputSimulator.Keyboard.;
        }*/
    }
}
