using Domain.Enums;

namespace Domain.Entities
{
    public class HexTile
    {
        public int Q { get; }
        public int R { get; }
        public int S => -Q - R;
        public HexState State { get; private set; }
        public int EnemyCount = 0;

        // Pathfinding Meta-data (Internal use for A*)
        public int searchId = -1;
        public float G { get; set; }
        public float H { get; set; }
        public HexTile Parent { get; set; }

        public HexTile(int q, int r, HexState state)
        {
            Q = q;
            R = r;
            State = state;
        }

        public void SetState(HexState state)
        {
            State = state;
        }

        public bool IsWalkable()
        {
            return State == HexState.Walkable || State == HexState.Spawn || State == HexState.Target || State == HexState.Trap;
        }
    }
}