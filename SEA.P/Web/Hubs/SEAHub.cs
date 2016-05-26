using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using SEA.P.Models.Enum;
using SEA.P.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEA.P.Web.Hubs
{
    public class seaHub : Hub
    {
        public static HUBContext context;

        public override Task OnConnected()
        {
            Clients.All.connect(Context.ConnectionId);
            return base.OnConnected();
        }
        public override Task OnDisconnected( bool stopCalled )
        {
            Clients.Others.onDisconnected(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
        public async Task ConnectToGameSession()
        {
            Clients.Caller.onConnectToGameSession(GameSessionStatus.Standby);
            Clients.Caller.onConnectToGameSession(await Task.Factory.StartNew<GameSessionStatus>(() => { return context.ConnectToGameSession(); }));
        }

        #region Storage

        #region Worlds

        public async Task<World> WorldGetAsync( int id ) => await context.Storage.Worlds.GetAsync(id);
        public async Task<List<World>> WorldGetAllAsync() => await context.Storage.Worlds.GetAllAsync();
        public async Task<List<World>> WorldGetWhereAsync( long extId ) => await context.Storage.Worlds.GetWhereAsync(extId);
        public async Task<int> WorldAddAsync( World world )
        {
            world.Title = world.Title.Trim();
            var id = await context.Storage.Worlds.AddAsync(world);
            if (id > 0)
            {
                world.Id = id;
                Clients.Others.onWorldAdd(world);
            }
            return id;
        }
        public async Task<List<World>> WorldAddArrayAsync( List<World> worldList )
        {
            var _worldList = await context.Storage.Worlds.AddAsync(worldList);
            if (_worldList != null && _worldList.Count > 0)
                Clients.Others.onWorldAddArray(_worldList);

            return _worldList;
        }
        public async Task<int> WorldUpdateAsync( World world )
        {
            world.Title = world.Title.Trim();
            var id = await context.Storage.Worlds.UpdateAsync(world);
            if (id > 0)
                Clients.Others.onWorldUpdate(world);

            return id;
        }
        public async Task<int> WorldDeleteAsync( int id )
        {
            var _id = await context.Storage.Worlds.DeleteAsync(id);
            if (_id > 0)
            {
                Clients.Others.onWorldDelete(id);
                return id;
            }
            return _id;
        }
        #endregion

        #region Grids

        public async Task<Grid> GridGetAsync( int id ) => await context.Storage.Grids.GetAsync(id);
        public async Task<List<Grid>> GridGetAllAsync( int pId ) => await context.Storage.Grids.GetAllAsync(pId);
        public async Task<List<Grid>> GridGetWhereAsync( int pId, long extId ) => await context.Storage.Grids.GetWhereAsync(pId, extId);
        public async Task<int> GridAddAsync( Grid grid )
        {
            grid.Title = grid.Title.Trim();
            var id = await context.Storage.Grids.AddAsync(grid);
            if (id > 0)
            {
                grid.Id = id;
                Clients.Others.onGridAdd(grid);
            }
            return id;
        }
        public async Task<List<Grid>> GridAddArrayAsync( List<Grid> gridList )
        {
            var _gridList = await context.Storage.Grids.AddAsync(gridList);
            if (_gridList != null && _gridList.Count > 0)
                Clients.Others.onGridAddArray(_gridList);

            return _gridList;
        }
        public async Task<int> GridUpdateAsync( Grid grid )
        {
            grid.Title = grid.Title.Trim();
            var id = await context.Storage.Grids.UpdateAsync(grid);
            if (id > 0)
                Clients.Others.onGridUpdate(grid);

            return id;
        }
        public async Task<int> GridDeleteAsync( int id )
        {
            var _id = await context.Storage.Grids.DeleteAsync(id);
            if (_id > 0)
            {
                Clients.Others.onGridDelete(id);
                return id;
            }
            return _id;
        }
        #endregion

        #region Controls

        public async Task<Control> ControlGetAsync( int id ) => await context.Storage.Controls.GetAsync(id);
        public async Task<List<Control>> ControlGetAllAsync( int pId ) => await context.Storage.Controls.GetAllAsync(pId);
        public async Task<List<Control>> ControlGetWhereAsync( int pId, string extId ) => await context.Storage.Controls.GetWhereAsync(pId, extId);
        public async Task<int> ControlAddAsync( Control control )
        {
            control.Title = control.Title.Trim();
            var id = await context.Storage.Controls.AddAsync(control);
            if (id > 0)
            {
                control.Id = id;
                Clients.Others.onControlAdd(control);
            }
            return id;
        }
        public async Task<List<Control>> ControlAddArrayAsync( List<Control> controlList )
        {
            var _controlList = await context.Storage.Controls.AddAsync(controlList);
            if (_controlList != null && _controlList.Count > 0)
                Clients.Others.onControlAddArray(_controlList);

            return _controlList;
        }
        public async Task<int> ControlUpdateAsync( Control control )
        {
            control.Title = control.Title.Trim();
            var id = await context.Storage.Controls.UpdateAsync(control);
            if (id > 0)
                Clients.Others.onControlUpdate(control);

            return id;
        }
        public async Task<int> ControlDeleteAsync( int id )
        {
            var _id = await context.Storage.Controls.DeleteAsync(id);
            if (_id > 0)
            {
                Clients.Others.onControlDelete(id);
                return id;
            }
            return _id;
        }
        #endregion

        #region ControlsSettings

        public async Task<List<string>> ControlSettingsKeysAsync( int pId ) => await context.Storage.ControlSettings.KeysAsync(pId);
        public async Task<int> ControlSettingsSaveObjectAsync( ControlSettings controlSettings ) => await context.Storage.ControlSettings.SaveAsync(controlSettings);
        public async Task<string> ControlSettingsLoadAsync( int pId, string id ) => await context.Storage.ControlSettings.LoadAsync(pId, id);
        public async Task<int> ControlSettingsDeleteAsync( int pId, string id ) => await context.Storage.ControlSettings.DeleteAsync(pId, id);
        #endregion

        #region Data

        public async Task<List<string>> DataKeysAsync() => await context.Storage.UserData.KeysAsync();
        public async Task<int> DataSaveAsync( string id, string value ) => await context.Storage.UserData.SaveAsync(id, value);
        public async Task<int> DataSaveObjectAsync( UserData userData ) => await context.Storage.UserData.SaveAsync(userData);
        public async Task<string> DataLoadAsync( string id ) => await context.Storage.UserData.LoadAsync(id);
        public async Task<int> DataDeleteAsync( string id ) => await context.Storage.UserData.DeleteAsync(id);
        #endregion
        #endregion

        #region Game

        public Task TransmitAsync( JObject value ) => Clients.Others.onTransmit(value);

        public async Task<string> DoAsync( uint id, string value )
        {
            try
            {
                ExecuteCode executeCode = ExecuteCode.NotInit;
                string result = await Task.Factory.StartNew<string>(() => { return context.DoOut(new Command(id, value), out executeCode); });
                if (executeCode == ExecuteCode.Success)
                    return result;
                else if (executeCode == ExecuteCode.NotInit)
                    Clients.All.onConnectToGameSession(GameSessionStatus.Offline);

                return null;
            }
            catch { return null; }
        }
        #endregion

    }
}
