#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Kamgam.UGUIParticles
{
    public partial class ParticleImage
    {
        [MenuItem("GameObject/UI/Particle Image (uGUI)", false, 2009)]
        public static void AddParticleImageToSelection()
        {
            ParticleImage lastImage = null;

            // Create one for reach selected game object.
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                var go = new GameObject("Particle Image", typeof(RectTransform), typeof(CanvasRenderer));
                var image = go.AddComponent<ParticleImage>();
                go.transform.SetParent(Selection.gameObjects[i].transform);
                go.transform.localPosition = Vector3.zero;
                var rectTransform = go.transform as RectTransform;
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.localScale = Vector3.one;

                image.raycastTarget = false;

                lastImage = image;
            }

            if (lastImage != null)
            {
                Selection.objects = new GameObject[] { lastImage.gameObject };
            }
        }

        [MenuItem("GameObject/UI/Particle Image (uGUI)", true, 2009)]
        public static bool AddParticleImageToSelectionValidation()
        {
            return Selection.count > 0 && Selection.gameObjects[0].GetComponent<RectTransform>() != null;
        }
    }
}
#endif