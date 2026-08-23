using Unity.Mathematics;

namespace Vampiric.Simulation
{
    public static class SpatialHash
    {
        public const float DefaultCellSize = 3f;

        public static int CellKey(float2 position, float cellSize)
        {
            int x = (int)math.floor(position.x / cellSize);
            int y = (int)math.floor(position.y / cellSize);
            return (x * 73856093) ^ (y * 19349663);
        }

        public static int CellKey(int x, int y)
        {
            return (x * 73856093) ^ (y * 19349663);
        }

        public static int2 CellCoord(float2 position, float cellSize)
        {
            return new int2(
                (int)math.floor(position.x / cellSize),
                (int)math.floor(position.y / cellSize));
        }
    }
}
