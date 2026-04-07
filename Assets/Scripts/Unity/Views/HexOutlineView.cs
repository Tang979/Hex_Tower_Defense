using System.Collections.Generic;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexOutlineView : MonoBehaviour
{
    [Header("Settings")]
    public Material outlineMaterial;
    public float lineWidth = 0.05f;

    // --- Dependencies ---
    private Mesh mesh;
    private LaneService _laneService;
    private LayoutAdapter _layout;
    private HexMap _map; // Cần map để duyệt qua toàn bộ các ô

    public void Initialize(HexMap map, LaneService laneService, LayoutAdapter layout)
    {
        _map = map;
        _laneService = laneService;
        _layout = layout;
    }

    public void BuildOutlines()
    {
        if (_map == null || _laneService == null) return;

        if (mesh == null)
        {
            mesh = new Mesh { name = "Hex Outlines" };
            GetComponent<MeshFilter>().mesh = mesh;
        }

        // 1. Lấy tất cả các ô thuộc đường đi (để highlight)
        var lanePaths = _laneService.GetAllPaths();
        HashSet<HexTile> pathTilesSet = new HashSet<HexTile>();
        foreach (var path in lanePaths.Values)
        {
            if (path != null)
            {
                foreach (var tile in path)
                {
                    pathTilesSet.Add(tile);
                }
            }
            else
            {
                Debug.LogWarning("Lane path is null!");
            }
        }

        // 2. Chuẩn bị List dữ liệu
        List<Vector3> verts = new List<Vector3>();
        List<int> outlineTris = new List<int>();

        Vector3[] corners = _layout.Corners;
        int vertIndex = 0;

        // 3. Duyệt qua toàn bộ Map
        for (int q = 0; q < _map.Grid.Count; q++)
        {
            var row = _map.Grid[q];
            for (int r = 0; r < row.Count; r++)
            {
                var tile = row[r];
                // Bỏ qua ô trống
                if (tile == null || tile.State == HexState.None) continue;

                Vector3 center = _layout.HexToWorld(tile);
                bool isPath = pathTilesSet.Contains(tile);


                // 4. Vẽ 6 cạnh lục giác (Logic cũ của bạn)
                for (int i = 0; i < 6; i++)
                {
                    if (isPath)
                    {
                        // Nếu là path chỉ vẻ ở những cạnh không có neighbor là path
                        var neighbor = _map.GetNeighbor(tile, i);
                        if (neighbor != null && pathTilesSet.Contains(neighbor))
                            continue; // Bỏ qua cạnh này
                    }

                    Vector3 a = center + corners[i];
                    Vector3 b = center + corners[i + 1]; // LayoutAdapter Corners đã loop về đầu nên i+1 an toàn

                    // Tính toán độ dày nét vẽ (Extrude ra 2 bên)
                    Vector3 dir = (b - a).normalized;
                    Vector3 normal = Vector3.Cross(dir, Vector3.up) * lineWidth;

                    // Tạo 4 đỉnh cho 1 đoạn thẳng dày (Quad)
                    verts.Add(a - normal); // 0
                    verts.Add(a + normal); // 1
                    verts.Add(b + normal); // 2
                    verts.Add(b - normal); // 3

                    // Nối 2 tam giác tạo thành hình chữ nhật (Quad)
                    outlineTris.Add(vertIndex + 0);
                    outlineTris.Add(vertIndex + 1);
                    outlineTris.Add(vertIndex + 2);

                    outlineTris.Add(vertIndex + 0);
                    outlineTris.Add(vertIndex + 2);
                    outlineTris.Add(vertIndex + 3);

                    vertIndex += 4;
                }
            }
        }

        // 5. Apply vào Mesh
        mesh.Clear();
        mesh.vertices = verts.ToArray();

        mesh.SetTriangles(outlineTris.ToArray(), 0); // Map vào Material 0

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}