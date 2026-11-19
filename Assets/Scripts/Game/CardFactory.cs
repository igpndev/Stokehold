using UnityEngine;
using UnityEngine.EventSystems;

// Создание карт: инстанцирует префаб Card (Assets/Resources/Prefabs/Card.prefab)
// и настраивает размер под раскладку. Структура префаба:
//   корень (MeshCollider + CardHover + CardDrag) — стоит на базовой позиции и не двигается;
//   дочерний "Visual" (квад с материалом CardTemplate) — выдвигается при наведении.
// Разделение нужно, чтобы коллайдер не уезжал вместе с картой: иначе курсор
// у края карты "теряет" её при выдвижении и карта дёргается.
public static class CardFactory
{
    private const string CardPrefabPath = "Prefabs/Card";

    // position — рассчитанная точка на столе (центр карты, базовая позиция).
    public static GameObject CreateCard(Vector3 position, Vector2 size, Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>(CardPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"CardFactory: префаб \"{CardPrefabPath}\" не найден в Resources.");
            return null;
        }

        GameObject card = Instantiate(prefab, position, parent);
        card.transform.localScale = new Vector3(size.x, size.y, 1f);

        card.GetComponent<CardHover>().Init(position, card.transform.Find("Visual"));

        EnsurePointerRaycaster();
        return card;
    }

    // Поворот берём из префаба (квад лицом вверх, верх карты — вглубь стола)
    private static GameObject Instantiate(GameObject prefab, Vector3 position, Transform parent)
    {
        GameObject card = Object.Instantiate(prefab, position, prefab.transform.rotation, parent);
        card.name = "CardTemplate";
        return card;
    }

    // 3D-объекты ловят pointer-события только при PhysicsRaycaster на камере
    // (EventSystem в сцене уже есть).
    private static void EnsurePointerRaycaster()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.GetComponent<PhysicsRaycaster>() == null)
            cam.gameObject.AddComponent<PhysicsRaycaster>();
    }
}
