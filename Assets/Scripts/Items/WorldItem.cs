using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Items
{
    [RequireComponent(typeof(Collider))]
    public class WorldItem : MonoBehaviour, IInteractable
    {
        [Header("Item Configuration")]
        [SerializeField] private ItemDefinition itemDefinition;
        [SerializeField] private int quantity = 1;
        
        [Header("Interaction")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private bool showInteractionPrompt = true;
        
        [Header("Visual Effects")]
        [SerializeField] private bool enableHighlight = true;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float highlightIntensity = 1.5f;
        
        [Header("Physics")]
        [SerializeField] private bool enablePhysics = true;
        [SerializeField] private float pickupDelay = 0.1f;
        
        private Renderer[] renderers;
        private Material[] originalMaterials;
        private Material[] highlightMaterials;
        private bool isHighlighted = false;
        private bool canPickup = true;
        
        public ItemDefinition ItemDefinition => itemDefinition;
        public int Quantity => quantity;
        public string InteractionPrompt => $"Pick up {itemDefinition?.GetDisplayName() ?? "Item"}" + 
                                          (quantity > 1 ? $" ({quantity})" : "");
        public bool CanInteract => canPickup && itemDefinition != null;
        
        private void Awake()
        {
            ValidateSetup();
            SetupPhysics();
            CacheRenderers();
        }
        
        private void Start()
        {
            if (pickupDelay > 0)
            {
                canPickup = false;
                Invoke(nameof(EnablePickup), pickupDelay);
            }
        }
        
        private void ValidateSetup()
        {
            if (itemDefinition == null)
            {
                Debug.LogError($"WorldItem on {gameObject.name} has no ItemDefinition assigned!");
            }
            
            Collider col = GetComponent<Collider>();
            if (col.isTrigger == false)
            {
                Debug.LogWarning($"WorldItem on {gameObject.name} collider should be set as Trigger for interaction.");
            }
        }
        
        private void SetupPhysics()
        {
            if (enablePhysics)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = gameObject.AddComponent<Rigidbody>();
                }
            }
        }
        
        private void CacheRenderers()
        {
            if (enableHighlight)
            {
                renderers = GetComponentsInChildren<Renderer>();
                originalMaterials = new Material[renderers.Length];
                highlightMaterials = new Material[renderers.Length];
                
                for (int i = 0; i < renderers.Length; i++)
                {
                    originalMaterials[i] = renderers[i].material;
                    
                    // Create highlight material (simple emission-based highlight)
                    highlightMaterials[i] = new Material(originalMaterials[i]);
                    highlightMaterials[i].EnableKeyword("_EMISSION");
                    highlightMaterials[i].SetColor("_EmissionColor", highlightColor * highlightIntensity);
                }
            }
        }
        
        public void Interact(GameObject interactor)
        {
            Debug.Log($"WorldItem.Interact called by {interactor.name}");
            if (!CanInteract) 
            {
                Debug.Log("Cannot interact with this item");
                return;
            }
            
            // Try to get inventory controller from interactor
            IInventoryController inventory = interactor.GetComponent<IInventoryController>();
            if (inventory == null)
            {
                Debug.LogWarning($"Interactor {interactor.name} has no IInventoryController!");
                
                // Try to get it from ICharacterController
                var characterController = interactor.GetComponent<ICharacterController>();
                if (characterController != null)
                {
                    inventory = characterController.GetInventoryController();
                    Debug.Log($"Found inventory through character controller: {inventory != null}");
                }
                
                if (inventory == null)
                {
                    Debug.LogError("No inventory controller found anywhere!");
                    return;
                }
            }
            
            Debug.Log($"Attempting to add {quantity}x {itemDefinition.GetDisplayName()} to inventory");
            
            // Try to add item to inventory
            if (inventory.CanAddItem(itemDefinition, quantity))
            {
                if (inventory.AddItem(itemDefinition, quantity))
                {
                    Debug.Log("Item added successfully!");
                    OnItemPickedUp(interactor);
                }
                else
                {
                    Debug.Log("Failed to add item to inventory");
                }
            }
            else
            {
                Debug.Log("Inventory is full!");
            }
        }
        
        public void OnInteractionEnter(GameObject interactor)
        {
            if (enableHighlight && !isHighlighted)
            {
                SetHighlight(true);
            }
        }
        
        public void OnInteractionExit(GameObject interactor)
        {
            if (this == null || gameObject == null) return; // Check if destroyed
            
            if (enableHighlight && isHighlighted)
            {
                SetHighlight(false);
            }
        }
        
        private void SetHighlight(bool highlighted)
        {
            if (!enableHighlight || renderers == null) return;
            if (this == null || gameObject == null) return; // Check if destroyed
            
            isHighlighted = highlighted;
            Material[] materialsToUse = highlighted ? highlightMaterials : originalMaterials;
            
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) // Check if renderer still exists
                {
                    renderers[i].material = materialsToUse[i];
                }
            }
        }
        
        private void OnItemPickedUp(GameObject picker)
        {
            // Play pickup sound
            if (itemDefinition.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(itemDefinition.pickupSound, transform.position);
            }
            
            // Destroy the world item
            Destroy(gameObject);
        }
        
        private void EnablePickup()
        {
            canPickup = true;
        }
        
        public void SetItem(ItemDefinition item, int qty = 1)
        {
            itemDefinition = item;
            quantity = qty;
        }
        
        private void OnDestroy()
        {
            // Cleanup highlight materials
            if (highlightMaterials != null)
            {
                foreach (Material mat in highlightMaterials)
                {
                    if (mat != null)
                        Destroy(mat);
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}