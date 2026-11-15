using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class FatihMonitor : MonoBehaviour,IInteractable
{

    [SerializeField] private FPCameraController cameraController;
    [SerializeField] private Transform targetTransform;




    private void ExitMonitor()
    {
        GameObject selected = EventSystem.current?.currentSelectedGameObject;
        TMP_InputField inputField = selected != null ? selected.GetComponentInParent<TMP_InputField>() : null;
        
        if (inputField != null && inputField.isFocused)
        {
            Debug.Log("Inputfield focus mudounda");

            return;
        }
        if (cameraController.CurrentMode != CameraMode.Monitor)
        {
            return;
        }
        
        InputController.Instance.CloseUIPressed -= ExitMonitor;
        cameraController.ExitMonitor();
    }
    public void Interact()
    {
        if (cameraController.CurrentMode != CameraMode.Gameplay || targetTransform == null)
        {
            return;
        }
        
        cameraController.EnterMonitor(targetTransform);
        InputController.Instance.CloseUIPressed += ExitMonitor;


    }


}
