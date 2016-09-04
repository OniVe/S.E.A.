using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SEA.GM.Managers;

namespace SEA.GM.Context
{
    public class SEAContext
    {
        private delegate object Algorithm(object value);
        private bool sea_p_online = false;
        private System.Timers.Timer connectionToServerTimeoutTimer;
        private Func<uint, string, string> internalDoHandler;
        private Action<uint, string, IList<string>> externalDoHandler;
        private Dictionary<uint, Algorithm> algorithms;
        private SEASessionManager sessionManager;

        public SEAContext(out bool success)
        {
            sessionManager = new SEASessionManager(DoOut);

            #region Algorithms

            algorithms = new Dictionary<uint, Algorithm>() {
                { 0 , new Algorithm(__serverCommand) },
                { 1 , new Algorithm(GetCubeGrids) },
                { 2 , new Algorithm(GetAvalibleBlocks) },
                { 3 , new Algorithm(GetGroupBlocks) },

                { 10, new Algorithm(GetBlockProperties) },
                { 11, new Algorithm(GetBlockPropertiesBool) },
                { 12, new Algorithm(GetBlockPropertiesFloat) },
                { 13, new Algorithm(GetBlockActons) },

                { 21, new Algorithm(GetValueBool) },
                { 22, new Algorithm(GetValueFloat) },
                { 23, new Algorithm(GetValueFloatMinimum) },
                { 24, new Algorithm(GetValueFloatMaximum) },

                { 31, new Algorithm(SetValueBool) },
                { 32, new Algorithm(SetValueFloat) },
                { 33, new Algorithm(SetBlockAction) },

                { 41 , new Algorithm(AddValueTracking) },
                { 42 , new Algorithm(RemoveValueTracking) },
            };
            #endregion

            connectionToServerTimeoutTimer = new System.Timers.Timer(10000);
            connectionToServerTimeoutTimer.Elapsed += ConnectionToServerTimeout;
            connectionToServerTimeoutTimer.AutoReset = false;
            connectionToServerTimeoutTimer.Enabled = true;

            MyAPIUtilities.Static.Variables["SEA.GM-DoHandler"] = internalDoHandler = new Func<uint, string, string>(DoIn);// If it is not removed. Will cause serialization error when saving the game.
            MyAPIUtilities.Static.Variables["SEA.GM-Init"] = true;

            SEAUtilities.Logging.Static.WriteLine("SEA Listening Start");
            success = true;
        }

