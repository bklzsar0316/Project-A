using System.Collections.Generic;
using System.Reflection;
using Player;
using UnityEngine;

/// <summary>
/// Oyun seslerini programcinin betiklerine dokunmadan, disaridan dinleyerek calar (2D).
/// - Zil: EventBus BellRingEvent.
/// - Etkilesim: InputController.InteractPressed; PlayerInteractor ile ayni isinla bir IInteractable
///   bulunursa ya da elde tutulan bir nesne varsa calar.
/// - Monitor: FPCameraController.CurrentMode Gameplay'den monitore gecince calar.
/// - Adim: PlayerController Rigidbody yatay hizi ve yere degme durumuna gore stepInterval aralikla.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class GameSounds : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip bellClip;
    [SerializeField] private AudioClip interactClip;
    [SerializeField] private AudioClip monitorOnClip;
    [SerializeField] private AudioClip footstepClip;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float bellVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float interactVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float monitorOnVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.45f;

    [Header("Footsteps")]
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float minMoveSpeed = 0.5f;
    [SerializeField] private float groundCheckExtra = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool logPlays;

    private AudioSource _source;
    private PlayerController _player;
    private Rigidbody _playerBody;
    private CapsuleCollider _playerCapsule;
    private PlayerInteractor _interactor;
    private FPCameraController _cameraController;
    private InputController _input;
    private readonly HashSet<Collider> _playerColliders = new HashSet<Collider>();

    private FieldInfo _camField, _rayDistanceField, _heldItemField;
    private bool _heldLastFrame;
    private CameraMode _lastMode = CameraMode.Gameplay;
    private float _stepTimer;
    private bool _wasWalking;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
        _source.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BellRingEvent>(OnBellRing);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<BellRingEvent>(OnBellRing);
        if (_input != null) _input.InteractPressed -= OnInteractPressed;
    }

    private void Start()
    {
        _player = FindAnyObjectByType<PlayerController>();
        if (_player != null)
        {
            _playerBody = _player.GetComponent<Rigidbody>();
            _playerCapsule = _player.GetComponent<CapsuleCollider>();
            foreach (var c in _player.GetComponentsInChildren<Collider>(true)) _playerColliders.Add(c);
        }

        _interactor = FindAnyObjectByType<PlayerInteractor>();
        if (_interactor != null)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            _camField = typeof(PlayerInteractor).GetField("cam", flags);
            _rayDistanceField = typeof(PlayerInteractor).GetField("rayDistance", flags);
            _heldItemField = typeof(PlayerInteractor).GetField("_heldItem", flags);
        }

        _cameraController = FindAnyObjectByType<FPCameraController>();
        if (_cameraController != null) _lastMode = _cameraController.CurrentMode;

        _input = InputController.Instance;
        if (_input != null) _input.InteractPressed += OnInteractPressed;
        else Debug.LogWarning("[GameSounds] InputController.Instance bulunamadi, etkilesim sesi baglanamadi.");
    }

    private void Update()
    {
        if (_cameraController != null)
        {
            CameraMode mode = _cameraController.CurrentMode;
            bool enteringNow = mode == CameraMode.EnteringMonitor || mode == CameraMode.Monitor;
            bool wasOut = _lastMode == CameraMode.Gameplay || _lastMode == CameraMode.ExitingMonitor;
            if (enteringNow && wasOut) Play(monitorOnClip, monitorOnVolume);
            _lastMode = mode;
        }

        UpdateFootsteps(Time.deltaTime);
    }

    private void LateUpdate()
    {
        _heldLastFrame = IsHolding();
    }

    private void OnBellRing(BellRingEvent e)
    {
        Play(bellClip, bellVolume);
    }

    private void OnInteractPressed()
    {
        if (_heldLastFrame || IsHolding() || LookingAtInteractable())
            Play(interactClip, interactVolume);
    }

    private bool IsHolding()
    {
        return _interactor != null && _heldItemField != null && _heldItemField.GetValue(_interactor) != null;
    }

    private bool LookingAtInteractable()
    {
        if (_interactor == null || _camField == null || _rayDistanceField == null) return false;
        var cam = _camField.GetValue(_interactor) as Camera;
        if (cam == null) return false;
        float distance = (float)_rayDistanceField.GetValue(_interactor);
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Physics.Raycast(ray, out RaycastHit hit, distance) &&
               hit.collider.GetComponentInParent<IInteractable>() != null;
    }

    private void UpdateFootsteps(float dt)
    {
        bool walking = false;
        if (_playerBody != null)
        {
            Vector3 v = _playerBody.linearVelocity;
            v.y = 0f;
            walking = v.magnitude > minMoveSpeed && IsGrounded();
        }

        if (!walking)
        {
            _wasWalking = false;
            return;
        }

        if (!_wasWalking)
        {
            // Ilk adim kisa bir gecikmeyle: dokunup birakmalarda ses cikmasin.
            _stepTimer = stepInterval * 0.5f;
            _wasWalking = true;
        }

        _stepTimer -= dt;
        if (_stepTimer <= 0f)
        {
            Play(footstepClip, footstepVolume * Random.Range(0.85f, 1f));
            _stepTimer += stepInterval;
        }
    }

    private bool IsGrounded()
    {
        Transform t = _player.transform;
        float halfHeight = _playerCapsule != null ? _playerCapsule.height * 0.5f * t.lossyScale.y : 1f;
        Vector3 origin = t.position + (_playerCapsule != null ? t.TransformVector(_playerCapsule.center) : Vector3.zero);
        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, halfHeight + groundCheckExtra,
                     Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (!_playerColliders.Contains(hit.collider)) return true;
        }
        return false;
    }

    private void Play(AudioClip clip, float volume)
    {
        if (clip == null) return;
        _source.PlayOneShot(clip, volume);
        if (logPlays) Debug.Log($"[GameSounds] {clip.name} t={Time.time:F2}");
    }
}
