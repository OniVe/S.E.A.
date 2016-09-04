using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SEA.GM.ModelViews;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SEA.GM.Managers
{
    public class SEASessionManager
    {
        private Dictionary<long, Sandbox.ModAPI.Ingame.IMyTerminalBlock> blocksCache;
        private Dictionary<CompositeKey, Sandbox.ModAPI.Ingame.IMyBlockGroup> groupsCache;
        private Action<uint, string, IList<string>> doOut;

        public SEASessionManager(Action<uint, string, IList<string>> doOut)
        {
            blocksCache = new Dictionary<long, Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            groupsCache = new Dictionary<CompositeKey, Sandbox.ModAPI.Ingame.IMyBlockGroup>();
            this.doOut = doOut;
        }

        private Sandbox.ModAPI.Ingame.IMyGridTerminalSystem GetTerminalSystem(long entityId)
        {
            IMyEntity entity;
            if (MyAPIGateway.Entities.TryGetEntityById(entityId, out entity))
                return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(entity as IMyCubeGrid);

            return null;
        }

        private Sandbox.ModAPI.Ingame.IMyTerminalBlock GetTerminalBlock(long entityId)
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

        private Sandbox.ModAPI.Ingame.IMyBlockGroup GetBlockGroup(long entityId, string groupName)
        {
            var groupKey = new CompositeKey() { longId = entityId, stringId = groupName };
            Sandbox.ModAPI.Ingame.IMyBlockGroup blockGroup;
            if (!groupsCache.TryGetValue(groupKey, out blockGroup))
            {
                var gts = GetTerminalSystem(entityId);
                if (gts == null)
                    return null;

                blockGroup = gts.GetBlockGroupWithName(groupName);
                if (blockGroup == null)
                    return null;

                groupsCache[groupKey] = blockGroup;
            }

            return blockGroup;
        }

        private List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> GetGroupBlocks(long entityId, string groupName, bool first = false)
        {
            var blockGroup = GetBlockGroup(entityId, groupName);

            var blocks = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            if (first)
            {
                blockGroup.GetBlocks(blocks);
                var block = blocks.FirstOrDefault(x => x.HasLocalPlayerAccess());
                return block == null ? null : new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>() { block };
            }
            else
            {
                blockGroup.GetBlocks(blocks, x => x.HasLocalPlayerAccess());
                return blocks;
            }
        }

        public void ClearCache()
        {
            blocksCache.Clear();
            groupsCache.Clear();
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
        public List<BlockView<EntityType>> GetAvalibleBlocks(long entityId)
        {
            var blocksList = new List<BlockView<EntityType>>();
            var gts = GetTerminalSystem(entityId);
            if (gts != null)
            {
                var blockGroups = new List<Sandbox.ModAPI.Ingame.IMyBlockGroup>();
                var gBlocks = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();

                gts.GetBlockGroups(blockGroups);
                for (var i = 0; i < blockGroups.Count; ++i)
                    if (blockGroups[i].HasLocalPlayerAccess())
                    {
                        blockGroups[i].GetBlocks(gBlocks);
                        blocksList.Add(new BlockView<EntityType>() { type = EntityType.group, id = blockGroups[i].Name, name = blockGroups[i].Name + " [" + gBlocks.Count.ToString() + "]" });
                    }

                var blocks = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();

                gts.GetBlocks(blocks);
                for (var i = 0; i < blocks.Count; ++i)
                    if (blocks[i].IsFunctional && blocks[i].HasLocalPlayerAccess())
                        blocksList.Add(new BlockView<EntityType>() { type = EntityType.block, id = blocks[i].EntityId.ToString(SEAUtilities.CultureInfoUS), name = blocks[i].CustomName });
            }

            return blocksList;
        }
        public List<ITerminalAction> GetActions(long entityId)
        {
            var actionList = new List<ITerminalAction>();
            var block = GetTerminalBlock(entityId);
            if (block != null)
                block.GetActions(actionList);

            return actionList;
        }
        public List<ITerminalAction> GetActions(long entityId, string groupName)
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

        public List<ITerminalProperty> GetProperties(long entityId, PropertyType propertyType)
        {
            var propertyList = new List<ITerminalProperty>();

            var block = GetTerminalBlock(entityId);
            if (block != null)
                if (propertyType == PropertyType.All)
                    block.GetProperties(propertyList);
                else
                {
                    string typeName = propertyType.ToString("G");
                    block.GetProperties(propertyList, x => x.TypeName == typeName);
                }

            return propertyList;
        }
        public List<ITerminalProperty> GetProperties(long entityId, string groupName, PropertyType propertyType)
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
                    blocks[i].GetProperties(propertyList, x => x.TypeName == typeName);
                else
                    blocks[i].GetProperties(propertyList);

                if (i == 0)
                    uniquePropertys.UnionWith(propertyList);
                else
                    uniquePropertys.IntersectWith(propertyList);
            }

            return uniquePropertys.ToList();
        }

        public bool SetValueBool(long entityId, string propertyId, bool value)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            block.SetValue<bool>(propertyId, value);
            return true;
        }
        public bool SetValueBool(long entityId, string groupName, string propertyId, bool value)
        {
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return false;

            var i = blocks.Count;
            while (i > 0)
                blocks[--i].SetValue<bool>(propertyId, value);

            return true;
        }

        public bool SetValueFloat(long entityId, string propertyId, float value)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            block.SetValue<float>(propertyId, value);
            return true;
        }
        public bool SetValueFloat(long entityId, string groupName, string propertyId, float value)
        {
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return false;

            var i = blocks.Count;
            while (i > 0)
                blocks[--i].SetValue<float>(propertyId, value);

            return true;
        }

        public bool SetBlockAction(long entityId, string actionName)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            block.ApplyAction(actionName);
            return true;
        }
        public bool SetBlockAction(long entityId, string groupName, string actionName)
        {
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return false;

            var i = blocks.Count;
            while (i > 0)
                blocks[--i].ApplyAction(actionName);

            return true;
        }

        public bool SetValueColor(long entityId, string propertyId, Hashtable value)
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

        public List<BlockView<string>> GetBlocks(long entityId, string groupName)
        {
            var blockList = new List<BlockView<string>>();
            var blocks = GetGroupBlocks(entityId, groupName);
            if (blocks == null || blocks.Count == 0)
                return blockList;

            for (int i = 0; i < blocks.Count; ++i)
                blockList.Add(new BlockView<string>() { type = blocks[i].GetType().ToString(), id = blocks[i].EntityId.ToString(SEAUtilities.CultureInfoUS), name = blocks[i].CustomName });

            return blockList;
        }

        public bool? GetValueBool(long entityId, string propertyId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return null;

            return block.GetValue<bool>(propertyId);
        }
        public bool? GetValueBool(long entityId, string groupName, string propertyId)
        {
            var blocks = GetGroupBlocks(entityId, groupName, true);
            if (blocks == null || blocks.Count == 0)
                return null;
            else
                return blocks[0].GetValue<bool>(propertyId);
        }

        public float? GetValueFloat(long entityId, string propertyId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return null;

            return block.GetValue<float>(propertyId);
        }
        public float? GetValueFloat(long entityId, string groupName, string propertyId, bool aggregate = false)
        {
            var blocks = GetGroupBlocks(entityId, groupName, true);
            if (blocks == null || blocks.Count == 0)
                return null;
            else
                return aggregate ? blocks.Sum(e => e.GetValue<float>(propertyId)) : blocks[0].GetValue<float>(propertyId);
        }

        public float? GetValueFloatMinimum(long entityId, string propertyId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return null;

            return block.GetMinimum<float>(propertyId);
        }
        public float? GetValueFloatMinimum(long entityId, string groupName, string propertyId, bool aggregate = false)
        {
            var blocks = GetGroupBlocks(entityId, groupName, true);
            if (blocks == null || blocks.Count == 0)
                return null;
            else
                return aggregate ? blocks.Sum(e => e.GetMinimum<float>(propertyId)) : blocks[0].GetMinimum<float>(propertyId);
        }

        public float? GetValueFloatMaximum(long entityId, string propertyId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return null;

            return block.GetMaximum<float>(propertyId);
        }
        public float? GetValueFloatMaximum(long entityId, string groupName, string propertyId, bool aggregate = false)
        {
            var blocks = GetGroupBlocks(entityId, groupName, true);
            if (blocks == null || blocks.Count == 0)
                return null;
            else
                return aggregate ? blocks.Sum(e => e.GetMaximum<float>(propertyId)) : blocks[0].GetMaximum<float>(propertyId);
        }

        public bool AddValueTracking(long entityId, string propertyId, string connectionId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            var entityGameLogic = GameLogic.SEACompositeGameLogicComponent.Get(block);

            BlockPropertyValueTracking component;
            component = entityGameLogic.GetAs<BlockPropertyValueTracking>();
            if (component == null)
            {
                component = new BlockPropertyValueTracking(doOut);
                entityGameLogic.Add(component);
                component.Init(block);
            }

            return component.Add(connectionId, propertyId);
        }
        public bool AddValueTracking(long entityId, string groupName, string propertyId, bool aggregate, string connectionId)
        {
            var blockGroup = GetBlockGroup(entityId, groupName);
            if (blockGroup == null)
                return false;

            if (aggregate)
            {
                var component = AggregateProperties.Static.Get(blockGroup);
                if (component == null)
                {
                    var entityId_SB = new System.Text.StringBuilder(128);
                    entityId_SB
                        .JObjectStart()
                        .JStringKeyValuePair(
                            "id", entityId.ToString(),
                            "name", groupName)
                        .JSplit()
                        .JKeyValuePair(
                            "aggr", aggregate.ToString().ToLower())
                        .JObjectEnd();

                    component = new AggregatePropertyValueTracking(blockGroup, entityId_SB.ToString(), doOut);
                    AggregateProperties.Static.Add(blockGroup, component);
                }

                return component.Add(connectionId, propertyId);
            }
            else
            {
                var blocks = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
                blockGroup.GetBlocks(blocks);

                if (blocks.Count == 0)
                    return false;

                return AddValueTracking(blocks[0].EntityId, propertyId, connectionId);
            }
        }

        public bool RemoveValueTracking(long entityId, string propertyId, string connectionId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            var entityGameLogic = GameLogic.SEACompositeGameLogicComponent.Get(block);

            BlockPropertyValueTracking component;
            component = entityGameLogic.GetAs<BlockPropertyValueTracking>();
            if (component != null)
            {
                component.Remove(connectionId, propertyId);
                if (component.IsEmpty)
                    entityGameLogic.Remove(component);
            }

            return true;
        }
        public bool RemoveValueTracking(long entityId, string connectionId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            var entityGameLogic = GameLogic.SEACompositeGameLogicComponent.Get(block);

            BlockPropertyValueTracking component;
            component = entityGameLogic.GetAs<BlockPropertyValueTracking>();
            if (component != null)
            {
                component.Remove(connectionId);
                if (component.IsEmpty)
                    entityGameLogic.Remove(component);
            }

            return true;
        }

        public bool RemoveValueTracking(long entityId, string groupName, string propertyId, bool aggregate, string connectionId)
        {
            var blockGroup = GetBlockGroup(entityId, groupName);
            if (blockGroup == null)
                return false;

            if (aggregate)
            {
                var component = AggregateProperties.Static.Get(blockGroup);
                if (component != null)
                {
                    component.Remove(connectionId, propertyId);
                    if (component.IsEmpty)
                        AggregateProperties.Static.Remove(blockGroup);
                }

                return true;
            }
            else
            {
                var blocks = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
                blockGroup.GetBlocks(blocks);

                if (blocks.Count == 0)
                    return false;

                return RemoveValueTracking(blocks[0].EntityId, propertyId, connectionId);
            }
        }
        public bool RemoveValueTracking(long entityId, string groupName, bool aggregate, string connectionId)
        {
            var blockGroup = GetBlockGroup(entityId, groupName);
            if (blockGroup == null)
                return false;

            if (aggregate)
            {
                var component = AggregateProperties.Static.Get(blockGroup);
                if (component != null)
                {
                    component.Remove(connectionId);
                    if (component.IsEmpty)
                        AggregateProperties.Static.Remove(blockGroup);
                }

                return true;
            }
            else
            {
                var blocks = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
                blockGroup.GetBlocks(blocks);

                if (blocks.Count == 0)
                    return false;

                return RemoveValueTracking(blocks[0].EntityId, connectionId);
            }
        }

        public Hashtable GetValueColor(long entityId, string propertyId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return null;

            var color = block.GetValue<Color>(propertyId);
            return color == null ? null : color.ToHashtable();
        }

        private struct CompositeKey
        {
            public long longId;
            public string stringId;

            public CompositeKey(long entityId, string propertyId)
            {
                this.longId = entityId;
                this.stringId = propertyId;
            }
        }
    }

    public static class AccessExtensions
    {
        private static readonly HashSet<VRage.Game.MyRelationsBetweenPlayerAndBlock> allowAcces = new HashSet<VRage.Game.MyRelationsBetweenPlayerAndBlock>() {
            VRage.Game.MyRelationsBetweenPlayerAndBlock.Owner,
            VRage.Game.MyRelationsBetweenPlayerAndBlock.FactionShare};

        public static bool AccesIsAllowed(this Sandbox.ModAPI.Ingame.IMyTerminalBlock self, long playerId)
        {
            return allowAcces.Contains(self.GetUserRelationToOwner(playerId));
        }
        public static bool AccesIsAllowed(this Sandbox.ModAPI.Ingame.IMyBlockGroup self, long playerId)
        {
            var blocks = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            self.GetBlocks(blocks);
            return blocks.Any(x => x.AccesIsAllowed(playerId));
        }
        public static bool HasLocalPlayerAccess(this Sandbox.ModAPI.Ingame.IMyBlockGroup self)
        {
            var blocks = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            self.GetBlocks(blocks);
            return blocks.Any(x => x.HasLocalPlayerAccess());
        }
    }

    public static class ColorExtensions
    {
        public static Hashtable ToHashtable(this Color self)
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
