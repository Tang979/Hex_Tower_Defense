using Domain.Entities;
using Domain.Enums;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexMeshView : MonoBehaviour
{
    private Mesh mesh;
    private List<Vector3> vertices = new();
    private List<int> triangles = new();
    private List<Vector2> uvs = new();

    // Dùng Dictionary để lưu vết: Key là Tọa độ, Value là Index trong list vertices
    private Dictionary<Vector3, int> vertexCache = new(); 

    public void RenderMap(HexMap map, LayoutAdapter layoutAdapter)
    {
        if (mesh == null)
        {
            mesh = new Mesh { name = "HexMap" };
            GetComponent<MeshFilter>().mesh = mesh;
        }

        // Clear toàn bộ dữ liệu cũ
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        vertexCache.Clear(); // Quan trọng: Xóa cache cũ

        for (int q = 0; q < map.Rows; q++)
        {
            for (int r = 0; r < map.Cols; r++)
            {
                var tile = map.GetTile(q, r);
                if (tile == null || tile.State == HexState.None) continue;

                AddHexagon(layoutAdapter.HexToWorld(tile), layoutAdapter);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals(); // Lưu ý: Lúc này ánh sáng sẽ mượt (Smooth) tại các cạnh chung
        mesh.RecalculateBounds();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void AddHexagon(Vector3 center, LayoutAdapter layout)
    {
        Vector3[] corners = layout.Corners;

        // 1. Tìm hoặc thêm đỉnh TÂM
        int centerIndex = FindOrAddVertex(center);

        // 2. Tìm hoặc thêm 6 đỉnh VIỀN
        int[] cornerIndices = new int[6];
        for (int i = 0; i < 6; i++)
        {
            Vector3 pos = center + corners[i];
            cornerIndices[i] = FindOrAddVertex(pos);
        }

        // 3. Tạo tam giác từ các Index đã tìm được
        for (int i = 0; i < 6; i++)
        {
            // Nối Tâm -> Đỉnh hiện tại -> Đỉnh tiếp theo
            triangles.Add(centerIndex);
            triangles.Add(cornerIndices[i]);
            triangles.Add(cornerIndices[(i + 1) % 6]);
        }
    }

    // Hàm kiểm tra xem đỉnh đã tồn tại chưa
    private int FindOrAddVertex(Vector3 position)
    {
        
        if (vertexCache.TryGetValue(position, out int existingIndex))
        {
            return existingIndex; // Đã có: Trả về index cũ
        }

        // Chưa có: Tạo mới
        int newIndex = vertices.Count;
        vertices.Add(position);
        uvs.Add(GetPlanarUV(position)); // Tính UV ngay khi tạo vertex mới
        
        vertexCache.Add(position, newIndex); // Lưu vào bộ nhớ đệm

        return newIndex;
    }

    private Vector2 GetPlanarUV(Vector3 position)
    {
        float tiling = 0.2f;
        return new Vector2(position.x * tiling, position.z * tiling);
    }
}