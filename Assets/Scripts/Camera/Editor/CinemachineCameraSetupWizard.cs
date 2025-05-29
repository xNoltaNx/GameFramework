#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Cinemachine;
using GameFramework.Camera;
using System.IO;

namespace GameFramework.Camera.Editor
{
    /// <summary>
    /// Setup wizard for automatically configuring Cinemachine camera systems.
    /// Provides one-click setup for complete camera management with sensible defaults.
    /// </summary>
    public class CinemachineCameraSetupWizard : EditorWindow
    {
        [MenuItem("GameFramework/Camera/Setup Cinemachine Camera System")]
        public static void ShowWindow()
        {
            var window = GetWindow<CinemachineCameraSetupWizard>("Cinemachine Camera Setup");
            window.minSize = new Vector2(400, 600);
        }

        private GameObject targetPlayer;
        private UnityEngine.Camera mainCamera;
        private bool createCameraProfile = true;
        private CameraProfilePreset selectedPreset = CameraProfilePreset.Standard;
        private bool setupDebugComponents = false;
        private bool createNoiseProfiles = true;
        private string profileName = "DefaultMovementCameraProfile";
        private MovementStateCameraProfile existingProfile;
        
        // Setup options
        private bool useCustomCameraRig = false;
        private bool enablePerformanceOptimizations = true;
        private bool enableAccessibilityFeatures = true;
        
        private Vector2 scrollPosition;

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            DrawPlayerSetup();
            DrawCameraSetup();
            DrawProfileSetup();
            DrawAdvancedOptions();
            DrawSetupButtons();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Cinemachine Camera Setup Wizard", headerStyle);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.HelpBox(
                "This wizard will set up a complete Cinemachine-based camera system for your first-person character. " +
                "It will create virtual cameras for different movement states, configure camera effects, and set up shake systems.",
                MessageType.Info);
                
