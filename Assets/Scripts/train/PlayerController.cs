using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    
  
    [Header("References")]
    [SerializeField] private Transform orientationTransform;
    
    private InputController inputController;

    [Header("Settings")] 
    [SerializeField] private float playerSpeed;
    
    private Rigidbody rb;
    private Vector2 movementVector;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputController = GetComponent<InputController>();
    }


    private void FixedUpdate()
    {
        OnMovement();
    }

    private void OnMovement()
    {
        if(inputController) movementVector = inputController.GetMovementVector().normalized;
        else
        {
            Debug.Log("input controller algilanamadi");
            return;
        }
        Vector3 forward = orientationTransform.forward;
        Vector3 right = orientationTransform.right;
        
        forward.y = 0;
        right.y = 0;
        
        right.Normalize();
        forward.Normalize();
        
        Vector3 MoveDir =
            movementVector.x * right +
            movementVector.y * forward ;
         
        rb.linearVelocity = MoveDir * playerSpeed;
        
        



    }
}
