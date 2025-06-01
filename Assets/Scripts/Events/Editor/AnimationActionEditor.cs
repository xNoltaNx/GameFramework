using UnityEditor;
using GameFramework.Events.Actions;
using GameFramework.Core.Editor;

namespace GameFramework.Events.Editor
{
    [CustomEditor(typeof(AnimationAction))]
    public class AnimationActionEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.ObjectControl;
        protected override string ComponentIcon => "🎬";
        protected override string ComponentName => "ANIMATION ACTION";
    }
}