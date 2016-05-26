using Newtonsoft.Json;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;

namespace SEA.P.Web.Models
{
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

    public class ExtIdJsonConverter : JsonConverter
    {
        public override bool CanConvert( Type objectType ) => true;
        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            if (reader.ValueType == typeof(string))
            {
                long result;
                return long.TryParse((string)reader.Value, System.Globalization.NumberStyles.Integer, Utilities.CultureInfoUS, out result) ? result : 0L;
            }
            else
                return 0L;
        }
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            long _value = (long)value;
            JToken jt = JToken.FromObject(_value.ToString(Utilities.CultureInfoUS));
            jt.WriteTo(writer);
        }
    }
}
