#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Kamgam.UGUIParticles
{
    public enum ParticlesLengthUnit
    {
        Pixels = 0,
        Percent = 1
    }

    public enum ParticlesOrigin
    {
          Center = 9
        , Top = 10
        , Bottom = 11
        , Left = 12
        , Right = 13
        , TopLeft = 14
        , TopRight = 15
        , BottomLeft = 16
        , BottomRight = 17
    }

    public enum ParticlesEmitterShape
    {
        System = 0,
        BoxFill = 1
    }

    [ExecuteAlways]
    public partial class ParticleImage : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
    {
        /// <summary>
        /// Triggered once the element is shown. It takes canvas group alpha and the transform hierarchy active state into account.
        /// </summary>
        public event System.Action<ParticleImage> OnShow;

        /// <summary>
        /// Triggered once the element is hidden. It takes canvas group alpha and the transform hierarchy active state into account.
        /// </summary>
        public event System.Action<ParticleImage> OnHide;

        public bool PlayOnEnable {
            get => ParticleSystemForImage.PlayOnEnable;
            set => ParticleSystemForImage.PlayOnEnable = value;
        }

        public int PixelsPerUnit
        {
            get => ParticleSystemForImage.PixelsPerUnit;
            set => ParticleSystemForImage.PixelsPerUnit = value;
        }

        public Texture Texture
        {
            get => ParticleSystemForImage.Texture;
            set => ParticleSystemForImage.Texture = value;
        }

        public ParticlesOrigin Origin
        {
            get => ParticleSystemForImage.Origin;
            set => ParticleSystemForImage.Origin = value;
        }

        public Transform OriginTransform
        {
            get => ParticleSystemForImage.OriginTransform;
            set => ParticleSystemForImage.OriginTransform = value;
        }

        [System.NonSerialized]
        protected RectTransform _originRectTransform;

        [System.NonSerialized]
        protected Transform _originTransform;

        public ParticlesEmitterShape EmitterShape
        {
            get => ParticleSystemForImage.EmitterShape;
            set => ParticleSystemForImage.EmitterShape = value;
        }

        public float PositionX
        {
            get => ParticleSystemForImage.PositionX;
            set => ParticleSystemForImage.PositionX = value;
        }

        public ParticlesLengthUnit PositionXUnit
        {
            get => ParticleSystemForImage.PositionXUnit;
            set => ParticleSystemForImage.PositionXUnit = value;
        }

        public float PositionY
        {
            get => ParticleSystemForImage.PositionY;
            set => ParticleSystemForImage.PositionY = value;
        }

        public ParticlesLengthUnit PositionYUnit
        {
            get => ParticleSystemForImage.PositionYUnit;
            set => ParticleSystemForImage.PositionYUnit = value;
        }

        protected RectTransform _rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = transform as RectTransform;
                }
                return _rectTransform;
            }
        }

        public float Width => RectTransform.rect.width;
        public float Height => RectTransform.rect.height;

        public bool IsPlaying => ParticleSystemForImage.IsPlaying;

        /// <summary>
        /// Use this in a context where auto-creation of the particle system may not be allowed.
        /// </summary>
        public bool HasParticleSystemForImage
        {
            get
            {
                if (_particleSystemForImage == null || _particleSystemForImage.gameObject == null)
                    _particleSystemForImage = this.GetComponentInChildren<ParticleSystemForImage>(includeInactive: true);

                return _particleSystemForImage != null && _particleSystemForImage.gameObject != null;
            }
        }

        protected ParticleSystemForImage _particleSystemForImage;
        public ParticleSystemForImage ParticleSystemForImage
        {
            get
            {
                if (_particleSystemForImage == null || _particleSystemForImage.gameObject == null)
                {
                    _particleSystemForImage = this.GetComponentInChildren<ParticleSystemForImage>(includeInactive: true);
                    if (_particleSystemForImage == null)
                    {
                        _particleSystemForImage = ParticleSystemForImage.CreateParticleSystemForImage(this);
                    }
                }
                return _particleSystemForImage;
            }
        }

        public ParticleSystem ParticleSystem
        {
            get
            {
                if (ParticleSystemForImage == null || ParticleSystemForImage.gameObject == null)
                    return null;

                return ParticleSystemForImage.ParticleSystem;
            }
        }

        [System.NonSerialized]
        protected ParticleSystem.Particle[] _particles;

        protected Vector3[] _initialWorldCorners = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (IsInPlayModeOrInBuild() && PlayOnEnable)
            {
                ParticleSystemForImage.Play();
            }

            // Update rect transforms positions
            if (_initialWorldCorners == null)
                _initialWorldCorners = new Vector3[4];
            RectTransform.GetWorldCorners(_initialWorldCorners);

            if (ParticleSystemForImage.UseAttractor)
            {
                ParticleSystemForImage.EnableAttractor();
            }

            MarkDirtyRepaint();
        }

        public float? _lastWidth = null;
        public float? _lastHeight = null;

        public void Update()
        {
            if (!_lastWidth.HasValue || _lastWidth.Value != Width)
            {
                _lastWidth = Width;
                ParticleSystemForImage.UpdateEmitterShape(ParticleSystemForImage.EmitterShape, forceRefresh: true);
            }

            if (!_lastHeight.HasValue || _lastHeight.Value != Height)
            {
                _lastHeight = Height;
                ParticleSystemForImage.UpdateEmitterShape(ParticleSystemForImage.EmitterShape, forceRefresh: true);
            }

            MarkDirtyRepaint();
        }

        public void MarkDirtyRepaint()
        {
            SetVerticesDirty();
        }

        public bool IsInPlayModeOrInBuild()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
            return true;
