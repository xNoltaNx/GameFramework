using UnityEngine;
using UnityEngine.UI;
using GameFramework.Items;

namespace GameFramework.UI
{
    public class DragVisualManager : MonoBehaviour
    {
        private static DragVisualManager _instance;
        public static DragVisualManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DragVisualManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DragVisualManager");
                        _instance = go.AddComponent<DragVisualManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        private GameObject dragVisual;
        private Canvas canvas;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            // Find or create canvas for drag visuals
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("DragVisualManager: No Canvas found in scene!");
            }
        }
        
        public void StartDrag(ItemDefinition item, Vector2 startPosition)
        {
            if (item == null || canvas == null) return;
            
            // Clean up any existing drag visual
            StopDrag();
            
            // Create drag visual
            dragVisual = new GameObject("DragVisual");
            dragVisual.transform.SetParent(canvas.transform, false);
            dragVisual.transform.SetAsLastSibling(); // Ensure it's on top
            
            // Add Image component
            Image dragImage = dragVisual.AddComponent<Image>();
            dragImage.sprite = item.icon;
            dragImage.color = new Color(1f, 1f, 1f, 0.8f); // Semi-transparent
            dragImage.raycastTarget = false; // Don't block raycasts
            
            // Set size
            RectTransform rectTransform = dragVisual.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(64f, 64f); // Standard slot size
            
            // Position at start location
            UpdateDragPosition(startPosition);
            
            // Add CanvasGroup for additional control
            CanvasGroup canvasGroup = dragVisual.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        
        public void UpdateDragPosition(Vector2 position)
        {
            if (dragVisual != null)
            {
                dragVisual.transform.position = position;
            }
        }
        
        public void StopDrag()
        {
            if (dragVisual != null)
            {
                Destroy(dragVisual);
                dragVisual = null;
            }
        }
        
        public bool IsDragging()
        {
            return dragVisual != null;
        }
    }
}