using System.Collections;
using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Time;
using HarvestSystems.Unity.Composition;
using HarvestSystems.Unity.Economy;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HarvestSystems.Tests.PlayMode
{
    public sealed class Phase1CompositionTests
    {
        [UnityTest]
        public IEnumerator Bootstrap_CreatesInitializedSimulationWithSixPlots()
        {
            var root = new GameObject("Test Bootstrap");
            Phase1DemoBootstrap bootstrap = root.AddComponent<Phase1DemoBootstrap>();

            yield return null;

            Assert.That(bootstrap.GameController, Is.Not.Null);
            Assert.That(bootstrap.GameController.Simulation, Is.Not.Null);
            Assert.That(bootstrap.GameController.Economy, Is.Not.Null);
            Assert.That(bootstrap.GameController.Economy.Wallet.Balance, Is.Zero);
            Assert.That(Object.FindFirstObjectByType<SellStationInteractable>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<SeedShopInteractable>(), Is.Not.Null);
            Assert.That(bootstrap.GameController.Simulation.Clock.CurrentDay, Is.EqualTo(1));
            Assert.That(bootstrap.GameController.Simulation.Clock.Date.ToString(), Is.EqualTo("Year 1, Day 1"));
            Assert.That(bootstrap.GameController.Simulation.Clock.CalendarDate.ToString(), Is.EqualTo("Year 1, Spring 1"));
            Assert.That(bootstrap.GameController.Simulation.Clock.Time.ToString(), Is.EqualTo("06:00"));
            Assert.That(bootstrap.GameController.AvailableCrops.Count, Is.EqualTo(2));
            Assert.That(bootstrap.GameController.SelectedCropId, Is.EqualTo(new StableId("crop.carrot")));
            Assert.That(bootstrap.GameController.AvailableCrops[0].CanPlantIn(Season.Spring), Is.True);
            Assert.That(bootstrap.GameController.AvailableCrops[0].CanPlantIn(Season.Autumn), Is.False);
            Assert.That(bootstrap.GameController.AvailableCrops[1].CanPlantIn(Season.Autumn), Is.True);
            Assert.That(
                bootstrap.GameController.Simulation.Inventory.GetQuantity(new StableId("item.carrot_seed")),
                Is.EqualTo(4));
            Assert.That(
                bootstrap.GameController.Simulation.Inventory.GetQuantity(new StableId("item.turnip_seed")),
                Is.EqualTo(4));

            bootstrap.GameController.SelectNextCrop();

            Assert.That(bootstrap.GameController.SelectedCropId, Is.EqualTo(new StableId("crop.turnip")));
            var firstPlotId = new StableId("plot.demo_1");
            bootstrap.GameController.InteractWithPlot(firstPlotId);
            bootstrap.GameController.InteractWithPlot(firstPlotId);
            Assert.That(
                bootstrap.GameController.Simulation.GetPlot(firstPlotId).Crop.CropId,
                Is.EqualTo(new StableId("crop.turnip")));
            Assert.That(
                bootstrap.GameController.Simulation.Inventory.GetQuantity(new StableId("item.turnip_seed")),
                Is.EqualTo(3));
            Assert.That(
                bootstrap.GameController.Simulation.Inventory.GetQuantity(new StableId("item.carrot_seed")),
                Is.EqualTo(4));

            bootstrap.GameController.Simulation.Inventory.Add(new StableId("item.carrot"), 2);
            var sale = bootstrap.GameController.SellHarvestedProduce();

            Assert.That(sale.UnitsSold, Is.EqualTo(2));
            Assert.That(sale.Revenue, Is.EqualTo(70));
            Assert.That(bootstrap.GameController.Economy.Wallet.Balance, Is.EqualTo(70));
            Assert.That(
                bootstrap.GameController.Simulation.Inventory.GetQuantity(new StableId("item.carrot")),
                Is.Zero);

            var purchase = bootstrap.GameController.BuySelectedSeed();

            Assert.That(purchase.Status, Is.EqualTo(HarvestSystems.Domain.Economy.PurchaseStatus.Success));
            Assert.That(purchase.TotalCost, Is.EqualTo(10));
            Assert.That(bootstrap.GameController.Economy.Wallet.Balance, Is.EqualTo(60));
            Assert.That(
                bootstrap.GameController.Simulation.Inventory.GetQuantity(new StableId("item.turnip_seed")),
                Is.EqualTo(4));

            for (int i = 1; i <= 6; i++)
            {
                Assert.That(
                    bootstrap.GameController.Simulation.GetPlot(new StableId($"plot.demo_{i}")),
                    Is.Not.Null);
            }

            Object.Destroy(root);
        }
    }
}
