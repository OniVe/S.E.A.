using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System;
using VRage.Game.Components;
using VRage.ObjectBuilders;


namespace SEA.GM.Controls
{
    public static class SEACustomControls
    {
        public static void Init()
        {
            MotorStatorAngleProperty.InitControl();
        }
    }

    struct DeltaLimitSwitch<T> where T : class, Sandbox.ModAPI.Ingame.IMyFunctionalBlock
    {
        private static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(30);

        private bool enabled;
        private float deltaLimit;
        private float maxVelociy;

        private T block;
        private float value;
        private Func<T, float> propertyGetter;
        private Func<T, float> deltaValueGetter;
        private string propertyId;
        private DateTime timeStamp;
        private float deltaValue;

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

        public float Value { get { return value; } set { this.value = value; timeStamp = DateTime.UtcNow; enabled = true; } }

        public DeltaLimitSwitch(VRage.ModAPI.IMyEntity block, float deltaLimit, float maxVelociy, string propertyId, Func<T, float> propertyGetter, Func<T, float> deltaValueGetter)
        {
            this.block = block as T;

            this.deltaLimit = deltaLimit;
            this.maxVelociy = maxVelociy;
            this.propertyId = propertyId;
            this.propertyGetter = propertyGetter;
            this.deltaValueGetter = deltaValueGetter;

            value = 0;
            deltaValue = 0;
            enabled = false;
            timeStamp = DateTime.UtcNow;
        }

        public void Update()
        {
            deltaValue = deltaValueGetter(block);

            if (((deltaValue < 0f ? -deltaValue : deltaValue) <= deltaLimit) || (DateTime.UtcNow.Subtract(timeStamp) > TIMEOUT))
            {
                Enabled = false;
                block.SetValue<float>(propertyId, 0f);
            }
            else if ((deltaValue < 0f != propertyGetter(block) < 0f) || propertyGetter(block) == 0f)
                block.SetValue<float>(propertyId, deltaValue < 0f ? -maxVelociy : maxVelociy);
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorStator))]
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 10000)]
    class MotorStatorAngleProperty : MyGameLogicComponent
    {
        MyObjectBuilder_EntityBase _objectBuilder;

        /// <summary>
        /// DeltaLimitSwitch
        /// </summary>
        DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyMotorStator> dls;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _objectBuilder = objectBuilder;

            dls = new DeltaLimitSwitch<Sandbox.ModAPI.Ingame.IMyMotorStator>(
                Entity,
                (float)(Math.PI / 360f),
                4,
                "Velocity",
                (block) => block.Angle,
                (block) =>
                {
                    if (float.IsInfinity(block.LowerLimit) && float.IsInfinity(block.UpperLimit))
                        return MyMath.ShortestAngle(block.Angle, dls.Value);
                    else
                    {
                        if (dls.Value < block.LowerLimit) dls.Value = block.LowerLimit;
                        else if (dls.Value > block.UpperLimit) dls.Value = block.UpperLimit;

                        return dls.Value - block.Angle;
                    }
                });

            NeedsUpdate = VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            if (!dls.Enabled)
                return;

            dls.Update();

            base.UpdateAfterSimulation();
        }

        public static void InitControl()
        {
            var customProperty = MyAPIGateway.TerminalControls.CreateProperty<float, Sandbox.ModAPI.Ingame.IMyMotorStator>("CustomAngle");
            customProperty.Getter = CustomPropertyGetter;
            customProperty.Setter = CustomPropertySetter;
            MyAPIGateway.TerminalControls.AddControl<Sandbox.ModAPI.Ingame.IMyMotorStator>(customProperty);
        }

        private static float CustomPropertyGetter(IMyTerminalBlock block)
        {
            return block.GetValue<float>("CustomAngle");
        }
        private static void CustomPropertySetter(IMyTerminalBlock block, float value)
        {
            block.GameLogic.GetAs<MotorStatorAngleProperty>().dls.Value = value;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder;
        }
    }


}
