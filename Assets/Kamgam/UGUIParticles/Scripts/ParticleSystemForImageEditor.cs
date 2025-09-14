#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Kamgam.UGUIParticles
{
    [UnityEditor.CustomEditor(typeof(ParticleSystemForImage))]
    public class PropertyEditor : UnityEditor.Editor
    {
        ParticleSystemForImage obj;

        public void OnEnable()
        {
            obj = target as ParticleSystemForImage;

            obj.ParticleImage.canvasRenderer.SetTexture(obj.Texture);
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Back to Image"))
            {
                var ps = obj.gameObject.GetComponentInParent<ParticleImage>();
                Selection.objects = new GameObject[] { ps.gameObject };
            }

            GUILayout.BeginHorizontal();
            if (EditorParticleSystemEffectUtils.IsSupported())
            {
                if (GUILayout.Button("Play"))
                {
                    EditorParticleSystemEffectUtils.Play();
                    obj.Play();
                    obj.ParticleImage.MarkDirtyRepaint();
                }
                if (GUILayout.Button("Stop"))
                {
                    EditorParticleSystemEffectUtils.Stop();
                    obj.Stop();
                    obj.ParticleImage.MarkDirtyRepaint();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            bool oldUseAttractor = obj.UseAttractor;
            base.OnInspectorGUI();
            if (oldUseAttractor != obj.UseAttractor)
                obj.OnUseAttractorChanged(obj.UseAttractor);

            GUILayout.Space(10);

            GUILayout.Label(new GUIContent("Image Properties"));

            var oldMaterial = obj.Material;
            var newMaterial = (Material)EditorGUILayout.ObjectField("Material", obj.Material, typeof(Material), allowSceneObjects: false);
            if (newMaterial == obj.ParticleImage.defaultMaterial)
            {
                obj.Material = null;
            }
            else
            {
                obj.Material = newMaterial;
            }
            if (oldMaterial != obj.Material)
                EditorUtility.SetDirty(obj.ParticleImage);

            var oldColor = obj.Color;
            obj.Color = EditorGUILayout.ColorField("Color", obj.Color);
            if (oldColor != obj.Color)
                EditorUtility.SetDirty(obj.ParticleImage);
        }
    }
}
#endif
