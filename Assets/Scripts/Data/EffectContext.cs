using System.Collections.Generic;
using UnityEngine;

namespace Stokehold
{
    public struct ScoreEvent
    {
        public BuildingData Source;
        public int Amount;
        public string Reason;
    }

    // Всё, что нужно эффекту, чтобы посчитать результат. Поле (Board) эффект не знает
    // тот, кто запускает срабатывание, сам собирает соседей и список всех построек
    public class EffectContext
    {
        public BuildingData Source { get; }
        public Vector2Int Position { get; }
        public IReadOnlyList<BuildingData> Neighbors { get; }
        public IReadOnlyList<BuildingData> AllBuildings { get; }

        public List<ScoreEvent> Events { get; } = new List<ScoreEvent>();
        public int TotalScore { get; private set; }

        public EffectContext(BuildingData source, Vector2Int position,
            IReadOnlyList<BuildingData> neighbors, IReadOnlyList<BuildingData> allBuildings)
        {
            Source = source;
            Position = position;
            Neighbors = neighbors;
            AllBuildings = allBuildings;
        }

        public void AddScore(int amount, string reason)
        {
            TotalScore += amount;
            Events.Add(new ScoreEvent { Source = Source, Amount = amount, Reason = reason });
        }
    }
}
