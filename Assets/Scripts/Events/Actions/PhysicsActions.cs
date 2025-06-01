using UnityEngine;
using System.Collections.Generic;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Action that applies physics forces and manipulates rigidbody properties.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Physics Action")]
    [ActionDefinition("physics-control", "⚽", "Physics Action", "Applies forces, velocities, explosions, and modifies rigidbody properties for physics interactions", "Physics", 180)]
    public class PhysicsAction : BaseTriggerAction
    {
        [System.Serializable]
        public enum PhysicsActionType
        {
            AddForce,
            AddImpulse,
            AddExplosion,
            SetVelocity,
            SetAngularVelocity,
            SetMass,
            SetDrag,
            SetAngularDrag,
            SetGravity,
            SetKinematic,
            SetConstraints,
            AddTorque
        }
        
        [System.Serializable]
        public enum ForceDirection
        {
            WorldSpace,
            LocalSpace,
            TowardsTarget,
            AwayFromTarget,
            Custom
        }
        
        [Header("Physics Settings")]
        [SerializeField] private PhysicsActionType actionType = PhysicsActionType.AddForce;
        [SerializeField] private Rigidbody targetRigidbody;
        [SerializeField] private bool useContext = false;
        [SerializeField] private bool createRigidbodyIfMissing = false;
        
        [Header("Force Settings")]
        [SerializeField] private ForceDirection forceDirection = ForceDirection.WorldSpace;
        [SerializeField] private Vector3 forceVector = Vector3.up * 10f;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float forceMagnitude = 10f;
        [SerializeField] private ForceMode forceMode = ForceMode.Force;
        [SerializeField] private bool randomizeForce = false;
        [SerializeField] private Vector2 forceMagnitudeRange = new Vector2(5f, 15f);
        
        [Header("Explosion Settings")]
        [SerializeField] private float explosionForce = 50f;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private Vector3 explosionPosition;
        [SerializeField] private bool useTransformAsExplosionCenter = true;
        [SerializeField] private float upwardsModifier = 0f;
        [SerializeField] private LayerMask explosionLayers = -1;
        
        [Header("Velocity Settings")]
        [SerializeField] private Vector3 velocity = Vector3.zero;
        [SerializeField] private Vector3 angularVelocity = Vector3.zero;
        [SerializeField] private bool addToExistingVelocity = false;
        
        [Header("Property Settings")]
        [SerializeField] private float mass = 1f;
        [SerializeField] private float drag = 0f;
        [SerializeField] private float angularDrag = 0.05f;
        [SerializeField] private bool useGravity = true;
        [SerializeField] private bool isKinematic = false;
        [SerializeField] private RigidbodyConstraints constraints = RigidbodyConstraints.None;
        
        [Header("Torque Settings")]
        [SerializeField] private Vector3 torqueVector = Vector3.up * 10f;
        [SerializeField] private bool relativeTorque = false;
        
        protected override void PerformAction(GameObject context)
        {
            Rigidbody rb = GetTargetRigidbody(context);
            
            if (rb == null)
            {
                LogWarning("No Rigidbody found for physics action");
                return;
            }
            
            switch (actionType)
            {
                case PhysicsActionType.AddForce:
                    AddForce(rb);
                    break;
                case PhysicsActionType.AddImpulse:
                    AddImpulse(rb);
                    break;
                case PhysicsActionType.AddExplosion:
                    AddExplosion(rb);
                    break;
                case PhysicsActionType.SetVelocity:
                    SetVelocity(rb);
                    break;
                case PhysicsActionType.SetAngularVelocity:
                    SetAngularVelocity(rb);
                    break;
                case PhysicsActionType.SetMass:
                    SetMass(rb);
                    break;
                case PhysicsActionType.SetDrag:
                    SetDrag(rb);
                    break;
                case PhysicsActionType.SetAngularDrag:
                    SetAngularDrag(rb);
                    break;
                case PhysicsActionType.SetGravity:
                    SetGravity(rb);
                    break;
                case PhysicsActionType.SetKinematic:
                    SetKinematic(rb);
                    break;
                case PhysicsActionType.SetConstraints:
                    SetConstraints(rb);
                    break;
                case PhysicsActionType.AddTorque:
                    AddTorque(rb);
                    break;
            }
        }
        
        private Rigidbody GetTargetRigidbody(GameObject context)
        {
            Rigidbody rb = null;
            
            if (useContext && context != null)
            {
                rb = context.GetComponent<Rigidbody>();
            }
            else if (targetRigidbody != null)
            {
                rb = targetRigidbody;
            }
            else
            {
                rb = GetComponent<Rigidbody>();
            }
            
            if (rb == null && createRigidbodyIfMissing)
            {
                GameObject targetObj = useContext && context != null ? context : gameObject;
                rb = targetObj.AddComponent<Rigidbody>();
                LogDebug($"Created Rigidbody on {targetObj.name}");
            }
            
            return rb;
        }
        
        private Vector3 CalculateForceVector(Rigidbody rb)
        {
            Vector3 finalForce = forceVector;
            float finalMagnitude = randomizeForce ? Random.Range(forceMagnitudeRange.x, forceMagnitudeRange.y) : forceMagnitude;
            
            switch (forceDirection)
            {
                case ForceDirection.WorldSpace:
                    finalForce = forceVector.normalized * finalMagnitude;
                    break;
                    
                case ForceDirection.LocalSpace:
                    finalForce = rb.transform.TransformDirection(forceVector.normalized) * finalMagnitude;
                    break;
                    
                case ForceDirection.TowardsTarget:
                    if (targetTransform != null)
                    {
                        Vector3 direction = (targetTransform.position - rb.transform.position).normalized;
                        finalForce = direction * finalMagnitude;
                    }
                    else
                    {
                        LogWarning("No target transform specified for TowardsTarget force direction");
                        finalForce = Vector3.zero;
                    }
                    break;
                    
                case ForceDirection.AwayFromTarget:
                    if (targetTransform != null)
                    {
                        Vector3 direction = (rb.transform.position - targetTransform.position).normalized;
                        finalForce = direction * finalMagnitude;
                    }
                    else
                    {
                        LogWarning("No target transform specified for AwayFromTarget force direction");
                        finalForce = Vector3.zero;
                    }
                    break;
                    
                case ForceDirection.Custom:
                    finalForce = forceVector;
                    break;
            }
            
            return finalForce;
        }
        
        private void AddForce(Rigidbody rb)
        {
            Vector3 force = CalculateForceVector(rb);
            rb.AddForce(force, forceMode);
            LogDebug($"Added force: {force} to {rb.name}");
        }
        
        private void AddImpulse(Rigidbody rb)
        {
            Vector3 force = CalculateForceVector(rb);
            rb.AddForce(force, ForceMode.Impulse);
            LogDebug($"Added impulse: {force} to {rb.name}");
        }
        
        private void AddExplosion(Rigidbody rb)
        {
            Vector3 explosionCenter = useTransformAsExplosionCenter ? transform.position : explosionPosition;
            
            // Apply explosion force to single rigidbody
            rb.AddExplosionForce(explosionForce, explosionCenter, explosionRadius, upwardsModifier, forceMode);
            
            LogDebug($"Applied explosion force {explosionForce} at {explosionCenter} with radius {explosionRadius} to {rb.name}");
        }
        
        private void SetVelocity(Rigidbody rb)
        {
            if (addToExistingVelocity)
            {
                rb.linearVelocity += velocity;
            }
            else
            {
                rb.linearVelocity = velocity;
            }
            LogDebug($"Set velocity: {rb.linearVelocity} on {rb.name}");
        }
        
        private void SetAngularVelocity(Rigidbody rb)
        {
            if (addToExistingVelocity)
            {
                rb.angularVelocity += angularVelocity;
            }
            else
            {
                rb.angularVelocity = angularVelocity;
            }
            LogDebug($"Set angular velocity: {rb.angularVelocity} on {rb.name}");
        }
        
        private void SetMass(Rigidbody rb)
        {
            rb.mass = mass;
            LogDebug($"Set mass: {mass} on {rb.name}");
        }
        
        private void SetDrag(Rigidbody rb)
        {
            rb.linearDamping = drag;
            LogDebug($"Set drag: {drag} on {rb.name}");
        }
        
        private void SetAngularDrag(Rigidbody rb)
        {
            rb.angularDamping = angularDrag;
            LogDebug($"Set angular drag: {angularDrag} on {rb.name}");
        }
        
        private void SetGravity(Rigidbody rb)
        {
            rb.useGravity = useGravity;
            LogDebug($"Set use gravity: {useGravity} on {rb.name}");
        }
        
        private void SetKinematic(Rigidbody rb)
        {
            rb.isKinematic = isKinematic;
            LogDebug($"Set kinematic: {isKinematic} on {rb.name}");
        }
        
        private void SetConstraints(Rigidbody rb)
        {
            rb.constraints = constraints;
            LogDebug($"Set constraints: {constraints} on {rb.name}");
        }
        
        private void AddTorque(Rigidbody rb)
        {
            Vector3 torque = relativeTorque ? rb.transform.TransformDirection(torqueVector) : torqueVector;
            rb.AddTorque(torque, forceMode);
            LogDebug($"Added torque: {torque} to {rb.name}");
        }
        
        #region Public API
        
        public void SetTargetRigidbody(Rigidbody rb)
        {
            targetRigidbody = rb;
            useContext = false;
        }
        
        public void SetActionType(PhysicsActionType type)
        {
            actionType = type;
        }
        
        public void SetForceVector(Vector3 force)
        {
            forceVector = force;
            forceDirection = ForceDirection.Custom;
        }
        
        public void SetForceMagnitude(float magnitude)
        {
            forceMagnitude = magnitude;
        }
        
        public void SetForceDirection(ForceDirection direction)
        {
            forceDirection = direction;
        }
        
        public void SetExplosionProperties(float force, float radius, Vector3 center)
        {
            explosionForce = force;
            explosionRadius = radius;
            explosionPosition = center;
            useTransformAsExplosionCenter = false;
        }
        
        public void SetVelocity(Vector3 vel, bool additive = false)
        {
            velocity = vel;
            addToExistingVelocity = additive;
        }
        
        public void SetMass(float m)
        {
            mass = Mathf.Max(0.01f, m);
        }
        
        #endregion
        
        #region Area Explosion
        
        /// <summary>
        /// Apply explosion force to all rigidbodies in the explosion radius.
        /// </summary>
        public void ApplyExplosionToArea()
        {
            Vector3 explosionCenter = useTransformAsExplosionCenter ? transform.position : explosionPosition;
            Collider[] colliders = Physics.OverlapSphere(explosionCenter, explosionRadius, explosionLayers);
            
            foreach (var col in colliders)
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, explosionCenter, explosionRadius, upwardsModifier, forceMode);
                }
            }
            
            LogDebug($"Applied area explosion to {colliders.Length} objects");
        }
        
        #endregion
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            forceMagnitude = Mathf.Max(0f, forceMagnitude);
            explosionForce = Mathf.Max(0f, explosionForce);
            explosionRadius = Mathf.Max(0f, explosionRadius);
            mass = Mathf.Max(0.01f, mass);
            drag = Mathf.Max(0f, drag);
            angularDrag = Mathf.Max(0f, angularDrag);
            
            // Ensure force magnitude range is valid
            forceMagnitudeRange.x = Mathf.Max(0f, forceMagnitudeRange.x);
            forceMagnitudeRange.y = Mathf.Max(forceMagnitudeRange.x, forceMagnitudeRange.y);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (actionType == PhysicsActionType.AddExplosion)
            {
                Vector3 center = useTransformAsExplosionCenter ? transform.position : explosionPosition;
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(center, explosionRadius);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(center, 0.1f);
            }
            
            if (forceDirection == ForceDirection.Custom || forceDirection == ForceDirection.WorldSpace)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, forceVector.normalized * 2f);
            }
        }
        #endif
    }
}