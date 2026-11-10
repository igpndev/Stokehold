using System.Collections.Generic;
using UnityEngine;

namespace Stokehold
{
    [CreateAssetMenu(fileName = "Building_", menuName = "Stokehold/Building")]
    public class BuildingData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private BuildingType type;
        [SerializeField, Min(0)] private int energyCost = 1;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;

        [Header("Neighbors")]
        [SerializeField, Min(1)] private int radius = 1;
        [SerializeField] private RadiusShape radiusShape = RadiusShape.Circle;

        [Header("Timing")]
        [SerializeField, Min(1)] private int cooldown = 1;

        [Header("Effects")]
        [SerializeField] private List<EffectEntry> effects = new List<EffectEntry>();

        public string DisplayName => displayName;
        public BuildingType Type => type;
        public int EnergyCost => energyCost;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;
        public int Radius => radius;
        public RadiusShape Shape => radiusShape;
        public int Cooldown => cooldown;

        // Попадает ли клетка "to" в радиус здания, стоящего в клетке "from".
        // Сама клетка "from" в радиус не входит.
        public bool IsInRange(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Abs(from.x - to.x);
            int dy = Mathf.Abs(from.y - to.y);
            if (dx == 0 && dy == 0) return false;
            switch (radiusShape)
            {
                case RadiusShape.Square: return Mathf.Max(dx, dy) <= radius;
                case RadiusShape.Diamond: return dx + dy <= radius;
                default: return dx * dx + dy * dy <= radius * radius; // Circle
            }
        }

        // Должно ли здание сработать в зависимости от кулдауна
        public bool ShouldTrigger(int completedTurns)
        {
            return (completedTurns + 1) % cooldown == 0;
        }

        // Сколько концов хода осталось до срабатывания (для счётчика на здании)
        public int TurnsUntilTrigger(int completedTurns)
        {
            return cooldown - (completedTurns % cooldown);
        }

        // Эффекты
        public void Trigger(EffectContext context)
        {
            foreach (var effect in effects)
                if (effect != null) effect.Apply(context);
        }
    }
}