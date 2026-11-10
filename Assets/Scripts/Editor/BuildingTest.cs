using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Stokehold.Tests
{
    // Запуск: меню Stokehold -> Run Building Tests. Результат в Console.
    // Сцена и ассеты не нужны: постройки создаются в памяти и удаляются после проверки.
    public static class BuildingTests
    {
        private static int passed;
        private static int failed;

        [MenuItem("Stokehold/Run Building Tests")]
        public static void RunAll()
        {
            passed = 0;
            failed = 0;
            var created = new List<Object>();
            try
            {
                DefaultsTests(created);
                RangeTests(created);
                CooldownTests(created);
                EffectTests(created);
            }
            finally
            {
                foreach (var o in created) Object.DestroyImmediate(o);
            }

            string msg = "Building tests: " + passed + " passed, " + failed + " failed";
            if (failed == 0) Debug.Log(msg); else Debug.LogError(msg);
        }

        // ---------- helpers ----------

        private static void Check(bool condition, string name)
        {
            if (condition) { passed++; return; }
            failed++;
            Debug.LogError("FAIL: " + name);
        }

        private static void Eq(int expected, int actual, string name)
        {
            Check(expected == actual, name + " (expected " + expected + ", got " + actual + ")");
        }

        private static void SetField(BuildingData b, string field, object value)
        {
            var f = typeof(BuildingData).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) throw new Exception("Field not found: " + field);
            f.SetValue(b, value);
        }

        private static BuildingData Make(List<Object> created, string name,
            BuildingType type = BuildingType.City, int radius = 1,
            RadiusShape shape = RadiusShape.Circle, int cooldown = 1,
            params EffectEntry[] effects)
        {
            var b = ScriptableObject.CreateInstance<BuildingData>();
            b.name = name;
            created.Add(b);
            SetField(b, "displayName", name);
            SetField(b, "type", type);
            SetField(b, "radius", radius);
            SetField(b, "radiusShape", shape);
            SetField(b, "cooldown", cooldown);
            SetField(b, "effects", new List<EffectEntry>(effects));
            return b;
        }

        private static EffectContext Ctx(BuildingData source, List<BuildingData> neighbors, List<BuildingData> all)
        {
            return new EffectContext(source, Vector2Int.zero, neighbors, all);
        }

        private static EffectEntry Flat(int amount)
        {
            return new EffectEntry { kind = EffectKind.FlatScore, amount = amount };
        }

        private static EffectEntry PerType(BuildingType t, int amount, EffectScope scope)
        {
            return new EffectEntry { kind = EffectKind.PerBuildingType, targetType = t, amount = amount, scope = scope };
        }

        private static EffectEntry PerBuilding(BuildingData target, int amount, EffectScope scope)
        {
            return new EffectEntry { kind = EffectKind.PerSpecificBuilding, targetBuilding = target, amount = amount, scope = scope };
        }

        // ---------- tests ----------

        private static void DefaultsTests(List<Object> created)
        {
            var fresh = ScriptableObject.CreateInstance<BuildingData>();
            created.Add(fresh);
            Eq(1, fresh.Radius, "default radius");
            Eq(1, fresh.Cooldown, "default cooldown");
            Check(fresh.Shape == RadiusShape.Circle, "default shape is Circle");
        }

        private static void RangeTests(List<Object> created)
        {
            var o = Vector2Int.zero;

            var c1 = Make(created, "circle1", radius: 1, shape: RadiusShape.Circle);
            Check(c1.IsInRange(o, new Vector2Int(1, 0)), "circle r1: side neighbor in range");
            Check(!c1.IsInRange(o, new Vector2Int(1, 1)), "circle r1: diagonal out of range");
            Check(!c1.IsInRange(o, new Vector2Int(0, 2)), "circle r1: two cells away out of range");
            Check(!c1.IsInRange(o, o), "circle r1: own cell is not in range");

            var c2 = Make(created, "circle2", radius: 2, shape: RadiusShape.Circle);
            Check(c2.IsInRange(o, new Vector2Int(1, 1)), "circle r2: diagonal in range");
            Check(c2.IsInRange(o, new Vector2Int(2, 0)), "circle r2: (2,0) in range");
            Check(!c2.IsInRange(o, new Vector2Int(2, 1)), "circle r2: (2,1) out of range");
            Check(!c2.IsInRange(o, new Vector2Int(2, 2)), "circle r2: (2,2) out of range");

            var s1 = Make(created, "square1", radius: 1, shape: RadiusShape.Square);
            Check(s1.IsInRange(o, new Vector2Int(1, 1)), "square r1: diagonal in range");
            Check(!s1.IsInRange(o, new Vector2Int(2, 0)), "square r1: (2,0) out of range");

            var d2 = Make(created, "diamond2", radius: 2, shape: RadiusShape.Diamond);
            Check(d2.IsInRange(o, new Vector2Int(1, 1)), "diamond r2: (1,1) in range");
            Check(d2.IsInRange(o, new Vector2Int(2, 0)), "diamond r2: (2,0) in range");
            Check(!d2.IsInRange(o, new Vector2Int(2, 1)), "diamond r2: (2,1) out of range");

            // Работает и из клетки не в нуле, и в обе стороны
            Check(c1.IsInRange(new Vector2Int(5, 5), new Vector2Int(4, 5)), "circle r1: works away from origin");
        }

        private static void CooldownTests(List<Object> created)
        {
            var every = Make(created, "cd1", cooldown: 1);
            for (int t = 0; t < 6; t++)
                Check(every.ShouldTrigger(t), "cooldown 1 triggers on turn " + t);

            var third = Make(created, "cd3", cooldown: 3);
            bool[] expected = { false, false, true, false, false, true };
            for (int t = 0; t < expected.Length; t++)
                Check(third.ShouldTrigger(t) == expected[t], "cooldown 3, completed turns " + t);

            Eq(3, third.TurnsUntilTrigger(0), "cooldown 3: until trigger after placement");
            Eq(2, third.TurnsUntilTrigger(1), "cooldown 3: until trigger, 1 turn passed");
            Eq(1, third.TurnsUntilTrigger(2), "cooldown 3: until trigger, 2 turns passed");
            Eq(3, third.TurnsUntilTrigger(3), "cooldown 3: counter resets");

            // ShouldTrigger совпадает с "остался 1 ход"
            for (int t = 0; t < 9; t++)
                Check(third.ShouldTrigger(t) == (third.TurnsUntilTrigger(t) == 1),
                    "ShouldTrigger agrees with TurnsUntilTrigger at " + t);
        }

        private static void EffectTests(List<Object> created)
        {
            var nature1 = Make(created, "nature1", BuildingType.Nature);
            var nature2 = Make(created, "nature2", BuildingType.Nature);
            var city = Make(created, "city", BuildingType.City);
            var mill = Make(created, "mill");
            var forge = Make(created, "forge");

            // FlatScore
            var flat = Make(created, "flat", effects: new[] { Flat(3) });
            var ctx = Ctx(flat, new List<BuildingData>(), new List<BuildingData>());
            flat.Trigger(ctx);
            Eq(3, ctx.TotalScore, "flat: total");
            Eq(1, ctx.Events.Count, "flat: one event");

            // PerType: все постройки и только соседи
            var all = new List<BuildingData> { nature1, nature2, city };
            var perTypeAll = Make(created, "perTypeAll", effects: new[] { PerType(BuildingType.Nature, 2, EffectScope.AllBuildings) });
            ctx = Ctx(perTypeAll, new List<BuildingData> { nature1 }, all);
            perTypeAll.Trigger(ctx);
            Eq(4, ctx.TotalScore, "per type, all buildings");

            var perTypeNb = Make(created, "perTypeNb", effects: new[] { PerType(BuildingType.Nature, 2, EffectScope.NeighborsOnly) });
            ctx = Ctx(perTypeNb, new List<BuildingData> { nature1 }, all);
            perTypeNb.Trigger(ctx);
            Eq(2, ctx.TotalScore, "per type, neighbors only");

            // PerType: нет совпадений -> 0 очков и нет событий
            var perMech = Make(created, "perMech", effects: new[] { PerType(BuildingType.Mechanism, 5, EffectScope.AllBuildings) });
            ctx = Ctx(perMech, new List<BuildingData>(), all);
            perMech.Trigger(ctx);
            Eq(0, ctx.TotalScore, "per type, no matches: total");
            Eq(0, ctx.Events.Count, "per type, no matches: no events");

            // PerSpecificBuilding
            var allMills = new List<BuildingData> { mill, mill, forge };
            var perMill = Make(created, "perMill", effects: new[] { PerBuilding(mill, 5, EffectScope.AllBuildings) });
            ctx = Ctx(perMill, new List<BuildingData>(), allMills);
            perMill.Trigger(ctx);
            Eq(10, ctx.TotalScore, "per specific building: two mills");

            var perMillNb = Make(created, "perMillNb", effects: new[] { PerBuilding(mill, 5, EffectScope.NeighborsOnly) });
            ctx = Ctx(perMillNb, new List<BuildingData> { forge }, allMills);
            perMillNb.Trigger(ctx);
            Eq(0, ctx.TotalScore, "per specific building: neighbors have no mill");

            var perNull = Make(created, "perNull", effects: new[] { PerBuilding(null, 5, EffectScope.AllBuildings) });
            ctx = Ctx(perNull, new List<BuildingData>(), allMills);
            perNull.Trigger(ctx);
            Eq(0, ctx.TotalScore, "per specific building: empty target gives 0");

            // Несколько эффектов у одной постройки складываются
            var multi = Make(created, "multi", effects: new[] { Flat(1), PerType(BuildingType.Nature, 1, EffectScope.AllBuildings) });
            ctx = Ctx(multi, new List<BuildingData>(), all);
            multi.Trigger(ctx);
            Eq(3, ctx.TotalScore, "multiple effects add up");
            Eq(2, ctx.Events.Count, "multiple effects: two events");
        }
    }
}