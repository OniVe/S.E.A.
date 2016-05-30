using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System;
using VRage.Game.Components;
using VRage.ObjectBuilders;

namespace SEA.GM
{
    public static class SEACustomProperties
    {
        public static void Init()
        {
            MotorStatorAngleProperty.InitControl();
            MotorAdvancedStatorAngleProperty.InitControl();
            PistonBasePositionProperty.InitControl();
        }
    }

    internal struct DeltaLimitSwitch<T> where T : class, Sandbox.ModAPI.Ingame.IMyFunctionalBlock
    {
        private static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(30);

        private bool enabled;
        private float deltaLimit;
        private float maxVelociy;

        private T block;
        private float limit;
        private Func<T, float> propertyGetter;
        private Func<T, float> deltaLimitGetter;
        private string propertyId;
        private DateTime timeStamp;

        public bool Enabled
        {
            get
            {
                if (!block.Enabled || !block.IsWorking)
                    enabled = false;
                return enabled;
            }
            set { enabled = value; }
        }

        public float Value { get { return propertyGetter(block); } }
        public float Limit { get { return limit; } set { this.limit = value; timeStamp = DateTime.UtcNow; enabled = true; } }

        public DeltaLimitSwitch(VRage.ModAPI.IMyEntity block, float deltaLimit, float maxVelociy, string propertyId, Func<T, float> propertyGetter, Func<T, float> deltaLimitGetter)
        {
            this.block = block as T;

            this.deltaLimit = deltaLimit;
            this.maxVelociy = maxVelociy;
            this.propertyId = propertyId;
            this.propertyGetter = propertyGetter;
            this.deltaLimitGetter = deltaLimitGetter;

            limit = 0;
            enabled = false;
            timeStamp = DateTime.UtcNow;
        }

        public void Update()
        {
            if (!Enabled)
                return;

            var _deltaLimit = deltaLimitGetter(block);

            if (((_deltaLimit < 0f ? -_deltaLimit : _deltaLimit) <= deltaLimit) || (DateTime.UtcNow.Subtract(timeStamp) > TIMEOUT))
            {
                Enabled = false;
                block.SetValue<float>(propertyId, 0f);
            }
            else if ((_deltaLimit < 0f != Value < 0f) || Value == 0f)
                block.SetValue<float>(propertyId, _deltaLimit < 0f ? -maxVelociy : maxVelociy);
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorStator))]
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 10000)]
    public class MotorStatorAngleProperty : MyGameLogicComponent
    {
        MyObjectBuilder_EntityBase _objectBuilder;

        DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyMotorStator> context;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _objectBuilder = objectBuilder;

            context = new DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyMotorStator>(
                Entity,
                (float)(Math.PI / 360f),
                4f,
                "Velocity",
                (block) => block.Angle,
                (block) =>
                {
                    if (float.IsInfinity(block.LowerLimit) && float.IsInfinity(block.UpperLimit))
                        return MyMath.ShortestAngle(block.Angle, context.Limit);
                    else
                    {
                        if (context.Limit < block.LowerLimit) context.Limit = block.LowerLimit;
                        else if (context.Limit > block.UpperLimit) context.Limit = block.UpperLimit;

                        return context.Limit - block.Angle;
                    }
                });

            NeedsUpdate = VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            context.Update();

            base.UpdateAfterSimulation();
        }

        public static void InitControl()
        {
            var customProperty = MyAPIGateway.TerminalControls.CreateProperty<float, Sandbox.ModAPI.Ingame.IMyMotorStator>("Virtual Angle");
            customProperty.SupportsMultipleBlocks = true;
            customProperty.Getter = (block) => { return MyMath.RadiansToDegrees(block.GameLogic.GetAs<MotorStatorAngleProperty>().context.Value); };
            customProperty.Setter = (block, value) => { block.GameLogic.GetAs<MotorStatorAngleProperty>().context.Limit = MyMath.DegreesToRadians(value); };

            MyAPIGateway.TerminalControls.AddControl<Sandbox.ModAPI.Ingame.IMyMotorStator>(customProperty);
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder;
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorAdvancedStator))]
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 10000)]
    public class MotorAdvancedStatorAngleProperty : MyGameLogicComponent
    {
        MyObjectBuilder_EntityBase _objectBuilder;

        DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator> context;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _objectBuilder = objectBuilder;

            context = new DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator>(
                Entity,
                (float)(Math.PI / 360f),
                4f,
                "Velocity",
                (block) => block.Angle,
                (block) =>
                {
                    if (float.IsInfinity(block.LowerLimit) && float.IsInfinity(block.UpperLimit))
                        return MyMath.ShortestAngle(block.Angle, context.Limit);
                    else
                    {
                        if (context.Limit < block.LowerLimit) context.Limit = block.LowerLimit;
                        else if (context.Limit > block.UpperLimit) context.Limit = block.UpperLimit;

                        return context.Limit - block.Angle;
                    }
                });

            NeedsUpdate = VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            context.Update();

            base.UpdateAfterSimulation();
        }

        public static void InitControl()
        {
            var customProperty = MyAPIGateway.TerminalControls.CreateProperty<float, Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator>("Virtual Angle");
            customProperty.SupportsMultipleBlocks = true;
            customProperty.Getter = (block) => { return MyMath.RadiansToDegrees(block.GameLogic.GetAs<MotorAdvancedStatorAngleProperty>().context.Value); };
            customProperty.Setter = (block, value) => { block.GameLogic.GetAs<MotorAdvancedStatorAngleProperty>().context.Limit = MyMath.DegreesToRadians(value); };

            MyAPIGateway.TerminalControls.AddControl<Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator>(customProperty);
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder;
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_PistonBase))]
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 10000)]
    public class PistonBasePositionProperty : MyGameLogicComponent
    {
        MyObjectBuilder_EntityBase _objectBuilder;

        DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyPistonBase> context;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _objectBuilder = objectBuilder;

            context = new DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyPistonBase>(
                Entity,
                0.25f,
                2f,
                "Velocity",
                (block) => block.CurrentPosition,
                (block) =>
                {
                    if (context.Limit < block.MinLimit) context.Limit = block.MinLimit;
                    else if (context.Limit > block.MaxLimit) context.Limit = block.MaxLimit;

                    return context.Limit - block.CurrentPosition;
                });

            NeedsUpdate = VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            context.Update();

            base.UpdateAfterSimulation();
        }

        public static void InitControl()
        {
            var customProperty = MyAPIGateway.TerminalControls.CreateProperty<float, Sandbox.ModAPI.Ingame.IMyPistonBase>("Virtual Position");
            customProperty.SupportsMultipleBlocks = true;
            customProperty.Getter = (block) => { return block.GameLogic.GetAs<PistonBasePositionProperty>().context.Value; };
            customProperty.Setter = (block, value) => { block.GameLogic.GetAs<PistonBasePositionProperty>().context.Limit = value; };

            MyAPIGateway.TerminalControls.AddControl<Sandbox.ModAPI.Ingame.IMyPistonBase>(customProperty);
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder;
        }
    }
}
