using System;
using Vampiric.Stats;

namespace Vampiric.Stats
{
    [Serializable]
    public struct StatModifier
    {
        public StatId Id;
        public float Value;
        public StatOp Op;

        public StatModifier(StatId id, float value, StatOp op = StatOp.Add)
        {
            Id = id;
            Value = value;
            Op = op;
        }
    }

    public enum StatOp : byte
    {
        Add = 0,
        Multiply = 1
    }
}
