using UnityEditor;
using GameFramework.Items;
using GameFramework.Core.Editor;

namespace GameFramework.Items.Editor
{
    [CustomEditor(typeof(InventoryController))]
    public class InventoryControllerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.System;
        protected override string ComponentIcon => "📦";
        protected override string ComponentName => "INVENTORY CONTROLLER";
        protected override int ComponentIconSize => 14;
    }
}