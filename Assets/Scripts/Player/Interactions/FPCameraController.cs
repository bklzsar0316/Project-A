using System.Collections;
using Player;
using UnityEngine;

public class FPCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField ] private Transform playerTransform;
    [SerializeField] private Transform orientationTransform;
    
    [Header("Settings")]
    [SerializeField] private float transitionDuration = 1f;

    
    private float _mouseSensitivity;
    private float _cameraBobSpeed;
    private float _amplitude;
    private float _timeToReturnNormalHeight;
    private bool _setCursorVisible;
    
    private float _totalTimeToReturnNormalHeight;
        
    private float _sinTime;
    private float _xRotation;
    private float _yRotation;

    private float _currentCamHeight;

    private Vector3 _originalCamPositon;
    private Vector3 _tempCamPositon;
    
    private Vector3 _savedLocalPos;
    private Quaternion _savedLocalRot;

    private CameraMode _cameraMode = CameraMode.Gameplay;
    public CameraMode CurrentMode => _cameraMode;
 
    
    
    private void Start()    
    {
        _originalCamPositon = transform.localPosition;
        _tempCamPositon = transform.localPosition;
        _currentCamHeight = _originalCamPositon.y;
        
        _mouseSensitivity = PlayerStats.Instance.GetMouseSensitivity();
        _cameraBobSpeed = PlayerStats.Instance.GetCameraBobSpeed();
        _amplitude = PlayerStats.Instance.GetAmplitude();
        _timeToReturnNormalHeight = PlayerStats.Instance.GetTimeToReturnNormalHeight();
        _setCursorVisible = PlayerStats.Instance.GetCursorVisible();
        
        
        
        switch (_setCursorVisible)
        {
            case true:
            {
                Cursor.lockState = CursorLockMode.None;
            }
                break;
            case false:
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
                break;
        }
        Cursor.visible = _setCursorVisible;
        
    }
    
    private void Update()
    {
        if (_cameraMode != CameraMode.Gameplay)
        {
            return;

        }   
        HandleLook();
        HandleBob();

    }

    private void HandleLook()
    {
        // interpolate acikken rigidbodynin transformunu unity yaziyor cakisma olmamasi icin orientationtransform uzerinden 
        // sag sol kontrolu yappiyorum cakisma olumuyor nesneler titremiyor.
        Vector2 delta = InputController.Instance.GetMouseDelta();
        
        _yRotation += delta.x * _mouseSensitivity;
        _xRotation = Mathf.Clamp(_xRotation - delta.y * _mouseSensitivity, -90, 90f);
        orientationTransform.localRotation = Quaternion.Euler(0, _yRotation, 0);
        transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        
        
    }

    private void HandleBob()
    {
        if (InputController.Instance.GetMovementVector() != Vector2.zero)
        {
            _totalTimeToReturnNormalHeight = 0;
            _sinTime += Time.deltaTime * _cameraBobSpeed;
            _currentCamHeight = _originalCamPositon.y + Mathf.Sin(_sinTime) * _amplitude ;
            _tempCamPositon.y = _currentCamHeight;
            
        }
        else
        {
            _sinTime = 0;
            _totalTimeToReturnNormalHeight += Time.deltaTime / _timeToReturnNormalHeight;
            _totalTimeToReturnNormalHeight = Mathf.Clamp01(_totalTimeToReturnNormalHeight);
            
            float normalHeight = Mathf.Lerp(_currentCamHeight, _originalCamPositon.y, _totalTimeToReturnNormalHeight);
            _tempCamPositon.y = normalHeight;
        }
        transform.localPosition = _tempCamPositon;

    }


    public void EnterMonitor(Transform target)
    {
        if (_cameraMode != CameraMode.Gameplay || target == null)
        {
            return;
        }

        _savedLocalPos = transform.localPosition;
        _savedLocalRot  = transform.localRotation;
        
        _cameraMode = CameraMode.EnteringMonitor;
        StartCoroutine(MoveCamera(target.position, target.rotation, CameraMode.Monitor));
        InputController.Instance.EnterUIMode();
        
    
    
}

    public void ExitMonitor()
    {
        if (_cameraMode != CameraMode.Monitor)
        {
            return;
        }
        
        Vector3 returnPos = transform.parent.TransformPoint(_savedLocalPos);
        Quaternion returnRot = transform.parent.rotation * _savedLocalRot;
        
        
        _cameraMode = CameraMode.ExitingMonitor;
        StartCoroutine(MoveCamera(returnPos, returnRot, CameraMode.Gameplay));
    }

    

    private IEnumerator MoveCamera (Vector3 targetPos, Quaternion targetRot, CameraMode completedMode)
    {

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }
        transform.position = targetPos;
        transform.rotation = targetRot;
        _cameraMode = completedMode;

        if (completedMode == CameraMode.Gameplay)
        {
            InputController.Instance.ExitUIMode();
        }
    }
}



public enum CameraMode
{
    Gameplay,
    EnteringMonitor,
    ExitingMonitor,
    Monitor
}