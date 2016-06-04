using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System;
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
        internal DeltaLimitSwitch<T> context;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _objectBuilder = objectBuilder;
            if (Entity != null)
            {
                context = new DeltaLimitSwitch<T>(Entity);
                isInit = this.Init();

                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            }
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
            property.SupportsMultipleBlocks = false;
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

}
