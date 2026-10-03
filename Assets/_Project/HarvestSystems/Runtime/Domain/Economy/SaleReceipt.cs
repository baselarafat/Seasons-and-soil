namespace HarvestSystems.Domain.Economy
{
    public readonly struct SaleReceipt
    {
        public SaleReceipt(int unitsSold, int revenue)
        {
            UnitsSold = unitsSold;
            Revenue = revenue;
        }

        public int UnitsSold { get; }
        public int Revenue { get; }
        public bool WasSuccessful => UnitsSold > 0;
    }
}
