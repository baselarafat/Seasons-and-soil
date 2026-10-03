using HarvestSystems.Unity.Composition;
using HarvestSystems.Unity.Interaction;
using UnityEngine;

namespace HarvestSystems.Unity.Economy
{
    public sealed class SellStationInteractable : MonoBehaviour, IInteractable
    {
        private HarvestGameController controller;

        public void Bind(HarvestGameController gameController)
        {
            controller = gameController;
        }

        public void Interact()
        {
            controller?.SellHarvestedProduce();
        }
    }
}
