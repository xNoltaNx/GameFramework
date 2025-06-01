using UnityEditor;
using GameFramework.Camera;
using GameFramework.Core.Editor;

namespace GameFramework.Camera.Editor
{
    [CustomEditor(typeof(CinemachineCameraManager))]
    public class CinemachineCameraManagerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Camera;
        protected override string ComponentIcon => "🎬";
        protected override string ComponentName => "CINEMACHINE CAMERA MANAGER";
        protected override int ComponentIconSize => 14;
    }
}