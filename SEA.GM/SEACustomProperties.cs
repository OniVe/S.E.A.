using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SEA.GM.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace SEA.GM
{
    public static class SEACustomProperties
    {
        public static void Init()
        {
            try
            {
                CustomProperty<Sandbox.ModAPI.Ingame.IMyMotorStator, DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyMotorStator>>.ControlProperty("Virtual Angle",
                    (context) => MyMath.RadiansToDegrees(context.Value),
                    (context, value) => context.Value = MyMath.DegreesToRadians(value),
                    (entity) => new DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyMotorStator>(entity),
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

                CustomProperty<Sandbox.ModAPI.Ingame.IMyPistonBase, DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyPistonBase>>.ControlProperty("Virtual Position",
                    (context) => context.Value,
                    (context, value) => context.Value = value,
                    (entity) => new DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyPistonBase>(entity),
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
            }
            catch (Exception ex)
            {
                SEAUtilities.Logging.Static.WriteLine(SEAUtilities.GetExceptionString(ex));
            }
        }
    }

    public class CustomProperty<T, U> : MyGameLogicComponent where T : class, Sandbox.ModAPI.Ingame.IMyFunctionalBlock where U : CustomPropertyContext
    {
        internal MyObjectBuilder_EntityBase _objectBuilder;
        internal U context;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            _objectBuilder = objectBuilder;
            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
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
        private static U GetContext(IMyEntity entity, Func<IMyEntity, U> constructor, Action<U> initiator)
        {
            var entityGameLogic = SEACompositeGameLogicComponent.Get(entity);

            CustomProperty<T, U> component;
            component = entityGameLogic.GetAs<CustomProperty<T, U>>();
            if (component != null)
                return component.context;

            component = new CustomProperty<T, U>() { context = constructor(entity) };
            entityGameLogic.Add(component);
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
        public static void ControlProperty(string id, Func<U, float> getter, Action<U, float> setter, Func<IMyEntity, U> constructor, Action<U> initiator)
        {
            var property = MyAPIGateway.TerminalControls.CreateProperty<float, T>(id);
            property.SupportsMultipleBlocks = true;
            property.Getter = (block) => getter(GetContext(block, constructor, initiator));
            property.Setter = (block, value) => setter(GetContext(block, constructor, initiator), value);

            MyAPIGateway.TerminalControls.AddControl<T>(property);
        }
    }

    public abstract class CustomPropertyContext
    {
        internal abstract void Update();
    }

    public class DeltaLimitSwitch<T> : CustomPropertyContext where T : class, Sandbox.ModAPI.Ingame.IMyFunctionalBlock
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

        public DeltaLimitSwitch(IMyEntity entity)
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

    public class PropertyValueTracking : MyGameLogicComponent
    {
        private bool isInit = false;
        private uint frameCounter = 0;
        private MyObjectBuilder_EntityBase _objectBuilder;

        private string entityId;
        private Sandbox.ModAPI.Ingame.IMyFunctionalBlock block;

        private Dictionary<string, IContext> propertis;

        private StringBuilder tempStringBuilder;

        private Action<uint, string, IList<string>> doOut;
        public bool IsEmpty { get { return propertis.Count == 0; } }

        public PropertyValueTracking(Action<uint, string, IList<string>> doOut)
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

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            _objectBuilder = objectBuilder;

            block = Entity as Sandbox.ModAPI.Ingame.IMyFunctionalBlock;
            entityId = block.EntityId.ToString();

            tempStringBuilder = new StringBuilder(96);
            propertis = new Dictionary<string, IContext>();

            isInit = true;

            //NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;//Why does not work on all blocks?? (Or is triggered once)
            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
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
                                tempStringBuilder.JObjectStringKeyValuePair(
                                    "eId", entityId,
                                    "propId", element.propertyId,
                                    "value", element.ToString()).ToString(),
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
            bool ValueChange(Sandbox.ModAPI.Ingame.IMyFunctionalBlock value);
        }
        private class Context<T> : IContext where T : struct, IComparable<T>
        {
            public string propertyId { get; private set; }
            public T Value { get; set; }
            public HashSet<string> Clients { get; set; }

            private Context() { }
            public Context(Sandbox.ModAPI.Ingame.IMyFunctionalBlock block, string propertyId, string connectionId)
            {
                this.propertyId = propertyId;
                Clients = new HashSet<string>();
                Clients.Add(connectionId);
                Value = block.GetValue<T>(propertyId);
            }

            public bool ValueChange(Sandbox.ModAPI.Ingame.IMyFunctionalBlock block)
            {
                T value = block.GetValue<T>(propertyId);
                if (Value.Equals(value))
                    return false;

                Value = value;
                return true;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}
