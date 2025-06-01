using UnityEditor;
using GameFramework.Events.Actions;
using GameFramework.Core.Editor;

namespace GameFramework.Events.Editor
{
    [CustomEditor(typeof(ParticleAction))]
    public class ParticleActionEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.ObjectControl;
        protected override string ComponentIcon => "✨";
        protected override string ComponentName => "PARTICLE ACTION";
    }
}