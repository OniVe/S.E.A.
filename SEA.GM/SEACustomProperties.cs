using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SEA.GM.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.Components;
using VRage.ObjectBuilders;

namespace SEA.GM
{
    using SMI = Sandbox.ModAPI.Ingame;

    public static class SEACustomProperties
    {
        public static void Init()
        {
            try
            {
                #region DeltaLimitSwitch Propertis

                CustomProperty<SMI.IMyMotorStator, DeltaLimitSwitch<SMI.IMyMotorStator>>.ControlProperty("Virtual Angle",
                    (context) => MyMath.RadiansToDegrees(context.Value),
                    (context, value) => context.Value = MyMath.DegreesToRadians(value),
                    (entity) => new DeltaLimitSwitch<SMI.IMyMotorStator>(entity),
                    (context) => context.Init(
                        (float)(Math.PI / 360f), // Δ 1.0 degress
                        4f,
                        "Velocity",
                        (block) => block.Angle,
                        (block) =>
                        {
                            if (float.IsInfinity(block.LowerLimit) && float.IsInfinity(block.UpperLimit))
                                return MyMath.ShortestAngle(block.Angle, context.Limit);
                            else
                            {
                                if (context.Limit < block.LowerLimit) context.Value = block.LowerLimit;
                                else if (context.Limit > block.UpperLimit) context.Value = block.UpperLimit;

                                return context.Limit - block.Angle;
                            }
                        })
                    );

                CustomProperty<SMI.IMyPistonBase, DeltaLimitSwitch<SMI.IMyPistonBase>>.ControlProperty("Virtual Position",
                    (context) => context.Value,
                    (context, value) => context.Value = value,
                    (entity) => new DeltaLimitSwitch<SMI.IMyPistonBase>(entity),
                    (context) => context.Init(
                        0.1f, // Δ 0.2 meters
                        2f,
                        "Velocity",
                        (block) => block.CurrentPosition,
                        (block) =>
                        {
                            if (context.Limit < block.MinLimit) context.Value = block.MinLimit;
                            else if (context.Limit > block.MaxLimit) context.Value = block.MaxLimit;

                            return context.Limit - block.CurrentPosition;
                        })
                    );

                #endregion

                #region ReadOnly TerminalBlock Propertis

                /** IMyBatteryBlock **/
                CustomProperty<SMI.IMyBatteryBlock, TerminalBlock<SMI.IMyBatteryBlock>>.ControlProperty("Readonly CurrentStoredPower",
                    (context) => context.Block.CurrentStoredPower, null,
                    (entity) => new TerminalBlock<SMI.IMyBatteryBlock>(entity), null);

                CustomProperty<SMI.IMyBatteryBlock, TerminalBlock<SMI.IMyBatteryBlock>>.ControlProperty("Readonly CurrentInput",
                    (context) => context.Block.CurrentInput, null,
                    (entity) => new TerminalBlock<SMI.IMyBatteryBlock>(entity), null);

                CustomProperty<SMI.IMyBatteryBlock, TerminalBlock<SMI.IMyBatteryBlock>>.ControlProperty("Readonly CurrentOutput",
                    (context) => context.Block.CurrentOutput, null,
                    (entity) => new TerminalBlock<SMI.IMyBatteryBlock>(entity), null);

                CustomProperty<SMI.IMyBatteryBlock, TerminalBlock<SMI.IMyBatteryBlock>>.ControlProperty("Readonly IsCharging",
                    (context) => context.Block.IsCharging, null,
                    (entity) => new TerminalBlock<SMI.IMyBatteryBlock>(entity), null);

                /**  IMyOxygenTank **/
                //CustomProperty<SMI.IMyGasTank, TerminalBlock<SMI.IMyGasTank>>.ControlProperty("Readonly OxygenLevel",
                //   (context) => context.Block.GetValue<>, null,
                //   (entity) => new TerminalBlock<SMI.IMyGasTank>(entity), null);

                /**  IMyProductionBlock **/
                CustomProperty<SMI.IMyProductionBlock, TerminalBlock<SMI.IMyProductionBlock>>.ControlProperty("Readonly IsProducing",
                   (context) => context.Block.IsProducing, null,
                   (entity) => new TerminalBlock<SMI.IMyProductionBlock>(entity), null);

                CustomProperty<SMI.IMyProductionBlock, TerminalBlock<SMI.IMyProductionBlock>>.ControlProperty("Readonly IsQueueEmpty",
                   (context) => context.Block.IsQueueEmpty, null,
                   (entity) => new TerminalBlock<SMI.IMyProductionBlock>(entity), null);

                CustomProperty<SMI.IMyProductionBlock, TerminalBlock<SMI.IMyProductionBlock>>.ControlProperty("Readonly NextItemId",
                   (context) => (float)context.Block.NextItemId, null,
                   (entity) => new TerminalBlock<SMI.IMyProductionBlock>(entity), null);

                /**  IMyReactor **/
                CustomProperty<SMI.IMyReactor, TerminalBlock<SMI.IMyReactor>>.ControlProperty("Readonly CurrentOutput",
                   (context) => context.Block.CurrentOutput, null,
                   (entity) => new TerminalBlock<SMI.IMyReactor>(entity), null);

                /**  IMyShipConnector **/
                CustomProperty<SMI.IMyShipConnector, TerminalBlock<SMI.IMyShipConnector>>.ControlProperty("Readonly IsConnected",
                   (context) => context.Block.Status == SMI.MyShipConnectorStatus.Connected, null,
                   (entity) => new TerminalBlock<SMI.IMyShipConnector>(entity), null);

                CustomProperty<SMI.IMyShipConnector, TerminalBlock<SMI.IMyShipConnector>>.ControlProperty("Readonly IsLocked",
                   (context) => context.Block.Status == SMI.MyShipConnectorStatus.Connected, null,
                   (entity) => new TerminalBlock<SMI.IMyShipConnector>(entity), null);

                /**  IMyUserControllableGun **/
                CustomProperty<SMI.IMyUserControllableGun, TerminalBlock<SMI.IMyUserControllableGun>>.ControlProperty("Readonly IsShooting",
                   (context) => context.Block.IsShooting, null,
                   (entity) => new TerminalBlock<SMI.IMyUserControllableGun>(entity), null);

                /**  IMyCargoContainer **/
                CustomProperty<SMI.IMyCargoContainer, InventoryBlock<SMI.IMyCargoContainer>>.ControlProperty("Readonly CurrentVolume",
                    (context) => (float)context.Inventory.CurrentVolume.RawValue, null,
                    (entity) => new InventoryBlock<SMI.IMyCargoContainer>(entity), null);

                CustomProperty<SMI.IMyCargoContainer, InventoryBlock<SMI.IMyCargoContainer>>.ControlProperty("Readonly CurrentMass",
                    (context) => (float)context.Inventory.CurrentMass.RawValue, null,
                    (entity) => new InventoryBlock<SMI.IMyCargoContainer>(entity), null);

                CustomProperty<SMI.IMyCargoContainer, InventoryBlock<SMI.IMyCargoContainer>>.ControlProperty("Readonly IsFull",
                    (context) => context.Inventory.IsFull, null,
                    (entity) => new InventoryBlock<SMI.IMyCargoContainer>(entity), null);

                /** IMyJumpDrive **/

                CustomProperty<SMI.IMyJumpDrive, TerminalBlock<SMI.IMyJumpDrive>>.ControlProperty("Readonly CanJump",
                    (context) => (context.Block.GetActionWithName("Jump") != null), null,
                    (entity) => new TerminalBlock<SMI.IMyJumpDrive>(entity), null);

                var blockAction = MyAPIGateway.TerminalControls.CreateAction<SMI.IMyJumpDrive>("Virtual Jump");
                blockAction.Enabled = (x) => true;
                blockAction.ValidForGroups = false;
                blockAction.Name = new StringBuilder("Virtual Jump");
                blockAction.Action = (block) =>
                {
                    var a = block.GetActionWithName("Jump");
                    if (a != null) a.Apply(block);
                };
                MyAPIGateway.TerminalControls.AddAction<SMI.IMyJumpDrive>(blockAction);

                //CustomAction<SMI.IMyJumpDrive, TerminalBlock<SMI.IMyJumpDrive>>.ControlAction("Virtual Jump",
                //    (context) =>
                //    {
                //        var a = context.Block.GetActionWithName("Jump");
                //        if (a != null) a.Apply(context.Block);
                //    },
                //    (entity) => new TerminalBlock<SMI.IMyJumpDrive>(entity), null);
                #endregion
            }
            catch (Exception ex)
            {
                SEAUtilities.Logging.Static.WriteLine(SEAUtilities.GetExceptionString(ex));
            }
        }
    }

