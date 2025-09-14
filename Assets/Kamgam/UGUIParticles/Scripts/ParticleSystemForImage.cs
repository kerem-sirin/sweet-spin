using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.UGUIParticles
{
    [ExecuteAlways]
    [RequireComponent(typeof(ParticleSystem))]
    public partial class ParticleSystemForImage : MonoBehaviour
    {
        public static Vector3 DefaultPosition = new Vector3(0f, 0f, -1000f);

        public static ParticleSystemForImage CreateParticleSystemForImage(ParticleImage image)
        {
#if UNITY_EDITOR
            if (BuildPipeline.isBuildingPlayer)
                return null;
#endif

            Logger.Log("Trying to create a particle system for ParticleImage " + image.name);

            ParticleSystemForImage system = null;
            GameObject go = null;
            try
            {
                // Create new system if not found
                go = new GameObject("Particle System (for Image)");
                go.transform.SetParent(image.transform);
                go.transform.position = DefaultPosition;
                go.transform.localScale = Vector3.one;
                go.SetActive(false);

                var renderer = go.GetComponent<CanvasRenderer>();
                if (renderer != null)
                    Utils.SmartDestroy(renderer);

                go.AddComponent<ParticleSystem>();
                system = go.AddComponent<ParticleSystemForImage>();
                system.ResetTransform();
                system.InitializeAfterCreation(image);
                system.Play();

                go.SetActive(true); // Will trigger OnEable()

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(go);
#endif
                Logger.Log("Particle System created.");
            }
            catch (System.Exception e)
            {
                if (go != null)
                    Utils.SmartDestroy(go);

                throw e;
            }

#if UNITY_EDITOR
            // Move component up (fail silently)
            if (system != null)
            {
                EditorApplication.delayCall += () => UnityEditorInternal.ComponentUtility.MoveComponentUp(system);
            }

            // Auto Play
            if (system != null)
            {
                ParticleImageEditor.StartPlaying(system.ParticleImage);
            }
#endif

            return system;
        }



        /// <summary>
        /// Should the particle system start playing whenever it is shown?
        /// </summary>
        public bool PlayOnEnable = true;

        [SerializeField]
        [Tooltip("Defines the size conversion factor from particle system units to UI reference pixels (pixels refer to the reference resolution in the CanvasScaler component).")]
        protected int _pixelsPerUnit = 50;
        public int PixelsPerUnit
        {
            get => _pixelsPerUnit;

            set
            {
                if (value != _pixelsPerUnit)
                {
                    _pixelsPerUnit = value;
                    ParticleImage?.MarkDirtyRepaint(); 
                }
            }
        }

        [SerializeField]
        protected Texture _texture;
        public Texture Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                updateTexture();
            }
        }

        public Material Material
        {
            get => ParticleImage.material;
            set
            {
                ParticleImage.material = value;
            }
        }

        public Color Color
        {
            get => ParticleImage.color;
            set
            {
                ParticleImage.color = value;
            }
        }

        [Header("Origin")]

        [SerializeField]
        [ShowIfAttribute("OriginTransform", null, ShowIfAttribute.DisablingType.ReadOnly)]
        public ParticlesOrigin _origin = ParticlesOrigin.Center;
        public ParticlesOrigin Origin
        {
            get => _origin;
            set
            {
                if (_origin == value)
                    return;

                _origin = value;
                ParticleImage?.MarkDirtyRepaint();
            }
        }

        [Tooltip("Specify a Transfrom or a RectTransfrom to use as the origin of particles.\n" +
            "If an origin transform is used then the value of 'Origin' is ignored. The origin will be at the center of the transform.")]
        public Transform OriginTransform;

        [Space(4)]
        [SerializeField]
        [Tooltip("The position is a delta value that is added to the origin position. It is always based on the ParticleImage RectTransform.")]
        protected float _positionX = 0f;
        public float PositionX
        {
            get => _positionX;
            set
            {
                _positionX = value;
            }
        }

        [SerializeField]
        protected ParticlesLengthUnit _positionXUnit = ParticlesLengthUnit.Percent;
        public ParticlesLengthUnit PositionXUnit
        {
            get => _positionXUnit;
            set
            {
                _positionXUnit = value;
            }
        }

        [Space(4)]
        [SerializeField]
        [Tooltip("The position is a delta value that is added to the origin position. It is always based on the ParticleImage RectTransform.")]
        protected float _positionY = 0f;
        public float PositionY
        {
            get => _positionY;
            set
            {
                _positionY = value;
            }
        }

        [SerializeField]
        protected ParticlesLengthUnit _positionYUnit = ParticlesLengthUnit.Percent;
        public ParticlesLengthUnit PositionYUnit
        {
            get => _positionYUnit;
            set
            {
                _positionYUnit = value;
            }
        }
        [Space(4)]

        [Header("Emitter")]
        [SerializeField]
        [Tooltip("Whether the shape of the particle emitter should be based on the ParticleImage rect transform or the particle system shape module.")]
        protected ParticlesEmitterShape _emitterShape = ParticlesEmitterShape.System;
        public ParticlesEmitterShape EmitterShape
        {
            get
            {
                return _emitterShape;
            }

            set
            {
                if (_emitterShape == value)
                    return;

                _emitterShape = value;
                UpdateEmitterShape(_emitterShape);
            }
        }

        [Header("Attractor")]

        [SerializeField]
        protected bool _useAttractor = false;
        public bool UseAttractor
        {
            get => _useAttractor;
            set
            {
                if (value != _useAttractor)
                {
                    _useAttractor = value;
                    OnUseAttractorChanged(value);
                }
            }
        }

        public Transform Attractor;

        protected ParticleImage _particleImage;
        public ParticleImage ParticleImage
        {
            get
            {
                if (_particleImage == null)
                {
                    _particleImage = this.GetComponentInParent<ParticleImage>();
                }
                return _particleImage;
            }
        }

        public bool IsPlaying => ParticleSystem.isPlaying;

        public void Play()
        {
            ParticleSystem.Play();
        }

        protected void updateTexture()
        {
            if (ParticleImage != null)
            {
                ParticleImage.canvasRenderer.SetTexture(_texture);
            }
        }

        public void Pause(bool withChildren = true)
        {
            ParticleSystem.Pause(withChildren);
        }

        public void Stop(bool withChildren = true, ParticleSystemStopBehavior stopBehaviour = ParticleSystemStopBehavior.StopEmitting)
        {
            ParticleSystem.Stop(withChildren, stopBehaviour);
        }

        protected ParticleSystem _particleSystem;
        public ParticleSystem ParticleSystem
        {
            get
            {
                if (_particleSystem == null && this != null)
                {
                    _particleSystem = this.GetComponent<ParticleSystem>();
                }
                return _particleSystem;
            }
        }

        public void InitializeAfterCreation(ParticleImage image)
        {
            // Disable renderer, we don't need it.
            var renderer = ParticleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.enabled = false;

            // Make sure the system is always simulated.
            var main = ParticleSystem.main;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = false;
            main.maxParticles = 100;

            UpdateEmitterShape(image.EmitterShape);
        }

        public void ResetTransform()
        {
            transform.position = DefaultPosition;
            transform.localRotation = Quaternion.identity;
        }

        public void ApplyPositionDelta(Vector2 delta, RenderMode renderMode)
        {
            // TODO: Revisit later, sadly this fix causes too many side effects :-( !!

            // If the simulation space is world then we have to fix some odd behaviour of the particle system (see Support case 2024-10-31).
            // This only needs to be done is the canvas is a ScreenSpaceOverlay and is the simulation space is World.
            // Further more we only do this at runtime because we move the particle system to the root.
            // That's also why we don't do it in the prefab stage.
            //bool isScreenSpaceOverlay = renderMode == RenderMode.ScreenSpaceOverlay;
            //bool isWorld = ParticleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World;
            //if (isScreenSpaceOverlay && isWorld && !isEditing() && !isInPrefabStage())
            //    MoveToRoot();

            transform.position = new Vector3(
                DefaultPosition.x + delta.x,
                DefaultPosition.y + delta.y,
                DefaultPosition.z
            );
            transform.localRotation = Quaternion.identity;
        }

        public void MoveToRoot()
        {
            _particleImage = ParticleImage;
            transform.parent = null;
        }

        static bool isEditing()
        {
#if !UNITY_EDITOR
            return false;
#else
            return !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#endif
        }

