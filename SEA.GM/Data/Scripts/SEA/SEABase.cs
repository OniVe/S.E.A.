using Sandbox.ModAPI;
using SEA.Context;
using VRage.Game.Components;

namespace SEA
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
            if (MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer)
            {
                SEAUtilities.Logging.Static.WriteLine("Initialization error");
                return;
            }

            Context = new SEAContext(out allowUpdate);
            SEAUtilities.Logging.Static.WriteLine("Initialized");
        }
        public override void UpdateAfterSimulation()
        {
            if (!initialized && MyAPIGateway.Session != null)
                Initialize();

            if (allowUpdate)
                MyAPIGateway.Utilities.InvokeOnGameThread(Context.UpdateAfterSimulationCallback);

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
