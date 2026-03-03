using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public event Action<Vector3> OnConfirm;
    public event Action<Vector3> OnCancel;
    public event Action<Vector3> OnMouseMove;
    
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        mainCamera = Camera.main;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        Vector3 mouseWorldPos = GetMousePosition();
        OnMouseMove?.Invoke(mouseWorldPos);
        // if (Input.GetKeyDown(KeyCode.Z))
        //     commandInvoker.Undo();

        // if (Input.GetKeyDown(KeyCode.Y))
        //     commandInvoker.Redo();

        if (Input.GetMouseButtonDown(0))
        {
            OnConfirm?.Invoke(mouseWorldPos);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            OnCancel?.Invoke(mouseWorldPos);
        }
    }

    private Vector3 GetMousePosition()
    {
        var mousePos = Input.mousePosition;
        var mouseWorldPos = mainCamera.ScreenToWorldPoint(mousePos);
        mouseWorldPos.z = 0;
        return mouseWorldPos;
    }
}