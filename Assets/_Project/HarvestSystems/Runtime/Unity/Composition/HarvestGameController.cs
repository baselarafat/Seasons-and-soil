using System;
using System.Collections.Generic;
using System.Linq;
using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Economy;
using HarvestSystems.Domain.Farming;
using HarvestSystems.Domain.Inventory;
using HarvestSystems.Domain.Time;
using HarvestSystems.Unity.Data;
using HarvestSystems.Unity.Economy;
using HarvestSystems.Unity.Farming;
using HarvestSystems.Unity.Time;
using UnityEngine;

namespace HarvestSystems.Unity.Composition
{
    /// <summary>Scene composition boundary between Unity objects and the plain C# simulation.</summary>
    public sealed class HarvestGameController : MonoBehaviour
    {
        [SerializeField, Min(1)] private int initialSeedCount = 4;

        private CropDefinitionSO[] cropAssets = Array.Empty<CropDefinitionSO>();
        private CropDefinition[] cropDefinitions = Array.Empty<CropDefinition>();
        private readonly Dictionary<StableId, CropDefinitionSO> cropAssetsById = new Dictionary<StableId, CropDefinitionSO>();
        private int selectedCropIndex;

        public event Action StateChanged;

        public FarmSimulation Simulation { get; private set; }
        public EconomyService Economy { get; private set; }
        public IReadOnlyList<CropDefinition> AvailableCrops => cropDefinitions;
        public StableId SelectedCropId => SelectedCrop.Id;
        public CropDefinition SelectedCrop => cropDefinitions[selectedCropIndex];
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
            cropAssets = Resources.LoadAll<CropDefinitionSO>("Definitions/Crops")
                .OrderBy(asset => asset.Id.Value, StringComparer.Ordinal)
                .ToArray();
            if (cropAssets.Length == 0)
            {
                throw new InvalidOperationException("No CropDefinitionSO assets were found in Resources/Definitions/Crops.");
            }

            cropDefinitions = cropAssets.Select(asset => asset.ToDomain()).ToArray();
            ItemDefinition[] itemDefinitions = Resources.LoadAll<ItemDefinitionSO>("Definitions/Items")
                .OrderBy(asset => asset.Id.Value, StringComparer.Ordinal)
                .Select(asset => asset.ToDomain())
                .ToArray();
            if (itemDefinitions.Length == 0)
            {
                throw new InvalidOperationException("No ItemDefinitionSO assets were found in Resources/Definitions/Items.");
            }

            cropAssetsById.Clear();
            foreach (CropDefinitionSO cropAsset in cropAssets)
            {
                cropAssetsById.Add(cropAsset.Id, cropAsset);
            }

            selectedCropIndex = 0;

            SoilPlotView[] plotViews = FindObjectsByType<SoilPlotView>();
            var plots = new List<SoilPlot>(plotViews.Length);
            foreach (SoilPlotView view in plotViews)
            {
                plots.Add(new SoilPlot(view.PlotId));
            }

            var inventory = new HarvestSystems.Domain.Inventory.Inventory();
            foreach (CropDefinitionSO cropAsset in cropAssets)
            {
                inventory.Add(cropAsset.SeedItem.Id, initialSeedCount);
            }

            Simulation = new FarmSimulation(new GameClock(), inventory, plots, cropDefinitions);
            Economy = new EconomyService(inventory, new CurrencyWallet(), itemDefinitions);

            foreach (SoilPlotView view in plotViews)
            {
                view.Bind(this, Simulation.GetPlot(view.PlotId));
            }

            foreach (DayAdvanceInteractable dayAdvance in FindObjectsByType<DayAdvanceInteractable>())
            {
                dayAdvance.Bind(this);
            }

            foreach (TimeAdvanceInteractable timeAdvance in FindObjectsByType<TimeAdvanceInteractable>())
            {
                timeAdvance.Bind(this);
            }

            foreach (SellStationInteractable sellStation in FindObjectsByType<SellStationInteractable>())
            {
                sellStation.Bind(this);
            }

            foreach (SeedShopInteractable seedShop in FindObjectsByType<SeedShopInteractable>())
            {
                seedShop.Bind(this);
            }

