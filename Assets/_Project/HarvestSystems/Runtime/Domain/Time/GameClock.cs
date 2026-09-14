using System;

namespace HarvestSystems.Domain.Time
{
    /// <summary>Phase 1 day clock. Sub-day time and calendar rules are intentionally deferred.</summary>
    public sealed class GameClock
    {
        public GameClock(int startingDay = 1)
        {
            if (startingDay < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startingDay));
            }

            CurrentDay = startingDay;
        }

        public event Action<int> DayAdvanced;

        public int CurrentDay { get; private set; }

        public void AdvanceDay()
        {
            CurrentDay++;
            DayAdvanced?.Invoke(CurrentDay);
        }
    }
}
