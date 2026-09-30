using System.Collections;
using HarvestSystems.Domain.Common;
using HarvestSystems.Unity.Composition;
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
            Assert.That(bootstrap.GameController.Simulation.Clock.CurrentDay, Is.EqualTo(1));
            Assert.That(bootstrap.GameController.Simulation.Clock.Date.ToString(), Is.EqualTo("Year 1, Day 1"));
            Assert.That(bootstrap.GameController.Simulation.Clock.Time.ToString(), Is.EqualTo("06:00"));
            Assert.That(bootstrap.GameController.AvailableCrops.Count, Is.EqualTo(2));
            Assert.That(bootstrap.GameController.SelectedCropId, Is.EqualTo(new StableId("crop.carrot")));
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
