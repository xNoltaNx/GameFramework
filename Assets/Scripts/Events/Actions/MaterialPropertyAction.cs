using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Unified action for animating multiple material properties of different types.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Material Property Action")]
    public class MaterialPropertyAction : BaseTriggerAction
    {
        [System.Serializable]
        public enum PropertyType
        {
            Float,
            Color,
            Vector,
            Texture
        }
        
        [System.Serializable]
        public class MaterialPropertyChange
        {
            [Header("Property Selection")]
            public PropertyType propertyType = PropertyType.Float;
            public string propertyName = "_Color";
            
            [Header("Float Settings")]
            public float floatStartValue = 0f;
            public float floatTargetValue = 1f;
            
            [Header("Color Settings")]
            public Color colorStartValue = Color.black;
            public Color colorTargetValue = Color.white;
            
            [Header("Vector Settings")]
            public Vector4 vectorStartValue = Vector4.zero;
            public Vector4 vectorTargetValue = Vector4.one;
            
            [Header("Texture Settings")]
            public Texture targetTexture;
            public bool animateOffset = false;
            public Vector2 offsetStartValue = Vector2.zero;
            public Vector2 offsetTargetValue = Vector2.zero;
            public bool animateScale = false;
            public Vector2 scaleStartValue = Vector2.one;
            public Vector2 scaleTargetValue = Vector2.one;
            
            [Header("Animation Settings")]
            public bool animateFromCurrent = true;
            public EasingSettings easingSettings = new EasingSettings();
            
            // Runtime cache
            [System.NonSerialized]
            public int propertyID;
            [System.NonSerialized]
            public bool isValid;
        }
        
        [Header("Material Settings")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Material targetMaterial;
        [SerializeField] private int materialIndex = 0;
        [SerializeField] private bool useSharedMaterial = false;
        [SerializeField] private bool useMaterialPropertyBlock = true;
        
        [Header("Animation")]
        [SerializeField] private float duration = 1f;
        [SerializeField] private bool animateSequentially = false;
        [SerializeField] private float sequentialDelay = 0.1f;
        
        [Header("Property Changes")]
        [SerializeField] private List<MaterialPropertyChange> propertyChanges = new List<MaterialPropertyChange>();
        
        private Material materialInstance;
        private MaterialPropertyBlock materialPropertyBlock;
        private List<Coroutine> animationCoroutines = new List<Coroutine>();
        
        protected virtual void Awake()
        {
            CachePropertyIDs();
            
            if (useMaterialPropertyBlock)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
            }
        }
        
        protected override void PerformAction(GameObject context)
        {
            // Note: context parameter is not used by MaterialPropertyAction as it operates on
            // configured target renderer/material rather than context objects
            
            if (!ValidateSetup())
            {
                return;
            }
            
            GetMaterialInstance();
            
            if (duration <= 0f)
            {
                SetPropertiesImmediate();
            }
            else
            {
                if (animateSequentially)
                {
                    StartCoroutine(AnimatePropertiesSequentially());
                }
                else
                {
                    AnimatePropertiesSimultaneously();
                }
            }
        }
        
        private bool ValidateSetup()
        {
            if (useMaterialPropertyBlock && targetRenderer == null)
            {
                LogWarning("MaterialPropertyBlock mode requires a target renderer");
                return false;
            }
            
            if (!useMaterialPropertyBlock && targetRenderer == null && targetMaterial == null)
            {
                LogWarning("No target renderer or material specified");
                return false;
            }
            
            if (propertyChanges.Count == 0)
            {
                LogWarning("No property changes specified");
                return false;
            }
            
            return true;
        }
        
        private void GetMaterialInstance()
        {
            if (useMaterialPropertyBlock)
            {
                // When using MaterialPropertyBlock, we need the shared material for property validation
                if (targetRenderer != null)
                {
                    var sharedMaterials = targetRenderer.sharedMaterials;
                    if (materialIndex >= 0 && materialIndex < sharedMaterials.Length)
                    {
                        materialInstance = sharedMaterials[materialIndex];
                        
                        // Get current property block values
                        targetRenderer.GetPropertyBlock(materialPropertyBlock, materialIndex);
                    }
                    else
                    {
                        LogWarning($"Material index {materialIndex} is out of bounds. Renderer has {sharedMaterials.Length} materials");
                        materialInstance = null;
                    }
                }
            }
            else
            {
                // Traditional material instance approach
                if (targetMaterial != null)
                {
                    materialInstance = targetMaterial;
                }
                else if (targetRenderer != null)
                {
                    if (useSharedMaterial)
                    {
                        var sharedMaterials = targetRenderer.sharedMaterials;
                        if (materialIndex >= 0 && materialIndex < sharedMaterials.Length)
                        {
                            materialInstance = sharedMaterials[materialIndex];
                        }
                        else
                        {
                            LogWarning($"Material index {materialIndex} is out of bounds. Renderer has {sharedMaterials.Length} shared materials");
                            materialInstance = null;
                        }
                    }
                    else
                    {
                        var materials = targetRenderer.materials;
                        if (materialIndex >= 0 && materialIndex < materials.Length)
                        {
                            materialInstance = materials[materialIndex];
                        }
                        else
                        {
                            LogWarning($"Material index {materialIndex} is out of bounds. Renderer has {materials.Length} materials");
                            materialInstance = null;
                        }
                    }
                }
            }
        }
        
        private void CachePropertyIDs()
        {
            foreach (var change in propertyChanges)
            {
                if (!string.IsNullOrEmpty(change.propertyName))
                {
                    change.propertyID = Shader.PropertyToID(change.propertyName);
                    change.isValid = true;
                }
                else
                {
                    change.isValid = false;
                }
            }
        }
        
        private void SetPropertiesImmediate()
        {
            if (materialInstance == null)
            {
                LogWarning("Cannot set properties - materialInstance is null");
                return;
            }
            
            foreach (var change in propertyChanges)
            {
                if (!change.isValid || !materialInstance.HasProperty(change.propertyID))
                    continue;
                
                SetPropertyImmediate(change);
            }
            
            LogDebug("Set all material properties immediately");
        }
        
        private void SetPropertyImmediate(MaterialPropertyChange change)
        {
            if (useMaterialPropertyBlock)
            {
                SetPropertyOnPropertyBlock(change);
                ApplyPropertyBlock();
            }
            else
            {
                SetPropertyOnMaterial(change);
            }
        }
        
        private void SetPropertyOnPropertyBlock(MaterialPropertyChange change)
        {
            switch (change.propertyType)
            {
                case PropertyType.Float:
                    materialPropertyBlock.SetFloat(change.propertyID, change.floatTargetValue);
                    break;
                    
                case PropertyType.Color:
                    materialPropertyBlock.SetColor(change.propertyID, change.colorTargetValue);
                    break;
                    
                case PropertyType.Vector:
                    materialPropertyBlock.SetVector(change.propertyID, change.vectorTargetValue);
                    break;
                    
                case PropertyType.Texture:
                    if (change.targetTexture != null)
                    {
                        materialPropertyBlock.SetTexture(change.propertyID, change.targetTexture);
                    }
                    // Note: Texture offset/scale cannot be set on MaterialPropertyBlock
                    // These need to be handled differently or set on the material
                    if (change.animateOffset || change.animateScale)
                    {
                        LogWarning($"Texture offset/scale animation not supported with MaterialPropertyBlock for property: {change.propertyName}");
                    }
                    break;
            }
        }
        
        private void SetPropertyOnMaterial(MaterialPropertyChange change)
        {
            switch (change.propertyType)
            {
                case PropertyType.Float:
                    materialInstance.SetFloat(change.propertyID, change.floatTargetValue);
                    break;
                    
                case PropertyType.Color:
                    materialInstance.SetColor(change.propertyID, change.colorTargetValue);
                    break;
                    
                case PropertyType.Vector:
                    materialInstance.SetVector(change.propertyID, change.vectorTargetValue);
                    break;
                    
                case PropertyType.Texture:
                    if (change.targetTexture != null)
                    {
                        materialInstance.SetTexture(change.propertyID, change.targetTexture);
                    }
                    if (change.animateOffset)
                    {
                        materialInstance.SetTextureOffset(change.propertyName, change.offsetTargetValue);
                    }
                    if (change.animateScale)
                    {
                        materialInstance.SetTextureScale(change.propertyName, change.scaleTargetValue);
                    }
                    break;
            }
        }
        
        private void ApplyPropertyBlock()
        {
            if (targetRenderer != null && materialPropertyBlock != null)
            {
                // Bounds check for materialIndex
                if (materialIndex >= 0 && materialIndex < targetRenderer.sharedMaterials.Length)
                {
                    targetRenderer.SetPropertyBlock(materialPropertyBlock, materialIndex);
                }
                else
                {
                    LogWarning($"Cannot apply property block - material index {materialIndex} is out of bounds");
                }
            }
        }
        
        private void AnimatePropertiesSimultaneously()
        {
            if (materialInstance == null)
            {
                LogWarning("Cannot animate properties - materialInstance is null");
                return;
            }
            
            StopAllAnimations();
            
            foreach (var change in propertyChanges)
            {
                if (!change.isValid || !materialInstance.HasProperty(change.propertyID))
                    continue;
                
                var coroutine = StartCoroutine(AnimateProperty(change, 0f));
                animationCoroutines.Add(coroutine);
            }
        }
        
        private IEnumerator AnimatePropertiesSequentially()
        {
            if (materialInstance == null)
            {
                LogWarning("Cannot animate properties - materialInstance is null");
                yield break;
            }
            
            StopAllAnimations();
            
            for (int i = 0; i < propertyChanges.Count; i++)
            {
                var change = propertyChanges[i];
                if (!change.isValid || !materialInstance.HasProperty(change.propertyID))
                    continue;
                
                var coroutine = StartCoroutine(AnimateProperty(change, 0f));
                animationCoroutines.Add(coroutine);
                
                if (i < propertyChanges.Count - 1)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
        }
        
        private IEnumerator AnimateProperty(MaterialPropertyChange change, float startDelay)
        {
            if (startDelay > 0f)
            {
                yield return new WaitForSeconds(startDelay);
            }
            
            // Get start values
            var startValues = GetCurrentValues(change);
            float elapsedTime = 0f;
            
            LogDebug($"Animating property {change.propertyName} ({change.propertyType}) over {duration}s");
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float easedT = change.easingSettings.Evaluate(t);
                
                ApplyInterpolatedValue(change, startValues, easedT);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Set final values
            SetPropertyImmediate(change);
        }
        
        private PropertyValues GetCurrentValues(MaterialPropertyChange change)
        {
            var values = new PropertyValues();
            
            if (!change.animateFromCurrent)
            {
                values.floatValue = change.floatStartValue;
                values.colorValue = change.colorStartValue;
                values.vectorValue = change.vectorStartValue;
                values.offsetValue = change.offsetStartValue;
                values.scaleValue = change.scaleStartValue;
                return values;
            }
            
            // Get current values from MaterialPropertyBlock or Material
            if (useMaterialPropertyBlock)
            {
                GetCurrentValuesFromPropertyBlock(change, ref values);
            }
            else
            {
                GetCurrentValuesFromMaterial(change, ref values);
            }
            
            return values;
        }
        
        private void GetCurrentValuesFromPropertyBlock(MaterialPropertyChange change, ref PropertyValues values)
        {
            switch (change.propertyType)
            {
                case PropertyType.Float:
                    // Try to get from property block, fallback to material
                    if (materialPropertyBlock.HasFloat(change.propertyID))
                        values.floatValue = materialPropertyBlock.GetFloat(change.propertyID);
                    else if (materialInstance != null)
                        values.floatValue = materialInstance.GetFloat(change.propertyID);
                    else
                        values.floatValue = change.floatStartValue;
                    break;
                    
                case PropertyType.Color:
                    if (materialPropertyBlock.HasColor(change.propertyID))
                        values.colorValue = materialPropertyBlock.GetColor(change.propertyID);
                    else if (materialInstance != null)
                        values.colorValue = materialInstance.GetColor(change.propertyID);
                    else
                        values.colorValue = change.colorStartValue;
                    break;
                    
                case PropertyType.Vector:
                    if (materialPropertyBlock.HasVector(change.propertyID))
                        values.vectorValue = materialPropertyBlock.GetVector(change.propertyID);
                    else if (materialInstance != null)
                        values.vectorValue = materialInstance.GetVector(change.propertyID);
                    else
                        values.vectorValue = change.vectorStartValue;
                    break;
                    
                case PropertyType.Texture:
                    // MaterialPropertyBlock doesn't support texture offset/scale
                    if (materialInstance != null)
                    {
                        values.offsetValue = materialInstance.GetTextureOffset(change.propertyName);
                        values.scaleValue = materialInstance.GetTextureScale(change.propertyName);
                    }
                    else
                    {
                        values.offsetValue = change.offsetStartValue;
                        values.scaleValue = change.scaleStartValue;
                    }
                    break;
            }
        }
        
        private void GetCurrentValuesFromMaterial(MaterialPropertyChange change, ref PropertyValues values)
        {
            switch (change.propertyType)
            {
                case PropertyType.Float:
                    values.floatValue = materialInstance.GetFloat(change.propertyID);
                    break;
                    
                case PropertyType.Color:
                    values.colorValue = materialInstance.GetColor(change.propertyID);
                    break;
                    
                case PropertyType.Vector:
                    values.vectorValue = materialInstance.GetVector(change.propertyID);
                    break;
                    
                case PropertyType.Texture:
                    values.offsetValue = materialInstance.GetTextureOffset(change.propertyName);
                    values.scaleValue = materialInstance.GetTextureScale(change.propertyName);
                    break;
            }
        }
        
        private void ApplyInterpolatedValue(MaterialPropertyChange change, PropertyValues startValues, float t)
        {
            if (useMaterialPropertyBlock)
            {
                ApplyInterpolatedValueToPropertyBlock(change, startValues, t);
                ApplyPropertyBlock();
            }
            else
            {
                ApplyInterpolatedValueToMaterial(change, startValues, t);
            }
        }
        
        private void ApplyInterpolatedValueToPropertyBlock(MaterialPropertyChange change, PropertyValues startValues, float t)
        {
            switch (change.propertyType)
            {
                case PropertyType.Float:
                    float currentFloat = Mathf.Lerp(startValues.floatValue, change.floatTargetValue, t);
                    materialPropertyBlock.SetFloat(change.propertyID, currentFloat);
                    break;
                    
                case PropertyType.Color:
                    Color currentColor = Color.Lerp(startValues.colorValue, change.colorTargetValue, t);
                    materialPropertyBlock.SetColor(change.propertyID, currentColor);
                    break;
                    
                case PropertyType.Vector:
                    Vector4 currentVector = Vector4.Lerp(startValues.vectorValue, change.vectorTargetValue, t);
                    materialPropertyBlock.SetVector(change.propertyID, currentVector);
                    break;
                    
                case PropertyType.Texture:
                    // MaterialPropertyBlock doesn't support texture offset/scale animation
                    if (change.animateOffset || change.animateScale)
                    {
                        // Fall back to material for texture offset/scale
                        if (materialInstance != null)
                        {
                            if (change.animateOffset)
                            {
                                Vector2 currentOffset = Vector2.Lerp(startValues.offsetValue, change.offsetTargetValue, t);
                                materialInstance.SetTextureOffset(change.propertyName, currentOffset);
                            }
                            if (change.animateScale)
                            {
                                Vector2 currentScale = Vector2.Lerp(startValues.scaleValue, change.scaleTargetValue, t);
                                materialInstance.SetTextureScale(change.propertyName, currentScale);
                            }
                        }
                    }
                    break;
            }
        }
        
        private void ApplyInterpolatedValueToMaterial(MaterialPropertyChange change, PropertyValues startValues, float t)
        {
            switch (change.propertyType)
            {
                case PropertyType.Float:
                    float currentFloat = Mathf.Lerp(startValues.floatValue, change.floatTargetValue, t);
                    materialInstance.SetFloat(change.propertyID, currentFloat);
                    break;
                    
                case PropertyType.Color:
                    Color currentColor = Color.Lerp(startValues.colorValue, change.colorTargetValue, t);
                    materialInstance.SetColor(change.propertyID, currentColor);
                    break;
                    
                case PropertyType.Vector:
                    Vector4 currentVector = Vector4.Lerp(startValues.vectorValue, change.vectorTargetValue, t);
                    materialInstance.SetVector(change.propertyID, currentVector);
                    break;
                    
                case PropertyType.Texture:
                    if (change.animateOffset)
                    {
                        Vector2 currentOffset = Vector2.Lerp(startValues.offsetValue, change.offsetTargetValue, t);
                        materialInstance.SetTextureOffset(change.propertyName, currentOffset);
                    }
                    if (change.animateScale)
                    {
                        Vector2 currentScale = Vector2.Lerp(startValues.scaleValue, change.scaleTargetValue, t);
                        materialInstance.SetTextureScale(change.propertyName, currentScale);
                    }
                    break;
            }
        }
        
        private void StopAllAnimations()
        {
            foreach (var coroutine in animationCoroutines)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            animationCoroutines.Clear();
        }
        
        public override void Stop()
        {
            base.Stop();
            StopAllAnimations();
        }
        
        #region Public API
        
        public void AddPropertyChange(PropertyType type, string propertyName)
        {
            var change = new MaterialPropertyChange
            {
                propertyType = type,
                propertyName = propertyName
            };
            propertyChanges.Add(change);
            CachePropertyIDs();
        }
        
        public void SetFloatProperty(string propertyName, float startValue, float targetValue)
        {
            var change = new MaterialPropertyChange
            {
                propertyType = PropertyType.Float,
                propertyName = propertyName,
                floatStartValue = startValue,
                floatTargetValue = targetValue
            };
            propertyChanges.Add(change);
            CachePropertyIDs();
        }
        
        public void SetColorProperty(string propertyName, Color startColor, Color targetColor)
        {
            var change = new MaterialPropertyChange
            {
                propertyType = PropertyType.Color,
                propertyName = propertyName,
                colorStartValue = startColor,
                colorTargetValue = targetColor
            };
            propertyChanges.Add(change);
            CachePropertyIDs();
        }
        
        public void SetTargetRenderer(Renderer renderer, int matIndex = 0)
        {
            targetRenderer = renderer;
            materialIndex = matIndex;
            targetMaterial = null;
        }
        
        public void SetTargetMaterial(Material material)
        {
            targetMaterial = material;
            targetRenderer = null;
        }
        
        #endregion
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            duration = Mathf.Max(0f, duration);
            sequentialDelay = Mathf.Max(0f, sequentialDelay);
            
            if (targetRenderer != null)
            {
                materialIndex = Mathf.Clamp(materialIndex, 0, targetRenderer.sharedMaterials.Length - 1);
            }
            
            // Cache property IDs when values change in editor
            if (Application.isPlaying)
            {
                CachePropertyIDs();
            }
        }
        #endif
        
        // Helper struct for storing current values
        private struct PropertyValues
        {
            public float floatValue;
            public Color colorValue;
            public Vector4 vectorValue;
            public Vector2 offsetValue;
            public Vector2 scaleValue;
        }
    }
}