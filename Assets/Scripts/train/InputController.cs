using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{

    public static InputController Instance { get; private set; }
    
    private PlayerControl inputActions;
    
    private Vector2 movementVector;
    private Vector2 mouseDelta;

    
    public event Action<Vector2> OnMouseDeltaChanged;
    public event Action InteractPressed;
    
    
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        
        inputActions = new PlayerControl();
        inputActions.Player.Movement.performed += SetMovementVector;
        inputActions.Player.Movement.canceled += SetMovementVector;

        inputActions.Player.Mouse.performed += ctx => OnMouseDeltaChanged?.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Player.Mouse.canceled += ctx => OnMouseDeltaChanged?.Invoke(Vector2.zero);

        inputActions.Player.Interact.performed += OnInteractPerformed;

    }
    
    private void OnEnable()
    {
        inputActions.Enable();

    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void SetMovementVector(InputAction.CallbackContext ctx)
    {
        movementVector  = ctx.ReadValue<Vector2>();
    }

    public Vector2 GetMovementVector()
    {
        return movementVector;
    }


    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        InteractPressed?.Invoke();
    }
    
    
    

}
