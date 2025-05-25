using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Interaction
{
    public class InteractionController : MonoBehaviour, IInteractionController
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactionLayerMask = -1;
        [SerializeField] private float detectionRadius = 0.5f;
        
        [Header("Raycast Settings")]
        [SerializeField] private bool useRaycast = true;
        [SerializeField] private bool useSphere = true;
        [SerializeField] private Transform raycastOrigin;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private IInteractable currentInteractable;
        private IInteractable lastInteractable;
        private UnityEngine.Camera playerCamera;
        private GameObject characterGameObject;
        
        public IInteractable CurrentInteractable => currentInteractable;
        public bool HasInteractable => currentInteractable != null;
        
        private void Awake()
        {
            // Find the character GameObject (should be the same as this GameObject or parent)
            characterGameObject = gameObject;
            var characterController = GetComponent<GameFramework.Character.FirstPersonCharacterController>();
            if (characterController == null)
            {
                characterController = GetComponentInParent<GameFramework.Character.FirstPersonCharacterController>();
                if (characterController != null)
                {
                    characterGameObject = characterController.gameObject;
                }
            }
            
            if (raycastOrigin == null)
            {
                playerCamera = UnityEngine.Camera.main;
                if (playerCamera == null)
                {
                    playerCamera = FindObjectOfType<UnityEngine.Camera>();
                }
                
                if (playerCamera != null)
                {
                    raycastOrigin = playerCamera.transform;
                }
                else
                {
                    raycastOrigin = transform;
                }
            }
        }
        
        private void Update()
        {
            DetectInteractables();
            HandleInteractableEvents();
        }
        
        private void DetectInteractables()
        {
            // Clear current interactable if it has been destroyed
            if (currentInteractable != null && currentInteractable is MonoBehaviour currentMB && 
                (currentMB == null || currentMB.gameObject == null))
            {
                currentInteractable = null;
            }
            
            IInteractable detectedInteractable = null;
            
            if (useRaycast)
            {
                detectedInteractable = RaycastForInteractable();
            }
            
            if (detectedInteractable == null && useSphere)
            {
                detectedInteractable = SphereDetectInteractable();
            }
            
            currentInteractable = detectedInteractable;
        }
        
        private IInteractable RaycastForInteractable()
        {
            if (raycastOrigin == null) return null;
            
            Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayerMask))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    if (debugMode)
                    {
                        Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                    }
                    return interactable;
                }
            }
            
            if (debugMode)
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red);
            }
            
            return null;
        }
        
        private IInteractable SphereDetectInteractable()
        {
            Vector3 center = raycastOrigin != null ? raycastOrigin.position : transform.position;
            Collider[] colliders = Physics.OverlapSphere(center, detectionRadius, interactionLayerMask);
            
            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;
            
            foreach (Collider col in colliders)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    float distance = Vector3.Distance(center, col.transform.position);
                    if (distance < closestDistance && distance <= interactionRange)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }
            
            return closestInteractable;
        }
        
        private void HandleInteractableEvents()
        {
            // Handle enter/exit events
            if (currentInteractable != lastInteractable)
            {
                // Exit previous interactable
                if (lastInteractable != null)
                {
                    // Check if the interactable still exists before calling OnInteractionExit
                    if (lastInteractable is MonoBehaviour lastMB && lastMB != null && lastMB.gameObject != null)
                    {
                        lastInteractable.OnInteractionExit(characterGameObject);
                    }
                }
                
                // Enter new interactable
                if (currentInteractable != null)
                {
                    currentInteractable.OnInteractionEnter(characterGameObject);
                }
                
                lastInteractable = currentInteractable;
            }
        }
        
        public void HandleInteraction()
        {
            if (currentInteractable != null && currentInteractable.CanInteract)
            {
                Debug.Log($"Interacting with: {((MonoBehaviour)currentInteractable).name}");
                currentInteractable.Interact(characterGameObject);
            }
            else
            {
                Debug.Log($"Cannot interact - currentInteractable: {currentInteractable}, CanInteract: {currentInteractable?.CanInteract}");
            }
        }
        
        public void SetInteractionRange(float range)
        {
            interactionRange = range;
        }
        
        public void SetLayerMask(LayerMask mask)
        {
            interactionLayerMask = mask;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (raycastOrigin != null)
            {
                // Draw interaction range
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(raycastOrigin.position, interactionRange);
                
                // Draw detection radius
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(raycastOrigin.position, detectionRadius);
                
                // Draw forward ray
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * interactionRange);
            }
        }
    }
}