using System;

namespace HarvestSystems.Domain.Time
{
    public readonly struct GameDate : IEquatable<GameDate>
    {
        public GameDate(int year, int dayOfYear)
        {
            if (year < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(year));
            }

            if (dayOfYear < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dayOfYear));
            }

            Year = year;
            DayOfYear = dayOfYear;
        }

        public int Year { get; }
        public int DayOfYear { get; }

        public bool Equals(GameDate other) => Year == other.Year && DayOfYear == other.DayOfYear;
        public override bool Equals(object obj) => obj is GameDate other && Equals(other);
        public override int GetHashCode() => (Year * 397) ^ DayOfYear;
        public override string ToString() => $"Year {Year}, Day {DayOfYear}";

        public static bool operator ==(GameDate left, GameDate right) => left.Equals(right);
        public static bool operator !=(GameDate left, GameDate right) => !left.Equals(right);

        internal static GameDate FromAbsoluteDay(int absoluteDay, int daysPerYear)
        {
            int zeroBasedDay = absoluteDay - 1;
            return new GameDate((zeroBasedDay / daysPerYear) + 1, (zeroBasedDay % daysPerYear) + 1);
        }
    }
}
