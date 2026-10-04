using System;
using HarvestSystems.Domain.Common;

namespace HarvestSystems.Domain.Inventory
{
    /// <summary>Immutable item configuration used by inventory, economy, and future tooling.</summary>
    public sealed class ItemDefinition
    {
        public ItemDefinition(StableId id, string displayName, int sellPrice, int purchasePrice)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
            }

            if (sellPrice < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sellPrice));
            }

            if (purchasePrice < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(purchasePrice));
            }

            Id = id;
            DisplayName = displayName;
            SellPrice = sellPrice;
            PurchasePrice = purchasePrice;
        }

        public StableId Id { get; }
        public string DisplayName { get; }
        public int SellPrice { get; }
        public int PurchasePrice { get; }
        public bool IsSellable => SellPrice > 0;
        public bool IsPurchasable => PurchasePrice > 0;
    }
}
