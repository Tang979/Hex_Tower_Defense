using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrystalButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private string _towerId; // Đổi thành private, không cần nhập tay nữa
    public float threshold = 0.1f;

    [SerializeField] private CrystalPlacementController _placementController;
    [SerializeField] private Image _buttonImage; // Kéo component Image của nút vào đây

    // Hàm này sẽ được gọi bởi hệ thống UI/GameController lúc khởi tạo game
    public void SetupButton(TowerOS towerData, CrystalPlacementController controller)
    {
        _towerId = towerData.Id;                   // Tự động nhận ID
        if (towerData.towerIcon != null)
            _buttonImage.sprite = towerData.towerIcon; // Tự động đổi hình ảnh icon
        _placementController = controller;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(_towerId))
        {
            _placementController.StartDragging(_towerId);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _placementController.EndDragging();
    }

    void Start()
    {
        if (_buttonImage == null) _buttonImage = GetComponent<Image>();
        _buttonImage.alphaHitTestMinimumThreshold = threshold;
    }

    public void OnDrag(PointerEventData eventData) { }
}