#endif
        }

        public RenderMode GetRenderMode()
        {
            var renderMode = RenderMode.ScreenSpaceOverlay;

            if (canvas == null)
                renderMode = RenderMode.ScreenSpaceOverlay;
            else
                renderMode = canvas.renderMode;

            // Check if the camera matches the render mode and if not alter the render mode.
            if (renderMode == RenderMode.ScreenSpaceCamera && canvas != null)
            {
                Camera cam = canvas.worldCamera;
                // If no camera is specified then it will behave like screen scpae OVERLAY.
                if (cam == null)
                    renderMode = RenderMode.ScreenSpaceOverlay;
            }

            return renderMode;
        }

        public bool IsInEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public void Play()
        {
            ParticleSystemForImage.Play();
        }

        public void Pause(bool withChildren = true)
        {
            ParticleSystemForImage.Pause(withChildren);
        }

        public void Stop(bool withChildren = true, ParticleSystemStopBehavior stopBehaviour = ParticleSystemStopBehavior.StopEmitting)
        {
            ParticleSystemForImage.Stop(withChildren, stopBehaviour);
        }

        // Mesh Data caches
        UIVertex[] _vertices;
        int[] _triangles;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (!IsActiveInHierarchyAndEnabled())
                return;

            if (!HasParticleSystemForImage)
                return;

            updateVisibilityEvents();

            int maxParticles = ParticleSystem.main.maxParticles;

            if (_particles == null || _particles.Length < maxParticles)
                _particles = new ParticleSystem.Particle[maxParticles];

            if (_vertices == null || _vertices.Length < maxParticles * 4)
            {
                _vertices = new UIVertex[maxParticles * 4];
                _triangles = new int[maxParticles * 6];
            }

            // Fetch particles from ParticleSystem
            int numParticlesAlive = ParticleSystem.GetParticles(_particles);

            float width = RectTransform.rect.width;
            float height = RectTransform.rect.height;
            Vector3 origin = resolveOrigin(ParticleSystemForImage.Origin, width, height);

            // Abort if num of active particles is zero
            if (numParticlesAlive == 0)
                return;

            int vertex = 0;
            int index = 0;
            
            // Cache values needed for every particle
            float startRotationConst = ParticleSystem.main.startRotation.constant * Mathf.Rad2Deg;

            // Create one quad per particle
            for (int i = 0; i < numParticlesAlive; i++)
            {
                var position = new Vector3(
                    (_particles[i].position.x - ParticleSystemForImage.DefaultPosition.x) * PixelsPerUnit,
                    (_particles[i].position.y - ParticleSystemForImage.DefaultPosition.y) * PixelsPerUnit,
                    _particles[i].position.z * PixelsPerUnit
                );

                // Compensate pivot position
                position.x += -RectTransform.pivot.x * width;
                position.y += -RectTransform.pivot.y * height;

                var size = _particles[i].GetCurrentSize(ParticleSystem) * PixelsPerUnit;
                var color = _particles[i].GetCurrentColor(ParticleSystem) * this.color;
                var rotation = Quaternion.Euler(_particles[i].rotation3D);

                // Interpret "alignToDirection" as always align to direction (not only at the start).
                if (ParticleSystem.shape.alignToDirection)
                {
                    var velocity2D = ((Vector2)_particles[i].velocity).normalized;
                    var angle = Mathf.Atan2(velocity2D.y, velocity2D.x) * Mathf.Rad2Deg;
                    rotation = Quaternion.Euler(0f, 0f, -angle + startRotationConst);
                }
                createQuad(ref vertex, _vertices, ref index, _triangles, origin + position, rotation, size, color);
            }
            // Move all the other particles off screen and make them invisible
            for (int i = numParticlesAlive; i < maxParticles; i++)
            {
                var position = new Vector3(-300000, -300000, -300000);
                var size = _particles[i].GetCurrentSize(ParticleSystem) * 0.001f;
                var color = new Color(0, 0, 0, 0);
                var rotation = Quaternion.identity;
                createQuad(ref vertex, _vertices, ref index, _triangles, position, rotation, size, color);
            }

            writeMeshData(_vertices, _triangles, vh);

            if (ParticleSystemForImage != null)
            {
                canvasRenderer.SetTexture(ParticleSystemForImage.Texture);
            }
        }

        private void createQuad(ref int vertex, UIVertex[] vertices, ref int index, int[] triangles, Vector3 pos, Quaternion rotation, float size, Color tint)
        {
            // The coordinate system starts top left and clock-wise oriented tris are front facing.

            vertices[vertex].position = new Vector3(pos.x - size * 0.5f, pos.y - size * 0.5f, 0f);
            vertices[vertex].position = rotateAround(pos, vertices[vertex].position, rotation);
            vertices[vertex].uv0 = new Vector2(0f, 0f);
            vertices[vertex].color = tint;
            vertex++;

            vertices[vertex].position = new Vector3(pos.x + size * 0.5f, pos.y - size * 0.5f, 0f);
            vertices[vertex].position = rotateAround(pos, vertices[vertex].position, rotation);
            vertices[vertex].uv0 = new Vector2(1f, 0f);
            vertices[vertex].color = tint;
            vertex++;

            vertices[vertex].position = new Vector3(pos.x + size * 0.5f, pos.y + size * 0.5f, 0f);
            vertices[vertex].position = rotateAround(pos, vertices[vertex].position, rotation);
            vertices[vertex].uv0 = new Vector2(1f, 1f);
            vertices[vertex].color = tint;
            vertex++;

            vertices[vertex].position = new Vector3(pos.x - size * 0.5f, pos.y + size * 0.5f, 0f);
            vertices[vertex].position = rotateAround(pos, vertices[vertex].position, rotation);
            vertices[vertex].uv0 = new Vector2(0f, 1f);
            vertices[vertex].color = tint;
            vertex++;

            triangles[index++] = (ushort)(vertex - 4);
            triangles[index++] = (ushort)(vertex - 3);
            triangles[index++] = (ushort)(vertex - 1);

            triangles[index++] = (ushort)(vertex - 3);
            triangles[index++] = (ushort)(vertex - 2);
            triangles[index++] = (ushort)(vertex - 1);
        }

        private void writeMeshData(UIVertex[] vertices, int[] triangles, VertexHelper vh)
        {
            vh.Clear();

            int existingVertexCount = vh.currentVertCount;

            int vCount = vertices.Length;
            int tCount = triangles.Length;

            for (int i = 0; i < vCount; i++)
            {
                vh.AddVert(vertices[i]);
            }

            for (int i = 0; i < tCount; i += 3)
            {
                vh.AddTriangle(
                    existingVertexCount + triangles[i + 2],
                    existingVertexCount + triangles[i + 1],
                    existingVertexCount + triangles[i + 0]
                    );
            }
        }

        /// <summary>
        /// Calculates the particle system origin in the space of the ParticleImage rect.
        /// The result is the position relative to the BOTTOM LEFT corner of the rect.<br />
        /// Unless the simulation space is WORLD SPACE, then it will always return the center rect pos.
        /// </summary>
        /// <param name="originSetting"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>The local position relative to the BOTTOM LEFT corner of the ParticleImage rect.</returns>
        private Vector3 resolveOrigin(ParticlesOrigin originSetting, float width, float height)
        {
            var unitX = PositionXUnit;
            var unitY = PositionYUnit;

            if (OriginTransform != null)
            {
                _originRectTransform = OriginTransform as RectTransform;
                if (_originRectTransform == null)
                {
                    _originTransform = OriginTransform as Transform;
                }
            }
            else
            {
                _originRectTransform = null;
                _originTransform = null;
            }

            // How far the particle origin should be from the calculated origin.
            Vector3 offset = new Vector3(
                PositionXUnit == ParticlesLengthUnit.Pixels ? PositionX : width * PositionX / 100f,
                PositionYUnit == ParticlesLengthUnit.Pixels ? PositionY : height * PositionY / 100f
            );

            // Reset the particle system position to the default
            if (ParticleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local)
            {
                ParticleSystemForImage.ResetTransform();
            }

            Vector3 origin = new Vector3(0, 0, 0);
            Vector3 originRelativeToBottomLeft = origin;
            Vector2 deltaLocal = Vector2.zero;

            if (_originRectTransform != null || _originTransform != null)
            {
                Vector2 localPos;
                if (_originRectTransform != null)
                    localPos = WorldSpaceToUISpace(_originRectTransform.TransformPoint(_originRectTransform.rect.center), worldPosFromRect: true, RectTransform, GetRenderMode());
                else
                    localPos = WorldSpaceToUISpace(OriginTransform, RectTransform, GetRenderMode());

                originRelativeToBottomLeft = localPos + (Vector2) offset;

                if (ParticleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local)
                {
                    // Move the particles inside the ParticleImage element.
                    origin = localPos;
                    ParticleSystemForImage.ResetTransform();
                }
                else if (ParticleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World)
                {
                    localPos += (Vector2) offset;

                    // Here we do NOT move the particles inside the VisualElement.
                    // Instead we move the particle system in the WORLD. That way we can retain the
                    // properties set by the SimulationSpace setting (old particles will be left behind).
                    deltaLocal = localPos - new Vector2(width * 0.5f, height * 0.5f);
                    var deltaInWorldSpace = deltaLocal / (float)PixelsPerUnit;
                    // Debug.Log(deltaLocal + " > " + " > " + deltaInWorldSpace);

                    ParticleSystemForImage.ApplyPositionDelta(deltaInWorldSpace, GetRenderMode());

                    // Center pos is the default origin.
                    origin.x = width * 0.5f;
                    origin.y = height * 0.5f;
                }
                else
                {
                    // Custom Space (not supported)
                    // Center pos is the default origin.
                    origin.x = width * 0.5f;
                    origin.y = height * 0.5f;
                }
            }
            else
            {
                switch (originSetting)
                {
                    case ParticlesOrigin.BottomRight:
                        origin.x = width;
                        origin.y = 0f;
                        break;

                    case ParticlesOrigin.Top:
                        origin.x = width * 0.5f;
                        origin.y = height;
                        break;

                    case ParticlesOrigin.Bottom:
                        origin.x = width * 0.5f;
                        origin.y = 0f;
                        break;

                    case ParticlesOrigin.Left:
                        origin.x = 0f;
                        origin.y = height * 0.5f;
                        break;

                    case ParticlesOrigin.Right:
                        origin.x = width;
                        origin.y = height * 0.5f;
                        break;

                    case ParticlesOrigin.TopRight:
                        origin.x = width;
                        origin.y = height;
                        break;

                    case ParticlesOrigin.TopLeft:
                        origin.y = height;
                        break;

                    case ParticlesOrigin.Center:
                        origin.x = width * 0.5f;
                        origin.y = height * 0.5f;
                        break;

                    case ParticlesOrigin.BottomLeft:
                    default:
                        origin.x = 0f;
                        origin.y = 0f;
                        break;
                }

                originRelativeToBottomLeft = origin;
                applyWorldSimulationSpace(ref origin, ref deltaLocal, offset);
            }

            if (ParticleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World)
            {
                ParticleSystemForImage.UpdateAttractorPosition(originRelativeToBottomLeft, width, height);

                return new Vector3(origin.x, origin.y, origin.z);
            }
            else
            {
                var result = new Vector3(origin.x + offset.x, origin.y + offset.y, origin.z);

                ParticleSystemForImage.UpdateAttractorPosition(result, width, height);

                return result;
            }
        }

        private void applyWorldSimulationSpace(ref Vector3 origin, ref Vector2 deltaLocal, Vector3 positionDeltaInPx)
        {
            if (ParticleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World)
            {
                if (_initialWorldCorners != null)
                {
                    var center = new Vector3(
                        _initialWorldCorners[0].x + (_initialWorldCorners[2].x - _initialWorldCorners[0].x) * 0.5f,
                        _initialWorldCorners[0].y + (_initialWorldCorners[2].y - _initialWorldCorners[0].y) * 0.5f,
                        _initialWorldCorners[0].z + (_initialWorldCorners[2].z - _initialWorldCorners[0].z) * 0.5f
                        );
                    deltaLocal = GetCenterPosLocalDelta(center, RectTransform);
                    // Here we do NOT move the particles inside the UI.
                    // Instead we move the particle system in the WORLD. That way we can retain the
                    // properties set by the SimulationSpace setting (old particles will be left behind).
                    var deltaInWorldSpace = (deltaLocal + (Vector2)positionDeltaInPx) / (float)PixelsPerUnit; // we also add the position delta here.

                    ParticleSystemForImage.ApplyPositionDelta(deltaInWorldSpace, GetRenderMode());

                    origin.x -= deltaLocal.x;
                    origin.y -= deltaLocal.y;
                }
            }
        }

        /// <summary>
        /// Returns the distance vector between the source center and the target rect center in target local space.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Vector2 GetCenterPosLocalDelta(RectTransform source, RectTransform target)
        {
            var sourceWorldCenter = source.TransformPoint(source.rect.center);
            return GetCenterPosLocalDelta(sourceWorldCenter, target);
        }

        /// <summary>
        /// Returns the distance vector between the world position and the target rect center in target local space.
        /// </summary>
        /// <param name="sourceWorldCenter"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Vector2 GetCenterPosLocalDelta(Vector3 sourceWorldCenter, RectTransform target)
        {
            var sourcePosInTaget = target.InverseTransformPoint(sourceWorldCenter);
            var deltaInTarget = new Vector2(
                  target.rect.center.x - sourcePosInTaget.x
                , target.rect.center.y - sourcePosInTaget.y
            );

            return deltaInTarget;
        }

        public Vector2 WorldSpaceToUISpace(RectTransform source, RectTransform target, RenderMode renderMode)
        {
            return WorldSpaceToUISpace(source.position, worldPosFromRect: true, target, renderMode);
        }

        public Vector2 WorldSpaceToUISpace(Transform source, RectTransform target, RenderMode renderMode)
        {
            return WorldSpaceToUISpace(source.position, worldPosFromRect: false, target, renderMode);
        }

        /// <summary>
        /// Transform the absolute world space position to the local UI position based on the camera and the rect of the given ui target.<br />
        /// The result is a position is in the target's local space relative to the BOTTOM LEFT corner of the rect.
        /// </summary>
        /// <param name="worldSpacePos"></param>
        /// <param name="worldPosFromRect"></param>
        /// <param name="target"></param>
        /// <param name="renderMode"></param>
        /// <returns></returns>
        public Vector2 WorldSpaceToUISpace(Vector3 worldSpacePos, bool worldPosFromRect, RectTransform target, RenderMode renderMode)
        {
            bool isScreenSpaceOverlay = false;

            Camera worldCam = null;
            Camera uiCam = null;

            switch (renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    isScreenSpaceOverlay = true;
                    break;

                case RenderMode.ScreenSpaceCamera:
                    if (canvas.worldCamera != null)
                    {
                        worldCam = canvas.worldCamera;
                        uiCam = canvas.worldCamera;
                    }
                    else
                    {
                        isScreenSpaceOverlay = true;
                    }
                    break;

                case RenderMode.WorldSpace:
                    worldCam = canvas.worldCamera;
                    uiCam = canvas.worldCamera;
                    break;

                default:
                    break;
            }

            if (isScreenSpaceOverlay)
            {
                if (worldPosFromRect)
                {
                    worldCam = null;
                    uiCam = null;
                }
                else
                {
                    worldCam = Camera.main != null ? Camera.main : Camera.current;
                    uiCam = null;
                }
            }

            var screenPos = RectTransformUtility.WorldToScreenPoint(worldCam, worldSpacePos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screenPos, uiCam, out Vector2 localPos);

            // Shift to lower left corner
            localPos.x += target.rect.width * target.pivot.x;
            localPos.y += target.rect.height * target.pivot.y;

            return localPos;
        }

        private Vector3 rotateAround(Vector3 pivot, Vector3 point, float angle)
        {
            if (angle == 0f)
                return point;

            Quaternion rot = Quaternion.Euler(new Vector3(0f, 0f, angle));

            var result = point - pivot;
            result = rot * result;
            result = pivot + result;
            return result;
        }

        private Vector3 rotateAround(Vector3 pivot, Vector3 point, Quaternion rotation)
        {
            var result = point - pivot;
            result = rotation * result;
            result = pivot + result;
            return result;
        }

        protected bool? _lastKnownVisibility = null;

        protected void updateVisibilityEvents()
        {
            bool isShown = IsActiveInHierarchyAndEnabled();
            bool changed = false;

            if (_lastKnownVisibility.HasValue)
            {
                if (isShown != _lastKnownVisibility.Value)
                    changed = true;
            }
            else
            {
                changed = true;
            }

            _lastKnownVisibility = isShown;

            if (changed)
            {
                if (isShown)
                    onShow();
                else
                    onHide();
            }
        }

        public bool IsActiveInHierarchyAndEnabled()
        {
            return gameObject.activeInHierarchy && enabled;
        }

        public void Show()
        {
            bool wasShown = IsActiveInHierarchyAndEnabled();

            gameObject.SetActive(true);

            if (!wasShown)
                onShow();
        }

        public void Hide()
        {
            bool wasShown = IsActiveInHierarchyAndEnabled();

            gameObject.SetActive(false);

            if (wasShown)
                onHide();
        }

        public void Toggle()
        {
            if (IsActiveInHierarchyAndEnabled())
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        protected void onShow()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                return;
#endif

            if (PlayOnEnable)
                ParticleSystemForImage.Play();

            OnShow?.Invoke(this);
        }

        protected void onHide()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                return;
