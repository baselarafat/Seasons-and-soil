using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Economy;
using HarvestSystems.Domain.Inventory;
using NUnit.Framework;

namespace HarvestSystems.Tests.EditMode
{
    public sealed class EconomyServiceTests
    {
        private static readonly StableId Carrot = new StableId("item.carrot");
        private static readonly StableId Turnip = new StableId("item.turnip");
        private static readonly StableId CarrotSeed = new StableId("item.carrot_seed");

        [Test]
        public void SellAll_TransfersSellableInventoryToWalletAtConfiguredPrices()
        {
            var inventory = new Inventory();
            inventory.Add(Carrot, 2);
            inventory.Add(Turnip, 3);
            inventory.Add(CarrotSeed, 5);
            var wallet = new CurrencyWallet();
            var economy = CreateEconomy(inventory, wallet);
            SaleReceipt publishedReceipt = default;
            economy.SaleCompleted += receipt => publishedReceipt = receipt;

            SaleReceipt receipt = economy.SellAll(new[] { Carrot, Turnip, Carrot, CarrotSeed });

            Assert.That(receipt.WasSuccessful, Is.True);
            Assert.That(receipt.UnitsSold, Is.EqualTo(5));
            Assert.That(receipt.Revenue, Is.EqualTo(130));
            Assert.That(publishedReceipt.Revenue, Is.EqualTo(130));
            Assert.That(wallet.Balance, Is.EqualTo(130));
            Assert.That(inventory.GetQuantity(Carrot), Is.Zero);
            Assert.That(inventory.GetQuantity(Turnip), Is.Zero);
            Assert.That(inventory.GetQuantity(CarrotSeed), Is.EqualTo(5));
        }

        [Test]
        public void SellAll_WithNoSellableInventory_ReturnsEmptyReceipt()
        {
            var inventory = new Inventory();
            inventory.Add(CarrotSeed, 5);
            var wallet = new CurrencyWallet();
            var economy = CreateEconomy(inventory, wallet);
            int completedSales = 0;
            economy.SaleCompleted += _ => completedSales++;

            SaleReceipt receipt = economy.SellAll(new[] { Carrot, CarrotSeed });

            Assert.That(receipt.WasSuccessful, Is.False);
            Assert.That(wallet.Balance, Is.Zero);
            Assert.That(completedSales, Is.Zero);
            Assert.That(inventory.GetQuantity(CarrotSeed), Is.EqualTo(5));
        }

        [Test]
        public void SellAll_WhenWalletWouldOverflow_DoesNotRemoveInventory()
        {
            var inventory = new Inventory();
            inventory.Add(Carrot, 1);
            var wallet = new CurrencyWallet(int.MaxValue);
            var economy = CreateEconomy(inventory, wallet);

            Assert.That(() => economy.SellAll(new[] { Carrot }), Throws.TypeOf<System.OverflowException>());
            Assert.That(inventory.GetQuantity(Carrot), Is.EqualTo(1));
            Assert.That(wallet.Balance, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void ItemDefinition_RejectsNegativeSellPrice()
        {
            Assert.That(
                () => new ItemDefinition(Carrot, "Carrot", -1),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        private static EconomyService CreateEconomy(Inventory inventory, CurrencyWallet wallet)
        {
            return new EconomyService(
                inventory,
                wallet,
                new[]
                {
                    new ItemDefinition(Carrot, "Carrot", 35),
                    new ItemDefinition(Turnip, "Turnip", 20),
                    new ItemDefinition(CarrotSeed, "Carrot Seed", 0)
                });
        }
    }
}
