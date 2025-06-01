using UnityEditor;
using GameFramework.Events.Listeners;
using GameFramework.Core.Editor;

namespace GameFramework.Events.Editor
{
    [CustomEditor(typeof(GameEventListener))]
    public class GameEventListenerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Event;
        protected override string ComponentIcon => "🎧";
        protected override string ComponentName => "GAME EVENT LISTENER";
    }
}