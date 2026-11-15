using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
    
  
        [Header("References")]
        [SerializeField] private Transform orientationTransform;
    
        
        private InputController _inputController;
        private Rigidbody _rb;
        private Vector2 _movementVector;

        private float _playerSpeed;
        public float PlayerSpeed => _playerSpeed;
        
        
        
        
        
        

        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _inputController = GetComponent<InputController>();
        }

        private void Start()
        {
            _playerSpeed = PlayerStats.Instance.GetPlayerSpeed();
        }


        private void FixedUpdate()
        {
            OnMovement();
        }

        private void OnMovement()
        {
            if(_inputController) _movementVector = _inputController.GetMovementVector().normalized;
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
                _movementVector.x * right +
                _movementVector.y * forward ;
         
            _rb.linearVelocity = MoveDir * _playerSpeed;
        
        



        }
    }
}
