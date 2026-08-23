namespace Vampiric.Weapons
{
    public enum FirePattern : byte
    {
        Cone = 0,
        CircleSpread = 1,
        HomingSpread = 2,
        Bounce = 3,
        ArmorBreak = 4,
        Minigun = 5,
        Grenade = 6
    }

    public enum AimMode : byte
    {
        Nearest = 0,
        Random = 1,
        Mouse = 2,
        Weakest = 3,
        Strongest = 4
    }
}