    public class CustomProperty<TBlock, TContext> : MyGameLogicComponent where TBlock : class, SMI.IMyTerminalBlock where TContext : CustomPropertyContext
    {
        internal MyObjectBuilder_EntityBase _objectBuilder;
        internal TContext context;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            _objectBuilder = objectBuilder;
            NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateBeforeSimulation() { context.Update(); }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) { return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="constructor">new U(entity) !System.Activator not allowed in scipt</param>
        /// <param name="initiator">Like context.init(...)</param>
        /// <returns></returns>
        private static TContext GetContext(VRage.ModAPI.IMyEntity entity, Func<VRage.ModAPI.IMyEntity, TContext> constructor, Action<TContext> initiator)
        {
            var entityGameLogic = SEACompositeGameLogicComponent.Get(entity);

            CustomProperty<TBlock, TContext> component;
            component = entityGameLogic.GetAs<CustomProperty<TBlock, TContext>>();
            if (component != null)
                return component.context;

            component = new CustomProperty<TBlock, TContext>() { context = constructor(entity) };
            entityGameLogic.Add(component);

            if (initiator != null)
                initiator(component.context);

            component.Init(entity.GetObjectBuilder());

            return component.context;
        }

        /// <summary>
        /// Get/set the property for the block.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        /// <param name="constructor">new U(entity) !System.Activator not allowed in scipt</param>
        /// <param name="initiator">Like context.init(...)</param>
        public static void ControlProperty<TValue>(string id, Func<TContext, TValue> getter, Action<TContext, TValue> setter, Func<VRage.ModAPI.IMyEntity, TContext> constructor, Action<TContext> initiator)
        {
            var property = MyAPIGateway.TerminalControls.CreateProperty<TValue, TBlock>(id);

            property.SupportsMultipleBlocks = true;
            property.Getter = (block) => getter(GetContext(block, constructor, initiator));

            if (setter == null)
                property.Setter = (block, value) => { /* Void */ };
            else
                property.Setter = (block, value) => setter(GetContext(block, constructor, initiator), value);

            MyAPIGateway.TerminalControls.AddControl<TBlock>(property);
        }
    }

