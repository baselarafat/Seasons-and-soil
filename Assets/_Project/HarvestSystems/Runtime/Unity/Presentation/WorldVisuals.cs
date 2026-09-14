using UnityEngine;

namespace HarvestSystems.Unity.Presentation
{
    public static class WorldVisuals
    {
        private static Sprite squareSprite;

        public static Sprite SquareSprite
        {
            get
            {
                if (squareSprite != null)
                {
                    return squareSprite;
                }

                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "Runtime White Pixel",
                    filterMode = FilterMode.Point,
                    hideFlags = HideFlags.HideAndDontSave
                };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                squareSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                squareSprite.name = "Runtime Square";
                squareSprite.hideFlags = HideFlags.HideAndDontSave;
                return squareSprite;
            }
        }

        public static SpriteRenderer AddSquare(GameObject target, Color color, Vector2 size, int sortingOrder = 0)
        {
            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = color;
            renderer.size = size;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }
    }
}
