using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stokehold
{
    public enum EffectKind
    {
        FlatScore,            // +amount очков
        PerBuildingType,      // +amount за каждую постройку типа targetType
        PerSpecificBuilding   // +amount за каждую конкретную постройку targetBuilding
    }

    public enum EffectScope
    {
        AllBuildings,
        NeighborsOnly
    }

    [Serializable]
    public class EffectEntry
    {
        public EffectKind kind = EffectKind.FlatScore;
        public int amount = 1;

        [Tooltip("Для PerBuildingType и PerSpecificBuilding")]
        public EffectScope scope = EffectScope.AllBuildings;

        [Tooltip("Только для PerBuildingType")]
        public BuildingType targetType;

        [Tooltip("Только для PerSpecificBuilding")]
        public BuildingData targetBuilding;

        public void Apply(EffectContext context)
        {
            switch (kind)
            {
                case EffectKind.FlatScore:
                    context.AddScore(amount, "+" + amount);
                    break;

                case EffectKind.PerBuildingType:
                    {
                        int count = 0;
                        foreach (var b in Pool(context))
                            if (b != null && b.Type == targetType) count++;
                        if (count > 0)
                            context.AddScore(count * amount, count + " x " + targetType);
                        break;
                    }

                case EffectKind.PerSpecificBuilding:
                    {
                        if (targetBuilding == null) break;
                        int count = 0;
                        foreach (var b in Pool(context))
                            if (b == targetBuilding) count++;
                        if (count > 0)
                            context.AddScore(count * amount, count + " x " + targetBuilding.DisplayName);
                        break;
                    }
            }
        }

        private IReadOnlyList<BuildingData> Pool(EffectContext context)
        {
            return scope == EffectScope.NeighborsOnly ? context.Neighbors : context.AllBuildings;
        }
    }
}
