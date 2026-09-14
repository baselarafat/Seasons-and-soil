using HarvestSystems.Domain.Common;

namespace HarvestSystems.Domain.Farming
{
    public sealed class CropState
    {
        public CropState(StableId cropId, int plantedOnDay)
        {
            CropId = cropId;
            PlantedOnDay = plantedOnDay;
        }

        public StableId CropId { get; }
        public int PlantedOnDay { get; }
        public int GrowthDays { get; private set; }

        public bool IsMature(CropDefinition definition)
        {
            return GrowthDays >= definition.DaysToMature;
        }

        internal void AdvanceOneDay()
        {
            GrowthDays++;
        }
    }
}
