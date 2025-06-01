#if UNITY_EDITOR
using UnityEditor;
using GameFramework.Events.Templates;
using GameFramework.Core.Editor;

namespace GameFramework.Events.Templates.Editor
{
    [CustomEditor(typeof(TriggerResponseTemplate))]
    public class TriggerResponseTemplateEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.System;
        protected override string ComponentIcon => "🎯";
        protected override string ComponentName => "TRIGGER RESPONSE TEMPLATE";
        protected override int ComponentIconSize => 16;
    }
}
#endif