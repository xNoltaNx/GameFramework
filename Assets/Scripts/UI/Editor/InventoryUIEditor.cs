using UnityEditor;
using GameFramework.UI;
using GameFramework.Core.Editor;

namespace GameFramework.UI.Editor
{
    [CustomEditor(typeof(InventoryUI))]
    public class InventoryUIEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.UI;
        protected override string ComponentIcon => "🎒";
        protected override string ComponentName => "INVENTORY UI";
        protected override int ComponentIconSize => 14;
    }
}