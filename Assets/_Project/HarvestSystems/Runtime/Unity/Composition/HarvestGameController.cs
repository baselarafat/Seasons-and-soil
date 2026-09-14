using System;
using System.Collections.Generic;
using System.Linq;
using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Farming;
using HarvestSystems.Domain.Time;
using HarvestSystems.Unity.Data;
using HarvestSystems.Unity.Farming;
using HarvestSystems.Unity.Time;
using UnityEngine;

namespace HarvestSystems.Unity.Composition
{
    /// <summary>Scene composition boundary between Unity objects and the plain C# simulation.</summary>
    public sealed class HarvestGameController : MonoBehaviour
    {
        [SerializeField, Min(1)] private int initialSeedCount = 4;

        private CropDefinitionSO selectedCropAsset;
        private CropDefinition selectedCropDefinition;

        public event Action StateChanged;

        public FarmSimulation Simulation { get; private set; }
        public StableId SelectedCropId => selectedCropAsset.Id;
        public CropDefinition SelectedCrop => selectedCropDefinition;
        public string StatusMessage { get; private set; } = "Till a brown plot with E.";

        private void Start()
        {
            if (Simulation == null)
            {
                InitializeFromScene();
            }
        }

        private void OnDestroy()
        {
            Simulation?.Dispose();
        }

        public void InitializeFromScene()
        {
            CropDefinitionSO[] cropAssets = Resources.LoadAll<CropDefinitionSO>("Definitions/Crops");
            if (cropAssets.Length == 0)
            {
                throw new InvalidOperationException("No CropDefinitionSO assets were found in Resources/Definitions/Crops.");
            }

            selectedCropAsset = cropAssets.OrderBy(asset => asset.Id.Value, StringComparer.Ordinal).First();
            CropDefinition[] cropDefinitions = cropAssets.Select(asset => asset.ToDomain()).ToArray();
            selectedCropDefinition = cropDefinitions.First(definition => definition.Id == selectedCropAsset.Id);

            SoilPlotView[] plotViews = FindObjectsByType<SoilPlotView>();
            var plots = new List<SoilPlot>(plotViews.Length);
            foreach (SoilPlotView view in plotViews)
            {
                plots.Add(new SoilPlot(view.PlotId));
            }

            var inventory = new HarvestSystems.Domain.Inventory.Inventory();
            inventory.Add(selectedCropAsset.SeedItem.Id, initialSeedCount);
            Simulation = new FarmSimulation(new GameClock(), inventory, plots, cropDefinitions);

            foreach (SoilPlotView view in plotViews)
            {
                view.Bind(this, Simulation.GetPlot(view.PlotId));
            }

            foreach (DayAdvanceInteractable dayAdvance in FindObjectsByType<DayAdvanceInteractable>())
            {
                dayAdvance.Bind(this);
            }

            Simulation.Inventory.Changed += _ => StateChanged?.Invoke();
            Simulation.Clock.DayAdvanced += _ => StateChanged?.Invoke();
            Simulation.CropHarvested += harvested =>
            {
                StatusMessage = $"Harvested {harvested.Quantity} {selectedCropAsset.HarvestedItem.DisplayName}.";
                StateChanged?.Invoke();
            };

            StateChanged?.Invoke();
        }

        public void InteractWithPlot(StableId plotId)
        {
            SoilPlot plot = Simulation.GetPlot(plotId);
            if (!plot.IsTilled)
            {
                StatusMessage = Simulation.Till(plotId) ? "Soil tilled. Interact again to plant." : "That soil is already tilled.";
            }
            else if (plot.Crop == null)
            {
                StatusMessage = Simulation.Plant(plotId, SelectedCropId)
                    ? $"Planted {selectedCropDefinition.DisplayName}."
                    : $"No {selectedCropAsset.SeedItem.DisplayName} available.";
            }
            else
            {
                CropDefinition definition = Simulation.GetCrop(plot.Crop.CropId);
                if (plot.Crop.IsMature(definition))
                {
                    Simulation.Harvest(plotId);
                }
                else
                {
                    StatusMessage = $"{definition.DisplayName}: {plot.Crop.GrowthDays}/{definition.DaysToMature} growth days.";
                }
            }

            StateChanged?.Invoke();
        }

        public void AdvanceDay()
        {
            Simulation.AdvanceDay();
            StatusMessage = $"Day {Simulation.Clock.CurrentDay} began. Crops advanced one growth day.";
            StateChanged?.Invoke();
        }
    }
}
