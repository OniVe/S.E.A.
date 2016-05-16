using Sandbox.ModAPI;
using SEA.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SEA.Context
{
    public class SEAContext
    {
        public Action UpdateAfterSimulationCallback;

        public delegate StringBuilder DoHandler( Command command );
        private delegate object Algorithm( object value );
        private DoHandler doCallback;
        private Dictionary<uint, Algorithm> algorithms;
        private SEADataManager dataManager;
        private SEASessionManager sessionManager;

        public SEAContext( out bool success )
        {
            doCallback = new DoHandler(Do);
            dataManager = new SEADataManager();
            sessionManager = new SEASessionManager();
            UpdateAfterSimulationCallback = new Action(sessionManager.UpdateAfterSimulation);

            if (dataManager.Init(doCallback))
            {

                SEAUtilities.Logging.Static.WriteLine("SEA Listening Start");
                success = true;
            }
            else
            {

                SEAUtilities.Logging.Static.WriteLine("SEA Listening Start Error");
                success = false;
            }

            #region Algorithms

            algorithms = new Dictionary<uint, Algorithm>() {
                { 1 , new Algorithm(GetCubeGrids) },
                { 2 , new Algorithm(GetAvalibleBlocks) },
                { 3 , new Algorithm(GetGroupBlocks) },

                { 10, new Algorithm(GetBlockProperties) },
                { 11, new Algorithm(GetBlockPropertiesBool) },
                { 12, new Algorithm(GetBlockPropertiesFloat) },
                { 13, new Algorithm(GetBlockActons) },

                { 21, new Algorithm(GetValueBool) },
                { 22, new Algorithm(GetValueFloat) },

                { 31, new Algorithm(SetValueBool) },
                { 32, new Algorithm(SetValueFloat) },
                { 33, new Algorithm(SetBlockAction) },
            };
            #endregion
        }

        public void Close()
        {
            if (dataManager != null)
                dataManager.Close();

            SEAUtilities.Logging.Static.WriteLine("SEA Listening Stop");
        }

        private StringBuilder Do( Command command )
        {
            if (MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null && algorithms.ContainsKey(command.Key))
                try
                {
                    bool success = false;
                    object json = command.IsEmpty(ref success) ? null : SEAUtilities.JSON.JsonDecode(command.Value, ref success);
                    return success ? SEAUtilities.JSON.JsonEncode(algorithms[command.Key](json)) : null;
                }
                catch (Exception ex)
                {
                    var errorMsg = new StringBuilder(1024);
                    errorMsg
                        .Append("Algorithm N[")
                        .Append(command.Key)
                        .Append("] Exception occured: ")
                        .Append(ex.TargetSite)
                        .Append(": ")
                        .Append(ex.Message)
                        .AppendLine(ex.StackTrace);
                    SEAUtilities.Logging.Static.WriteLine(errorMsg.ToString());
                }

            return null;
        }
        private static bool TryParseEntityId( object eId, out EntityKey entityKey )
        {
            entityKey = new EntityKey();
            if (eId is String)
                return long.TryParse((string)eId, System.Globalization.NumberStyles.Integer, SEAUtilities.CultureInfoUS, out entityKey.EntityId) && entityKey.EntityId > 0;
            else if (eId is Hashtable)
            {
                var _eId = (Hashtable)eId;
                if (_eId.ContainsKey("id") && _eId.ContainsKey("name"))
                {
                    entityKey.IsGroup = true;
                    entityKey.GroupName = (string)_eId["name"];
                    return long.TryParse((string)_eId["id"], System.Globalization.NumberStyles.Integer, SEAUtilities.CultureInfoUS, out entityKey.EntityId) && entityKey.EntityId > 0;
                }
            }
            return false;
        }

        private object SetValueBool( object value )
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
        private object SetValueFloat( object value )
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
                    return ((string)_value["propId"]).StartsWith("Virtual") ?
                        (entityKey.IsGroup ?
                            sessionManager.SetVirtualValueFloat(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"], (float)_value["value"]) :
                            sessionManager.SetVirtualValueFloat(entityKey.EntityId, (string)_value["propId"], (float)_value["value"])) :
                        (entityKey.IsGroup ?
                            sessionManager.SetValueFloat(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"], (float)_value["value"]) :
                            sessionManager.SetValueFloat(entityKey.EntityId, (string)_value["propId"], (float)_value["value"]));
                }
            }
            return false;
        }
        private object SetBlockAction( object value )
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

        private object GetCubeGrids( object value )
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
        private object GetAvalibleBlocks( object value )
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

        private object GetBlockPropertiesByTypeName( object value, PropertyType propertyType )
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
        private object GetBlockProperties( object value )
        {
            return GetBlockPropertiesByTypeName(value, PropertyType.All);
        }
        private object GetBlockPropertiesBool( object value )
        {
            return GetBlockPropertiesByTypeName(value, PropertyType.Boolean);
        }
        private object GetBlockPropertiesFloat( object value )
        {
            return GetBlockPropertiesByTypeName(value, PropertyType.Single);
        }
        private object GetBlockActons( object value )
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

        private object GetValueBool( object value )
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
        private object GetValueFloat( object value )
        {
            if (value is Hashtable)
            {
                var _value = (Hashtable)value;
                EntityKey entityKey;
                if (_value.ContainsKey("eId") &&
                    _value.ContainsKey("propId") &&
                    TryParseEntityId(_value["eId"], out entityKey))
                {
                    return ((string)_value["propId"]).StartsWith("Virtual") ?
                        (entityKey.IsGroup ?
                            sessionManager.GetVirtualValueFloat(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"]) :
                            sessionManager.GetVirtualValueFloat(entityKey.EntityId, (string)_value["propId"])) :
                        (entityKey.IsGroup ?
                            sessionManager.GetValueFloat(entityKey.EntityId, entityKey.GroupName, (string)_value["propId"]) :
                            sessionManager.GetValueFloat(entityKey.EntityId, (string)_value["propId"]));
                }
            }
            return null;
        }

        private object GetGroupBlocks( object value )
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
        }
    }
}
