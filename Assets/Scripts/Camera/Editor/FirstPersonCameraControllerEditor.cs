using UnityEditor;
using GameFramework.Camera;
using GameFramework.Core.Editor;

namespace GameFramework.Camera.Editor
{
    [CustomEditor(typeof(FirstPersonCameraController))]
    public class FirstPersonCameraControllerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Camera;
        protected override string ComponentIcon => "📷";
        protected override string ComponentName => "FIRST PERSON CAMERA CONTROLLER";
        protected override int ComponentIconSize => 14;
    }
}