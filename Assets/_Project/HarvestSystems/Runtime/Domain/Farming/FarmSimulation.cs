using System;
using System.Collections.Generic;
using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Inventory;
using HarvestSystems.Domain.Time;
using InventoryModel = HarvestSystems.Domain.Inventory.Inventory;

namespace HarvestSystems.Domain.Farming
{
    public readonly struct CropHarvested
    {
        public CropHarvested(StableId plotId, StableId cropId, StableId itemId, int quantity)
        {
            PlotId = plotId;
            CropId = cropId;
            ItemId = itemId;
            Quantity = quantity;
        }

        public StableId PlotId { get; }
        public StableId CropId { get; }
        public StableId ItemId { get; }
        public int Quantity { get; }
    }

    /// <summary>Coordinates the farming systems and provides explicit player commands.</summary>
    public sealed class FarmSimulation : IDisposable
    {
        private readonly Dictionary<StableId, SoilPlot> plots;
        private readonly Dictionary<StableId, CropDefinition> crops;

        public FarmSimulation(
            GameClock clock,
            InventoryModel inventory,
            IEnumerable<SoilPlot> plots,
            IEnumerable<CropDefinition> crops)
        {
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            this.plots = IndexUnique(plots, plot => plot.Id, "plot");
            this.crops = IndexUnique(crops, crop => crop.Id, "crop");
            Clock.DayAdvanced += OnDayAdvanced;
        }

        public event Action<CropHarvested> CropHarvested;

        public GameClock Clock { get; }
        public InventoryModel Inventory { get; }

        public SoilPlot GetPlot(StableId plotId)
        {
            if (!plots.TryGetValue(plotId, out SoilPlot plot))
            {
                throw new KeyNotFoundException($"Unknown soil plot '{plotId}'.");
            }

            return plot;
        }

        public CropDefinition GetCrop(StableId cropId)
        {
            if (!crops.TryGetValue(cropId, out CropDefinition crop))
            {
                throw new KeyNotFoundException($"Unknown crop '{cropId}'.");
            }

            return crop;
        }

        public bool Till(StableId plotId) => GetPlot(plotId).Till();

        public bool Water(StableId plotId) => GetPlot(plotId).Water();

        public bool Plant(StableId plotId, StableId cropId)
        {
            SoilPlot plot = GetPlot(plotId);
            CropDefinition definition = GetCrop(cropId);

            if (!plot.IsTilled || plot.Crop != null)
            {
                return false;
            }

            if (!Inventory.TryRemove(definition.SeedItemId, 1))
            {
                return false;
            }

            if (plot.Plant(definition, Clock.CurrentDay))
            {
                return true;
            }

            Inventory.Add(definition.SeedItemId, 1);
            return false;
        }

        public bool Harvest(StableId plotId)
        {
            SoilPlot plot = GetPlot(plotId);
            if (plot.Crop == null)
            {
                return false;
            }

            CropDefinition definition = GetCrop(plot.Crop.CropId);
            if (!plot.RemoveMatureCrop(definition))
            {
                return false;
            }

            Inventory.Add(definition.HarvestedItemId, definition.HarvestQuantity);
            CropHarvested?.Invoke(new CropHarvested(
                plot.Id,
                definition.Id,
                definition.HarvestedItemId,
                definition.HarvestQuantity));
            return true;
        }

        public void AdvanceDay() => Clock.AdvanceDay();

        public void AdvanceMinutes(int minutes) => Clock.AdvanceMinutes(minutes);

        public void Dispose()
        {
            Clock.DayAdvanced -= OnDayAdvanced;
        }

        private void OnDayAdvanced(GameDate _)
        {
            foreach (SoilPlot plot in plots.Values)
            {
                plot.ProcessDayTransition();
            }
        }

        private static Dictionary<StableId, T> IndexUnique<T>(
            IEnumerable<T> source,
            Func<T, StableId> getId,
            string label)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var result = new Dictionary<StableId, T>();
            foreach (T value in source)
            {
                StableId id = getId(value);
                if (result.ContainsKey(id))
                {
                    throw new ArgumentException($"Duplicate {label} ID '{id}'.", nameof(source));
                }

                result.Add(id, value);
            }

            return result;
        }
    }
}
