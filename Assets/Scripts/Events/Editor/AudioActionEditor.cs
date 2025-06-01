using UnityEditor;
using GameFramework.Events.Actions;
using GameFramework.Core.Editor;

namespace GameFramework.Events.Editor
{
    [CustomEditor(typeof(AudioAction))]
    public class AudioActionEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.ObjectControl;
        protected override string ComponentIcon => "🔊";
        protected override string ComponentName => "AUDIO ACTION";
    }
}