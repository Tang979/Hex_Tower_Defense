using UnityEngine;
using Domain.Entities;

public class LayoutAdapter : MonoBehaviour
{
    [SerializeField] private float outerRadius = 1f; // Chỉnh trong Unity
    private float innerRadius;
    
    // Getter public để các class khác đọc nếu cần (readonly)
    public float OuterRadius => outerRadius;
    public float InnerRadius => innerRadius;

    public Vector3[] Corners { get; private set; }

    public void Initialize()
    {
        innerRadius = outerRadius * 0.866025404f;
        Corners = new Vector3[] {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(0f, 0f, outerRadius) // Loop về đầu để vẽ Line khép kín
        };
    }

    public Vector3 HexToWorld(HexTile hex)
    {
        if (hex == null) return Vector3.zero;
        // Logic của bạn đang dùng: Z = Q * 1.5R (Pointy Top chuẩn)
        // Lưu ý: Unity Z ngược chiều với một số tài liệu Hex, dấu -z là để khớp hướng camera
        float x = (hex.R + hex.Q * 0.5f - hex.Q / 2) * (innerRadius * 2f);
        float z = hex.Q * (outerRadius * 1.5f);
        return new Vector3(x, 0, -z); 
    }

    public (int q, int r) WorldToHexCoords(Vector3 pos)
    {
        int q = Mathf.RoundToInt(-pos.z / (outerRadius * 1.5f));
        int r = Mathf.RoundToInt(pos.x / (innerRadius * 2f) - (q * 0.5f) + (q / 2));
        return (q, r);
    }
}