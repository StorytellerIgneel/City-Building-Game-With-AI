using UnityEngine;

public class GridController : MonoBehaviour
{
    public Material gridMaterial;
    public Vector4 rectHighlightRadius {set;get;}

    private void Start()
    {
        gridMaterial.SetFloat("_GridSize", 1f);
        gridMaterial.SetFloat("_LineThickness", 0.08f);
        gridMaterial.SetFloat("_HighlightRadius", 5f);
        gridMaterial.SetVector("_RectExtension", new Vector4(2, 2, 0, 0));

        if (InputManager.Instance == null)
        {
            Logger.LogError("InputManager not initialized!");
            return;
        }
    }

    public void Initialize(Material material)
    {
        this.gridMaterial = material;
    }

    public void HandleMouseMove(Vector3 mousePos)
    {
        gridMaterial.SetVector("_MousePos", new Vector4(mousePos.x, mousePos.y+1, 0, 0)); //+1 to be closer to the ghost
    }


}