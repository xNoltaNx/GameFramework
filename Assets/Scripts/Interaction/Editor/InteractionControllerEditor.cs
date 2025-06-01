using UnityEditor;
using GameFramework.Interaction;
using GameFramework.Core.Editor;

namespace GameFramework.Interaction.Editor
{
    [CustomEditor(typeof(InteractionController))]
    public class InteractionControllerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.System;
        protected override string ComponentIcon => "🤝";
        protected override string ComponentName => "INTERACTION CONTROLLER";
        protected override int ComponentIconSize => 12;
    }
}