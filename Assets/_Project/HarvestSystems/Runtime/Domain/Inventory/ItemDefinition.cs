using System;
using HarvestSystems.Domain.Common;

namespace HarvestSystems.Domain.Inventory
{
    /// <summary>Immutable item configuration used by inventory, economy, and future tooling.</summary>
    public sealed class ItemDefinition
    {
        public ItemDefinition(StableId id, string displayName, int sellPrice)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
            }

            if (sellPrice < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sellPrice));
            }

            Id = id;
            DisplayName = displayName;
            SellPrice = sellPrice;
        }

        public StableId Id { get; }
        public string DisplayName { get; }
        public int SellPrice { get; }
        public bool IsSellable => SellPrice > 0;
    }
}
