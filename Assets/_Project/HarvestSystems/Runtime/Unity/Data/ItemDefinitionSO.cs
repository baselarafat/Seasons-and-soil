using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Inventory;
using UnityEngine;

namespace HarvestSystems.Unity.Data
{
    [CreateAssetMenu(menuName = "Harvest Systems/Item Definition", fileName = "ItemDefinition")]
    public sealed class ItemDefinitionSO : ScriptableObject
    {
        [SerializeField] private string stableId = "item.new_item";
        [SerializeField] private string displayName = "New Item";
        [SerializeField, Min(0)] private int sellPrice;
        [SerializeField, Min(0)] private int purchasePrice;

        public StableId Id => new StableId(stableId);
        public string DisplayName => displayName;

        public ItemDefinition ToDomain() => new ItemDefinition(Id, displayName, sellPrice, purchasePrice);

        private void OnValidate()
        {
            stableId = stableId?.Trim();
            displayName = displayName?.Trim();
            sellPrice = Mathf.Max(0, sellPrice);
            purchasePrice = Mathf.Max(0, purchasePrice);
        }
    }
}
