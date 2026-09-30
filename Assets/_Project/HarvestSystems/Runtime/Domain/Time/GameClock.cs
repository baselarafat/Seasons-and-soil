using System;

namespace HarvestSystems.Domain.Time
{
    /// <summary>A deterministic, command-driven clock with no dependency on Unity frame time.</summary>
    public sealed class GameClock
    {
        public const int MinutesPerDay = 24 * 60;
        public const int DefaultDayStartMinute = 6 * 60;
        public const int DefaultDaysPerYear = 28;

        public GameClock(
            int startingDay = 1,
            int startingMinuteOfDay = DefaultDayStartMinute,
            int daysPerYear = DefaultDaysPerYear)
        {
            if (startingDay < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startingDay));
            }

            if (startingMinuteOfDay < 0 || startingMinuteOfDay >= MinutesPerDay)
            {
                throw new ArgumentOutOfRangeException(nameof(startingMinuteOfDay));
            }

            if (daysPerYear < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(daysPerYear));
            }

            CurrentDay = startingDay;
            CurrentMinuteOfDay = startingMinuteOfDay;
            DaysPerYear = daysPerYear;
        }

        public event Action<GameDate> DayAdvanced;
        public event Action<GameTime> TimeAdvanced;

        public int CurrentDay { get; private set; }
        public int CurrentMinuteOfDay { get; private set; }
        public int DaysPerYear { get; }
        public GameDate Date => GameDate.FromAbsoluteDay(CurrentDay, DaysPerYear);
        public GameTime Time => GameTime.FromMinuteOfDay(CurrentMinuteOfDay);

        public void AdvanceDay()
        {
            AdvanceMinutes(MinutesPerDay - CurrentMinuteOfDay + DefaultDayStartMinute);
        }

        public void AdvanceMinutes(int minutes)
        {
            if (minutes < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes));
            }

            long totalMinutes = (long)CurrentMinuteOfDay + minutes;
            int crossedDays = checked((int)(totalMinutes / MinutesPerDay));
            int finalMinuteOfDay = (int)(totalMinutes % MinutesPerDay);

            for (int i = 0; i < crossedDays; i++)
            {
                CurrentDay = checked(CurrentDay + 1);
                CurrentMinuteOfDay = 0;
                DayAdvanced?.Invoke(Date);
            }

            CurrentMinuteOfDay = finalMinuteOfDay;
            TimeAdvanced?.Invoke(Time);
        }
    }
}
