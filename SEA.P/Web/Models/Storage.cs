using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SEA.P.Models.Enum;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SEA.P.Web.Models
{
    #region DataModel

    [Table("Worlds")]
    [JsonObject(MemberSerialization.OptIn)]
    public class World
    {
        #region Require
        [Ignore, JsonIgnore]
        public bool isNew => this.Id <= 0;
        [Ignore, JsonIgnore]
        public bool isValid => !string.IsNullOrWhiteSpace(this.Title);

        [PrimaryKey, AutoIncrement, JsonProperty("id")]
        public int Id { get; set; }
        /* - - - - - - - - - - - - - - - - */

        [OneToMany(CascadeOperations = CascadeOperation.All), JsonIgnore]
        public List<Grid> Grids { get; set; }
        #endregion

        //[ForeignKey(typeof(World), Unique = false)]
        [JsonProperty("eId"), JsonConverter(typeof(ExtIdJsonConverter))]
        public long ExtId { get; set; }

        [NotNull, MaxLength(64), JsonProperty("title")]
        public string Title { get; set; }
    }

    [Table("Grids")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Grid
    {
        #region Require
        [Ignore, JsonIgnore]
        public bool isNew => this.Id <= 0;
        [Ignore, JsonIgnore]
        public bool isValid => this.WorldId > 0 && !string.IsNullOrWhiteSpace(this.Title);

        [PrimaryKey, AutoIncrement, JsonProperty("id")]
        public int Id { get; set; }
        /* - - - - - - - - - - - - - - - - */

        [ForeignKey(typeof(World)), JsonProperty("pId")]
        public int WorldId { get; set; }
        /* - - - - - - - - - - - - - - - - */

        [OneToMany(CascadeOperations = CascadeOperation.All), JsonIgnore]
        public List<Control> Controls { get; set; }
        #endregion

        //[ForeignKey(typeof(Grid), Unique = false)]
        [JsonProperty("eId"), JsonConverter(typeof(ExtIdJsonConverter))]
        public long ExtId { get; set; }

        [NotNull, MaxLength(64), JsonProperty("title")]
        public string Title { get; set; }
    }

    [Table("Controls")]
    public class Control
    {
        #region Require
        [Ignore, JsonIgnore]
        public bool isNew => this.Id <= 0;
        [Ignore, JsonIgnore]
        public bool isValid => this.GridId > 0 && !string.IsNullOrWhiteSpace(this.Title);

        [PrimaryKey, AutoIncrement, JsonProperty("id")]
        public int Id { get; set; }

        [ForeignKey(typeof(Grid)), JsonProperty("pId")]
        public int GridId { get; set; }
        /* - - - - - - - - - - - - - - - - */

        [OneToMany(CascadeOperations = CascadeOperation.All), JsonIgnore]
        public List<ControlSettings> ControlsSettings { get; set; }
        #endregion

        [MaxLength(128), Collation("RTRIM"), JsonProperty("eId")]
        public string ExtId { get; set; }

        [NotNull, MaxLength(64), JsonProperty("title")]
        public string Title { get; set; }
    }

    [Table("ControlsSettings")]
    public class ControlSettings
    {
        #region Require
        [Ignore, JsonIgnore]
        public bool isNew => this.Id <= 0;
        [Ignore, JsonIgnore]
        public bool isValid => this.ControlId > 0 && !string.IsNullOrWhiteSpace(this.Key);

        [PrimaryKey, AutoIncrement, JsonIgnore]
        public int Id { get; set; }

        [MaxLength(36), Collation("RTRIM"), JsonProperty("key")]
        public string Key { get; set; }

        [ForeignKey(typeof(Control)), JsonProperty("pId")]
        public int ControlId { get; set; }
        #endregion

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    [Table("UserData")]
    public class UserData
    {
        [Ignore, JsonIgnore]
        public bool isValid => !string.IsNullOrWhiteSpace(this.Id);

        [PrimaryKey, MaxLength(36), Collation("RTRIM"), JsonProperty("key")]
        public string Id { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    internal class ExtIdJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                long result;
                return long.TryParse((string)reader.Value, System.Globalization.NumberStyles.Integer, Utilities.CultureInfoUS, out result) ? result : 0L;
            }
            else
                return 0L;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long _value = (long)value;
            JToken jt = JToken.FromObject(_value.ToString(Utilities.CultureInfoUS));
            jt.WriteTo(writer);
        }
    }
    #endregion

    #region TablesModel

    public struct TableQueryResult<TValue>
    {
        public TableQueryCode Code;
        public TValue Value;
        public TableQueryResult(TableQueryCode code, TValue value)
        {
            this.Code = code;
            this.Value = value;
        }

        public static TableQueryResult<TValue> GetAs(TableQueryCode code) => new TableQueryResult<TValue>(code, default(TValue));
    }

    public abstract class Table
    {
        internal static AsyncLock lockModify = new AsyncLock();
        internal SQLiteAsyncConnection db;

        public Table()
        { }
        public Table(SQLiteAsyncConnection db)
        {
            this.db = db;
        }
    }

    public class TableWorlds : Table
    {
        public TableWorlds()
        { }
        public TableWorlds(SQLiteAsyncConnection db) : base(db)
        { }

        public async Task<TableQueryResult<List<World>>> GetAllAsync()
        {
            try
            {
                return new TableQueryResult<List<World>>(
                    TableQueryCode.Success,
                    (await db.Table<World>().ToListAsync()).OrderBy(x => x.Title, Utilities.NaturalNumericComparer).ToList());
            }
            catch
            {
                return TableQueryResult<List<World>>.GetAs(TableQueryCode.InternalError);
            }
        }
        public async Task<TableQueryResult<World>> GetAsync(int id)
        {
            try
            {
                if (id > 0)
                    return new TableQueryResult<World>(
                        TableQueryCode.Success,
                        await db.Table<World>().Where(x => x.Id == id).FirstOrDefaultAsync());
                else
                    return TableQueryResult<World>.GetAs(TableQueryCode.InvalidRequest);
            }
            catch
            {
                return TableQueryResult<World>.GetAs(TableQueryCode.InternalError);
            }
        }
        public async Task<TableQueryResult<List<World>>> GetWhereAsync(long extId)
        {
            try
            {
                if (extId <= 0)
                    return TableQueryResult<List<World>>.GetAs(TableQueryCode.InvalidRequest);

                return new TableQueryResult<List<World>>(
                    TableQueryCode.Success,
                    await db.Table<World>().Where(x => x.ExtId == extId).ToListAsync());
            }
            catch
            {
                return TableQueryResult<List<World>>.GetAs(TableQueryCode.InternalError);
            }
        }
        public async Task<TableQueryResult<int>> AddAsync(World world)
        {
            try
            {
                if (world != null && world.isNew && world.isValid)
                    using (await lockModify.LockAsync())
                    {
                        if (await db.Table<World>().Where(x => x.Title == world.Title).CountAsync() > 0)
                            return TableQueryResult<int>.GetAs(TableQueryCode.ValueExist);

                        var result = await db.InsertAsync(world);
                        return new TableQueryResult<int>(
                            result > 0 ? TableQueryCode.Success : TableQueryCode.QueryError,
                            result);
                    }
                else
                    return TableQueryResult<int>.GetAs(TableQueryCode.InvalidRequest);
            }
            catch
            {
                return TableQueryResult<int>.GetAs(TableQueryCode.InternalError);
            }
        }
        public async Task<TableQueryResult<List<World>>> AddAsync(List<World> worldList)
        {
            try
            {
                if (worldList != null && worldList.Count > 0 && worldList.All(x => x.isNew && x.isValid))
                    using (await lockModify.LockAsync())
                    {
                        string[] _titleList = new string[worldList.Count];
                        int couter = 0;
                        foreach (var world in worldList)
                        {
                            world.Title = world.Title.Trim();
                            _titleList[couter++] = world.Title;
                        }
                        if (await db.Table<World>().Where(x => _titleList.Contains(x.Title)).CountAsync() > 0)
                            return TableQueryResult<List<World>>.GetAs(TableQueryCode.ValueExist);

                        var result = await db.InsertAllAsync(worldList);
                        return new TableQueryResult<List<World>>(result > 0 ? TableQueryCode.Success : TableQueryCode.QueryError, worldList);
                    }
                else
                    return TableQueryResult<List<World>>.GetAs(TableQueryCode.InvalidRequest);
            }
            catch
            {
                return TableQueryResult<List<World>>.GetAs(TableQueryCode.InternalError);
            }
        }
    }
    #endregion

    public interface IStorage
    {
        bool IsConnected { get; }
        T GetTable<T>() where T : Table;
    }

    public class Storage : IStorage, IDisposable
    {
        private const string FILE_PATH_TEMPLATE = @"\storage.s3db";

        private SQLiteConnectionWithLock connection;
        private SQLiteAsyncConnection db;

        public bool IsConnected => connection != null;

        bool IStorage.IsConnected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private Storage()
        {
            try
            {
                connection = new SQLiteConnectionWithLock(
                    new SQLite.Net.Platform.Win32.SQLitePlatformWin32(),
                    new SQLiteConnectionString(Environment.CurrentDirectory + FILE_PATH_TEMPLATE,
                    false));
                db = new SQLiteAsyncConnection(() => { return connection; });
                CreateAndSeedAsync();
            }
            catch
            {
                connection = null;
                db = null;
            }
        }
        private async void CreateAndSeedAsync()
        {
            if (db == null) return;

            //_lockCascadeDelete = new AsyncLock();

            await db.CreateTablesAsync<World, Grid, Control, ControlSettings, UserData>();
        }

        protected T GetTable<T>()
        {
            return (T)Activator.CreateInstance(typeof(T), db);
        }

        T IStorage.GetTable<T>()
        {
            return this.GetTable<T>();
        }

        public static IStorage Get()
        {
            return new Storage();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    connection.Dispose();
                }

                db = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
