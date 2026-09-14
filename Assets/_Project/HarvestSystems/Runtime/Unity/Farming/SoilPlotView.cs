using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Farming;
using HarvestSystems.Unity.Composition;
using HarvestSystems.Unity.Interaction;
using UnityEngine;

namespace HarvestSystems.Unity.Farming
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public sealed class SoilPlotView : MonoBehaviour, IInteractable
    {
        [SerializeField] private string plotId = "plot.unconfigured";

        private static readonly Color UntilledColor = new Color(0.39f, 0.25f, 0.12f);
        private static readonly Color TilledColor = new Color(0.57f, 0.36f, 0.17f);
        private static readonly Color GrowingColor = new Color(0.26f, 0.58f, 0.24f);
        private static readonly Color MatureColor = new Color(0.91f, 0.55f, 0.18f);

        private SpriteRenderer spriteRenderer;
        private HarvestGameController controller;
        private SoilPlot plot;

        public StableId PlotId => new StableId(plotId);

        public void Configure(string id)
        {
            plotId = id;
        }

        public void Bind(HarvestGameController gameController, SoilPlot soilPlot)
        {
            Unbind();
            controller = gameController;
            plot = soilPlot;
            plot.Changed += OnPlotChanged;
            Refresh();
        }

        public void Interact()
        {
            controller?.InteractWithPlot(PlotId);
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void OnPlotChanged(SoilPlot _)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (plot == null || spriteRenderer == null)
            {
                return;
            }

            if (!plot.IsTilled)
            {
                spriteRenderer.color = UntilledColor;
            }
            else if (plot.Crop == null)
            {
                spriteRenderer.color = TilledColor;
            }
            else
            {
                CropDefinition definition = controller.Simulation.GetCrop(plot.Crop.CropId);
                spriteRenderer.color = plot.Crop.IsMature(definition) ? MatureColor : GrowingColor;
            }
        }

        private void Unbind()
        {
            if (plot != null)
            {
                plot.Changed -= OnPlotChanged;
            }

            plot = null;
        }
    }
}