        private void ConnectionToServerTimeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!sea_p_online)
            {
                SEAUtilities.Logging.Static.WriteLine("SEA Timeout connection to the plugin-server");
                this.Close();
            }
        }

        public void Close()
        {
            MyAPIUtilities.Static.Variables.Remove("SEA.GM-Init");
            MyAPIUtilities.Static.Variables.Remove("SEA.GM-DoHandler");
            DoOut(0, "\"disconnect\"", null);// Notify the clients about disconnecting
            SEAUtilities.Logging.Static.WriteLine("SEA Listening Stop");
        }

        private object __serverCommand(object value)
        {
            if (value is string)
                switch ((string)value)
                {
                    case "connect":

                        externalDoHandler = MyAPIUtilities.Static.Variables["SEA.P-DoHandler"] as Action<uint, string, IList<string>>;
                        MyAPIUtilities.Static.Variables.Remove("SEA.P-DoHandler");

                        sea_p_online = true;
                        return true;
                    default: return null;
                }
            else
                return null;
        }

        public void DoOut(uint id, string value, IList<string> connectionIds)
        {
            if (sea_p_online)
                externalDoHandler(id, value, connectionIds);
        }

        /* ! Runs in the main thread ! */
        private string DoIn(uint id, string value)
        {
            if (MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null && algorithms.ContainsKey(id))
                try
                {
                    bool success = false;
                    object json;
                    if (string.IsNullOrEmpty(value))
                    {
                        success = true;
                        json = null;
                    }
                    else
                        json = SEAUtilities.JSON.JsonDecode(value, ref success);

                    return success ? SEAUtilities.JSON.JsonEncode(algorithms[id](json)) : null;
                }
                catch (Exception ex)
                {
                    var errorMsg = new StringBuilder(1024);
                    errorMsg
                        .Append("Algorithm N[")
                        .Append(id)
                        .Append("] ");
                    SEAUtilities.Logging.Static.WriteLine(SEAUtilities.GetExceptionString(ex, errorMsg));
                }

            return null;
        }
        private static bool TryParseEntityId(object eId, out EntityKey entityKey)
        {
            entityKey = new EntityKey();
            if (eId is string)
                return long.TryParse((string)eId, System.Globalization.NumberStyles.Integer, SEAUtilities.CultureInfoUS, out entityKey.EntityId) && entityKey.EntityId > 0;
            else if (eId is Hashtable)
            {
                var _eId = (Hashtable)eId;
                if (_eId.ContainsKey("id") && _eId.ContainsKey("name"))
                {
                    entityKey.IsGroup = true;
                    entityKey.GroupName = (string)_eId["name"];
                    entityKey.Aggregate = _eId.ContainsKey("aggr") ? (bool)_eId["aggr"] : false;
                    return long.TryParse((string)_eId["id"], System.Globalization.NumberStyles.Integer, SEAUtilities.CultureInfoUS, out entityKey.EntityId) && entityKey.EntityId > 0;
                }
            }
            return false;
        }

        private object SetValueBool(object value)
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                EntityKey entityKey;
                if (_value.ContainsKey("eId") &&
                    _value.ContainsKey("propId") &&
                    _value.ContainsKey("value") &&
                    TryParseEntityId(_value["eId"], out entityKey))
                {
                    return entityKey.IsGroup ?
                        sessionManager.SetValueBool(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"], (bool)_value["value"]) :
                        sessionManager.SetValueBool(entityKey.EntityId, (string)_value["propId"], (bool)_value["value"]);
                }
            }
            return false;
        }
        private object SetValueFloat(object value)
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                EntityKey entityKey;
                if (_value.ContainsKey("eId") &&
                    _value.ContainsKey("propId") &&
                    _value.ContainsKey("value") &&
                    TryParseEntityId(_value["eId"], out entityKey))
                {
                    return entityKey.IsGroup ?
                        sessionManager.SetValueFloat(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"], (float)_value["value"]) :
                        sessionManager.SetValueFloat(entityKey.EntityId, (string)_value["propId"], (float)_value["value"]);
                }
            }
            return false;
        }
        private object SetBlockAction(object value)
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                EntityKey entityKey;
                if (_value.ContainsKey("eId") &&
                    _value.ContainsKey("action") &&
                    TryParseEntityId(_value["eId"], out entityKey))
                {
                    return entityKey.IsGroup ?
                        sessionManager.SetBlockAction(entityKey.EntityId, entityKey.GroupName, (string)_value["action"]) :
                        sessionManager.SetBlockAction(entityKey.EntityId, (string)_value["action"]);
                }
            }
            return false;
        }

        private object GetCubeGrids(object value)
        {
            return new ArrayList(sessionManager
                .GetAvalibleCubeGrids()
                .OrderBy(s => s.size)
                .ThenBy(s => s.name, SEAUtilities.NaturalNumericComparer)
                .Select(i => new Hashtable(){
                    { "eId" , i.id},
                    { "text", i.name },
                    { "type", i.size.ToString("g") }
                }).ToArray());
        }
        private object GetAvalibleBlocks(object value)
        {
            EntityKey entityKey;
            if (!TryParseEntityId(value, out entityKey) || entityKey.IsGroup)
                return null;

            return new ArrayList(sessionManager
                .GetAvalibleBlocks(entityKey.EntityId)
                .OrderByDescending(s => s.type)
                .ThenBy(s => s.name, SEAUtilities.NaturalNumericComparer)
                .Select(i => new Hashtable(){
                    { "eId" , i.id},
                    { "text", i.name },
                    { "type", i.type.ToString("g") }
                }).ToArray());
        }

        private object GetBlockPropertiesByTypeName(object value, PropertyType propertyType)
        {
            EntityKey entityKey;
            if (!TryParseEntityId(value, out entityKey))
                return null;

            return new ArrayList((entityKey.IsGroup ?
                sessionManager.GetProperties(entityKey.EntityId, entityKey.GroupName, propertyType) :
                sessionManager.GetProperties(entityKey.EntityId, propertyType))
                .Select(item => new Hashtable(){
                    { "value", item.Id},
                    { "text", item.Id},
                    { "type", item.TypeName}
                }).ToArray());
        }
        private object GetBlockProperties(object value)
        {
            return GetBlockPropertiesByTypeName(value, PropertyType.All);
        }
        private object GetBlockPropertiesBool(object value)
        {
            return GetBlockPropertiesByTypeName(value, PropertyType.Boolean);
        }
        private object GetBlockPropertiesFloat(object value)
        {
            return GetBlockPropertiesByTypeName(value, PropertyType.Single);
        }
        private object GetBlockActons(object value)
        {
            EntityKey entityKey;
            if (!TryParseEntityId(value, out entityKey))
                return null;

            return new ArrayList((entityKey.IsGroup ?
                sessionManager.GetActions(entityKey.EntityId, entityKey.GroupName) :
                sessionManager.GetActions(entityKey.EntityId))
                .Select(item => new Hashtable(){
                    { "value", item.Id},
                    { "text", item.Name.ToString()}
                }).ToArray());
        }

        private object AddValueTracking(object value)
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                EntityKey entityKey;
                if (_value.ContainsKey("eId") &&
                    _value.ContainsKey("propId") &&
                    _value.ContainsKey("connId") &&
                    TryParseEntityId(_value["eId"], out entityKey))
                {
                    return entityKey.IsGroup ?
                        sessionManager.AddValueTracking(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"], entityKey.Aggregate, (string)_value["connId"]) :
                        sessionManager.AddValueTracking(entityKey.EntityId, (string)_value["propId"], (string)_value["connId"]);
                }
            }
            return null;
        }
        private object RemoveValueTracking(object value)
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                if (_value.ContainsKey("connId"))
                {
                    EntityKey entityKey;
                    if (_value.ContainsKey("eId") && _value.ContainsKey("propId"))
                    {
                        if (TryParseEntityId(_value["eId"], out entityKey))
                            return entityKey.IsGroup ?
                                sessionManager.RemoveValueTracking(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"], entityKey.Aggregate, (string)_value["connId"]) :
                                sessionManager.RemoveValueTracking(entityKey.EntityId, (string)_value["propId"], (string)_value["connId"]);
                    }
                    else if (_value.ContainsKey("eId"))
                    {
                        if (TryParseEntityId(_value["eId"], out entityKey))
                            return entityKey.IsGroup ?
                                sessionManager.RemoveValueTracking(entityKey.EntityId, entityKey.GroupName, entityKey.Aggregate, (string)_value["connId"]) :
                                sessionManager.RemoveValueTracking(entityKey.EntityId, (string)_value["connId"]);
                    }
                }
            }
            return null;
        }

        private object GetValueBool(object value)
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                EntityKey entityKey;
                if (_value.ContainsKey("eId") &&
                    _value.ContainsKey("propId") &&
                    TryParseEntityId(_value["eId"], out entityKey))
                {
                    return entityKey.IsGroup ?
                        sessionManager.GetValueBool(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"]) :
                        sessionManager.GetValueBool(entityKey.EntityId, (string)_value["propId"]);
                }
            }
            return null;
        }
        private object GetValueFloat(object value)
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                EntityKey entityKey;
                if (_value.ContainsKey("eId") &&
                    _value.ContainsKey("propId") &&
                    TryParseEntityId(_value["eId"], out entityKey))
                {
                    return entityKey.IsGroup ?
                        sessionManager.GetValueFloat(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"], entityKey.Aggregate) :
                        sessionManager.GetValueFloat(entityKey.EntityId, (string)_value["propId"]);
                }
            }
            return null;
        }
        private object GetValueFloatMinimum(object value)
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                EntityKey entityKey;
                if (_value.ContainsKey("eId") &&
                    _value.ContainsKey("propId") &&
                    TryParseEntityId(_value["eId"], out entityKey))
                {
                    return entityKey.IsGroup ?
                        sessionManager.GetValueFloatMinimum(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"], _value.ContainsKey("aggr") ? (bool)_value["aggr"] : false) :
                        sessionManager.GetValueFloatMinimum(entityKey.EntityId, (string)_value["propId"]);
                }
            }
            return null;
        }
        private object GetValueFloatMaximum(object value)
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                EntityKey entityKey;
                if (_value.ContainsKey("eId") &&
                    _value.ContainsKey("propId") &&
                    TryParseEntityId(_value["eId"], out entityKey))
                {
                    return entityKey.IsGroup ?
                        sessionManager.GetValueFloatMaximum(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"], _value.ContainsKey("aggr") ? (bool)_value["aggr"] : false) :
                        sessionManager.GetValueFloatMaximum(entityKey.EntityId, (string)_value["propId"]);
                }
            }
            return null;
        }

        private object GetGroupBlocks(object value)
        {
            EntityKey entityKey;
            if (!TryParseEntityId(value, out entityKey) || !entityKey.IsGroup)
                return null;

            return new ArrayList(sessionManager
                .GetBlocks(entityKey.EntityId, entityKey.GroupName)
                .OrderBy(s => s.name, SEAUtilities.NaturalNumericComparer)
                .Select(i => new Hashtable(){
                    { "eId" , i.id},
                    { "text", i.name },
                    { "type", i.type }
                }).ToArray());
        }

        private struct EntityKey
        {
            public bool IsGroup;
            public long EntityId;
            public string GroupName;
            public bool Aggregate;
        }
    }
}
