using System.Collections.Generic;
using UnityEngine;

// Разметка столешницы на клетки для зданий.
// Чистая геометрия и учёт занятости: сетка покрывает ВСЮ столешницу,
// клетки почти квадратные (число строк подбирается по ширине).
public class BoardGrid
{
    private readonly Vector3 origin; // ближний угол сетки = угол столешницы (высота = её поверхность)
    private readonly float cellSizeX;
    private readonly float cellSizeZ;
    private readonly int columns;
    private readonly int rows;
    private readonly HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    public int Columns { get { return columns; } }
    public int Rows { get { return rows; } }
    public float CellSizeX { get { return cellSizeX; } }
    public float CellSizeZ { get { return cellSizeZ; } }

    /// <summary>Ближний угол сетки; Y = поверхность стола.</summary>
    public Vector3 Origin { get { return origin; } }

    public BoardGrid(Bounds feltBounds, int columns)
    {
        this.columns = Mathf.Max(1, columns);
        float cellByWidth = feltBounds.size.x / this.columns;
        rows = Mathf.Max(1, Mathf.RoundToInt(feltBounds.size.z / cellByWidth)); // клетки ближе к квадрату

        cellSizeX = feltBounds.size.x / this.columns;
        cellSizeZ = feltBounds.size.z / rows;
        origin = new Vector3(feltBounds.min.x, feltBounds.max.y, feltBounds.min.z); // вся столешница
    }

    // Клетка под мировой точкой (по XZ). false — точка вне сетки.
    public bool TryGetCell(Vector3 worldPos, out Vector2Int index)
    {
        index = Vector2Int.zero;
        float localX = worldPos.x - origin.x;
        float localZ = worldPos.z - origin.z;

        int col = Mathf.FloorToInt(localX / cellSizeX);
        int row = Mathf.FloorToInt(localZ / cellSizeZ);
        if (col < 0 || col >= columns || row < 0 || row >= rows) return false;

        index = new Vector2Int(col, row);
        return true;
    }

    public Vector3 GetCellCenter(Vector2Int index)
    {
        return origin + new Vector3((index.x + 0.5f) * cellSizeX, 0f, (index.y + 0.5f) * cellSizeZ);
    }

    public bool IsOccupied(Vector2Int index) => occupied.Contains(index);

    public void Occupy(Vector2Int index) => occupied.Add(index);

    // Куб-заглушка здания: инстанс префаба Building (Assets/Resources/Prefabs),
    // вдвое меньше клетки, стоит НА поверхности стола.
    public GameObject SpawnBuilding(Vector2Int index, Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Building");
        if (prefab == null)
        {
            Debug.LogWarning("BoardGrid: префаб \"Prefabs/Building\" не найден в Resources.");
            return null;
        }

        GameObject cube = Object.Instantiate(prefab, GetCellCenter(index), prefab.transform.rotation, parent);
        cube.name = "Building_" + index.x + "_" + index.y;

        float size = Mathf.Min(cellSizeX, cellSizeZ) * 0.5f;
        cube.transform.localScale = new Vector3(size, size, size);
        cube.transform.position = GetCellCenter(index) + Vector3.up * (size * 0.5f + 0.002f);
        return cube;
    }
}
