using HarvestSystems.Domain.Common;
using UnityEngine;

namespace HarvestSystems.Unity.Data
{
    [CreateAssetMenu(menuName = "Harvest Systems/Item Definition", fileName = "ItemDefinition")]
    public sealed class ItemDefinitionSO : ScriptableObject
    {
        [SerializeField] private string stableId = "item.new_item";
        [SerializeField] private string displayName = "New Item";

        public StableId Id => new StableId(stableId);
        public string DisplayName => displayName;

        private void OnValidate()
        {
            stableId = stableId?.Trim();
            displayName = displayName?.Trim();
        }
    }
}
