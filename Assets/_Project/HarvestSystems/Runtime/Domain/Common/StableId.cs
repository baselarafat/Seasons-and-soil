using System;

namespace HarvestSystems.Domain.Common
{
    /// <summary>A stable, serialization-friendly identity for authored definitions and world entities.</summary>
    public readonly struct StableId : IEquatable<StableId>
    {
        public StableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A stable ID cannot be empty.", nameof(value));
            }

            Value = value.Trim();
        }

        public string Value { get; }

        public bool Equals(StableId other) => StringComparer.Ordinal.Equals(Value, other.Value);

        public override bool Equals(object obj) => obj is StableId other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);

        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(StableId left, StableId right) => left.Equals(right);

        public static bool operator !=(StableId left, StableId right) => !left.Equals(right);
    }
}
