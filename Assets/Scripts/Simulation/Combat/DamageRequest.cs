using Unity.Entities;

namespace Vampiric.Combat
{
    public readonly struct DamageRequest
    {
        public readonly Entity Source;
        public readonly float Amount;
        public readonly WeaponCategory Category;
        public readonly float DefenseShred;
        public readonly bool CanCrit;

        public DamageRequest(
            Entity source,
            float amount,
            WeaponCategory category = WeaponCategory.None,
            float defenseShred = 0f,
            bool canCrit = true)
        {
            Source = source;
            Amount = amount;
            Category = category;
            DefenseShred = defenseShred;
            CanCrit = canCrit;
        }

        public static Builder Create(float amount)
        {
            return new Builder(amount);
        }

        public readonly struct Builder
        {
            private readonly Entity _source;
            private readonly float _amount;
            private readonly WeaponCategory _category;
            private readonly float _defenseShred;
            private readonly bool _canCrit;

            public Builder(float amount)
            {
                _source = Entity.Null;
                _amount = amount;
                _category = WeaponCategory.None;
                _defenseShred = 0f;
                _canCrit = true;
            }

            private Builder(
                Entity source,
                float amount,
                WeaponCategory category,
                float defenseShred,
                bool canCrit)
            {
                _source = source;
                _amount = amount;
                _category = category;
                _defenseShred = defenseShred;
                _canCrit = canCrit;
            }

            public Builder WithSource(Entity source)
            {
                return new Builder(source, _amount, _category, _defenseShred, _canCrit);
            }

            public Builder WithCategory(WeaponCategory category)
            {
                return new Builder(_source, _amount, category, _defenseShred, _canCrit);
            }

            public Builder WithShred(float defenseShred)
            {
                return new Builder(_source, _amount, _category, defenseShred, _canCrit);
            }

            public Builder WithCrit(bool canCrit)
            {
                return new Builder(_source, _amount, _category, _defenseShred, canCrit);
            }

            public DamageRequest Build()
            {
                return new DamageRequest(_source, _amount, _category, _defenseShred, _canCrit);
            }
        }
    }
}
