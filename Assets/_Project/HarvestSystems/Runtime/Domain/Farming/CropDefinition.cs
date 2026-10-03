using System;
using System.Collections.Generic;
using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Time;

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
            int harvestQuantity,
            IEnumerable<Season> plantingSeasons = null)
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

            var uniqueSeasons = new HashSet<Season>();
            IEnumerable<Season> configuredSeasons = plantingSeasons ?? (Season[])Enum.GetValues(typeof(Season));
            foreach (Season season in configuredSeasons)
            {
                if (!Enum.IsDefined(typeof(Season), season))
                {
                    throw new ArgumentOutOfRangeException(nameof(plantingSeasons));
                }

                uniqueSeasons.Add(season);
            }

            if (uniqueSeasons.Count == 0)
            {
                throw new ArgumentException("At least one planting season is required.", nameof(plantingSeasons));
            }

            var orderedSeasons = new List<Season>(uniqueSeasons);
            orderedSeasons.Sort();
            PlantingSeasons = orderedSeasons.ToArray();
        }

        public StableId Id { get; }
        public string DisplayName { get; }
        public StableId SeedItemId { get; }
        public StableId HarvestedItemId { get; }
        public int DaysToMature { get; }
        public int HarvestQuantity { get; }
        public IReadOnlyList<Season> PlantingSeasons { get; }

        public bool CanPlantIn(Season season)
        {
            for (int i = 0; i < PlantingSeasons.Count; i++)
            {
                if (PlantingSeasons[i] == season)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
