using System;
using UnityEngine;
using UnityEngine.Serialization;

public class FPCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField ] private Transform playerTransform;
    
    
    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 1.0f;
    [SerializeField] private float cameraBobSpeed;
    [SerializeField] private float amplitude;
    [SerializeField] private float timeToReturnNormalHeight;
    
    
    private float _totalTimeToReturnNormalHeight;
        
    private float _sinTime;
    private float _xRotation;

    private float _currentCamHeight;

    private Vector3 _originalCamPositon;
    private Vector3 _tempCamPositon;

    
    private void Start()    
    {
        if (InputController.Instance != null)
        {
            InputController.Instance.OnMouseDeltaChanged += HandleMouseDelta;    
        }
        
        _originalCamPositon = transform.localPosition;
        
        _tempCamPositon = transform.localPosition;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        
        
    }

    private void Update()
    {
        if (InputController.Instance.GetMovementVector() != Vector2.zero)
        {
            _totalTimeToReturnNormalHeight = 0;
            _sinTime += Time.deltaTime * cameraBobSpeed;
            _currentCamHeight = _originalCamPositon.y + Mathf.Sin(_sinTime) * amplitude ;

            _tempCamPositon.y = _currentCamHeight;
            transform.localPosition = _tempCamPositon;
            

        }
        else
        {
            _sinTime = 0;
            
            _totalTimeToReturnNormalHeight += Time.deltaTime / timeToReturnNormalHeight;
            _totalTimeToReturnNormalHeight = Mathf.Clamp01(_totalTimeToReturnNormalHeight);
            
            float normalHeight = Mathf.Lerp(_currentCamHeight, _originalCamPositon.y, _totalTimeToReturnNormalHeight);
            _tempCamPositon.y = normalHeight;
            transform.localPosition = _tempCamPositon;
        }
        
    }

    private void HandleMouseDelta(Vector2 mouseDelta)
    {
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;    
        
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerTransform.Rotate(Vector3.up * mouseX);
        
        
    }
}
