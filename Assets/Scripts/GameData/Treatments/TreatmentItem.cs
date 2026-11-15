using System;
using UnityEngine;

public class TreatmentItem : MonoBehaviour, IInteractable
{

    [SerializeField] private TreatmentController  treatmentController;
    [SerializeField] private SO_TreatmentData itemData;
    
    public SO_TreatmentData ItemData => itemData;
    
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;


    private void Awake()
    {
        _initialPosition =  transform.position;
        _initialRotation = transform.rotation;
    }

    public void ReturnToTable()
    {
        transform.SetPositionAndRotation(_initialPosition, _initialRotation);
        gameObject.SetActive(true);
    }

    public void Interact()
    {
        treatmentController.SelectItem(this);
    }

    public void SetHighLighted(bool isHighlighted)
    {
        
    }
}