    public abstract class CustomPropertyContext
    {
        internal abstract void Update();
    }

    public class DeltaLimitSwitch<T> : CustomPropertyContext where T : class, SMI.IMyFunctionalBlock
    {
        private static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(30);

        private bool enabled;
        private float maxDeltaLimit;
        private float maxVelociy;

        private T block;
        private float limit;
        private string propertyId;
        private Func<T, float> valueGetter;
        private Func<T, float> deltaLimitGetter;
        private DateTime timeStamp;

        public bool IsInit { get; private set; }
        public bool Enabled
        {
            get
            {
                if (enabled && (!block.Enabled || !block.IsWorking))
                    enabled = false;

                return enabled;
            }
            set { enabled = value; }
        }

        public float Value { get { return valueGetter(block); } set { this.limit = value; timeStamp = DateTime.UtcNow; enabled = true; } }
        public float Limit { get { return limit; } }

        public DeltaLimitSwitch(VRage.ModAPI.IMyEntity entity)
        {
            IsInit = false;
            this.block = entity as T;
        }

        public void Init(float maxDeltaLimit, float maxVelociy, string propertyId, Func<T, float> valueGetter, Func<T, float> deltaLimitGetter)
        {
            if (block == null)
                return;

            this.maxDeltaLimit = maxDeltaLimit;
            this.maxVelociy = maxVelociy;
            this.propertyId = propertyId;
            this.valueGetter = valueGetter;
            this.deltaLimitGetter = deltaLimitGetter;

            limit = 0;
            enabled = false;
            timeStamp = DateTime.UtcNow;

            IsInit = true;
        }

