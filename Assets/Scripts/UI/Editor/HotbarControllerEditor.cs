using UnityEditor;
using GameFramework.UI;
using GameFramework.Core.Editor;

namespace GameFramework.UI.Editor
{
    [CustomEditor(typeof(HotbarController))]
    public class HotbarControllerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.UI;
        protected override string ComponentIcon => "🔥";
        protected override string ComponentName => "HOTBAR CONTROLLER";
        protected override int ComponentIconSize => 12;
    }
}