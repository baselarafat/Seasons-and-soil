using UnityEngine;

namespace HarvestSystems.Unity.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(GameplayInput))]
    public sealed class TopDownPlayerController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 4f;

        private Rigidbody2D body;
        private GameplayInput input;
        private Vector2 movement;

        public Vector2 Facing { get; private set; } = Vector2.down;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<GameplayInput>();
        }

        private void Update()
        {
            movement = Vector2.ClampMagnitude(input.Movement, 1f);

            if (movement.sqrMagnitude > 0.001f)
            {
                Facing = movement.normalized;
            }
        }

        private void FixedUpdate()
        {
            body.linearVelocity = movement * moveSpeed;
        }

    }
}
