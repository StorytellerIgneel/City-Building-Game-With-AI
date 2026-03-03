using UnityEngine;

public class BuildingInteraction : MonoBehaviour
{
    public GameObject uiPrefab; // prefab with Upgrade/Demolish buttons
    private GameObject uiInstance;

    // private void OnMouseDown()
    // {
    //     if (uiInstance == null)
    //     {
    //         uiInstance = Instantiate(uiPrefab, transform.position, Quaternion.identity);
    //         uiInstance.GetComponent<BuildingUI>().Init(this);
    //     }
    //     else
    //     {
    //         Destroy(uiInstance);
    //     }
    // }

    public void Upgrade()
    {
        Debug.Log("Upgraded " + gameObject.name);
        // e.g. change sprite, increase stats, etc.
    }

    public void Demolish()
    {
        Debug.Log("Demolished " + gameObject.name);
        Destroy(gameObject);
    }
}