#if UNITY_EDITOR
        static bool isInPrefabStage()
        {
#if UNITY_2021_2_OR_NEWER
            return UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null;
#else
            return UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null;
#endif
        }
#endif 

        protected ParticlesEmitterShape? _lastKnownEmitterShape = null;
        protected ParticleSystemShapeType? _lastKnownEmitterShapeType = null;

        /// <summary>
        /// The last emitter shape is cached and it only updates if the shape has changed or if forseRefresh is set to true.
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="forceRefresh"></param>
        public void UpdateEmitterShape(ParticlesEmitterShape shape, bool forceRefresh = false)
        {
            if (ParticleSystem == null)
                return;

            var shapeModule = ParticleSystem.shape;
            bool changed = !_lastKnownEmitterShape.HasValue || shape != _lastKnownEmitterShape.Value;
            _lastKnownEmitterShape = shape;

            if (changed || forceRefresh)
            {
                if (!_lastKnownEmitterShapeType.HasValue)
                    _lastKnownEmitterShapeType = shapeModule.shapeType;

                switch (shape)
                {
                    case ParticlesEmitterShape.BoxFill:
                        shapeModule.shapeType = ParticleSystemShapeType.Rectangle;
                        shapeModule.scale = new Vector3(
                            ParticleImage.Width / ParticleImage.PixelsPerUnit,
                            ParticleImage.Height / ParticleImage.PixelsPerUnit,
                            1.01f); // The 1.01f is used as a flag for none-user-created rect shape.
                                    // If the z scale is 1.01f it is assumed to have been set by this code.

                        break;

                    case ParticlesEmitterShape.System:
                    default:
                        // Only reset if there is a known value and it has not been changed by the user.
                        // Check for 1.01 flag to determine if set by user.
                        if (shapeModule.shapeType != ParticleSystemShapeType.Rectangle || !Mathf.Approximately(shapeModule.scale.z, 1.01f))
                            _lastKnownEmitterShapeType = null; // Reset if user changed it
                        if (_lastKnownEmitterShapeType.HasValue)
                        {
                            // Reset to cone if rect was remembered as this is probably wrong.
                            if (_lastKnownEmitterShapeType.Value == ParticleSystemShapeType.Rectangle)
                                _lastKnownEmitterShapeType = ParticleSystemShapeType.Cone;
                            shapeModule.shapeType = _lastKnownEmitterShapeType.Value;
                            shapeModule.scale = Vector3.one;
                        }
                        break;
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (ParticleImage != null)
            {
                updateTexture();
                UpdateEmitterShape(EmitterShape);
                ParticleImage.MarkDirtyRepaint();
            }
        }
#endif
    }
}