#endif

            OnHide?.Invoke(this);
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            MarkDirtyRepaint();
        }
#endif


        // I N T E R F A C E S

        /// <summary>
        /// See ILayoutElement.CalculateLayoutInputHorizontal.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal() { }

        /// <summary>
        /// See ILayoutElement.CalculateLayoutInputVertical.
        /// </summary>
        public virtual void CalculateLayoutInputVertical() { }

        /// <summary>
        /// See ILayoutElement.minWidth.
        /// </summary>
        public virtual float minWidth { get { return 0; } }

        /// <summary>
        /// If there is a sprite being rendered returns the size of that sprite.
        /// In the case of a slided or tiled sprite will return the calculated minimum size possible
        /// </summary>
        public virtual float preferredWidth
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// See ILayoutElement.flexibleWidth.
        /// </summary>
        public virtual float flexibleWidth { get { return -1; } }

        /// <summary>
        /// See ILayoutElement.minHeight.
        /// </summary>
        public virtual float minHeight { get { return 0; } }

        /// <summary>
        /// If there is a sprite being rendered returns the size of that sprite.
        /// In the case of a slided or tiled sprite will return the calculated minimum size possible
        /// </summary>
        public virtual float preferredHeight
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// See ILayoutElement.flexibleHeight.
        /// </summary>
        public virtual float flexibleHeight { get { return -1; } }

        /// <summary>
        /// See ILayoutElement.layoutPriority.
        /// </summary>
        public virtual int layoutPriority { get { return 0; } }

        /// <summary>
        /// Calculate if the ray location for this image is a valid hit location. Takes into account a Alpha test threshold.
        /// </summary>
        /// <param name="screenPoint">The screen point to check against</param>
        /// <param name="eventCamera">The camera in which to use to calculate the coordinating position</param>
        /// <returns>If the location is a valid hit or not.</returns>
        /// <remarks> Also see See:ICanvasRaycastFilter.</remarks>
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return true;
        }
    }
}
