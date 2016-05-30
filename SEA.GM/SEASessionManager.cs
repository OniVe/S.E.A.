using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SEA.ModelViews;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SEA.Managers
{
    public class SEASessionManager
    {
        private Dictionary<long, Sandbox.ModAPI.Ingame.IMyTerminalBlock> blocksCache;
        private Dictionary<CompositeKey, Sandbox.ModAPI.Ingame.IMyBlockGroup> groupsCache;
        private Dictionary<CompositeKey, IVirtualTerminalProperty> virtualTerminalPropertiesCache;

        public SEASessionManager()
        {
            blocksCache = new Dictionary<long, Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            groupsCache = new Dictionary<CompositeKey, Sandbox.ModAPI.Ingame.IMyBlockGroup>();
            virtualTerminalPropertiesCache = new Dictionary<CompositeKey, IVirtualTerminalProperty>();
        }
        private IMyGridTerminalSystem GetTerminalSystem( long entityId )
        {
            IMyEntity entity;
            if (MyAPIGateway.Entities.TryGetEntityById(entityId, out entity))
                return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(entity as IMyCubeGrid);

            return null;
        }

        private Sandbox.ModAPI.Ingame.IMyTerminalBlock GetTerminalBlock( long entityId )
        {
            Sandbox.ModAPI.Ingame.IMyTerminalBlock block;
            if (blocksCache.TryGetValue(entityId, out block))
                return block.HasLocalPlayerAccess() ? block : null;

            IMyEntity entity;
            if (!MyAPIGateway.Entities.TryGetEntityById(entityId, out entity))
                return null;

            if (entity == null || !(entity is Sandbox.ModAPI.Ingame.IMyTerminalBlock))
                return null;

            blocksCache[entityId] = block = entity as Sandbox.ModAPI.Ingame.IMyTerminalBlock;
            return block;
        }

        private List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> GetGroupBlocks( long entityId, string groupName, bool first = false )
        {
            var groupKey = new CompositeKey() { longId = entityId, stringId = groupName };
            Sandbox.ModAPI.Ingame.IMyBlockGroup group;
            if (!groupsCache.TryGetValue(groupKey, out group))
            {
                var gts = GetTerminalSystem(entityId);
                if (gts == null)
                    return null;

                group = gts.GetBlockGroupWithName(groupName);
                if (group == null)
                    return null;

                groupsCache[groupKey] = group;
            }

            if (first)
            {
                var block = group.Blocks.FirstOrDefault(x => x.HasLocalPlayerAccess());
                return block == null ? null : new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>() { block };
            }
            else
                return group.Blocks.Where(x => x.HasLocalPlayerAccess()).ToList();
        }

        private IVirtualTerminalProperty GetVirtualTerminalProperty( long entityId, string propertyId )
        {
            var key = new CompositeKey(entityId, propertyId);
            IVirtualTerminalProperty property;
            if (virtualTerminalPropertiesCache.TryGetValue(key, out property))
                return property.block.HasLocalPlayerAccess() ? property : null;

            var block = GetTerminalBlock(entityId);
            if (block == null)
                return null;

            property = block.GetVirtualProperty(propertyId, true) as IVirtualTerminalProperty;
            if (property == null)
                return null;

            virtualTerminalPropertiesCache[key] = property;
            return property;
        }

        public void ClearCache()
        {
            blocksCache.Clear();
            groupsCache.Clear();
            virtualTerminalPropertiesCache.Clear();
        }

        public void UpdateAfterSimulation()
        {
            foreach (var virtualProperty in virtualTerminalPropertiesCache.Values)
                if (virtualProperty != null)
                    virtualProperty.Update();
        }

        public List<GrigView> GetAvalibleCubeGrids()
        {
            var grigList = new List<GrigView>();
            IMyEntity entity;
            IMyCubeGrid cubeGrid;
            foreach (var entityId in MyAPIGateway.Session.LocalHumanPlayer.Grids)
                if (MyAPIGateway.Entities.TryGetEntityById(entityId, out entity))
                {
                    cubeGrid = entity as IMyCubeGrid;
                    if (cubeGrid != null)
                        grigList.Add(new GrigView(cubeGrid));
                }
            return grigList;
        }
        public List<BlockView<EntityType>> GetAvalibleBlocks( long entityId )
        {
            var blocksList = new List<BlockView<EntityType>>();
            var gts = GetTerminalSystem(entityId);
            if (gts != null)
            {
                var blockGroups = new List<IMyBlockGroup>();

                gts.GetBlockGroups(blockGroups);
                for (var i = 0; i < blockGroups.Count; ++i)
                    if (blockGroups[i].HasLocalPlayerAccess())
                        blocksList.Add(new BlockView<EntityType>() { type = EntityType.group, id = blockGroups[i].Name, name = blockGroups[i].Name + " [" + blockGroups[i].Blocks.Count.ToString() + "]" });

                var blocks = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();

                gts.GetBlocks(blocks);
                for (var i = 0; i < blocks.Count; ++i)
                    if (blocks[i].IsFunctional && blocks[i].HasLocalPlayerAccess())
                        blocksList.Add(new BlockView<EntityType>() { type = EntityType.block, id = blocks[i].EntityId.ToString(SEAUtilities.CultureInfoUS), name = blocks[i].CustomName });
            }

            return blocksList;
        }
        public List<ITerminalAction> GetActions( long entityId )
        {
            var actionList = new List<ITerminalAction>();
            var block = GetTerminalBlock(entityId);
            if (block != null)
                block.GetActions(actionList);

            return actionList;
        }
        public List<ITerminalAction> GetActions( long entityId, string groupName )
        {
            var actionList = new List<ITerminalAction>();
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return null;

            var uniqueActions = new HashSet<ITerminalAction>();
            for (var i = 0; i < blocks.Count; ++i)
            {
                blocks[i].GetActions(actionList);
                if (i == 0)
                    uniqueActions.UnionWith(actionList);
                else
                    uniqueActions.IntersectWith(actionList);
            }
            return uniqueActions.ToList();
        }

        public List<ITerminalProperty> GetProperties( long entityId, PropertyType propertyType )
        {
            var propertyList = new List<ITerminalProperty>();

            var block = GetTerminalBlock(entityId);
            if (block != null)
                if (propertyType == PropertyType.All)
                    block.GetAllProperties(propertyList);
                else
                {
                    string typeName = propertyType.ToString("G");
                    block.GetAllProperties(propertyList, x => x.TypeName == typeName);
                }

            return propertyList;
        }
        public List<ITerminalProperty> GetProperties( long entityId, string groupName, PropertyType propertyType )
        {
            var propertyList = new List<ITerminalProperty>();
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return null;

            bool hasTypeName = propertyType != PropertyType.All;
            string typeName = propertyType.ToString("G");
            var uniquePropertys = new HashSet<ITerminalProperty>();

            for (var i = 0; i < blocks.Count; i++)
            {
                if (hasTypeName)
                    blocks[i].GetAllProperties(propertyList, x => x.TypeName == typeName);
                else
                    blocks[i].GetAllProperties(propertyList);

                if (i == 0)
                    uniquePropertys.UnionWith(propertyList);
                else
                    uniquePropertys.IntersectWith(propertyList);
            }

            return uniquePropertys.ToList();
        }

        public bool SetValueBool( long entityId, string propertyId, bool value )
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            block.SetValue<bool>(propertyId, value);
            return true;
        }
        public bool SetValueBool( long entityId, string groupName, string propertyId, bool value )
        {
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return false;

            var i = blocks.Count;
            while (i > 0)
                blocks[--i].SetValue<bool>(propertyId, value);

            return true;
        }

        public bool SetValueFloat( long entityId, string propertyId, float value )
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            block.SetValue<float>(propertyId, value);
            return true;
        }
        public bool SetValueFloat( long entityId, string groupName, string propertyId, float value )
        {
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return false;

            var i = blocks.Count;
            while (i > 0)
                blocks[--i].SetValue<float>(propertyId, value);

            return true;
        }

        public bool SetBlockAction( long entityId, string actionName )
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            block.ApplyAction(actionName);
            return true;
        }
        public bool SetBlockAction( long entityId, string groupName, string actionName )
        {
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return false;

            var i = blocks.Count;
            while (i > 0)
                blocks[--i].ApplyAction(actionName);

            return true;
        }

        public bool SetValueColor( long entityId, string propertyId, Hashtable value )
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            block.SetValue<Color>(propertyId, new Color(
                value.ContainsKey("r") ? (float)value["r"] : 0,
                value.ContainsKey("g") ? (float)value["g"] : 0,
                value.ContainsKey("b") ? (float)value["b"] : 0,
                value.ContainsKey("a") ? (float)value["a"] : 255));
            return true;
        }

        public bool SetVirtualValueFloat( long entityId, string propertyId, float value )
        {
            var virtualProperty = GetVirtualTerminalProperty(entityId, propertyId);
            if (virtualProperty == null)
                return false;

            virtualProperty.Value = value;
            return true;
        }
        public bool SetVirtualValueFloat( long entityId, string groupName, string propertyId, float value )
        {
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return false;

            var i = blocks.Count;
            while (i > 0)
                SetVirtualValueFloat(blocks[--i].EntityId, propertyId, value);

            return true;
        }

        public List<BlockView<string>> GetBlocks( long entityId, string groupName )
        {
            var blockList = new List<BlockView<string>>();
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return blockList;

            for (int i = 0; i < blocks.Count; ++i)
                blockList.Add(new BlockView<string>() { type = blocks[i].GetType().ToString(), id = blocks[i].EntityId.ToString(SEAUtilities.CultureInfoUS), name = blocks[i].Name });
            return blockList;
        }

        public bool? GetValueBool( long entityId, string propertyId )
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return null;

            return block.GetValue<bool>(propertyId);
        }
        public bool? GetValueBool( long entityId, string groupName, string propertyId )
        {
            var blocks = GetGroupBlocks(entityId, groupName, true);
            if (blocks == null || blocks.Count == 0)
                return null;
            else
                return blocks[0].GetValue<bool>(propertyId);
        }

        public float? GetValueFloat( long entityId, string propertyId )
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return null;

            return block.GetValue<float>(propertyId);
        }
        public float? GetValueFloat( long entityId, string groupName, string propertyId )
        {
            var blocks = GetGroupBlocks(entityId, groupName, true);
            if (blocks == null || blocks.Count == 0)
                return null;
            else
                return blocks[0].GetValue<float>(propertyId);
        }

        public Hashtable GetValueColor( long entityId, string propertyId )
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return null;

            var color = block.GetValue<Color>(propertyId);
            return color == null ? null : color.ToHashtable();
        }

        public float? GetVirtualValueFloat( long entityId, string propertyId )
        {
            var virtualProperty = GetVirtualTerminalProperty(entityId, propertyId);
            if (virtualProperty == null)
                return null;

            return virtualProperty.Value;
        }
        public float? GetVirtualValueFloat( long entityId, string groupName, string propertyId )
        {
            var blocks = GetGroupBlocks(entityId, groupName, true);
            if (blocks == null || blocks.Count == 0)
                return null;
            else
                return GetVirtualValueFloat(blocks[0].EntityId, propertyId);

        }

        #region VirtualTerminalPropertis

        public interface IVirtualTerminalProperty : ITerminalProperty
        {
            float Value { get; set; }
            void Update();
            Sandbox.ModAPI.Ingame.IMyTerminalBlock block { get; }
            IVirtualTerminalProperty Copy( Sandbox.ModAPI.Ingame.IMyTerminalBlock block );
        }
        public abstract class LimitSwitch
        {
            internal static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(30);

            internal float _value;
            internal float _deltaValue;
            internal DateTime _timeStamp;
            internal bool _enabled;
        }

        public class MotorStatorAngleProperty : LimitSwitch, IVirtualTerminalProperty
        {
            private const float MAX_VELOCITY = 4f;
            private const float DELTA_LIMIT = (float)(Math.PI / 360f);// +- 0.5 degrees

            public string Id { get { return "Virtual Angle"; } }
            public string TypeName { get { return "Single"; } }

            private Sandbox.ModAPI.Ingame.IMyMotorStator _terminalBlock;

            public float Value
            {
                get { return MyMath.RadiansToDegrees(_terminalBlock.Angle); }
                set
                {
                    _timeStamp = DateTime.UtcNow;
                    _value = MyMath.DegreesToRadians(value);
                    _enabled = true;
                }
            }
            public Sandbox.ModAPI.Ingame.IMyTerminalBlock block { get { return _terminalBlock; } }

            public void Update()
            {
                if (!_enabled)
                    return;

                if (_terminalBlock.Enabled && _terminalBlock.IsWorking)
                {
                    if (float.IsInfinity(_terminalBlock.LowerLimit) && float.IsInfinity(_terminalBlock.UpperLimit))
                        _deltaValue = MyMath.ShortestAngle(_terminalBlock.Angle, _value);
                    else
                    {
                        if (_value < _terminalBlock.LowerLimit) _value = _terminalBlock.LowerLimit;
                        else if (_value > _terminalBlock.UpperLimit) _value = _terminalBlock.UpperLimit;

                        _deltaValue = _value - _terminalBlock.Angle;
                    }

                    if (((_deltaValue < 0f ? -_deltaValue : _deltaValue) <= DELTA_LIMIT) || (DateTime.UtcNow.Subtract(_timeStamp) > TIMEOUT))
                        Stop();
                    else
                        Move();
                }
                else
                    _enabled = false;
            }
            private void Stop()
            {
                _enabled = false;
                _terminalBlock.SetValue<float>("Velocity", 0f);
            }
            private void Move()
            {
                if ((_deltaValue < 0f != _terminalBlock.Velocity < 0f) || _terminalBlock.Velocity == 0f)
                    _terminalBlock.SetValue<float>("Velocity", _deltaValue < 0f ? -MAX_VELOCITY : MAX_VELOCITY);
            }
            public MotorStatorAngleProperty()
            {
                _enabled = false;
            }
            public MotorStatorAngleProperty( Sandbox.ModAPI.Ingame.IMyMotorStator terminalBlock )
            {
                _enabled = false;
                _terminalBlock = terminalBlock;
            }
            public IVirtualTerminalProperty Copy( Sandbox.ModAPI.Ingame.IMyTerminalBlock block )
            {
                return new MotorStatorAngleProperty(block as Sandbox.ModAPI.Ingame.IMyMotorStator);
            }
        }
        public class MotorAdvancedStatorAngleProperty : LimitSwitch, IVirtualTerminalProperty
        {
            private const float MAX_VELOCITY = 4f;
            private const float DELTA_LIMIT = (float)(Math.PI / 360f);// +- 0.5 degrees
            public string Id { get { return "Virtual Angle"; } }
            public string TypeName { get { return "Single"; } }

            private Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator _terminalBlock;

            public float Value
            {
                get { return MyMath.RadiansToDegrees(_terminalBlock.Angle); }
                set
                {
                    _timeStamp = DateTime.UtcNow;
                    _value = MyMath.DegreesToRadians(value);
                    _enabled = true;
                }
            }
            public Sandbox.ModAPI.Ingame.IMyTerminalBlock block { get { return _terminalBlock; } }
            public void Update()
            {
                if (!_enabled)
                    return;

                if (_terminalBlock.Enabled && _terminalBlock.IsWorking)
                {
                    if (float.IsInfinity(_terminalBlock.LowerLimit) && float.IsInfinity(_terminalBlock.UpperLimit))
                        _deltaValue = MyMath.ShortestAngle(_terminalBlock.Angle, _value);
                    else
                    {
                        if (_value < _terminalBlock.LowerLimit) _value = _terminalBlock.LowerLimit;
                        else if (_value > _terminalBlock.UpperLimit) _value = _terminalBlock.UpperLimit;

                        _deltaValue = _value - _terminalBlock.Angle;
                    }

                    if (((_deltaValue < 0f ? -_deltaValue : _deltaValue) <= DELTA_LIMIT) || (DateTime.UtcNow.Subtract(_timeStamp) > TIMEOUT))
                        Stop();
                    else
                        Move();
                }
                else
                    _enabled = false;
            }
            private void Stop()
            {
                _enabled = false;
                _terminalBlock.SetValue<float>("Velocity", 0f);
            }
            private void Move()
            {
                if ((_deltaValue < 0f != _terminalBlock.Velocity < 0f) || _terminalBlock.Velocity == 0f)
                    _terminalBlock.SetValue<float>("Velocity", _deltaValue < 0f ? -MAX_VELOCITY : MAX_VELOCITY);
            }
            public MotorAdvancedStatorAngleProperty()
            {
                _enabled = false;
            }
            public MotorAdvancedStatorAngleProperty( Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator terminalBlock )
            {
                _enabled = false;
                _terminalBlock = terminalBlock;
            }
            public IVirtualTerminalProperty Copy( Sandbox.ModAPI.Ingame.IMyTerminalBlock block )
            {
                return new MotorAdvancedStatorAngleProperty(block as Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator);
            }
        }
        public class PistonBasePositionProperty : LimitSwitch, IVirtualTerminalProperty
        {
            private const float MAX_VELOCITY = 2f;
            private const float DELTA_LIMIT = 0.25f;// +- 0.125  meter
            public string Id { get { return "Virtual Position"; } }
            public string TypeName { get { return "Single"; } }

            private Sandbox.ModAPI.Ingame.IMyPistonBase _terminalBlock;

            public float Value
            {
                get { return _terminalBlock.CurrentPosition; }
                set
                {
                    _timeStamp = DateTime.UtcNow;
                    _value = value;
                    _enabled = true;
                }
            }
            public Sandbox.ModAPI.Ingame.IMyTerminalBlock block { get { return _terminalBlock; } }

            public void Update()
            {
                if (!_enabled)
                    return;

                if (_terminalBlock.Enabled && _terminalBlock.IsWorking)
                {
                    if (_value < _terminalBlock.MinLimit) _value = _terminalBlock.MinLimit;
                    else if (_value > _terminalBlock.MaxLimit) _value = _terminalBlock.MaxLimit;

                    _deltaValue = _value - _terminalBlock.CurrentPosition;

                    if ((_deltaValue < 0f ? -_deltaValue : _deltaValue) <= DELTA_LIMIT || DateTime.UtcNow.Subtract(_timeStamp) > TIMEOUT)
                        Stop();
                    else
                        Move();
                }
                else
                    _enabled = false;
            }
            private void Stop()
            {
                _enabled = false;
                _terminalBlock.SetValue<float>("Velocity", 0f);
            }
            private void Move()
            {
                if ((_deltaValue < 0f != _terminalBlock.Velocity < 0f) || _terminalBlock.Velocity == 0f)
                    _terminalBlock.SetValue<float>("Velocity", _deltaValue < 0f ? -MAX_VELOCITY : MAX_VELOCITY);
            }
            public PistonBasePositionProperty()
            {
                _enabled = false;
            }
            public PistonBasePositionProperty( Sandbox.ModAPI.Ingame.IMyPistonBase terminalBlock )
            {
                _enabled = false;
                _terminalBlock = terminalBlock;
            }
            public IVirtualTerminalProperty Copy( Sandbox.ModAPI.Ingame.IMyTerminalBlock block )
            {
                return new PistonBasePositionProperty(block as Sandbox.ModAPI.Ingame.IMyPistonBase);
            }
        }

        #endregion

        private struct CompositeKey
        {
            public long longId;
            public string stringId;

            public CompositeKey( long entityId, string propertyId )
            {
                this.longId = entityId;
                this.stringId = propertyId;
            }
        }
    }

    public static class VirtualTerminalBlockExtensions
    {
        private static readonly Dictionary<uint, HashSet<SEASessionManager.IVirtualTerminalProperty>> virtualPropertis = new Dictionary<uint, HashSet<SEASessionManager.IVirtualTerminalProperty>>()
        {
            { 1, new HashSet<SEASessionManager.IVirtualTerminalProperty>() { new SEASessionManager.MotorStatorAngleProperty() } },
            { 2, new HashSet<SEASessionManager.IVirtualTerminalProperty>() { new SEASessionManager.MotorAdvancedStatorAngleProperty() } },
            { 3, new HashSet<SEASessionManager.IVirtualTerminalProperty>() { new SEASessionManager.PistonBasePositionProperty() } },
        };
        private static uint GetVirtualBlockKey( this Sandbox.ModAPI.Ingame.IMyTerminalBlock block )
        {
            if (block is Sandbox.ModAPI.Ingame.IMyMotorStator)
                return 1;
            else if (block is Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator)
                return 2;
            else if (block is Sandbox.ModAPI.Ingame.IMyPistonBase)
                return 3;
            else
                return 0;
        }
        public static ITerminalProperty GetVirtualProperty( this Sandbox.ModAPI.Ingame.IMyTerminalBlock block, string id, bool copy = false )
        {
            var key = block.GetVirtualBlockKey();
            if (key > 0)
            {
                SEASessionManager.IVirtualTerminalProperty property = virtualPropertis[key].FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
                return property == null ? null : (copy ? property.Copy(block) : property);
            }

            return null;
        }
        public static void GetVirtualProperties( this Sandbox.ModAPI.Ingame.IMyTerminalBlock block, List<ITerminalProperty> resultList, Func<ITerminalProperty, bool> collect = null )
        {
            if (resultList == null)
                resultList = new List<ITerminalProperty>();

            var key = block.GetVirtualBlockKey();
            if (key > 0)
                resultList.AddRange(virtualPropertis[key].Where(collect));
        }
        public static void GetAllProperties( this Sandbox.ModAPI.Ingame.IMyTerminalBlock block, List<ITerminalProperty> resultList, Func<ITerminalProperty, bool> collect = null )
        {
            block.GetProperties(resultList, collect);
            GetVirtualProperties(block, resultList, collect);
        }
    }
    public static class AccesExtensions
    {
        private static readonly HashSet<VRage.Game.MyRelationsBetweenPlayerAndBlock> allowAcces = new HashSet<VRage.Game.MyRelationsBetweenPlayerAndBlock>() {
            VRage.Game.MyRelationsBetweenPlayerAndBlock.Owner,
            VRage.Game.MyRelationsBetweenPlayerAndBlock.FactionShare};

        public static bool AccesIsAllowed( this Sandbox.ModAPI.Ingame.IMyTerminalBlock self, long playerId )
        {
            return allowAcces.Contains(self.GetUserRelationToOwner(playerId));
        }
        public static bool AccesIsAllowed( this Sandbox.ModAPI.Ingame.IMyBlockGroup self, long playerId )
        {
            return self.Blocks.Any(x => x.AccesIsAllowed(playerId));
        }
        public static bool HasLocalPlayerAccess( this Sandbox.ModAPI.Ingame.IMyBlockGroup self )
        {
            return self.Blocks.Any(x => x.HasLocalPlayerAccess());
        }
    }

    public static class ColorExtensions
    {
        public static Hashtable ToHashtable( this Color self )
        {
            return new Hashtable() { { "r", self.R }, { "g", self.G }, { "b", self.B }, { "a", self.A } };
        }
    }
    public enum PropertyType : byte
    {
        All = 0,
        Boolean = 1,
        Single = 2,
        Color = 3
    }
}
