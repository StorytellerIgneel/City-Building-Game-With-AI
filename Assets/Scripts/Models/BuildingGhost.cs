using UnityEngine;

// todo: change name of file and reassign the empty clone obj
[RequireComponent(typeof(SpriteRenderer))]
public class PlacementPreview : MonoBehaviour
{
    [SerializeField] private SpriteRenderer previewRenderer;

    private void Awake()
    {
        if (previewRenderer == null)
            previewRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        if (previewRenderer == null || sprite == null) return;

        previewRenderer.sprite = sprite;

        Color c = previewRenderer.color;
        c.a = 0.5f;
        previewRenderer.color = c;
    }

    public void SetSprite(SpriteRenderer sourceRenderer)
    {
        if (sourceRenderer == null) return;
        SetSprite(sourceRenderer.sprite);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetValid(bool isValid)
    {
        if (previewRenderer == null) return;

        Color c = previewRenderer.color;
        c.a = 0.5f;

        if (isValid)
        {
            c.r = 1f;
            c.g = 1f;
            c.b = 1f;
        }
        else
        {
            c.r = 1f;
            c.g = 0.4f;
            c.b = 0.4f;
        }

        previewRenderer.color = c;
    }
}