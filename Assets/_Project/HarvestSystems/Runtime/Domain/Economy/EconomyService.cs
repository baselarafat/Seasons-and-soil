using System;
using System.Collections.Generic;
using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Inventory;
using InventoryModel = HarvestSystems.Domain.Inventory.Inventory;

namespace HarvestSystems.Domain.Economy
{
    /// <summary>Executes inventory-to-currency transactions without Unity dependencies.</summary>
    public sealed class EconomyService
    {
        private readonly InventoryModel inventory;
        private readonly Dictionary<StableId, ItemDefinition> items;

        public EconomyService(
            InventoryModel inventory,
            CurrencyWallet wallet,
            IEnumerable<ItemDefinition> itemDefinitions)
        {
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            if (itemDefinitions == null)
            {
                throw new ArgumentNullException(nameof(itemDefinitions));
            }

            items = new Dictionary<StableId, ItemDefinition>();
            foreach (ItemDefinition definition in itemDefinitions)
            {
                if (definition == null)
                {
                    throw new ArgumentException("Item definitions cannot contain null.", nameof(itemDefinitions));
                }

                if (items.ContainsKey(definition.Id))
                {
                    throw new ArgumentException($"Duplicate item ID '{definition.Id}'.", nameof(itemDefinitions));
                }

                items.Add(definition.Id, definition);
            }
        }

        public event Action<SaleReceipt> SaleCompleted;

        public CurrencyWallet Wallet { get; }

        public SaleReceipt SellAll(IEnumerable<StableId> itemIds)
        {
            if (itemIds == null)
            {
                throw new ArgumentNullException(nameof(itemIds));
            }

            var uniqueIds = new HashSet<StableId>();
            var saleLines = new List<SaleLine>();
            int unitsSold = 0;
            int revenue = 0;

            foreach (StableId itemId in itemIds)
            {
                if (!uniqueIds.Add(itemId))
                {
                    continue;
                }

                if (!items.TryGetValue(itemId, out ItemDefinition definition))
                {
                    throw new KeyNotFoundException($"Unknown item '{itemId}'.");
                }

                int quantity = inventory.GetQuantity(itemId);
                if (!definition.IsSellable || quantity == 0)
                {
                    continue;
                }

                unitsSold = checked(unitsSold + quantity);
                revenue = checked(revenue + checked(quantity * definition.SellPrice));
                saleLines.Add(new SaleLine(itemId, quantity));
            }

            if (unitsSold == 0)
            {
                return default;
            }

            checked
            {
                _ = Wallet.Balance + revenue;
            }

            foreach (SaleLine line in saleLines)
            {
                if (!inventory.TryRemove(line.ItemId, line.Quantity))
                {
                    throw new InvalidOperationException("Inventory changed while a sale was being committed.");
                }
            }

            Wallet.Credit(revenue);
            var receipt = new SaleReceipt(unitsSold, revenue);
            SaleCompleted?.Invoke(receipt);
            return receipt;
        }

        private readonly struct SaleLine
        {
            public SaleLine(StableId itemId, int quantity)
            {
                ItemId = itemId;
                Quantity = quantity;
            }

            public StableId ItemId { get; }
            public int Quantity { get; }
        }
    }
}
