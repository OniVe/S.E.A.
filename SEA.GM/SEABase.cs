﻿using Sandbox.ModAPI;
using SEA.GM.Context;
using VRage.Game.Components;

namespace SEA.GM
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 10000)]
    public class SEABase : MySessionComponentBase
    {
        private bool initialized = false;
        private bool allowUpdate = false;
        private SEAContext Context;
        private void Initialize()
        {
            initialized = true;
            if (MyAPIGateway.Session != null && MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer)
            {
                SEAUtilities.Logging.Static.WriteLine("Initialization error");
                return;
            }

            SEACustomProperties.Init();
            Context = new SEAContext(out allowUpdate);
            SEAUtilities.Logging.Static.WriteLine("Initialized");
        }
        public override void UpdateAfterSimulation()
        {
            if (!initialized && MyAPIGateway.Session != null)
                Initialize();

            //if (allowUpdate)
            //    MyAPIGateway.Utilities.InvokeOnGameThread(Context.UpdateAfterSimulationCallback);

            base.UpdateAfterSimulation();
        }
        protected override void UnloadData()
        {
            try
            {
                Context.Close();

                if (SEAUtilities.Logging.Static != null)
                {
                    SEAUtilities.Logging.Static.WriteLine("Unload");
                    SEAUtilities.Logging.Static.Close();
                }
            }
            finally
            {
                base.UnloadData();
            }
        }
    }
}
