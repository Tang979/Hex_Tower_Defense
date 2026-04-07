using System;

namespace Domain.Services
{
    public class BaseHealthService
    {
        public int MaxHealth {get; private set;}
        public int CurrentHealth {get; private set;}
        public event Action<int> OnHealthChanged;
        public event Action OnBaseDestroyed;

        public BaseHealthService (int startHealth)
        {
            MaxHealth = startHealth;
            CurrentHealth = startHealth;
        }

        public void TakeDamage(int amount)
        {
            // 1. Chặn logic nếu căn cứ đã sập từ trước
            if (CurrentHealth <= 0) return;

            // 2. Trừ máu và giới hạn giá trị thấp nhất là 0
            CurrentHealth -= amount;
            if (CurrentHealth < 0) CurrentHealth = 0;

            // 3. Chỉ gọi Event cập nhật UI đúng 1 lần
            OnHealthChanged?.Invoke(CurrentHealth);

            // 4. Kiểm tra điều kiện thua cuộc
            if (CurrentHealth == 0)
            {
                OnBaseDestroyed?.Invoke();
            }
        }
    }
}