        internal override void Update()
        {
            if (!Enabled)
                return;

            var deltaLimit = deltaLimitGetter(block);
            var deltaLimitIsNegative = deltaLimit < 0f;

            if (((deltaLimitIsNegative ? -deltaLimit : deltaLimit) <= maxDeltaLimit) || (DateTime.UtcNow.Subtract(timeStamp) > TIMEOUT))
            {
                Enabled = false;
                block.SetValue<float>(propertyId, 0f);
            }
            else
            {

                var propertyValue = block.GetValue<float>(propertyId);
                if ((deltaLimitIsNegative != propertyValue < 0f) || propertyValue == 0f)
                    block.SetValue<float>(propertyId, deltaLimitIsNegative ? -maxVelociy : maxVelociy);
            }
        }
    }

    public class BlockPropertyValueTracking : MyGameLogicComponent
    {
        private bool isInit = false;
        private uint frameCounter = 0;
        private MyObjectBuilder_EntityBase _objectBuilder;

        private string entityId;
        private SMI.IMyTerminalBlock block;

        private Dictionary<string, IContext> propertis;

        private StringBuilder tempStringBuilder;

        private Action<uint, string, IList<string>> doOut;
        public bool IsEmpty { get { return propertis.Count == 0; } }

        public BlockPropertyValueTracking(Action<uint, string, IList<string>> doOut)
        {
            this.doOut = doOut;
        }

        public bool Add(string connectionId, string propertyId)
        {
            var property = block.GetProperty(propertyId);
            if (property == null)
                return false;

            if (propertis.ContainsKey(propertyId))
                return propertis[propertyId].Clients.Add(connectionId);
            else
            {
                if (property.Is<float>())
                    propertis.Add(propertyId, new Context<float>(block, propertyId, connectionId));
                else if (property.Is<bool>())
                    propertis.Add(propertyId, new Context<bool>(block, propertyId, connectionId));
                else
                    return false;
            }
            return true;
        }

        public void Remove(string connectionId)
        {
            foreach (var element in propertis.Where(e => e.Value.Clients.Contains(connectionId)).ToArray())
                if (element.Value.Clients.Count == 1)
                    propertis.Remove(element.Key);
                else
                    element.Value.Clients.Remove(connectionId);
        }
        public void Remove(string connectionId, string propertyId)
        {
            foreach (var element in propertis.Where(e => e.Key == propertyId && e.Value.Clients.Contains(connectionId)).ToArray())
                if (element.Value.Clients.Count == 1)
                    propertis.Remove(element.Key);
                else
                    element.Value.Clients.Remove(connectionId);
        }

        public void Init(VRage.Game.ModAPI.Ingame.IMyEntity entity)
        {
            _objectBuilder = MyAPIGateway.Entities.GetEntityById(entity.EntityId).GetObjectBuilder();

            block = Entity as SMI.IMyTerminalBlock;
            entityId = block.EntityId.ToString();

            tempStringBuilder = new StringBuilder(96);
            propertis = new Dictionary<string, IContext>();

            isInit = true;

            //NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;//Why does not work on all blocks?? (Or is triggered once)
            NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            if (!isInit)
                return;

            if (frameCounter == 0u)
            {
                frameCounter = 19u;//20fps
                try
                {
                    foreach (var element in propertis.Values)
                        if (element.ValueChange(block))
                        {
                            tempStringBuilder.Length = 0;
                            doOut(1,
                                tempStringBuilder
                                    .JObjectStart()
                                    .JStringKeyValuePair(
                                        "eId", entityId,
                                        "propId", element.propertyId)
                                    .JSplit()
                                    .JKeyValuePair(
                                        "value", element.ToString())
                                    .JObjectEnd()
                                    .ToString(),
                                element.Clients.ToArray());
                        }
                }
                catch { }
            }
            else
                frameCounter--;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) { return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder; }

