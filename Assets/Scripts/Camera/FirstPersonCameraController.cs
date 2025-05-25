using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Camera
{
    public class FirstPersonCameraController : MonoBehaviour, ICameraController
    {
        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalClampAngle = 80f;
        [SerializeField] private float fieldOfView = 75f;
        [SerializeField] private bool invertYAxis = false;
        [SerializeField] private bool debugInput = false;
        
        [Header("References")]
        [SerializeField] private UnityEngine.Camera playerCamera;
        
        private Transform target;
        private float verticalRotation;
        private float horizontalRotation;

        public UnityEngine.Camera Camera => playerCamera;
        public Transform CameraTransform => playerCamera.transform;

        private void Awake()
        {
            ValidateReferences();
            InitializeCamera();
        }

        private void ValidateReferences()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<UnityEngine.Camera>();
                if (playerCamera == null)
                {
                    Debug.LogError($"FirstPersonCameraController on {gameObject.name} requires a Camera component!");
                }
            }
        }

        private void InitializeCamera()
        {
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = fieldOfView;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void Initialize(Transform cameraTarget)
        {
            SetTarget(cameraTarget);
            
            if (target != null)
            {
                horizontalRotation = target.eulerAngles.y;
                verticalRotation = 0f;
            }
        }

        public void HandleLookInput(Vector2 lookInput)
        {
            if (target == null || playerCamera == null) return;

            if (debugInput && lookInput != Vector2.zero)
            {
                Debug.Log($"Look Input: {lookInput}, Target: {target.name}, MouseSensitivity: {mouseSensitivity}");
            }

            float mouseX = lookInput.x * mouseSensitivity;
            float mouseY = lookInput.y * mouseSensitivity;

            if (invertYAxis)
                mouseY = -mouseY;

            horizontalRotation += mouseX;
            verticalRotation -= mouseY;

            verticalRotation = Mathf.Clamp(verticalRotation, -verticalClampAngle, verticalClampAngle);

            target.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

            if (debugInput && lookInput != Vector2.zero)
            {
                Debug.Log($"Applied Rotation - Horizontal: {horizontalRotation}, Vertical: {verticalRotation}");
            }
        }

        public void SetSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        public void SetFieldOfView(float fov)
        {
            fieldOfView = fov;
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = fieldOfView;
            }
        }

        public void SetTarget(Transform cameraTarget)
        {
            target = cameraTarget;
        }

        public void SetVerticalClamp(float clampAngle)
        {
            verticalClampAngle = clampAngle;
        }

        public void SetInvertYAxis(bool invert)
        {
            invertYAxis = invert;
        }

        private void OnValidate()
        {
            if (playerCamera != null && Application.isPlaying)
            {
                playerCamera.fieldOfView = fieldOfView;
            }
        }
    }
}