            EditorGUILayout.Space(10);
        }

        private void DrawPlayerSetup()
        {
            EditorGUILayout.LabelField("Player Setup", EditorStyles.boldLabel);
            
            targetPlayer = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Target Player", "The player GameObject that will have the camera system"),
                targetPlayer, 
                typeof(GameObject), 
                true);
                
            if (targetPlayer == null)
            {
                EditorGUILayout.HelpBox("Please select the player GameObject to set up the camera system.", MessageType.Warning);
                return;
            }
            
            // Check if player already has camera components
            var existingController = targetPlayer.GetComponent<FirstPersonCameraController>();
            if (existingController != null)
            {
                EditorGUILayout.HelpBox("Player already has a FirstPersonCameraController. Setup will modify existing configuration.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
        }

        private void DrawCameraSetup()
        {
            EditorGUILayout.LabelField("Camera Setup", EditorStyles.boldLabel);
            
            mainCamera = (UnityEngine.Camera)EditorGUILayout.ObjectField(
                new GUIContent("Main Camera", "The main camera that will be controlled by Cinemachine"),
                mainCamera,
                typeof(UnityEngine.Camera),
                true);
                
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null)
                {
                    EditorGUILayout.HelpBox("No main camera found. Please assign a camera or tag one as 'MainCamera'.", MessageType.Warning);
                }
            }
            
            useCustomCameraRig = EditorGUILayout.Toggle(
                new GUIContent("Use Custom Camera Rig", "Create a separate camera rig for more advanced control"),
                useCustomCameraRig);
            
            EditorGUILayout.Space();
        }

        private void DrawProfileSetup()
        {
            EditorGUILayout.LabelField("Camera Profile Setup", EditorStyles.boldLabel);
            
            // Check for existing camera profiles
            var existingProfiles = FindExistingCameraProfiles();
            if (existingProfiles.Length > 0 && existingProfile == null)
            {
                existingProfile = existingProfiles[0]; // Auto-select first found profile
                createCameraProfile = false; // Default to using existing profile
            }
            
            // Show existing profile option if any are found
            if (existingProfiles.Length > 0)
            {
                EditorGUILayout.LabelField($"Found {existingProfiles.Length} existing camera profile(s):", EditorStyles.miniBoldLabel);
                existingProfile = (MovementStateCameraProfile)EditorGUILayout.ObjectField(
                    new GUIContent("Use Existing Profile", "Select an existing camera profile to use"),
                    existingProfile,
                    typeof(MovementStateCameraProfile),
                    false);
            }
            
            createCameraProfile = EditorGUILayout.Toggle(
                new GUIContent("Create New Camera Profile", "Create a new MovementStateCameraProfile asset"),
                createCameraProfile);
                
            if (createCameraProfile)
            {
                EditorGUI.indentLevel++;
                
                profileName = EditorGUILayout.TextField(
                    new GUIContent("Profile Name", "Name for the camera profile asset"),
                    profileName);
                
                selectedPreset = (CameraProfilePreset)EditorGUILayout.EnumPopup(
                    new GUIContent("Starting Preset", "Initial configuration preset"),
                    selectedPreset);
                    
                createNoiseProfiles = EditorGUILayout.Toggle(
                    new GUIContent("Create Noise Profiles", "Generate Cinemachine noise profiles for head bob"),
                    createNoiseProfiles);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
        }

        private void DrawAdvancedOptions()
        {
            EditorGUILayout.LabelField("Advanced Options", EditorStyles.boldLabel);
            
            enablePerformanceOptimizations = EditorGUILayout.Toggle(
                new GUIContent("Performance Optimizations", "Enable distance-based performance optimizations"),
                enablePerformanceOptimizations);
                
            enableAccessibilityFeatures = EditorGUILayout.Toggle(
                new GUIContent("Accessibility Features", "Enable accessibility options for camera effects"),
                enableAccessibilityFeatures);
            
            setupDebugComponents = EditorGUILayout.Toggle(
                new GUIContent("Setup Debug Components", "Add debug visualization and logging"),
                setupDebugComponents);
            
            EditorGUILayout.Space();
        }

        private void DrawSetupButtons()
        {
            EditorGUILayout.Space(10);
            
            GUI.enabled = targetPlayer != null && mainCamera != null;
            
            if (GUILayout.Button("Setup Complete Camera System", GUILayout.Height(40)))
            {
                SetupCompleteCameraSystem();
            }
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Setup Basic Components Only"))
            {
                SetupBasicComponents();
            }
            
            if (GUILayout.Button("Create Profile Only"))
            {
                CreateCameraProfile();
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUI.enabled = true;
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Open Documentation"))
            {
                Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@2.8/manual/index.html");
            }
        }

        private void SetupCompleteCameraSystem()
        {
            if (targetPlayer == null || mainCamera == null)
            {
                EditorUtility.DisplayDialog("Setup Error", "Please assign both target player and main camera.", "OK");
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Setting up Camera System", "Initializing...", 0f);
                
                // Step 1: Setup basic components
                SetupBasicComponents();
                EditorUtility.DisplayProgressBar("Setting up Camera System", "Setting up components...", 0.2f);
                
                // Step 2: Get or create camera profile
                MovementStateCameraProfile profile = null;
                if (createCameraProfile)
                {
                    profile = CreateCameraProfile();
                    EditorUtility.DisplayProgressBar("Setting up Camera System", "Creating camera profile...", 0.4f);
                }
                else if (existingProfile != null)
                {
                    profile = existingProfile;
                    EditorUtility.DisplayProgressBar("Setting up Camera System", "Using existing camera profile...", 0.4f);
                }
                else
                {
                    EditorUtility.DisplayProgressBar("Setting up Camera System", "No camera profile assigned...", 0.4f);
                }
                
                // Step 3: Setup virtual cameras
                SetupVirtualCameras();
                EditorUtility.DisplayProgressBar("Setting up Camera System", "Setting up virtual cameras...", 0.6f);
                
                // Step 4: Configure camera manager
                ConfigureCameraManager(profile);
                EditorUtility.DisplayProgressBar("Setting up Camera System", "Configuring camera manager...", 0.8f);
                
                // Step 5: Final configuration
                FinalizeSetup();
                EditorUtility.DisplayProgressBar("Setting up Camera System", "Finalizing setup...", 1f);
                
                EditorUtility.ClearProgressBar();
                
                EditorUtility.DisplayDialog("Setup Complete", 
                    "Cinemachine camera system has been set up successfully!\n\n" +
                    "The system includes:\n" +
                    "• Virtual cameras for all movement states\n" +
                    "• Camera shake system\n" +
                    "• Movement-responsive effects\n" +
                    "• Configurable camera profile\n\n" +
                    "You can now customize the camera behavior using the Camera Profile asset.", 
                    "OK");
                    
                Close();
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Setup Error", $"An error occurred during setup:\n{e.Message}", "OK");
                Debug.LogError($"[CinemachineCameraSetupWizard] Setup failed: {e}");
            }
        }

        private void SetupBasicComponents()
        {
            // Add or get FirstPersonCameraController
            var controller = targetPlayer.GetComponent<FirstPersonCameraController>();
            if (controller == null)
            {
                controller = targetPlayer.AddComponent<FirstPersonCameraController>();
            }
            
            // Add Cinemachine Brain to camera
            var brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
                brain.DefaultBlend.Time = 0.5f;
                brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
            }
            
            // Add camera manager
            var manager = targetPlayer.GetComponent<CinemachineCameraManager>();
            if (manager == null)
            {
                manager = targetPlayer.AddComponent<CinemachineCameraManager>();
            }
            
            // Ensure auto-create cameras is enabled for setup
            var autoCreateField = typeof(CinemachineCameraManager).GetField("autoCreateCameras", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (autoCreateField != null)
            {
                autoCreateField.SetValue(manager, true);
            }
            
            // Create vCameras GameObject as child of player for proper organization
            Transform vCamerasTransform = targetPlayer.transform.Find("vCameras");
            GameObject vCamerasGO;
            if (vCamerasTransform == null)
            {
                vCamerasGO = new GameObject("vCameras");
                vCamerasGO.transform.SetParent(targetPlayer.transform);
                vCamerasGO.transform.localPosition = Vector3.zero;
                vCamerasGO.transform.localRotation = Quaternion.identity;
            }
            else
            {
                vCamerasGO = vCamerasTransform.gameObject;
            }
            
            // Set virtual camera parent to vCameras GameObject
            var field = typeof(CinemachineCameraManager).GetField("virtualCameraParent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(manager, vCamerasGO.transform);
            }
            
            // Add shake manager
            var shakeManager = targetPlayer.GetComponent<CameraShakeManager>();
            if (shakeManager == null)
            {
                shakeManager = targetPlayer.AddComponent<CameraShakeManager>();
            }
            
            // Create camera rig if needed
            if (useCustomCameraRig)
            {
                CreateCameraRig();
            }
            
            // Configure camera manager targets
            ConfigureCameraTargets(manager);
            
            // Ensure the main camera reference is set
            SetMainCameraReference(manager);
        }

        private void CreateCameraRig()
        {
            var rigObject = new GameObject("CameraRig");
            rigObject.transform.SetParent(targetPlayer.transform);
            
            // Use actual camera position if available, otherwise use eye level height
            if (mainCamera != null)
            {
                rigObject.transform.position = mainCamera.transform.position;
                rigObject.transform.rotation = mainCamera.transform.rotation;
            }
            else
            {
                rigObject.transform.localPosition = new Vector3(0, 1.6f, 0); // Eye level height
                rigObject.transform.localRotation = Quaternion.identity;
            }
        }

        private void ConfigureCameraTargets(CinemachineCameraManager manager)
        {
            if (manager == null) return;

            // Find or create camera rig for follow target
            Transform cameraRig = targetPlayer.transform.Find("CameraRig");
            
            // If no camera rig exists, use the main camera position
            Transform followTarget = null;
            if (cameraRig != null)
            {
                followTarget = cameraRig;
            }
            else if (mainCamera != null)
            {
                // Create a simple follow target at camera position
                GameObject followGO = new GameObject("CameraFollowTarget");
                followGO.transform.SetParent(targetPlayer.transform);
                followGO.transform.position = mainCamera.transform.position;
                followTarget = followGO.transform;
            }

            if (followTarget != null)
            {
                // Use reflection to set the followTarget field
                var followField = typeof(CinemachineCameraManager).GetField("followTarget", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (followField != null)
                {
                    followField.SetValue(manager, followTarget);
                }

                // For first-person camera, we typically don't need a separate look target
                // Set lookAtTarget to null (first-person cameras usually don't need it)
                var lookField = typeof(CinemachineCameraManager).GetField("lookAtTarget", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (lookField != null)
                {
                    lookField.SetValue(manager, null); // First-person cameras typically don't need look targets
                }

                Debug.Log($"[CinemachineCameraSetupWizard] Configured camera targets - Follow: {followTarget.name}");
            }
        }

        private void SetMainCameraReference(CinemachineCameraManager manager)
        {
            if (manager == null || mainCamera == null) return;

            // Use reflection to set the mainCamera field
            var mainCameraField = typeof(CinemachineCameraManager).GetField("mainCamera", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mainCameraField != null)
            {
                mainCameraField.SetValue(manager, mainCamera);
                Debug.Log($"[CinemachineCameraSetupWizard] Set main camera reference: {mainCamera.name} at position {mainCamera.transform.position}");
            }
        }

        private MovementStateCameraProfile CreateCameraProfile()
        {
            string assetPath = $"Assets/Content/Camera/{profileName}.asset";
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Create profile asset
            var profile = ScriptableObject.CreateInstance<MovementStateCameraProfile>();
            
            // Apply preset
            profile.ApplyPreset(selectedPreset);
            
            // Create noise profiles if requested
            if (createNoiseProfiles)
            {
                CreateAndAssignNoiseProfiles(profile);
            }
            
            AssetDatabase.CreateAsset(profile, assetPath);
            AssetDatabase.SaveAssets();
            
            return profile;
        }

        private void CreateAndAssignNoiseProfiles(MovementStateCameraProfile profile)
        {
            // Create noise profiles for different movement states
            string noiseDirectory = "Assets/Content/Camera/NoiseProfiles/";
            if (!Directory.Exists(noiseDirectory))
            {
                Directory.CreateDirectory(noiseDirectory);
            }
            
            // Walking noise profile
            var walkingNoise = CreateBasicNoiseProfile("Walking_HeadBob", 1f, 0.5f);
            AssetDatabase.CreateAsset(walkingNoise, $"{noiseDirectory}Walking_HeadBob.asset");
            profile.WalkingState.noiseSettings.noiseProfile = walkingNoise;
            
            // Sprinting noise profile
            var sprintingNoise = CreateBasicNoiseProfile("Sprinting_HeadBob", 1.5f, 0.7f);
            AssetDatabase.CreateAsset(sprintingNoise, $"{noiseDirectory}Sprinting_HeadBob.asset");
            profile.SprintingState.noiseSettings.noiseProfile = sprintingNoise;
            
            // Crouching noise profile
            var crouchingNoise = CreateBasicNoiseProfile("Crouching_HeadBob", 0.5f, 0.3f);
            AssetDatabase.CreateAsset(crouchingNoise, $"{noiseDirectory}Crouching_HeadBob.asset");
            profile.CrouchingState.noiseSettings.noiseProfile = crouchingNoise;
        }

        private NoiseSettings CreateBasicNoiseProfile(string profileName, float frequency, float amplitude)
        {
            var noise = ScriptableObject.CreateInstance<NoiseSettings>();
            
            // Configure basic noise parameters
            // Note: Actual NoiseSettings configuration would depend on Cinemachine version
            // This is a simplified setup
            
            return noise;
        }

        private void SetupVirtualCameras()
        {
            var manager = targetPlayer.GetComponent<CinemachineCameraManager>();
            if (manager != null)
            {
                // Trigger auto-creation and configuration of virtual cameras
                manager.ConfigureCameras();
            }
        }

        private void ConfigureCameraManager(MovementStateCameraProfile profile)
        {
            var manager = targetPlayer.GetComponent<CinemachineCameraManager>();
            if (manager != null && profile != null)
            {
                // Use reflection to set the cameraProfile field
                var field = typeof(CinemachineCameraManager).GetField("cameraProfile", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(manager, profile);
                    EditorUtility.SetDirty(manager);
                    Debug.Log($"[CinemachineCameraSetupWizard] Assigned camera profile: {profile.name}");
                }
                
                // Configure performance settings would be done here
                // Note: Performance settings are configured through the camera profile
            }
        }

        private MovementStateCameraProfile[] FindExistingCameraProfiles()
        {
            // Search for MovementStateCameraProfile assets in the project
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(MovementStateCameraProfile)}");
            var profiles = new MovementStateCameraProfile[guids.Length];
            
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                profiles[i] = AssetDatabase.LoadAssetAtPath<MovementStateCameraProfile>(path);
            }
            
            return profiles;
        }

        private void FinalizeSetup()
        {
            // Mark scene as dirty
            EditorUtility.SetDirty(targetPlayer);
            if (mainCamera != null)
            {
                EditorUtility.SetDirty(mainCamera.gameObject);
            }
            
            // Focus on the player object
            Selection.activeGameObject = targetPlayer;
            
            // Save the scene
            if (EditorUtility.DisplayDialog("Save Scene", "Would you like to save the scene with the new camera setup?", "Yes", "No"))
            {
                EditorSceneManager.SaveOpenScenes();
            }
        }
    }

    /// <summary>
    /// Custom property drawer for better UI in the setup wizard
    /// </summary>
    [CustomPropertyDrawer(typeof(CameraProfilePreset))]
    public class CameraProfilePresetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var presetValue = (CameraProfilePreset)property.enumValueIndex;
            var newValue = (CameraProfilePreset)EditorGUI.EnumPopup(position, label, presetValue);
            
            if (newValue != presetValue)
            {
                property.enumValueIndex = (int)newValue;
            }
            
            EditorGUI.EndProperty();
        }
    }
}
#endif