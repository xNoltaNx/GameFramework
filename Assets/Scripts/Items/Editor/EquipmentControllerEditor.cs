using UnityEditor;
using GameFramework.Items;
using GameFramework.Core.Editor;

namespace GameFramework.Items.Editor
{
    [CustomEditor(typeof(EquipmentController))]
    public class EquipmentControllerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.System;
        protected override string ComponentIcon => "⚔️";
        protected override string ComponentName => "EQUIPMENT CONTROLLER";
        protected override int ComponentIconSize => 14;
    }
}