using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameFramework.Items;

namespace GameFramework.UI
{
    public class StackSplitDialog : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private Slider stackSlider;
        [SerializeField] private TextMeshProUGUI stackAmountText;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        
        private InventorySlot sourceSlot;
        private ItemDefinition currentItem;
        private int maxStackSize;
        private int selectedAmount;
        
        public System.Action<int> OnStackSplit;
        
        private void Awake()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);
                
            if (stackSlider != null)
            {
                stackSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }
            
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(ConfirmSplit);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelSplit);
            }
        }
        
        public void ShowDialog(InventorySlot slot, ItemDefinition item, int stackCount)
        {
            sourceSlot = slot;
            currentItem = item;
            maxStackSize = stackCount;
            
            if (itemNameText != null)
                itemNameText.text = item.itemName;
                
            if (itemIcon != null)
                itemIcon.sprite = item.icon;
            
            if (stackSlider != null)
            {
                stackSlider.minValue = 1;
                stackSlider.maxValue = stackCount - 1; // Can't split all items
                stackSlider.value = 1;
            }
            
            selectedAmount = 1;
            UpdateAmountText();
            
            if (dialogPanel != null)
                dialogPanel.SetActive(true);
        }
        
        public void HideDialog()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);
        }
        
        private void OnSliderValueChanged(float value)
        {
            selectedAmount = Mathf.RoundToInt(value);
            UpdateAmountText();
        }
        
        private void UpdateAmountText()
        {
            if (stackAmountText != null)
            {
                int remaining = maxStackSize - selectedAmount;
                stackAmountText.text = $"Split: {selectedAmount} | Remaining: {remaining}";
            }
        }
        
        private void ConfirmSplit()
        {
            OnStackSplit?.Invoke(selectedAmount);
            HideDialog();
        }
        
        private void CancelSplit()
        {
            HideDialog();
        }
        
        private void Update()
        {
            if (dialogPanel != null && dialogPanel.activeInHierarchy && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                CancelSplit();
            }
        }
    }
}