using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Player
{
    public class InputController : MonoBehaviour
    {

        public static InputController Instance { get; private set; }
    
        private PlayerControl _inputActions;
    
        private Vector2 _movementVector;
        private Vector2 _mouseDelta;

    
        public event Action InteractPressed;
        public event Action CloseUIPressed;
    
        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            _inputActions = new PlayerControl();
            _inputActions.Player.Movement.performed += SetMovementVector;
            _inputActions.Player.Movement.canceled += SetMovementVector;



            _inputActions.Player.Interact.performed += OnInteractPerformed;
            _inputActions.UI.CloseUI.performed +=  OnCloseUIPressed;

        }



        private void OnDestroy()
        {
            _inputActions.Player.Movement.performed -= SetMovementVector;
            _inputActions.Player.Movement.canceled -= SetMovementVector;
            _inputActions.Player.Interact.performed -= OnInteractPerformed;
            _inputActions.UI.CloseUI.performed -=  OnCloseUIPressed;


        
            if(Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();

        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void SetMovementVector(InputAction.CallbackContext ctx)
        {
            _movementVector  = ctx.ReadValue<Vector2>();
        }

        public Vector2 GetMovementVector()
        {
            return _movementVector;
        }

        public Vector2 GetMouseDelta()
        {
            return _inputActions.Player.Mouse.ReadValue<Vector2>();
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            InteractPressed?.Invoke();
        }
        
        private void OnCloseUIPressed(InputAction.CallbackContext obj)
        {
            CloseUIPressed?.Invoke();
        }

        public void EnterUIMode()
        {
            _inputActions.Player.Disable();
            _movementVector = Vector2.zero;
            _mouseDelta = Vector2.zero;
            _inputActions.UI.Enable();
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ExitUIMode()
        {
            _inputActions.UI.Disable();
            _inputActions.Player.Enable();
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }

        public bool IsUseToolHeld()
        {
            return _inputActions.Player.UseTool.IsPressed();
        }

        public bool IsInteractHold()
        {
            return _inputActions.Player.Interact.IsPressed();

        }
  
    

    }
}