        private interface IContext
        {
            string propertyId { get; }
            HashSet<string> Clients { get; set; }
            bool ValueChange(SMI.IMyTerminalBlock block);
        }
        private class Context<T> : IContext where T : struct, IComparable<T>
        {
            public string propertyId { get; private set; }
            public T Value { get; set; }
            public HashSet<string> Clients { get; set; }

            private Context() { }
            public Context(SMI.IMyTerminalBlock block, string propertyId, string connectionId)
            {
                this.propertyId = propertyId;
                Clients = new HashSet<string>();
                Clients.Add(connectionId);
                Value = block.GetValue<T>(propertyId);
            }

            public bool ValueChange(SMI.IMyTerminalBlock block)
            {
                T value = block.GetValue<T>(propertyId);
                if (Value.Equals(value))
                    return false;

                Value = value;
                return true;
            }

            public override string ToString()
            {
                return Convert.ToString(Value, SEAUtilities.CultureInfoUS).ToLower();
            }
        }
    }

    public class AggregatePropertyValueTracking
    {
        private string entityId;
        private SMI.IMyBlockGroup blockGroup;

        private Dictionary<string, IContext> propertis;

        private StringBuilder tempStringBuilder;

        private Action<uint, string, IList<string>> doOut;
        public bool IsEmpty { get { return propertis.Count == 0; } }

        public AggregatePropertyValueTracking(SMI.IMyBlockGroup blockGroup, string entityId, Action<uint, string, IList<string>> doOut)
        {
            tempStringBuilder = new StringBuilder(96);
            propertis = new Dictionary<string, IContext>();

            this.blockGroup = blockGroup;
            this.entityId = entityId;
            this.doOut = doOut;
        }

        public bool Add(string connectionId, string propertyId)
        {
            var property = blockGroup.GetProperty(propertyId);
            if (property == null)
                return false;

            if (propertis.ContainsKey(propertyId))
                return propertis[propertyId].Clients.Add(connectionId);
            else
            {
                if (property.Is<float>())
                    propertis.Add(propertyId, new ContextFloat(blockGroup, propertyId, connectionId));
                else if (property.Is<bool>())
                    propertis.Add(propertyId, new ContextBool(blockGroup, propertyId, connectionId));
                else
                    return false;
            }
            return true;
        }

        public void Remove(string connectionId)
        {
            foreach (var element in propertis.Where(e => e.Value.Clients.Contains(connectionId)).ToArray())
                if (element.Value.Clients.Count == 1)
                    propertis.Remove(element.Key);
                else
                    element.Value.Clients.Remove(connectionId);
        }
        public void Remove(string connectionId, string propertyId)
        {
            foreach (var element in propertis.Where(e => e.Key == propertyId && e.Value.Clients.Contains(connectionId)).ToArray())
                if (element.Value.Clients.Count == 1)
                    propertis.Remove(element.Key);
                else
                    element.Value.Clients.Remove(connectionId);
        }

        public void Update()
        {
            try
            {
                foreach (var element in propertis.Values)
                    if (element.ValueChange(blockGroup))
                    {
                        tempStringBuilder.Length = 0;
                        doOut(1,
                            tempStringBuilder
                                .JObjectStart()
                                .JKeyValuePair("eId", entityId)
                                .JSplit()
                                .JStringKeyValuePair("propId", element.propertyId)
                                .JSplit()
                                .JKeyValuePair("value", element.ToString())
                                .JObjectEnd().ToString(),
                            element.Clients.ToArray());
                    }
            }
            catch
            { }
        }

        private interface IContext
        {
            string propertyId { get; }
            HashSet<string> Clients { get; set; }
            bool ValueChange(SMI.IMyBlockGroup blockGroup);
        }
        private abstract class Context
        {
            public string propertyId { get; internal set; }
            public HashSet<string> Clients { get; set; }
        }

        private class ContextFloat : Context, IContext
        {
            public float Value { get; set; }

            private ContextFloat() { }
            public ContextFloat(SMI.IMyBlockGroup blockGroup, string propertyId, string connectionId)
            {
                this.propertyId = propertyId;
                Clients = new HashSet<string>();
                Clients.Add(connectionId);
                Value = blockGroup.GetValueFloat(propertyId);
            }

