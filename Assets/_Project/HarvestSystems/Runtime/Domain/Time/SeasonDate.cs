using System;

namespace HarvestSystems.Domain.Time
{
    public readonly struct SeasonDate : IEquatable<SeasonDate>
    {
        public SeasonDate(int year, Season season, int dayOfSeason)
        {
            if (year < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(year));
            }

            if (!Enum.IsDefined(typeof(Season), season))
            {
                throw new ArgumentOutOfRangeException(nameof(season));
            }

            if (dayOfSeason < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dayOfSeason));
            }

            Year = year;
            Season = season;
            DayOfSeason = dayOfSeason;
        }

        public int Year { get; }
        public Season Season { get; }
        public int DayOfSeason { get; }

        public bool Equals(SeasonDate other) =>
            Year == other.Year && Season == other.Season && DayOfSeason == other.DayOfSeason;

        public override bool Equals(object obj) => obj is SeasonDate other && Equals(other);
        public override int GetHashCode() => ((Year * 397) ^ (int)Season) * 397 ^ DayOfSeason;
        public override string ToString() => $"Year {Year}, {Season} {DayOfSeason}";

        public static bool operator ==(SeasonDate left, SeasonDate right) => left.Equals(right);
        public static bool operator !=(SeasonDate left, SeasonDate right) => !left.Equals(right);

        internal static SeasonDate FromAbsoluteDay(int absoluteDay, int daysPerSeason)
        {
            int daysPerYear = checked(daysPerSeason * 4);
            int zeroBasedDay = absoluteDay - 1;
            int zeroBasedDayOfYear = zeroBasedDay % daysPerYear;
            int year = (zeroBasedDay / daysPerYear) + 1;
            Season season = (Season)(zeroBasedDayOfYear / daysPerSeason);
            int dayOfSeason = (zeroBasedDayOfYear % daysPerSeason) + 1;
            return new SeasonDate(year, season, dayOfSeason);
        }
    }
}
