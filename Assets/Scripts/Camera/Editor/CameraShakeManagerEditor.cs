using UnityEditor;
using GameFramework.Camera;
using GameFramework.Core.Editor;

namespace GameFramework.Camera.Editor
{
    [CustomEditor(typeof(CameraShakeManager))]
    public class CameraShakeManagerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Camera;
        protected override string ComponentIcon => "📳";
        protected override string ComponentName => "CAMERA SHAKE MANAGER";
        protected override int ComponentIconSize => 12;
    }
}