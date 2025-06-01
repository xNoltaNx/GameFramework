using UnityEditor;
using GameFramework.Items;
using GameFramework.Core.Editor;

namespace GameFramework.Items.Editor
{
    [CustomEditor(typeof(WorldItem))]
    public class WorldItemEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.ObjectControl;
        protected override string ComponentIcon => "💎";
        protected override string ComponentName => "WORLD ITEM";
        protected override int ComponentIconSize => 12;
    }
}