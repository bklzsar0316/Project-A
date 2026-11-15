using UnityEngine;

public class Bell : MonoBehaviour, IInteractable
{

    public void Interact()
    {
        
        EventBus.Publish(new BellRingEvent());
    }
    
}

public struct BellRingEvent
{}
