using HarvestSystems.Unity.Composition;
using UnityEngine;

namespace HarvestSystems.Unity.Player
{
    [RequireComponent(typeof(GameplayInput))]
    public sealed class CropSelectionInput : MonoBehaviour
    {
        private GameplayInput input;
        private HarvestGameController controller;

        public void Bind(HarvestGameController gameController)
        {
            controller = gameController;
        }

        private void Awake()
        {
            input = GetComponent<GameplayInput>();
        }

        private void Update()
        {
            if (input.SelectNextCropWasPressedThisFrame)
            {
                controller?.SelectNextCrop();
            }
        }
    }
}
