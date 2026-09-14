using HarvestSystems.Unity.Composition;
using UnityEngine;

namespace HarvestSystems.Unity.Presentation
{
    public sealed class Phase1Hud : MonoBehaviour
    {
        private HarvestGameController controller;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;

        public void Bind(HarvestGameController gameController)
        {
            controller = gameController;
        }

        private void OnGUI()
        {
            if (controller == null || controller.Simulation == null)
            {
                return;
            }

            EnsureStyles();
            GUI.Box(new Rect(16, 16, 430, 142), GUIContent.none);
            GUI.Label(new Rect(30, 26, 400, 28), "HARVEST SYSTEMS — PHASE 1", titleStyle);
            GUI.Label(new Rect(30, 58, 400, 94), BuildStatus(), bodyStyle);
        }

        private string BuildStatus()
        {
            int seeds = controller.Simulation.Inventory.GetQuantity(controller.SelectedCrop.SeedItemId);
            int produce = controller.Simulation.Inventory.GetQuantity(controller.SelectedCrop.HarvestedItemId);
            return $"Day {controller.Simulation.Clock.CurrentDay}   Seeds: {seeds}   Harvested: {produce}\n" +
                   "Move: WASD / Arrows    Interact: E    Blue tile: next day\n" +
                   controller.StatusMessage;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
        }
    }
}
