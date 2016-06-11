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

        private IMyGridTerminalSystem GetTerminalSystem(long entityId)
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

        private List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> GetGroupBlocks(long entityId, string groupName, bool first = false)
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
                blockList.Add(new BlockView<string>() { type = blocks[i].GetType().ToString(), id = blocks[i].EntityId.ToString(SEAUtilities.CultureInfoUS), name = blocks[i].Name });
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
        public float? GetValueFloat(long entityId, string groupName, string propertyId)
        {
            var blocks = GetGroupBlocks(entityId, groupName, true);
            if (blocks == null || blocks.Count == 0)
                return null;
            else
                return blocks[0].GetValue<float>(propertyId);
        }

        public bool AddValueTracking(long entityId, string propertyId, string connectionId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;
            
            var entityGameLogic = GameLogic.SEACompositeGameLogicComponent.Get(block);

            PropertyValueTracking component;
            component = entityGameLogic.GetAs<PropertyValueTracking>();
            if (component == null)
            {
                component = new PropertyValueTracking(doOut);
                entityGameLogic.Add(component);
                component.Init(block.GetObjectBuilder());
            }

            return component.Add(connectionId, propertyId);
        }

        public bool RemoveValueTracking(long entityId, string propertyId, string connectionId)
        {
            var block = GetTerminalBlock(entityId);
            if (block == null)
                return false;

            var entityGameLogic = GameLogic.SEACompositeGameLogicComponent.Get(block);

            PropertyValueTracking component;
            component = entityGameLogic.GetAs<PropertyValueTracking>();
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

            PropertyValueTracking component;
            component = entityGameLogic.GetAs<PropertyValueTracking>();
            if (component != null)
            {
                component.Remove(connectionId);
                if (component.IsEmpty)
                    entityGameLogic.Remove(component);
            }

            return true;
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
            return self.Blocks.Any(x => x.AccesIsAllowed(playerId));
        }
        public static bool HasLocalPlayerAccess(this Sandbox.ModAPI.Ingame.IMyBlockGroup self)
        {
            return self.Blocks.Any(x => x.HasLocalPlayerAccess());
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
