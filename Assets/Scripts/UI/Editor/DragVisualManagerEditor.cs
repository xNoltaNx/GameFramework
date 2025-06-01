using UnityEditor;
using GameFramework.UI;
using GameFramework.Core.Editor;

namespace GameFramework.UI.Editor
{
    [CustomEditor(typeof(DragVisualManager))]
    public class DragVisualManagerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.UI;
        protected override string ComponentIcon => "👆";
        protected override string ComponentName => "DRAG VISUAL MANAGER";
        protected override int ComponentIconSize => 10;
    }
}