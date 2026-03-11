using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private float moveSpeed = 5f;
    private Vector3 currentInput;

    private void Start()
    {
        InputManager.Instance.OnCameraMove += HandleMoveInput;
        Debug.Log(Camera.main.name);
    }

    private void HandleMoveInput(Vector2 input)
    {
        currentInput = new Vector3(input.x, input.y, 0);
    }

    private void Update()
    {
        if(currentInput != Vector3.zero)
        {
            Vector3 move = Quaternion.Euler(0,30,0) * currentInput.normalized * moveSpeed * Time.deltaTime;
            transform.position += move;
        }
    }

}