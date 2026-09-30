using System;

namespace HarvestSystems.Domain.Time
{
    public readonly struct GameTime : IEquatable<GameTime>
    {
        public GameTime(int hour, int minute)
        {
            if (hour < 0 || hour > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hour));
            }

            if (minute < 0 || minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minute));
            }

            Hour = hour;
            Minute = minute;
        }

        public int Hour { get; }
        public int Minute { get; }
        public int MinuteOfDay => (Hour * 60) + Minute;

        public bool Equals(GameTime other) => Hour == other.Hour && Minute == other.Minute;
        public override bool Equals(object obj) => obj is GameTime other && Equals(other);
        public override int GetHashCode() => (Hour * 397) ^ Minute;
        public override string ToString() => $"{Hour:00}:{Minute:00}";

        public static bool operator ==(GameTime left, GameTime right) => left.Equals(right);
        public static bool operator !=(GameTime left, GameTime right) => !left.Equals(right);

        internal static GameTime FromMinuteOfDay(int minuteOfDay)
        {
            return new GameTime(minuteOfDay / 60, minuteOfDay % 60);
        }
    }
}
