using UnityEditor;
using GameFramework.Events.Actions;
using GameFramework.Core.Editor;

namespace GameFramework.Events.Editor
{
    [CustomEditor(typeof(RaiseGameEventAction))]
    public class RaiseGameEventActionEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Collision;
        protected override string ComponentIcon => "🚀";
        protected override string ComponentName => "RAISE GAME EVENT ACTION";
    }
}