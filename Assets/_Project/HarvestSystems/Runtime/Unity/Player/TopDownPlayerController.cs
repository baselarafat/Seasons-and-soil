using UnityEngine;

namespace HarvestSystems.Unity.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class TopDownPlayerController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 4f;

        private Rigidbody2D body;
        private Vector2 movement;

        public Vector2 Facing { get; private set; } = Vector2.down;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            movement = new Vector2(
                ReadAxis(KeyCode.A, KeyCode.LeftArrow, KeyCode.D, KeyCode.RightArrow),
                ReadAxis(KeyCode.S, KeyCode.DownArrow, KeyCode.W, KeyCode.UpArrow));
            movement = Vector2.ClampMagnitude(movement, 1f);

            if (movement.sqrMagnitude > 0.001f)
            {
                Facing = movement.normalized;
            }
        }

        private void FixedUpdate()
        {
            body.linearVelocity = movement * moveSpeed;
        }

        private static float ReadAxis(KeyCode negative, KeyCode negativeAlternative, KeyCode positive, KeyCode positiveAlternative)
        {
            float value = 0f;
            if (Input.GetKey(negative) || Input.GetKey(negativeAlternative))
            {
                value -= 1f;
            }

            if (Input.GetKey(positive) || Input.GetKey(positiveAlternative))
            {
                value += 1f;
            }

            return value;
        }
    }
}
