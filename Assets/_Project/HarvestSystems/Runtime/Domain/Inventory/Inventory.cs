using System;
using System.Collections.Generic;
using HarvestSystems.Domain.Common;

namespace HarvestSystems.Domain.Inventory
{
    public readonly struct InventoryChange
    {
        public InventoryChange(StableId itemId, int previousQuantity, int newQuantity)
        {
            ItemId = itemId;
            PreviousQuantity = previousQuantity;
            NewQuantity = newQuantity;
        }

        public StableId ItemId { get; }
        public int PreviousQuantity { get; }
        public int NewQuantity { get; }
        public int Delta => NewQuantity - PreviousQuantity;
    }

    public sealed class Inventory
    {
        private readonly Dictionary<StableId, int> quantities = new Dictionary<StableId, int>();

        public event Action<InventoryChange> Changed;

        public int GetQuantity(StableId itemId)
        {
            return quantities.TryGetValue(itemId, out int quantity) ? quantity : 0;
        }

        public void Add(StableId itemId, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Added quantity must be positive.");
            }

            int previous = GetQuantity(itemId);
            int next = checked(previous + quantity);
            quantities[itemId] = next;
            Changed?.Invoke(new InventoryChange(itemId, previous, next));
        }

        public bool TryRemove(StableId itemId, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Removed quantity must be positive.");
            }

            int previous = GetQuantity(itemId);
            if (previous < quantity)
            {
                return false;
            }

            int next = previous - quantity;
            if (next == 0)
            {
                quantities.Remove(itemId);
            }
            else
            {
                quantities[itemId] = next;
            }

            Changed?.Invoke(new InventoryChange(itemId, previous, next));
            return true;
        }
    }
}
