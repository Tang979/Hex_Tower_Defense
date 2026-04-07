using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;
    [SerializeField] private GameObject barContainer; // GameObject chứa ảnh nền và healthFill

    private float _hideTimer = 0f;
    private const float HIDE_DELAY = 5f;
    private float _targetFillAmount = 1f;
    private float _lerpSpeed = 10f;

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        barContainer.SetActive(true);
        _hideTimer = HIDE_DELAY;
        _targetFillAmount = currentHealth / maxHealth;
    }

    private void Update()
    {
        if (!barContainer.activeSelf) return;

        // Xử lý tự ẩn
        if (_hideTimer > 0)
        {
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0) barContainer.SetActive(false);
        }

        // Xử lý tụt máu mượt
        healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, _targetFillAmount, Time.deltaTime * _lerpSpeed);
    }

    private void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
        transform.Rotate(0, 180f, 0); // Đảo ngược để mặt trước hướng về camera
    }
}