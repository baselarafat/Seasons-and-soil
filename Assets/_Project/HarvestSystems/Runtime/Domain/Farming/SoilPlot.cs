using System;
using HarvestSystems.Domain.Common;

namespace HarvestSystems.Domain.Farming
{
    public sealed class SoilPlot
    {
        public SoilPlot(StableId id)
        {
            Id = id;
        }

        public event Action<SoilPlot> Changed;

        public StableId Id { get; }
        public bool IsTilled { get; private set; }
        public CropState Crop { get; private set; }

        public bool Till()
        {
            if (IsTilled)
            {
                return false;
            }

            IsTilled = true;
            Changed?.Invoke(this);
            return true;
        }

        public bool Plant(CropDefinition definition, int currentDay)
        {
            if (!IsTilled || Crop != null)
            {
                return false;
            }

            Crop = new CropState(definition.Id, currentDay);
            Changed?.Invoke(this);
            return true;
        }

        internal void AdvanceOneDay()
        {
            if (Crop == null)
            {
                return;
            }

            Crop.AdvanceOneDay();
            Changed?.Invoke(this);
        }

        internal bool RemoveMatureCrop(CropDefinition definition)
        {
            if (Crop == null || Crop.CropId != definition.Id || !Crop.IsMature(definition))
            {
                return false;
            }

            Crop = null;
            Changed?.Invoke(this);
            return true;
        }
    }
}
