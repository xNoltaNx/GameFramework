using UnityEditor;
using GameFramework.Locomotion;
using GameFramework.Core.Editor;

namespace GameFramework.Locomotion.Editor
{
    [CustomEditor(typeof(FirstPersonLocomotionController))]
    public class FirstPersonLocomotionControllerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Character;
        protected override string ComponentIcon => "🏃";
        protected override string ComponentName => "FIRST PERSON LOCOMOTION CONTROLLER";
        protected override int ComponentIconSize => 14;
    }
}