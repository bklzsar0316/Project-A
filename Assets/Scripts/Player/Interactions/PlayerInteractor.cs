
using UnityEngine;

namespace Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float rayDistance;

        private IInteractable _heldItem;

        private void OnEnable()
        {
            InputController.Instance.InteractPressed += TryInteract;
        }

        private void OnDisable()
        {
            if (InputController.Instance != null)
            {
                InputController.Instance.InteractPressed -= TryInteract;
            }
        }

        private void TryInteract()
        {
            if (_heldItem != null)
            {
                _heldItem.Interact();
                return;
            }

            if (cam == null)
            {
                Debug.Log("Kamera atamayi unutmussunuz");
            }
            else
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, rayDistance))
                {
                    IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact();
                    }
                }
            }
        }

        public bool TryHold(IInteractable item)
        {
            if (item == null || _heldItem != null)
            {
                return false;
            }

            _heldItem = item;
            return true;
        }

        public void ReleaseHeld(IInteractable item)
        {
            if (_heldItem == item)
            {
                _heldItem = null;
            }
        }
    }
}
