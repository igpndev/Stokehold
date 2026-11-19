using System.Collections.Generic;
using UnityEngine;

// Визуальная разметка сетки стола: тонкие линии-границы клеток на сукне.
// Создаётся спавнером, включается только на время перетаскивания карты.
public class BoardGridView : MonoBehaviour
{
    private const float LineWidth = 0.004f;
    private const float LineLift = 0.004f; // чуть выше сукна, чтобы не тонуло в нём

    private readonly List<LineRenderer> lines = new List<LineRenderer>();

    public void Build(BoardGrid grid)
    {
        float y = grid.Origin.y + LineLift;

        // Материал линий — ассет из Resources (Assets/Resources/Materials/GridLine.mat)
        Material lineMaterial = Resources.Load<Material>("Materials/GridLine");

        float gridWidth = grid.Columns * grid.CellSizeX;
        float gridDepth = grid.Rows * grid.CellSizeZ;

        // вертикальные линии (вдоль Z)
        for (int c = 0; c <= grid.Columns; c++)
        {
            float x = grid.Origin.x + c * grid.CellSizeX;
            AddLine(new Vector3(x, y, grid.Origin.z),
                    new Vector3(x, y, grid.Origin.z + gridDepth), lineMaterial);
        }

        // горизонтальные линии (вдоль X)
        for (int r = 0; r <= grid.Rows; r++)
        {
            float z = grid.Origin.z + r * grid.CellSizeZ;
            AddLine(new Vector3(grid.Origin.x, y, z),
                    new Vector3(grid.Origin.x + gridWidth, y, z), lineMaterial);
        }

        SetVisible(false); // по умолчанию разметка скрыта
    }

    private void AddLine(Vector3 from, Vector3 to, Material material)
    {
        GameObject lineGO = new GameObject("GridLine");
        lineGO.transform.SetParent(transform, false);

        LineRenderer line = lineGO.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        line.startWidth = LineWidth;
        line.endWidth = LineWidth;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        if (material != null) line.sharedMaterial = material;

        lines.Add(line);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
