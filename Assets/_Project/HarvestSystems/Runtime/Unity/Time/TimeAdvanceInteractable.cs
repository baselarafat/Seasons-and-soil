using HarvestSystems.Unity.Composition;
using HarvestSystems.Unity.Interaction;
using UnityEngine;

namespace HarvestSystems.Unity.Time
{
    public sealed class TimeAdvanceInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField, Min(1)] private int minutes = 60;

        private HarvestGameController controller;

        public void Bind(HarvestGameController gameController)
        {
            controller = gameController;
        }

        public void Interact()
        {
            controller?.AdvanceTime(minutes);
        }
    }
}
