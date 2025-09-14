using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kamgam.UGUIParticles
{
    public partial class ParticleSystemForImage // .Attractor
    {
        [System.NonSerialized]
        protected ParticleSystemForceField _particleSystemForceField;
        public ParticleSystemForceField ParticleSystemForceField
        {
            get
            {
                if (ParticleSystem == null)
                    return null;

                if (_particleSystemForceField == null)
                {
                    _particleSystemForceField = ParticleSystem.gameObject.GetComponentInChildren<ParticleSystemForceField>(includeInactive: true);
                }

                return _particleSystemForceField;
            }
        }

        public void OnUseAttractorChanged(bool value)
        {
            if (value)
                EnableAttractor();
            else
                DisableAttractor();

            ParticleImage?.MarkDirtyRepaint();
        }

        public void EnableAttractor()
        {
            if (ParticleSystem == null)
                return;

            // Find or create force field
            var forceField = ParticleSystem.gameObject.GetComponentInChildren<ParticleSystemForceField>(includeInactive: true);

            var forces = ParticleSystem.externalForces;
            forces.enabled = true;
            forces.influenceFilter = ParticleSystemGameObjectFilter.List;
            if (forces.influenceCount == 0 || forces.GetInfluence(0) == null)
            {
                if (forceField == null)
                {
                    var go = new GameObject("Particle Attractor ForceField");
                    go.SetActive(false);
                    go.transform.parent = ParticleSystem.gameObject.transform;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localPosition = new Vector3(0f, 0f, 0f);
                    go.transform.localScale = Vector3.one;
                    forceField = go.AddComponent<ParticleSystemForceField>();
                    forceField.gravity = 1f;
                    forceField.drag = 0.1f;
                    forceField.endRange = 1000f;
                    forceField.multiplyDragByParticleSize = false;
                    go.SetActive(true);
                }
                if (forces.influenceCount > 0)
                    forces.SetInfluence(0, forceField);
                else
                    forces.AddInfluence(forceField);
            }

            if (forceField != null)
                forceField.gameObject.SetActive(true);
        }

        public void DisableAttractor()
        {
            if (ParticleSystem == null)
                return;

            var forces = ParticleSystem.externalForces;
            forces.enabled = false;

            var forceField = ParticleSystem.gameObject.GetComponentInChildren<ParticleSystemForceField>(includeInactive: true);
            if (forceField != null)
            {
                forceField.gameObject.SetActive(false);
            }
        }

        RectTransform _attractorRect;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originRelativeToBottomLeft">The origin of the particles relative to the bottom left of the ParticleImage rect.</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void UpdateAttractorPosition(Vector3 originRelativeToBottomLeft, float width, float height)
        {
            if (UseAttractor && Attractor == null)
            {
                DisableAttractor();
                return;
            }
            else if(UseAttractor && Attractor && !ParticleSystemForceField.gameObject.activeSelf)
            {
                EnableAttractor();
            }

            if (!UseAttractor || ParticleImage == null || ParticleSystem == null || ParticleSystemForceField == null || Attractor == null)
                return;

            if (_attractorRect != Attractor)
            {
                if (Attractor != null)
                    _attractorRect = Attractor as RectTransform;
                else
                    _attractorRect = null;
            }

            Vector3 attractorDeltaToBottomLeft;
            if (_attractorRect != null)
            {
                attractorDeltaToBottomLeft = ParticleImage.WorldSpaceToUISpace(_attractorRect.TransformPoint(_attractorRect.rect.center), worldPosFromRect: true, ParticleImage.RectTransform, ParticleImage.GetRenderMode());
            }
            else
            {
                attractorDeltaToBottomLeft = ParticleImage.WorldSpaceToUISpace(Attractor, ParticleImage.RectTransform, ParticleImage.GetRenderMode());
            }

            Vector3 pixelsPerUnitScaled = new Vector3(
                PixelsPerUnit * ParticleImage.transform.lossyScale.x,
                PixelsPerUnit * ParticleImage.transform.lossyScale.y,
                PixelsPerUnit * ParticleImage.transform.lossyScale.z
            );
            Vector3 deltaInWorldSpace = (attractorDeltaToBottomLeft - originRelativeToBottomLeft);
            deltaInWorldSpace.x /= pixelsPerUnitScaled.x;
            deltaInWorldSpace.y /= pixelsPerUnitScaled.y;
            deltaInWorldSpace.z /= pixelsPerUnitScaled.z;
            ParticleSystemForceField.gameObject.transform.localPosition = deltaInWorldSpace;
        }
    }
}
