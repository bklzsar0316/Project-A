
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Thermometer : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private Camera playerCam;
    [SerializeField] private TMP_Text tempetureText;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform tipPoint;
    [SerializeField] private Transform basePoint;
    [SerializeField] private Image progressImage;

    [Header("Settings")]
    [SerializeField] private float measurementTime = 1f;
    [SerializeField] private float rayDistance = 3f;
    [SerializeField] private float touchRadius = 0.1f;
    [SerializeField] private Vector3 placedRotation;
    [SerializeField] private LayerMask foreheadMask;

    private bool _isHold = false;
    private bool _measurementFinished = false;

    private float _measurementProgress = 0f;
    private BoxCollider _boxCollider;
    private PatientNPC _measuredPatient;

    private readonly Collider[] _hits = new Collider[8];

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();

        tempetureText.text = "--.-°C";
        ResetProgress();
    }

    public void Interact()
    {
        if (_isHold)
        {
            TryPlace();
        }
        else
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        if (holdPoint == null || !interactor.TryHold(this))
        {
            return;
        }

        transform.SetParent(holdPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _boxCollider.enabled = false;
        _isHold = true;

        _measuredPatient = null;
        _measurementFinished = false;

        tempetureText.text = "--.-°C";
        ResetProgress();
    }

    private void TryPlace()
    {
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, rayDistance))
        {
            return;
        }

        if (!hit.collider.CompareTag("Counter"))
        {
            return;
        }

        if (Vector3.Dot(hit.normal, Vector3.up) < 0.7f)
        {
            return;
        }

        transform.SetParent(null, true);

        transform.rotation =
            Quaternion.Euler(0f, playerCam.transform.eulerAngles.y, 0f)
            * Quaternion.Euler(placedRotation);

        transform.position += hit.point - basePoint.position;

        _boxCollider.enabled = true;
        _isHold = false;

        _measuredPatient = null;
        _measurementFinished = false;

        ResetProgress();
        interactor.ReleaseHeld(this);
    }

    private void Update()
    {
        if (!_isHold)
        {
            return;
        }

        PatientNPC patient = GetTouchingPatient();

        if (patient == null)
        {
            _measuredPatient = null;
            _measurementFinished = false;
            ResetProgress();
            return;
        }

        if (_measuredPatient != patient)
        {
            _measuredPatient = patient;
            _measurementFinished = false;
            tempetureText.text = "--.-°C";
            ResetProgress();
        }

        if (_measurementFinished)
        {
            return;
        }

        _measurementProgress += Time.deltaTime;

        progressImage.enabled = true;
        progressImage.fillAmount = Mathf.Clamp01(_measurementProgress / measurementTime);

        if (_measurementProgress >= measurementTime)
        {
            float temperature = patient.Session.CaseData.Temperature;

            tempetureText.text = temperature.ToString("F1") + "°C";

            _measurementFinished = true;
            progressImage.enabled = false;
        }
    }

    private PatientNPC GetTouchingPatient()
    {
        int count = Physics.OverlapSphereNonAlloc(
            tipPoint.position,
            touchRadius,
            _hits,
            foreheadMask,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            PatientNPC patient = _hits[i].GetComponentInParent<PatientNPC>();

            if (patient != null && patient.Session != null)
            {
                return patient;
            }
        }

        return null;
    }

    private void ResetProgress()
    {
        _measurementProgress = 0f;
        progressImage.fillAmount = 0f;
        progressImage.enabled = false;
    }

    private void OnDestroy()
    {
        if (_isHold && interactor != null)
        {
            interactor.ReleaseHeld(this);
        }
    }
}
