using HarvestSystems.Unity.Player;
using UnityEngine;

namespace HarvestSystems.Unity.Interaction
{
    [RequireComponent(typeof(TopDownPlayerController))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float reach = 0.8f;
        [SerializeField, Min(0.1f)] private float radius = 0.45f;

        private TopDownPlayerController movement;

        private void Awake()
        {
            movement = GetComponent<TopDownPlayerController>();
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.E))
            {
                return;
            }

            Vector2 center = (Vector2)transform.position + movement.Facing * reach;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            for (int i = 0; i < hits.Length; i++)
            {
                MonoBehaviour[] behaviours = hits[i].GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour is IInteractable interactable)
                    {
                        interactable.Interact();
                        return;
                    }
                }
            }
        }
    }
}
