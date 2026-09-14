using HarvestSystems.Unity.Composition;
using UnityEngine;
using UnityEngine.UIElements;

namespace HarvestSystems.Unity.Presentation
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class Phase1Hud : MonoBehaviour
    {
        private HarvestGameController controller;
        private Label statusLabel;
        private PanelSettings runtimePanelSettings;

        private void Awake()
        {
            UIDocument document = GetComponent<UIDocument>();
            if (document.panelSettings == null)
            {
                runtimePanelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                runtimePanelSettings.name = "Phase 1 HUD Panel Settings";
                runtimePanelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                runtimePanelSettings.referenceResolution = new Vector2Int(1920, 1080);
                runtimePanelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                runtimePanelSettings.match = 0.5f;
                document.panelSettings = runtimePanelSettings;
            }

            BuildVisualTree(document.rootVisualElement);
        }

        public void Bind(HarvestGameController gameController)
        {
            if (controller != null)
            {
                controller.StateChanged -= Refresh;
            }

            controller = gameController;
            if (controller != null)
            {
                controller.StateChanged += Refresh;
            }

            Refresh();
        }

        private void Refresh()
        {
            if (statusLabel == null || controller == null || controller.Simulation == null)
            {
                return;
            }

            statusLabel.text = BuildStatus();
        }

        private string BuildStatus()
        {
            int seeds = controller.Simulation.Inventory.GetQuantity(controller.SelectedCrop.SeedItemId);
            int produce = controller.Simulation.Inventory.GetQuantity(controller.SelectedCrop.HarvestedItemId);
            return $"Day {controller.Simulation.Clock.CurrentDay}   Seeds: {seeds}   Harvested: {produce}\n" +
                   "Move: WASD / Arrows    Interact: E    Blue tile: next day\n" +
                   controller.StatusMessage;
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.StateChanged -= Refresh;
            }

            if (runtimePanelSettings != null)
            {
                Destroy(runtimePanelSettings);
            }
        }

        private void BuildVisualTree(VisualElement root)
        {
            VisualElement card = new VisualElement();
            card.name = "phase-one-hud";
            card.style.position = Position.Absolute;
            card.style.left = 16;
            card.style.top = 16;
            card.style.width = 480;
            card.style.paddingLeft = 16;
            card.style.paddingRight = 16;
            card.style.paddingTop = 12;
            card.style.paddingBottom = 12;
            card.style.backgroundColor = new Color(0.07f, 0.10f, 0.08f, 0.92f);
            card.style.borderTopLeftRadius = 8;
            card.style.borderTopRightRadius = 8;
            card.style.borderBottomLeftRadius = 8;
            card.style.borderBottomRightRadius = 8;

            Label title = new Label("HARVEST SYSTEMS — PHASE 1");
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = Color.white;
            title.style.marginBottom = 8;
            card.Add(title);

            statusLabel = new Label();
            statusLabel.style.fontSize = 14;
            statusLabel.style.color = Color.white;
            statusLabel.style.whiteSpace = WhiteSpace.Normal;
            card.Add(statusLabel);
            root.Add(card);
        }
    }
}
