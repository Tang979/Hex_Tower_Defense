using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputService
{
    private InputSystem inputActions;

    public event Action OnPressStarted;
    public event Action OnPressCanceled;

    public InputService()
    {
        inputActions = new InputSystem();

        inputActions.GamePlay.Press.started += ctx => OnPressStarted?.Invoke();
        inputActions.GamePlay.Press.canceled += ctx => OnPressCanceled?.Invoke();

        EnableAllInputs();
    }

    public Vector2 GetPanDelta() => inputActions.GamePlay.Pan.ReadValue<Vector2>();

    public Vector2 GetPointerPosition()
    {
        Vector2 pos = Vector2.zero;

        // 1. Ưu tiên kiểm tra Touchscreen trước (cho Mobile/Simulator)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pos = Touchscreen.current.primaryTouch.position.ReadValue();
            return pos;
        }

        // 2. Sau đó mới kiểm tra Mouse (cho PC Editor)
        if (Mouse.current != null)
        {
            pos = Mouse.current.position.ReadValue();
            return pos;
        }

        return pos;
    }

    public bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);

        // Lấy vị trí và in ra ngay lập tức
        Vector2 currentPos = GetPointerPosition();
        eventData.position = currentPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    public void EnableAllInputs() => inputActions.Enable();
    public void DisableAllInputs() => inputActions.Disable();

    public void Dispose()
    {
        DisableAllInputs();
        inputActions.Dispose();
    }
}