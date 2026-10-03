using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Farming;
using HarvestSystems.Domain.Time;
using UnityEngine;

namespace HarvestSystems.Unity.Data
{
    [CreateAssetMenu(menuName = "Harvest Systems/Crop Definition", fileName = "CropDefinition")]
    public sealed class CropDefinitionSO : ScriptableObject
    {
        [SerializeField] private string stableId = "crop.new_crop";
        [SerializeField] private string displayName = "New Crop";
        [SerializeField] private ItemDefinitionSO seedItem;
        [SerializeField] private ItemDefinitionSO harvestedItem;
        [SerializeField, Min(1)] private int daysToMature = 3;
        [SerializeField, Min(1)] private int harvestQuantity = 1;
        [SerializeField] private Season[] plantingSeasons = { Season.Spring };

        public StableId Id => new StableId(stableId);
        public string DisplayName => displayName;
        public ItemDefinitionSO SeedItem => seedItem;
        public ItemDefinitionSO HarvestedItem => harvestedItem;

        public CropDefinition ToDomain()
        {
            if (seedItem == null || harvestedItem == null)
            {
                throw new System.InvalidOperationException($"Crop '{name}' has missing item references.");
            }

            return new CropDefinition(
                Id,
                displayName,
                seedItem.Id,
                harvestedItem.Id,
                daysToMature,
                harvestQuantity,
                plantingSeasons);
        }

        private void OnValidate()
        {
            stableId = stableId?.Trim();
            displayName = displayName?.Trim();
            daysToMature = Mathf.Max(1, daysToMature);
            harvestQuantity = Mathf.Max(1, harvestQuantity);
        }
    }
}
