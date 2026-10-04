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
                () => new ItemDefinition(Carrot, "Carrot", -1, 0),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void ItemDefinition_RejectsNegativePurchasePrice()
        {
            Assert.That(
                () => new ItemDefinition(CarrotSeed, "Carrot Seed", 0, -1),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Buy_DebitsWalletAndAddsConfiguredItemQuantity()
        {
            var inventory = new Inventory();
            var wallet = new CurrencyWallet(40);
            var economy = CreateEconomy(inventory, wallet);
            PurchaseResult publishedResult = default;
            economy.PurchaseCompleted += result => publishedResult = result;

            PurchaseResult result = economy.Buy(CarrotSeed, 2);

            Assert.That(result.Status, Is.EqualTo(PurchaseStatus.Success));
            Assert.That(result.TotalCost, Is.EqualTo(30));
            Assert.That(publishedResult.TotalCost, Is.EqualTo(30));
            Assert.That(wallet.Balance, Is.EqualTo(10));
            Assert.That(inventory.GetQuantity(CarrotSeed), Is.EqualTo(2));
        }

        [Test]
        public void Buy_WithInsufficientFunds_DoesNotMutateState()
        {
            var inventory = new Inventory();
            var wallet = new CurrencyWallet(14);
            var economy = CreateEconomy(inventory, wallet);

            PurchaseResult result = economy.Buy(CarrotSeed, 1);

            Assert.That(result.Status, Is.EqualTo(PurchaseStatus.InsufficientFunds));
            Assert.That(result.TotalCost, Is.EqualTo(15));
            Assert.That(wallet.Balance, Is.EqualTo(14));
            Assert.That(inventory.GetQuantity(CarrotSeed), Is.Zero);
        }

        [Test]
        public void Buy_ItemWithoutPurchasePrice_IsRejectedWithoutMutation()
        {
            var inventory = new Inventory();
            var wallet = new CurrencyWallet(100);
            var economy = CreateEconomy(inventory, wallet);

            PurchaseResult result = economy.Buy(Carrot, 1);

            Assert.That(result.Status, Is.EqualTo(PurchaseStatus.NotPurchasable));
            Assert.That(wallet.Balance, Is.EqualTo(100));
            Assert.That(inventory.GetQuantity(Carrot), Is.Zero);
        }

        [Test]
        public void Buy_WhenInventoryWouldOverflow_DoesNotDebitWallet()
        {
            var inventory = new Inventory();
            inventory.Add(CarrotSeed, int.MaxValue);
            var wallet = new CurrencyWallet(15);
            var economy = CreateEconomy(inventory, wallet);

            Assert.That(() => economy.Buy(CarrotSeed, 1), Throws.TypeOf<System.OverflowException>());
            Assert.That(wallet.Balance, Is.EqualTo(15));
            Assert.That(inventory.GetQuantity(CarrotSeed), Is.EqualTo(int.MaxValue));
        }

        private static EconomyService CreateEconomy(Inventory inventory, CurrencyWallet wallet)
        {
            return new EconomyService(
                inventory,
                wallet,
                new[]
                {
                    new ItemDefinition(Carrot, "Carrot", 35, 0),
                    new ItemDefinition(Turnip, "Turnip", 20, 0),
                    new ItemDefinition(CarrotSeed, "Carrot Seed", 0, 15)
                });
        }
    }
}
