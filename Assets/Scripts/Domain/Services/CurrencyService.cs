using System;

namespace Domain.Services
{
    public class CurrencyService
    {
        public int CurrentCurrency { get; private set; }
        public event Action<int> OnCurrencyChanged;

        public CurrencyService(int startCurrency)
        {
            CurrentCurrency = startCurrency;
        }

        public void AddCurrency(int amount)
        {
            CurrentCurrency += amount;
            OnCurrencyChanged?.Invoke(CurrentCurrency);
        }

        public bool SpendCurrency(int amount)
        {
            if (CanAfford(amount))
            {
                CurrentCurrency -= amount;
                OnCurrencyChanged?.Invoke(CurrentCurrency);
                return true;
            }
            return false;
        }

        public bool CanAfford(int amount)
        {
            return CurrentCurrency >= amount;
        }
    }
}