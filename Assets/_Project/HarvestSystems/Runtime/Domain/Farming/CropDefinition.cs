using System;
using HarvestSystems.Domain.Common;

namespace HarvestSystems.Domain.Farming
{
    /// <summary>Immutable runtime crop rules, independent of their Unity authoring source.</summary>
    public sealed class CropDefinition
    {
        public CropDefinition(
            StableId id,
            string displayName,
            StableId seedItemId,
            StableId harvestedItemId,
            int daysToMature,
            int harvestQuantity)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
            }

            if (daysToMature < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(daysToMature));
            }

            if (harvestQuantity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(harvestQuantity));
            }

            Id = id;
            DisplayName = displayName;
            SeedItemId = seedItemId;
            HarvestedItemId = harvestedItemId;
            DaysToMature = daysToMature;
            HarvestQuantity = harvestQuantity;
        }

        public StableId Id { get; }
        public string DisplayName { get; }
        public StableId SeedItemId { get; }
        public StableId HarvestedItemId { get; }
        public int DaysToMature { get; }
        public int HarvestQuantity { get; }
    }
}
