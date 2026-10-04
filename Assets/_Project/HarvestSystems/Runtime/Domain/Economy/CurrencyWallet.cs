using System;

namespace HarvestSystems.Domain.Economy
{
    public sealed class CurrencyWallet
    {
        public CurrencyWallet(int startingBalance = 0)
        {
            if (startingBalance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingBalance));
            }

            Balance = startingBalance;
        }

        public event Action<int> BalanceChanged;

        public int Balance { get; private set; }

        public void Credit(int amount)
        {
            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Balance = checked(Balance + amount);
            BalanceChanged?.Invoke(Balance);
        }

        public bool TryDebit(int amount)
        {
            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (Balance < amount)
            {
                return false;
            }

            Balance -= amount;
            BalanceChanged?.Invoke(Balance);
            return true;
        }
    }
}
