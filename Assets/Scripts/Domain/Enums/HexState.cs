namespace Domain.Enums
{
    public enum HexState
    {
        None,
        Walkable,
        Blocked, // Đổi Tower thành Blocked để tổng quát hơn
        Spawn,
        Target
    }
}