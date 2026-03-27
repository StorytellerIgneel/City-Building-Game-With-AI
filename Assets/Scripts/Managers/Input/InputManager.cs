using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public event Action<Vector3> OnConfirm;
    public event Action<Vector3> OnCancel;
    public event Action<Vector3> OnMouseMove;
    public event Action<Vector2> OnCameraMove;
    public event Action<Vector3> OnUpgrade;
    public event Action<Vector3> OnDemolish;

    private GameInputActions inputActions;
    
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
        inputActions = new GameInputActions();
    }   

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Confirm.performed +=  HandleConfirm;
        inputActions.Player.Cancel.performed += HandleCancel;
        inputActions.Player.MoveMouse.performed += HandleMouseMove;
        inputActions.Player.MoveCamera.performed += HandleCameraMove;
        inputActions.Player.MoveCamera.canceled += HandleCameraMove; // Stop cam movement when input is released
        inputActions.Player.Upgrade.performed += HandleUpgrade;
        inputActions.Player.Demolish.performed += HandleDemolish;
    }

    private void OnDisable()
    {
        inputActions.Player.Confirm.performed -= HandleConfirm;
        inputActions.Player.Cancel.performed -= HandleCancel;
        inputActions.Player.MoveMouse.performed -= HandleMouseMove;
        inputActions.Player.MoveCamera.performed -= HandleCameraMove;
        inputActions.Player.MoveCamera.canceled -= HandleCameraMove;

        inputActions.Disable();
    }

    private void HandleConfirm(InputAction.CallbackContext context)
    {
        OnConfirm?.Invoke(GetMousePosition());
    }

    private void HandleCancel(InputAction.CallbackContext context)
    {
        OnCancel?.Invoke(GetMousePosition());
    }

    private void HandleMouseMove(InputAction.CallbackContext context)
    {
        OnMouseMove?.Invoke(GetMousePosition());
    }

    private void HandleCameraMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>(); // Already combined WASD vector
        OnCameraMove?.Invoke(input);
    }

    private void HandleUpgrade(InputAction.CallbackContext context)
    {
        OnUpgrade?.Invoke(GetMousePosition());
    }

    private void HandleDemolish(InputAction.CallbackContext context)
    {
        OnDemolish?.Invoke(GetMousePosition());
    }

    private Vector3 GetMousePosition()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
        mouseWorldPos.z = 0;
        return mouseWorldPos;
    }
}