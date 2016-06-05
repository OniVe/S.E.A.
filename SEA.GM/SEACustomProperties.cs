using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game;

namespace SEA.GM
{
    public static class SEACustomProperties
    {
        public static void Init()
        {
            try
            {
                MotorStatorAngleProperty.AddControlProperty<MotorStatorAngleProperty>("Virtual Angle",
                    (context) => MyMath.RadiansToDegrees(context.Value),
                    (context, value) => context.Value = MyMath.DegreesToRadians(value));

                PistonBasePositionProperty.AddControlProperty<PistonBasePositionProperty>("Virtual Position",
                    (context) => context.Value,
                    (context, value) => context.Value = value);
            }
            catch (Exception ex)
            {
                SEAUtilities.Logging.Static.WriteLine(SEAUtilities.GetExceptionString(ex));
            }
        }
    }

    public abstract class LimitProperty<T> : MyGameLogicComponent where T : class, Sandbox.ModAPI.Ingame.IMyFunctionalBlock
    {
        internal bool isInit = false;
        internal MyObjectBuilder_EntityBase _objectBuilder;
        internal DeltaLimitSwitch<T> context;//context should be changed to Dictionary (multiple Properties)

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (isInit)
                return;

            base.Init(objectBuilder);

            _objectBuilder = objectBuilder;

            context = new DeltaLimitSwitch<T>(Entity);
            isInit = this.Init();

            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        public abstract bool Init();

        public override void UpdateBeforeSimulation()
        {
            if (isInit)
                context.Update();
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) { return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder; }

        public static void AddControlProperty<U>(string id, Func<DeltaLimitSwitch<T>, float> getter, Action<DeltaLimitSwitch<T>, float> setter) where U : LimitProperty<T>
        {
            var property = MyAPIGateway.TerminalControls.CreateProperty<float, T>(id);
            property.SupportsMultipleBlocks = true;
            property.Getter = (block) => { return getter(block.GameLogic.GetAs<U>().context); };
            property.Setter = (block, value) => { setter(block.GameLogic.GetAs<U>().context, value); };

            MyAPIGateway.TerminalControls.AddControl<T>(property);
        }
    }

    public class DeltaLimitSwitch<T> where T : class, Sandbox.ModAPI.Ingame.IMyFunctionalBlock
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

        public DeltaLimitSwitch(IMyEntity block)
        {
            this.block = block as T;
        }

        public bool Init(float maxDeltaLimit, float maxVelociy, string propertyId, Func<T, float> valueGetter, Func<T, float> deltaLimitGetter)
        {
            if (block == null)
                return false;

            this.maxDeltaLimit = maxDeltaLimit;
            this.maxVelociy = maxVelociy;
            this.propertyId = propertyId;
            this.valueGetter = valueGetter;
            this.deltaLimitGetter = deltaLimitGetter;

            limit = 0;
            enabled = false;
            timeStamp = DateTime.UtcNow;

            return true;
        }

