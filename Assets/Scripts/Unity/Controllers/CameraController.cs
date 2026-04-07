using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public sealed class CameraController : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private float dragSpeed = 1f;
    

    private InputService inputService;

    private bool isDragging = false;
    public bool CanDragMap { get; private set; } = true;

    public void SetDragEnabled(bool enabled)
    {
        CanDragMap = enabled;
        if (!enabled) isDragging = false;
    }

    private void Awake()
    {
        cam = Camera.main;
    }

    public void Initialize(InputService inputService)
    {
        this.inputService = inputService;
        this.inputService.OnPressStarted += HandlerPressStarted;
        this.inputService.OnPressCanceled += HandlerPressCanceled;
    }

    private void HandlerPressStarted()
    {
        // Khi dùng Input System Callback, bắt buộc dùng Raycast thủ công
        // vì IsPointerOverGameObject() sẽ bị lỗi timing.
        if (!inputService.IsPointerOverUI())
        {
            isDragging = true;
        }
    }

    private void HandlerPressCanceled()
    {
        isDragging = false;
    }

    void LateUpdate()
    {
        if (!isDragging || !CanDragMap) return;

        Vector2 delta = inputService.GetPanDelta();
        if (delta == Vector2.zero) return;

        // Giả sử Setting Input đã Invert X,Y. Dùng dấu cộng (+)
        Vector3 move = new Vector3(delta.x, 0, delta.y) * dragSpeed;

        Vector3 targetPos = cam.transform.position + move;

        cam.transform.position = targetPos;
    }
}