            public bool ValueChange(SMI.IMyBlockGroup blockGroup)
            {
                float value = blockGroup.GetValueFloat(propertyId);
                if (Value == value)
                    return false;

                Value = value;
                return true;
            }

            public override string ToString()
            {
                return Convert.ToString(Value, SEAUtilities.CultureInfoUS).ToLower();
            }
        }
        private class ContextBool : Context, IContext
        {
            public bool Value { get; set; }

            private ContextBool() { }
            public ContextBool(SMI.IMyBlockGroup blockGroup, string propertyId, string connectionId)
            {
                this.propertyId = propertyId;
                Clients = new HashSet<string>();
                Clients.Add(connectionId);
                Value = blockGroup.GetValueBool(propertyId);
            }

            public bool ValueChange(SMI.IMyBlockGroup blockGroup)
            {
                bool value = blockGroup.GetValueBool(propertyId);
                if (Value == value)
                    return false;

                Value = value;
                return true;
            }

            public override string ToString()
            {
                return Convert.ToString(Value, SEAUtilities.CultureInfoUS).ToLower();
            }
        }
    }

    public class TerminalBlock<T> : CustomPropertyContext where T : class, SMI.IMyTerminalBlock
    {
        public T Block { get; private set; }

        public TerminalBlock(VRage.ModAPI.IMyEntity entity)
        {
            this.Block = entity as T;
        }

        internal override void Update() { return; }
    }

    public class InventoryBlock<T> : TerminalBlock<T> where T : class, SMI.IMyTerminalBlock
    {
        public VRage.Game.ModAPI.Ingame.IMyInventory Inventory;

        public InventoryBlock(VRage.ModAPI.IMyEntity entity) : base(entity)
        {
            Inventory = entity.GetInventory();
        }
    }

    public class AggregateProperties
    {
        private static AggregateProperties m_instance = null;
        private static bool isInit = false;
        private uint frameCounter = 0;
        private Dictionary<SMI.IMyBlockGroup, AggregatePropertyValueTracking> container;

        public static AggregateProperties Static
        {
            get
            {
                return m_instance;
            }
        }
        public static bool IsInit
        {
            get
            {
                return isInit;
            }
        }

        private AggregateProperties()
        {
            container = new Dictionary<SMI.IMyBlockGroup, AggregatePropertyValueTracking>();
        }
        public static void Init()
        {
            m_instance = new AggregateProperties();
            isInit = true;
        }

        public AggregatePropertyValueTracking Get(SMI.IMyBlockGroup blockGroup)
        {
            return container.GetValueOrDefault(blockGroup);
        }

        public void Add(SMI.IMyBlockGroup blockGroup, AggregatePropertyValueTracking element)
        {
            container.Add(blockGroup, element);
        }

        public void Remove(SMI.IMyBlockGroup blockGroup)
        {
            container.Remove(blockGroup);
        }

        public void UpdateAfterSimulation()
        {
            if (frameCounter == 0u)
            {
                frameCounter = 19u;
                foreach (var element in container.Values)
                    element.Update();
            }
            else
                frameCounter--;
        }
    }

    public static class IMyBlockGroupExtension
    {
        public static ITerminalProperty GetProperty(this SMI.IMyBlockGroup self, string id)
        {
            var blocks = new List<SMI.IMyTerminalBlock>();
            self.GetBlocks(blocks);

            return blocks.Count == 0 ? null : blocks[0].GetProperty(id);
        }

        public static float GetValueFloat(this SMI.IMyBlockGroup self, string propertyId)
        {
            var blocks = new List<SMI.IMyTerminalBlock>();
            self.GetBlocks(blocks);

            float value = 0;
            int i = blocks.Count;
            while (i > 0)
                value += blocks[--i].GetValue<float>(propertyId);

            return value;
        }

        public static bool GetValueBool(this SMI.IMyBlockGroup self, string propertyId)
        {
            var blocks = new List<SMI.IMyTerminalBlock>();
            self.GetBlocks(blocks);

            return blocks.Count == 0 ? false : blocks[0].GetValue<bool>(propertyId);
        }
    }
}
