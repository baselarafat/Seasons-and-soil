using HarvestSystems.Unity.Farming;
using HarvestSystems.Unity.Interaction;
using HarvestSystems.Unity.Player;
using HarvestSystems.Unity.Presentation;
using HarvestSystems.Unity.Time;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HarvestSystems.Unity.Composition
{
    /// <summary>Creates the no-art Phase 1 demo world from the checked-in bootstrap scene.</summary>
    public sealed class Phase1DemoBootstrap : MonoBehaviour
    {
        public HarvestGameController GameController { get; private set; }

        private void Awake()
        {
            BuildCamera();
            BuildBackground();

            GameController = new GameObject("Harvest Game Controller").AddComponent<HarvestGameController>();
            BuildPlots();
            BuildDayAdvanceTile();
            BuildPlayer();

            Phase1Hud hud = new GameObject("Phase 1 HUD").AddComponent<Phase1Hud>();
            hud.Bind(GameController);
        }

        private static void BuildCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.backgroundColor = new Color(0.13f, 0.20f, 0.16f);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void BuildBackground()
        {
            GameObject background = new GameObject("Grass");
            background.transform.position = Vector3.forward;
            WorldVisuals.AddSquare(background, new Color(0.36f, 0.57f, 0.28f), new Vector2(14f, 10f), -10);
        }

        private static void BuildPlots()
        {
            int index = 0;
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    GameObject plotObject = new GameObject($"Soil Plot {index + 1}");
                    plotObject.transform.position = new Vector3(-1f + column * 1.35f, -0.7f + row * 1.35f, 0f);
                    WorldVisuals.AddSquare(plotObject, new Color(0.39f, 0.25f, 0.12f), Vector2.one, 0);
                    plotObject.AddComponent<BoxCollider2D>().size = Vector2.one;
                    SoilPlotView view = plotObject.AddComponent<SoilPlotView>();
                    view.Configure($"plot.demo_{index + 1}");
                    index++;
                }
            }
        }

        private static void BuildDayAdvanceTile()
        {
            GameObject tile = new GameObject("Next Day Tile");
            tile.transform.position = new Vector3(-4f, 1.6f, 0f);
            WorldVisuals.AddSquare(tile, new Color(0.20f, 0.55f, 0.85f), new Vector2(1.5f, 1.5f), 0);
            tile.AddComponent<BoxCollider2D>().size = new Vector2(1.5f, 1.5f);
            tile.AddComponent<DayAdvanceInteractable>();
        }

        private static void BuildPlayer()
        {
            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(-3.6f, -1.2f, 0f);
            WorldVisuals.AddSquare(player, new Color(0.95f, 0.85f, 0.42f), new Vector2(0.65f, 0.85f), 10);

            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            player.AddComponent<BoxCollider2D>().size = new Vector2(0.65f, 0.85f);
            InputActionAsset controls = Resources.Load<InputActionAsset>("Input/HarvestSystemsControls");
            if (controls == null)
            {
                throw new MissingReferenceException("Missing Resources/Input/HarvestSystemsControls.inputactions.");
            }

            player.AddComponent<GameplayInput>().Configure(controls);
            player.AddComponent<TopDownPlayerController>();
            player.AddComponent<PlayerInteractor>();
        }
    }
}
