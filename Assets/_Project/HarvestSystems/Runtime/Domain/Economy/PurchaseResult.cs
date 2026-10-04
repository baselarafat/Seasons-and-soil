using HarvestSystems.Domain.Common;

namespace HarvestSystems.Domain.Economy
{
    public enum PurchaseStatus
    {
        None,
        Success,
        NotPurchasable,
        InsufficientFunds
    }

    public readonly struct PurchaseResult
    {
        internal PurchaseResult(
            PurchaseStatus status,
            StableId itemId,
            int quantity,
            int totalCost)
        {
            Status = status;
            ItemId = itemId;
            Quantity = quantity;
            TotalCost = totalCost;
        }

        public PurchaseStatus Status { get; }
        public StableId ItemId { get; }
        public int Quantity { get; }
        public int TotalCost { get; }
        public bool WasSuccessful => Status == PurchaseStatus.Success;
    }
}
