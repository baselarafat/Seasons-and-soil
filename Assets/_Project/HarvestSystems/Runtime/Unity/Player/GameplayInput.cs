using UnityEngine;
using UnityEngine.InputSystem;

namespace HarvestSystems.Unity.Player
{
    /// <summary>Unity Input System adapter for the Phase 1 gameplay action map.</summary>
    [DisallowMultipleComponent]
    public sealed class GameplayInput : MonoBehaviour
    {
        private const string GameplayMapName = "Gameplay";
        private const string MoveActionName = "Move";
        private const string InteractActionName = "Interact";
        private const string SelectNextCropActionName = "SelectNextCrop";

        [SerializeField] private InputActionAsset controls;

        private InputActionAsset runtimeControls;
        private InputActionMap gameplayMap;
        private InputAction moveAction;
        private InputAction interactAction;
        private InputAction selectNextCropAction;

        public Vector2 Movement => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool InteractWasPressedThisFrame => interactAction?.WasPressedThisFrame() ?? false;
        public bool SelectNextCropWasPressedThisFrame => selectNextCropAction?.WasPressedThisFrame() ?? false;

        public void Configure(InputActionAsset inputAsset)
        {
            controls = inputAsset != null
                ? inputAsset
                : throw new System.ArgumentNullException(nameof(inputAsset));
            InitializeActions();
        }

        private void Awake()
        {
            if (controls != null)
            {
                InitializeActions();
            }
        }

        private void OnEnable()
        {
            gameplayMap?.Enable();
        }

        private void OnDisable()
        {
            gameplayMap?.Disable();
        }

        private void OnDestroy()
        {
            if (runtimeControls != null)
            {
                Destroy(runtimeControls);
            }
        }

        private void InitializeActions()
        {
            gameplayMap?.Disable();
            if (runtimeControls != null)
            {
                Destroy(runtimeControls);
            }

            runtimeControls = Instantiate(controls);
            runtimeControls.name = $"{controls.name} (Runtime)";
            gameplayMap = runtimeControls.FindActionMap(GameplayMapName, true);
            moveAction = gameplayMap.FindAction(MoveActionName, true);
            interactAction = gameplayMap.FindAction(InteractActionName, true);
            selectNextCropAction = gameplayMap.FindAction(SelectNextCropActionName, true);

            if (isActiveAndEnabled)
            {
                gameplayMap.Enable();
            }
        }
    }
}
