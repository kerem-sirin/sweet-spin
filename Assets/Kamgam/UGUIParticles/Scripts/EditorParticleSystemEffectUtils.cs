#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kamgam.UGUIParticles
{
    public static class EditorParticleSystemEffectUtils
    {
        public static Type _editorUtils = typeof(UnityEditor.EditorUtility).Assembly.GetType("UnityEditor.ParticleSystemEditorUtils");
        public static Type _effectUtils = typeof(UnityEditor.EditorUtility).Assembly.GetType("UnityEditor.ParticleSystemEffectUtils");

        public static bool IsSupported()
        {
            return _editorUtils != null && _effectUtils != null;
        }

        public static void Play()
        {
            if (_editorUtils != null)
            {
                var playbackIsScrubbing = _editorUtils.GetProperty("playbackIsScrubbing", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (playbackIsScrubbing != null)
                    playbackIsScrubbing.SetValue(null, false);

                var playbackTime = _editorUtils.GetProperty("playbackTime", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (playbackTime != null)
                    playbackTime.SetValue(null, 0f);
            }

            if (_effectUtils != null)
            {
                var startEffect = _effectUtils.GetMethod("StartEffect", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (startEffect != null)
                    startEffect.Invoke(null, null);
            }
        }

        public static void Stop()
        {
            if (_editorUtils != null)
            {
                var playbackIsScrubbing = _editorUtils.GetProperty("playbackIsScrubbing", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (playbackIsScrubbing != null)
                    playbackIsScrubbing.SetValue(null, false);

                var playbackTime = _editorUtils.GetProperty("playbackTime", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (playbackTime != null)
                    playbackTime.SetValue(null, 0f);
            }

            if (_effectUtils != null)
            {
                var stopEffect = _effectUtils.GetMethod("StopEffect", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (stopEffect != null)
                    stopEffect.Invoke(null, null);
            }
        }
    }
}
#endif


