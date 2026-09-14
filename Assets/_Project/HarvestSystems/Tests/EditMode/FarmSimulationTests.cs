using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Farming;
using HarvestSystems.Domain.Inventory;
using HarvestSystems.Domain.Time;
using NUnit.Framework;

namespace HarvestSystems.Tests.EditMode
{
    public sealed class FarmSimulationTests
    {
        private static readonly StableId PlotId = new StableId("plot.test");
        private static readonly StableId CropId = new StableId("crop.carrot");
        private static readonly StableId SeedId = new StableId("item.carrot_seed");
        private static readonly StableId ProduceId = new StableId("item.carrot");

        [Test]
        public void VerticalSlice_TillPlantGrowHarvest_TransfersExpectedItems()
        {
            CropDefinition crop = CreateCrop(daysToMature: 2, harvestQuantity: 3);
            var inventory = new Inventory();
            inventory.Add(SeedId, 1);
            var plot = new SoilPlot(PlotId);

            using (var simulation = new FarmSimulation(
                       new GameClock(),
                       inventory,
                       new[] { plot },
                       new[] { crop }))
            {
                Assert.That(simulation.Till(PlotId), Is.True);
                Assert.That(simulation.Plant(PlotId, CropId), Is.True);
                Assert.That(inventory.GetQuantity(SeedId), Is.Zero);

                simulation.AdvanceDay();
                Assert.That(simulation.Harvest(PlotId), Is.False);

                simulation.AdvanceDay();
                Assert.That(simulation.Harvest(PlotId), Is.True);
                Assert.That(inventory.GetQuantity(ProduceId), Is.EqualTo(3));
                Assert.That(plot.Crop, Is.Null);
                Assert.That(plot.IsTilled, Is.True);
            }
        }

        [Test]
        public void Plant_WithoutSeed_DoesNotCreateCrop()
        {
            var plot = new SoilPlot(PlotId);
            plot.Till();

            using (var simulation = new FarmSimulation(
                       new GameClock(),
                       new Inventory(),
                       new[] { plot },
                       new[] { CreateCrop(2, 1) }))
            {
                Assert.That(simulation.Plant(PlotId, CropId), Is.False);
                Assert.That(plot.Crop, Is.Null);
            }
        }

        private static CropDefinition CreateCrop(int daysToMature, int harvestQuantity)
        {
            return new CropDefinition(CropId, "Carrot", SeedId, ProduceId, daysToMature, harvestQuantity);
        }
    }
}
