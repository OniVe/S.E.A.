using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SEA.GM.GameLogic;
using System;
using System.Collections.Generic;
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
            base.Init(objectBuilder);
            _objectBuilder = objectBuilder;
            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateBeforeSimulation()
        {
            //SEAUtilities.Logging.Static.WriteLine("LimitProperty UpdateBeforeSimulation(block:" + Entity.EntityId.ToString() + ") T: " + this.GetType().ToString());
            context.Update();
        }

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

            /*SEAUtilities.Logging.Static.WriteLine("GetContext (block:" + entity.EntityId.ToString() + ")");
            MyGameLogicComponent gameLogic;
            CustomProperty<T, U> component;

            if (entity.GameLogic.Container.TryGet<MyGameLogicComponent>(out gameLogic) && !(gameLogic is MyNullGameLogicComponent))
            {
                SEAUtilities.Logging.Static.WriteLine(" GetContext is GameLogic:" + gameLogic.GetType().ToString());
                if (gameLogic is SEACompositeGameLogicComponent)
                {
                    SEAUtilities.Logging.Static.WriteLine(" GetContext GameLogic is SEACompositeGameLogicComponent");
                    component = gameLogic.GetAs<CustomProperty<T, U>>();
                    if (component != null)
                        return component.context;
                }
            }
            else
            {
                gameLogic = new SEACompositeGameLogicComponent(entity);
                SEAUtilities.Logging.Static.WriteLine(" GetContext new SEACompositeGameLogicComponent");
                entity.GameLogic.Container.Add<MyGameLogicComponent>(gameLogic);
            }

            SEAUtilities.Logging.Static.WriteLine(" Types:");
            foreach (var e in entity.GameLogic.Container.GetComponentTypes())
                SEAUtilities.Logging.Static.WriteLine("     " + e.ToString());

            SEAUtilities.Logging.Static.WriteLine(" Full Types:");
            foreach (var e in entity.GameLogic.Container)
                SEAUtilities.Logging.Static.WriteLine("     " + e.GetType().ToString());

            SEAUtilities.Logging.Static.WriteLine(" GetContext Component Add to Container");
            component = new CustomProperty<T, U>() { context = constructor(entity) };

            ((SEACompositeGameLogicComponent)gameLogic).Add(component);

            initiator(component.context);
            component.Init(entity.GetObjectBuilder());

            return component.context;*/
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

    public class MonitorPropertyChanges : MyGameLogicComponent
    {
        internal bool isInit = false;
        internal MyObjectBuilder_EntityBase _objectBuilder;

        internal Sandbox.ModAPI.Ingame.IMyFunctionalBlock block;

        internal Dictionary<string, float> propertisFloat = new Dictionary<string, float>();
        internal Dictionary<string, bool> propertisBool = new Dictionary<string, bool>();

        internal float tempFloatValue = 0;
        internal bool tempBoolValue = false;
        internal StringBuilder tempStringBuilder = new StringBuilder();

        internal Action<uint, string> doOut;

        public MonitorPropertyChanges(Action<uint, string> doOut)
        {
            this.doOut = doOut;
        }

        internal bool Add(string propertyId)
        {
            SEAUtilities.Logging.Static.WriteLine("MonitorPropertyChanges Add(" + propertyId + ")");
            //propertyId = propertyId.ToLower();
            var property = block.GetProperty(propertyId);
            if (property == null)
                return false;

            if (property.Is<float>())
            {
                SEAUtilities.Logging.Static.WriteLine("    MonitorPropertyChanges is float");
                if (!propertisFloat.ContainsKey(propertyId))
                    propertisFloat.Add(propertyId, block.GetValue<float>(propertyId));
            }
            else if (property.Is<bool>())
            {
                SEAUtilities.Logging.Static.WriteLine("    MonitorPropertyChanges is bool");
                if (!propertisBool.ContainsKey(propertyId))
                    propertisBool.Add(propertyId, block.GetValue<bool>(propertyId));
            }
            else
                return false;
              
            return true;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            _objectBuilder = objectBuilder;

            SEAUtilities.Logging.Static.WriteLine("MonitorPropertyChanges Init()");

            block = Entity as Sandbox.ModAPI.Ingame.IMyFunctionalBlock;

            isInit = true;

            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            if (!isInit)
                return;

            //SEAUtilities.Logging.Static.WriteLine("MonitorPropertyChanges UpdateAfterSimulation100(block:" + Entity.EntityId.ToString() + ")");
            try
            {
                foreach (var element in propertisFloat)
                {
                    tempFloatValue = block.GetValue<float>(element.Key);
                    if (tempFloatValue == element.Value)
                        continue;

                    propertisFloat[element.Key] = tempFloatValue;

                    tempStringBuilder.Length = 0;
                    tempStringBuilder
                        .JObjectStringKeyValuePair(
                            "eId", block.EntityId.ToString(),
                            "propId", element.Key,
                            "value", tempFloatValue.ToString());

                    doOut(1, tempStringBuilder.ToString());
                }

                foreach (var element in propertisBool)
                {
                    tempBoolValue = block.GetValue<bool>(element.Key);
                    if (tempBoolValue == element.Value)
                        continue;

                    propertisBool[element.Key] = tempBoolValue;

                    tempStringBuilder.Length = 0;
                    tempStringBuilder
                        .JObjectStringKeyValuePair(
                            "eId", block.EntityId.ToString(),
                            "propId", element.Key,
                            "value", tempBoolValue.ToString());

                    doOut(1, tempStringBuilder.ToString());
                }
            }
            catch { }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) { return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder; }
    }
}