            Simulation.Inventory.Changed += _ => StateChanged?.Invoke();
            Simulation.Clock.DayAdvanced += _ => StateChanged?.Invoke();
            Simulation.Clock.TimeAdvanced += _ => StateChanged?.Invoke();
            Economy.Wallet.BalanceChanged += _ => StateChanged?.Invoke();
            Simulation.CropHarvested += harvested =>
            {
                CropDefinitionSO harvestedCrop = cropAssetsById[harvested.CropId];
                StatusMessage = $"Harvested {harvested.Quantity} {harvestedCrop.HarvestedItem.DisplayName}.";
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
                PlantResult result = Simulation.Plant(plotId, SelectedCropId);
                switch (result)
                {
                    case PlantResult.Success:
                        StatusMessage = $"Planted {SelectedCrop.DisplayName}.";
                        break;
                    case PlantResult.OutOfSeason:
                        StatusMessage = $"{SelectedCrop.DisplayName} cannot be planted in {Simulation.Clock.CalendarDate.Season}.";
                        break;
                    case PlantResult.MissingSeed:
                        StatusMessage = $"No {cropAssets[selectedCropIndex].SeedItem.DisplayName} available.";
                        break;
                    case PlantResult.PlotNotTilled:
                        StatusMessage = "Till the soil before planting.";
                        break;
                    case PlantResult.PlotOccupied:
                        StatusMessage = "That plot already contains a crop.";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                CropDefinition definition = Simulation.GetCrop(plot.Crop.CropId);
                if (plot.Crop.IsMature(definition))
                {
                    Simulation.Harvest(plotId);
                }
                else if (!plot.IsWatered)
                {
                    StatusMessage = Simulation.Water(plotId)
                        ? $"Watered {definition.DisplayName}."
                        : "That soil cannot be watered.";
                }
                else
                {
                    StatusMessage = $"{definition.DisplayName} is watered: {plot.Crop.GrowthDays}/{definition.DaysToMature} growth days.";
                }
            }

            StateChanged?.Invoke();
        }

        public void AdvanceDay()
        {
            Simulation.AdvanceDay();
            StatusMessage = $"{Simulation.Clock.CalendarDate} began. Watered crops grew and the soil dried.";
            StateChanged?.Invoke();
        }

        public void AdvanceTime(int minutes)
        {
            Simulation.AdvanceMinutes(minutes);
            StatusMessage = $"Advanced time by {minutes} minutes. It is now {Simulation.Clock.Time}.";
            StateChanged?.Invoke();
        }

        public void SelectNextCrop()
        {
            if (cropDefinitions.Length == 0)
            {
                return;
            }

            selectedCropIndex = (selectedCropIndex + 1) % cropDefinitions.Length;
            StatusMessage = $"Selected {cropAssets[selectedCropIndex].SeedItem.DisplayName}.";
            StateChanged?.Invoke();
        }

        public SaleReceipt SellHarvestedProduce()
        {
            SaleReceipt receipt = Economy.SellAll(cropDefinitions.Select(crop => crop.HarvestedItemId));
            StatusMessage = receipt.WasSuccessful
                ? $"Sold {receipt.UnitsSold} harvested items for {receipt.Revenue}g."
                : "There is no harvested produce to sell.";
            StateChanged?.Invoke();
            return receipt;
        }

        public PurchaseResult BuySelectedSeed()
        {
            StableId seedItemId = SelectedCrop.SeedItemId;
            ItemDefinition seedDefinition = Economy.GetItemDefinition(seedItemId);
            PurchaseResult result = Economy.Buy(seedItemId, 1);
            switch (result.Status)
            {
                case PurchaseStatus.Success:
                    StatusMessage = $"Bought 1 {seedDefinition.DisplayName} for {result.TotalCost}g.";
                    break;
                case PurchaseStatus.InsufficientFunds:
                    StatusMessage = $"Need {result.TotalCost}g to buy {seedDefinition.DisplayName}.";
                    break;
                case PurchaseStatus.NotPurchasable:
                    StatusMessage = $"{seedDefinition.DisplayName} is not sold here.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            StateChanged?.Invoke();
            return result;
        }
    }
}
