using System.Collections.Generic;
using UnityEngine;

// Визуал доски на столе: плашка-подложка видна всегда, линии-границы клеток
// показываются только на время перетаскивания карты. Создаётся спавнером.
public class BoardGridView : MonoBehaviour
{
    private const float PlateLift = 0.002f; // плашка чуть выше сукна
    private const float LineWidth = 0.004f;
    private const float LineLift = 0.004f; // чуть выше сукна, чтобы не тонуло в нём

    private readonly List<LineRenderer> lines = new List<LineRenderer>();
    private Transform linesRoot; // линии клеток: включаются только при перетаскивании

    public void Build(BoardGrid grid)
    {
        linesRoot = new GameObject("GridLines").transform;
        linesRoot.SetParent(transform, false);

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

        AddPlate(grid);
        SetVisible(false); // по умолчанию разметка скрыта
    }

    private void AddLine(Vector3 from, Vector3 to, Material material)
    {
        GameObject lineGO = new GameObject("GridLine");
        lineGO.transform.SetParent(linesRoot, false);

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

    // Плашка-подложка доски: плоский квадрат на сукне, без коллайдера —
    // не участвует в pointer-событиях. Материал — Assets/Resources/Materials/Board.mat
    private void AddPlate(BoardGrid grid)
    {
        GameObject plateGO = new GameObject("BoardPlate");
        plateGO.transform.SetParent(transform, false);
        plateGO.transform.position = grid.Center + Vector3.up * PlateLift;
        plateGO.transform.localScale = new Vector3(grid.Size.x, 1f, grid.Size.z);

        Mesh mesh = new Mesh { name = "BoardPlate" };
        mesh.SetVertices(new List<Vector3>
        {
            new Vector3(-0.5f, 0f, -0.5f), new Vector3(0.5f, 0f, -0.5f),
            new Vector3(-0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, 0.5f)
        });
        mesh.SetIndices(new[] { 0, 2, 1, 1, 2, 3 }, MeshTopology.Triangles, 0); // лицом вверх
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        plateGO.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer plate = plateGO.AddComponent<MeshRenderer>();
        plate.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        plate.receiveShadows = false;

        // Материал плашки — ассет из Resources
        Material plateMaterial = Resources.Load<Material>("Materials/Board");
        if (plateMaterial != null) plate.sharedMaterial = plateMaterial;
    }

    // Линии клеток показываются только на время перетаскивания (плашка не трогается)
    public void SetVisible(bool visible)
    {
        if (linesRoot != null) linesRoot.gameObject.SetActive(visible);
    }
}