        public void Update()
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

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorStator))]
    public class MotorStatorAngleProperty : LimitProperty<Sandbox.ModAPI.Ingame.IMyMotorStator>
    {
        public override bool Init()
        {
            return context.Init(
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
                });
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorAdvancedStator))]
    public class MotorAdvancedStatorAngleProperty : MotorStatorAngleProperty { }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_PistonBase))]
    public class PistonBasePositionProperty : LimitProperty<Sandbox.ModAPI.Ingame.IMyPistonBase>
    {
        public override bool Init()
        {
            return context.Init(
                0.1f, // Δ 0.2 meters
                2f,
                "Velocity",
                (block) => block.CurrentPosition,
                (block) =>
                {
                    if (context.Limit < block.MinLimit) context.Value = block.MinLimit;
                    else if (context.Limit > block.MaxLimit) context.Value = block.MaxLimit;

                    return context.Limit - block.CurrentPosition;
                });
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_ExtendedPistonBase))]
    public class ExtendedPistonBasePositionProperty : PistonBasePositionProperty { }

    public class MonitorPropertyChanges : SEAGameLogicComponent
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
            if (isInit)
                return;

            base.Init(objectBuilder);
            _objectBuilder = objectBuilder;

            block = Entity as Sandbox.ModAPI.Ingame.IMyFunctionalBlock;

            isInit = true;

            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            if (!isInit)
                return;

            SEAUtilities.Logging.Static.WriteLine("MonitorPropertyChanges UpdateAfterSimulation100(block:" + block.EntityId.ToString() + ")");

            foreach (var element in propertisFloat)
            {
                tempFloatValue = block.GetValue<float>(element.Key);
                if (tempFloatValue == element.Value)
                    continue;

                propertisFloat[element.Key] = tempFloatValue;

                tempStringBuilder.Length = 0;
                tempStringBuilder
                    .JObjectStart()
                    .JObjectStringKeyValuePair(
                        "eId", block.EntityId.ToString(),
                        "propId", element.Key,
                        "value", tempFloatValue.ToString())
                    .JObjectEnd();

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
                    .JObjectStart()
                    .JObjectStringKeyValuePair(
                        "eId", block.EntityId.ToString(),
                        "propId", element.Key,
                        "value", tempBoolValue.ToString())
                    .JObjectEnd();

                doOut(1, tempStringBuilder.ToString());
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) { return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder; }
    }


    public abstract class SEAGameLogicComponent : MyEntityComponentBase
    {
        public MyEntityUpdateEnum NeedsUpdate
        {
            get
            {
                MyEntityUpdateEnum needsUpdate = MyEntityUpdateEnum.NONE;

                if ((Container.Entity.Flags & EntityFlags.NeedsUpdate) != 0)
                    needsUpdate |= MyEntityUpdateEnum.EACH_FRAME;

                if ((Container.Entity.Flags & EntityFlags.NeedsUpdate10) != 0)
                    needsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;

                if ((Container.Entity.Flags & EntityFlags.NeedsUpdate100) != 0)
                    needsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;

                if ((Container.Entity.Flags & EntityFlags.NeedsUpdateBeforeNextFrame) != 0)
                    needsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

                return needsUpdate;
            }
            set
            {
                bool hasChanged = value != NeedsUpdate;

                if (hasChanged)
                {
                    if (Container.Entity.InScene)
                        MyAPIGatewayShortcuts.UnregisterEntityUpdate(Container.Entity, false);

                    Container.Entity.Flags &= ~EntityFlags.NeedsUpdateBeforeNextFrame;
                    Container.Entity.Flags &= ~EntityFlags.NeedsUpdate;
                    Container.Entity.Flags &= ~EntityFlags.NeedsUpdate10;
                    Container.Entity.Flags &= ~EntityFlags.NeedsUpdate100;

                    if ((value & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) != 0)
                        Container.Entity.Flags |= EntityFlags.NeedsUpdateBeforeNextFrame;
                    if ((value & MyEntityUpdateEnum.EACH_FRAME) != 0)
                        Container.Entity.Flags |= EntityFlags.NeedsUpdate;
                    if ((value & MyEntityUpdateEnum.EACH_10TH_FRAME) != 0)
                        Container.Entity.Flags |= EntityFlags.NeedsUpdate10;
                    if ((value & MyEntityUpdateEnum.EACH_100TH_FRAME) != 0)
                        Container.Entity.Flags |= EntityFlags.NeedsUpdate100;

                    if (Container.Entity.InScene)
                        MyAPIGatewayShortcuts.RegisterEntityUpdate(Container.Entity);
                }
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool Closed { get; protected set; }
        [System.Xml.Serialization.XmlIgnore]
        public bool MarkedForClose { get; protected set; }
        //Called after internal implementation
        public virtual void UpdateOnceBeforeFrame()
        { }
        public virtual void UpdateBeforeSimulation()
        { }
        public virtual void UpdateBeforeSimulation10()
        { }
        public virtual void UpdateBeforeSimulation100()
        { }
        public virtual void UpdateAfterSimulation()
        { }
        public virtual void UpdateAfterSimulation10()
        { }
        public virtual void UpdateAfterSimulation100()
        { }
        public virtual void UpdatingStopped()
        { }

        //Entities are usualy initialized from builder immediately after creation by factory
        public virtual void Init(MyObjectBuilder_EntityBase objectBuilder)
        { }
        public abstract MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false);

        //Only use for setting flags, no heavy logic or cleanup
        public virtual void MarkForClose()
        { }

        //Called before internal implementation
        //Cleanup here
        public virtual void Close()
        { }

        public override string ComponentTypeDebugString
        {
            get { return "SEA Game Logic"; }
        }
    }
}
