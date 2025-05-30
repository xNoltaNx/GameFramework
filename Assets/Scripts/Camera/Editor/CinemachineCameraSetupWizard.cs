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
                EditorUtility.DisplayProgressBar("Setting up Camera System", "Configuring camera manager...", 0.7f);
                
                // Step 5: Create and setup custom blender
                SetupCustomBlender(profile);
                EditorUtility.DisplayProgressBar("Setting up Camera System", "Setting up camera blends...", 0.8f);
                
                // Step 6: Final configuration
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
            if (manager == null)
            {
                Debug.LogError("[CinemachineCameraSetupWizard] No CinemachineCameraManager found");
                return;
            }

            // Create virtual cameras directly in the wizard instead of relying on runtime creation
            CreateVirtualCamerasInWizard(manager);
            
            // Also trigger the manager's configuration for any additional setup
            manager.ConfigureCameras();
        }

        private void CreateVirtualCamerasInWizard(CinemachineCameraManager manager)
        {
            // Create virtual camera parent if it doesn't exist
            Transform vcamParent = targetPlayer.transform.Find("vCameras");
            if (vcamParent == null)
            {
                GameObject vcamParentGO = new GameObject("vCameras");
                vcamParentGO.transform.SetParent(targetPlayer.transform);
                vcamParent = vcamParentGO.transform;
                
                // Set the virtualCameraParent field using reflection
                var parentField = typeof(CinemachineCameraManager).GetField("virtualCameraParent", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (parentField != null)
                {
                    parentField.SetValue(manager, vcamParent);
                }
            }

            // Create each virtual camera if it doesn't exist
            CreateOrUpdateVirtualCamera(manager, "standingCamera", "vcam_Standing", vcamParent);
            CreateOrUpdateVirtualCamera(manager, "walkingCamera", "vcam_Walking", vcamParent);
            CreateOrUpdateVirtualCamera(manager, "sprintingCamera", "vcam_Sprinting", vcamParent);
            CreateOrUpdateVirtualCamera(manager, "crouchingCamera", "vcam_Crouching", vcamParent);
            CreateOrUpdateVirtualCamera(manager, "slidingCamera", "vcam_Sliding", vcamParent);
            CreateOrUpdateVirtualCamera(manager, "airborneCamera", "vcam_Airborne", vcamParent);

            EditorUtility.SetDirty(manager);
            Debug.Log("[CinemachineCameraSetupWizard] Created virtual cameras in wizard");
        }

        private void CreateOrUpdateVirtualCamera(CinemachineCameraManager manager, string fieldName, string cameraName, Transform parent)
        {
            // Check if camera already exists in the field
            var field = typeof(CinemachineCameraManager).GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var existingCamera = field?.GetValue(manager) as CinemachineCamera;
            
            if (existingCamera == null)
            {
                // Create new virtual camera
                GameObject cameraGO = new GameObject(cameraName);
                cameraGO.transform.SetParent(parent);
                
                // Position at main camera location
                if (mainCamera != null)
                {
                    cameraGO.transform.position = mainCamera.transform.position;
                    cameraGO.transform.rotation = mainCamera.transform.rotation;
                }
                
                // Add CinemachineCamera component
                var vcam = cameraGO.AddComponent<CinemachineCamera>();
                vcam.Priority = 0; // Start with low priority
                
                // Add noise component
                var noise = cameraGO.AddComponent<CinemachineBasicMultiChannelPerlin>();
                
                // Set follow target
                Transform followTarget = GetFollowTarget();
                if (followTarget != null)
                {
                    vcam.Follow = followTarget;
                }
                
                // Assign to manager field
                field?.SetValue(manager, vcam);
                
                Debug.Log($"[CinemachineCameraSetupWizard] Created virtual camera: {cameraName}");
            }
            else
            {
                Debug.Log($"[CinemachineCameraSetupWizard] Virtual camera already exists: {existingCamera.name}");
            }
        }

        private Transform GetFollowTarget()
        {
            // Look for existing camera rig or create follow target
            Transform cameraRig = targetPlayer.transform.Find("CameraRig");
            if (cameraRig != null)
            {
                return cameraRig;
            }
            
            // Look for existing follow target
            Transform followTarget = targetPlayer.transform.Find("CameraFollowTarget");
            if (followTarget != null)
            {
                return followTarget;
            }
            
            // Create new follow target at main camera position
            if (mainCamera != null)
            {
                GameObject followGO = new GameObject("CameraFollowTarget");
                followGO.transform.SetParent(targetPlayer.transform);
                followGO.transform.position = mainCamera.transform.position;
                followGO.transform.rotation = mainCamera.transform.rotation;
                return followGO.transform;
            }
            
            return null;
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

        private void SetupCustomBlender(MovementStateCameraProfile profile)
        {
            if (profile == null || mainCamera == null)
            {
                Debug.LogWarning("[CinemachineCameraSetupWizard] Cannot setup custom blender - missing profile or main camera");
                return;
            }

            // Get the CinemachineBrain
            var brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                Debug.LogWarning("[CinemachineCameraSetupWizard] No CinemachineBrain found on main camera");
                return;
            }

            // Create the blender settings asset
            string blenderPath = $"Assets/Content/Camera/{profileName}_BlenderSettings.asset";
            var blenderSettings = CreateCustomBlenderSettings(profile, blenderPath);
            
            if (blenderSettings != null)
            {
                // Assign to the brain
                brain.CustomBlends = blenderSettings;
                EditorUtility.SetDirty(brain);
                
                Debug.Log($"[CinemachineCameraSetupWizard] Created and assigned custom blender: {blenderPath}");
            }
        }

        private CinemachineBlenderSettings CreateCustomBlenderSettings(MovementStateCameraProfile profile, string assetPath)
        {
            var blenderSettings = ScriptableObject.CreateInstance<CinemachineBlenderSettings>();
            
            // Get the camera manager to access virtual cameras
            var manager = targetPlayer.GetComponent<CinemachineCameraManager>();
            if (manager == null)
            {
                Debug.LogError("[CinemachineCameraSetupWizard] No CinemachineCameraManager found");
                return null;
            }

            // Get virtual cameras using reflection (since they're private fields)
            var standingCamera = GetVirtualCamera(manager, "standingCamera");
            var walkingCamera = GetVirtualCamera(manager, "walkingCamera");
            var sprintingCamera = GetVirtualCamera(manager, "sprintingCamera");
            var crouchingCamera = GetVirtualCamera(manager, "crouchingCamera");
            var slidingCamera = GetVirtualCamera(manager, "slidingCamera");
            var airborneCamera = GetVirtualCamera(manager, "airborneCamera");

            var blendList = new System.Collections.Generic.List<CinemachineBlenderSettings.CustomBlend>();

            // Define all the camera combinations and their blend times
            var cameraMap = new System.Collections.Generic.Dictionary<string, (CinemachineCamera camera, float blendTime)>
            {
                { "Standing", (standingCamera, profile.BlendSettings.toStanding) },
                { "Walking", (walkingCamera, profile.BlendSettings.toWalking) },
                { "Sprinting", (sprintingCamera, profile.BlendSettings.toSprinting) },
                { "Crouching", (crouchingCamera, profile.BlendSettings.toCrouching) },
                { "Sliding", (slidingCamera, profile.BlendSettings.toSliding) },
                { "Airborne", (airborneCamera, profile.BlendSettings.toAirborne) }
            };

            // Create blend instructions for all camera transitions
            foreach (var fromCamera in cameraMap)
            {
                foreach (var toCamera in cameraMap)
                {
                    if (fromCamera.Key == toCamera.Key) continue; // Skip same camera
                    if (fromCamera.Value.camera == null || toCamera.Value.camera == null) continue;

                    var customBlend = new CinemachineBlenderSettings.CustomBlend
                    {
                        From = fromCamera.Value.camera.name,
                        To = toCamera.Value.camera.name,
                        Blend = new CinemachineBlendDefinition(
                            profile.BlendSettings.blendStyle,
                            toCamera.Value.blendTime
                        )
                    };
                    blendList.Add(customBlend);
                }
            }

            // Use reflection to set the CustomBlends array
            var customBlendsField = typeof(CinemachineBlenderSettings).GetField("m_CustomBlends", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (customBlendsField != null)
            {
                customBlendsField.SetValue(blenderSettings, blendList.ToArray());
            }

            // Create asset
            AssetDatabase.CreateAsset(blenderSettings, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CinemachineCameraSetupWizard] Created custom blender settings with {blendList.Count} blend instructions");
            return blenderSettings;
        }

        private CinemachineCamera GetVirtualCamera(CinemachineCameraManager manager, string fieldName)
        {
            var field = typeof(CinemachineCameraManager).GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(manager) as CinemachineCamera;
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