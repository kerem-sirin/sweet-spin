#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Kamgam.UGUIParticles
{
    [CustomEditor(typeof(ParticleImage))]
    [CanEditMultipleObjects]
    public class ParticleImageEditor : UnityEditor.Editor
    {
        ParticleImage obj;

        public void OnEnable()
        {
            obj = target as ParticleImage;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Go to Particle System"))
            {
                var ps = obj.ParticleSystemForImage;
                Selection.objects = new GameObject[] { ps.gameObject };
            }

            GUILayout.BeginHorizontal();
            if (EditorParticleSystemEffectUtils.IsSupported())
            {
                if (GUILayout.Button("Play"))
                {
                    StartPlaying(obj);
                }
                if (GUILayout.Button("Stop"))
                {
                    StopPlaying(obj, returnToImage: true);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            base.OnInspectorGUI();

            GUI.enabled = true;
        }

        public static void StartPlaying(ParticleImage obj)
        {
            EditorApplication.delayCall += () =>
            {
                Selection.objects = new GameObject[] { obj.GetComponentInChildren<ParticleSystem>().gameObject };
                EditorScheduler.Schedule(0.1f, () =>
                {
                    EditorParticleSystemEffectUtils.Play();
                    obj.Play();
                    obj.MarkDirtyRepaint();
                }, "ParticleImageEditor.Play");
            };
        }

        public static void StopPlaying(ParticleImage obj, bool returnToImage)
        {
            EditorApplication.delayCall += () =>
            {
                Selection.objects = new GameObject[] { obj.GetComponentInChildren<ParticleSystem>().gameObject };
                // Sadly this code only works is the particle system is selected. Thus -^
                EditorScheduler.Schedule(0.1f, () =>
                {
                    EditorParticleSystemEffectUtils.Stop();
                    obj.Stop(withChildren: true, stopBehaviour: ParticleSystemStopBehavior.StopEmittingAndClear);
                    obj.MarkDirtyRepaint();

                    if (returnToImage)
                    {
                        EditorScheduler.Schedule(0.1f, () =>
                        {
                            Selection.objects = new GameObject[] { obj.gameObject };
                        }, "ParticleImageEditor.ReturnToObject");
                    }
                }, "ParticleImageEditor.Stop");
            };
        }
    }
}
#endif