using UnityEditor;
using GameFramework.Character;
using GameFramework.Core.Editor;

namespace GameFramework.Character.Editor
{
    [CustomEditor(typeof(FirstPersonCharacterController))]
    public class FirstPersonCharacterControllerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Character;
        protected override string ComponentIcon => "🚶";
        protected override string ComponentName => "FIRST PERSON CHARACTER CONTROLLER";
        protected override int ComponentIconSize => 16; // Important main controller
    }
}