using UnityEditor;
using GameFramework.Input;
using GameFramework.Core.Editor;

namespace GameFramework.Input.Editor
{
    [CustomEditor(typeof(PlayerInputHandler))]
    public class PlayerInputHandlerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.System;
        protected override string ComponentIcon => "🎮";
        protected override string ComponentName => "PLAYER INPUT HANDLER";
        protected override int ComponentIconSize => 14;
    }
}