using VRage.Game.ModAPI;

namespace SEA.GM.ModelViews
{
    public enum BlockSize : int
    {
        station = 1,
        large = 2,
        small = 3,
    }
    public enum EntityType : uint
    {
        block = 0,
        group = 1,
    }
    public struct BlockView<T>
    {
        public T type;
        public string id;
        public string name;
    }
    public struct GrigView
    {
        public BlockSize size;
        public string id;
        public string name;

        public GrigView( IMyCubeGrid cubeGrid )
        {
            this.id = cubeGrid.EntityId.ToString(SEAUtilities.CultureInfoUS);
            this.name = cubeGrid.DisplayName;
            this.size = cubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Small ?
                BlockSize.small : (cubeGrid.IsStatic ?
                BlockSize.station :
                BlockSize.large);
        }
    }
}
