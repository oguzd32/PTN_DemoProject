using System;
using UnityEngine;
using UnityEngine.Serialization;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintObject;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools;
using XDPaint.Tools.Raycast;

namespace XDPaint
{
    [DisallowMultipleComponent]
    public class PaintManager : MonoBehaviour
    {
        #region Properties and variables
        [FormerlySerializedAs("objectForPainting")] public GameObject ObjectForPainting;
        public Paint Material = new Paint();
        [FormerlySerializedAs("shouldOverrideCamera")] public bool ShouldOverrideCamera;
        public BasePaintObject PaintObject { get; private set; }

        [SerializeField] private Camera overrideCamera;
        public Camera Camera
        {
            private get { return ShouldOverrideCamera || overrideCamera == null ? Camera.main : overrideCamera; }
            set
            {
                overrideCamera = value;
                if (InputController.Instance != null)
                {
                    InputController.Instance.Camera = overrideCamera;
                }
                if (RaycastController.Instance != null)
                {
                    RaycastController.Instance.Camera = overrideCamera;
                }
                if (_initialized)
                {
                    PaintObject.Camera = overrideCamera;
                }
            }
        }
        
        [SerializeField] private bool useSourceTextureAsBackground = true;
        public bool UseSourceTextureAsBackground
        {
            get { return useSourceTextureAsBackground; }
            set
            {
                useSourceTextureAsBackground = value;
                if (!Application.isPlaying)
                {
                    if (!useSourceTextureAsBackground)
                    {
                        ClearTrianglesNeighborsData();
                    }
                }
                if (_initialized)
                {
                    PaintObject.UseSourceTextureAsBackground = useSourceTextureAsBackground;
                }
            }
        }

        [SerializeField] private bool useNeighborsVerticesForRaycasts;
        public bool UseNeighborsVerticesForRaycasts
        {
            get { return useNeighborsVerticesForRaycasts; }
            set
            {
                useNeighborsVerticesForRaycasts = value;
                if (!Application.isPlaying)
                {
                    if (!useNeighborsVerticesForRaycasts)
                    {
                        ClearTrianglesNeighborsData();
                    }
                }
                if (_initialized)
                {
                    PaintObject.UseNeighborsVertices = useNeighborsVerticesForRaycasts;
                }
            }
        }

        public bool HasTrianglesData
        {
            get { return triangles != null && triangles.Length > 0; }
        }

        public bool Initialized
        {
            get { return _initialized; }
        }

        [SerializeField] private TrianglesContainer trianglesContainer;
        [SerializeField] private Triangle[] triangles;
        public IRenderTextureHelper _renderTextureHelper;
        private IRenderComponentsHelper _renderComponentsHelper;
        private bool _initialized;
        #endregion
        
        [Obsolete("SetSourceMaterial() method marked as obsolete from 2.1 version and will be removed in future updates")]
        public void SetSourceMaterial()
        {
            if (_initialized)
            {
                _renderComponentsHelper.SetSourceMaterial(_renderComponentsHelper.Material);
            }
        }

        private void Start()
        {
            if (!_initialized)
            {
                Init();
            }
        }

        private void Update()
        {
            if (_initialized && (PaintObject.IsPainting || PaintController.Instance.Preview))
            {
                Render();
            }
        }
        
        private void OnDestroy()
        {
            if (_initialized)
            {
                //restore source material and texture
                Material.RestoreTexture();
                //destroy created material
                Material.Destroy();
                //free RenderTextures
                _renderTextureHelper.ReleaseTextures();
                //destroy raycast data
                if (_renderComponentsHelper.IsMesh())
                {
                    var renderComponent = _renderComponentsHelper.RendererComponent;
                    RaycastController.Instance.DestroyMeshData(renderComponent);
                }
                //unsubscribe input events
                UnsubscribeInputEvents();
                //free undo/redo RenderTextures and meshes
                PaintObject.Destroy();
            }
        }

        public void Init()
        {
            if (ObjectForPainting == null)
            {
                Debug.LogError("ObjectForPainting is null!");
                return;
            }

            //restore source material and texture
            if (_initialized && Material.SourceMaterial != null)
            {
                if (Material.SourceMaterial.GetTexture(Material.ShaderTextureName) == null)
                {
                    Material.SourceMaterial.SetTexture(Material.ShaderTextureName, Material.SourceTexture);
                }
                _renderComponentsHelper.SetSourceMaterial(Material.SourceMaterial);
            }
            
            if (_renderComponentsHelper == null)
            {
                _renderComponentsHelper = new RenderComponentsHelper();
            }
            ObjectComponentType componentType;
            _renderComponentsHelper.Init(ObjectForPainting, out componentType);
            if (componentType == ObjectComponentType.Unknown)
            {
                Debug.LogError("Unknown component type!");
                return;
            }
            
            if (ControllersContainer.Instance == null)
            {
                var containerGameObject = new GameObject(Settings.Instance.ContainerGameObjectName);
                containerGameObject.AddComponent<ControllersContainer>();
            }

            if (_renderComponentsHelper.IsMesh())
            {
                var paintComponent = _renderComponentsHelper.PaintComponent;
                var renderComponent = _renderComponentsHelper.RendererComponent;
                var mesh = _renderComponentsHelper.GetMesh();
                if (trianglesContainer != null)
                {
                    triangles = trianglesContainer.Data;
                }
                if (triangles == null || triangles.Length == 0)
                {
                    if (mesh != null)
                    {
                        Debug.LogWarning("PaintManager does not have triangles data! Getting it may take a while.");
                        triangles = TrianglesData.GetData(mesh, useNeighborsVerticesForRaycasts);
                    }
                    else
                    {
                        Debug.LogError("Mesh is null!");
                        return;
                    }
                }
                RaycastController.Instance.InitObject(Camera, paintComponent, renderComponent, triangles);
            }
            Material.Init(_renderComponentsHelper);
            InitRenderTexture();
            InitPaintObject();
            InputController.Instance.Camera = Camera;
            PaintController.Instance.Init(this);
            SetInputEvents();
            Render();
            _initialized = true;
        }
        
        public void Render()
        {
            if (_initialized)
            {
                PaintObject.OnRender();
                PaintObject.RenderCombined();
            }
        }
        
        public void FillTrianglesData(bool fillNeighbors = true)
        {
            if (_renderComponentsHelper == null)
            {
                _renderComponentsHelper = new RenderComponentsHelper();
            }
            ObjectComponentType componentType;
            _renderComponentsHelper.Init(ObjectForPainting, out componentType);
            if (componentType == ObjectComponentType.Unknown)
            {
                return;
            }
            if (_renderComponentsHelper.IsMesh())
            {
                var mesh = _renderComponentsHelper.GetMesh();
                if (mesh != null)
                {
                    triangles = TrianglesData.GetData(mesh, fillNeighbors);
                    if (fillNeighbors)
                    {
                        Debug.Log("Added triangles with neighbors data. Triangles count: " + triangles.Length);
                    }
                    else
                    {
                        Debug.Log("Added triangles data. Triangles count: " + triangles.Length);
                    }
                }
                else
                {
                    Debug.LogError("Mesh is null!");
                }
            }
        }
        
        public void ClearTrianglesData()
        {
            triangles = null;
        }

        public void ClearTrianglesNeighborsData()
        {
            if (triangles != null)
            {
                foreach (var triangle in triangles)
                {
                    triangle.N.Clear();
                }
            }
        }
                
        public Triangle[] GetTriangles()
        {
            return triangles;
        }

        public void SetTriangles(Triangle[] trianglesData)
        {
            triangles = trianglesData;
        }

        public void SetTriangles(TrianglesContainer trianglesContainerData)
        {
            trianglesContainer = trianglesContainerData;
            triangles = trianglesContainer.Data;
        }

#if XDPAINT_USE_FRAME_PAINT_TEXTURE
        public RenderTexture GetRenderTextureLine()
        {
            return _renderTextureHelper.PaintLine;
        }
#endif

        public RenderTexture GetPaintTexture()
        {
            return _renderTextureHelper.PaintTexture;
        }

        public RenderTexture GetResultRenderTexture()
        {
            if (!_initialized)
                return null;
            return _renderTextureHelper.CombinedTexture;
        }

        /// <summary>
        /// Returns result texture
        /// </summary>
        /// <returns></returns>
        public Texture2D GetResultTexture()
        {
            var material = Material.Material;
            var sourceTexture = material.mainTexture;
            var previousRenderTexture = RenderTexture.active;
            var texture2D = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.ARGB32, false);
            RenderTexture.active = GetResultRenderTexture();
            texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0, false);
            texture2D.Apply();
            RenderTexture.active = previousRenderTexture;
            return texture2D;
        }
        
        /// <summary>
        /// Bakes into Material source texture
        /// </summary>
        public void Bake()
        {
            PaintObject.FinishPainting();
            Render();
            var prevRenderTexture = RenderTexture.active;
            var renderTexture = GetResultRenderTexture();
            RenderTexture.active = renderTexture;
            if (Material.SourceTexture != null)
            {
                var texture = Material.SourceTexture as Texture2D;
                if (texture != null)
                {
                    texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                    texture.Apply();
                }
            }
            RenderTexture.active = prevRenderTexture;
        }

        private void InitPaintObject()
        {
            if (PaintObject != null)
            {
                UnsubscribeInputEvents();
                PaintObject.Destroy();
            }
            if (_renderComponentsHelper.ComponentType == ObjectComponentType.CanvasImage)
            {
                PaintObject = new CanvasRendererPaint();
            }
            else if (_renderComponentsHelper.ComponentType == ObjectComponentType.SpriteRenderer)
            {
                PaintObject = new SpriteRendererPaint();
            }
            else
            {
                PaintObject = new MeshRendererPaint();
            }
            PaintObject.Init(Camera, ObjectForPainting.transform, Material, _renderTextureHelper);
            PaintObject.UseNeighborsVertices = useNeighborsVerticesForRaycasts;
            PaintObject.UseSourceTextureAsBackground = useSourceTextureAsBackground;
        }

        private void InitRenderTexture()
        {
            if (_renderTextureHelper == null)
            {
                _renderTextureHelper = new RenderTextureHelper();
            }
            _renderTextureHelper.Init(Material.SourceTexture.width, Material.SourceTexture.height);
            if (Material.SourceTexture != null)
            {
                Graphics.Blit(Material.SourceTexture, _renderTextureHelper.CombinedTexture);
            }
            Material.SetObjectMaterialTexture(_renderTextureHelper.CombinedTexture);
            Material.SetPaintTexture(_renderTextureHelper.PaintTexture);
        }

        #region Setup input events
        private void SetInputEvents()
        {
            UpdatePreviewInput();
            if (_renderComponentsHelper.IsMesh())
            {
                InputController.Instance.OnMouseDownWithHit -= PaintObject.OnMouseDown;
                InputController.Instance.OnMouseDownWithHit += PaintObject.OnMouseDown;
                InputController.Instance.OnMouseButtonWithHit -= PaintObject.OnMouseButton;
                InputController.Instance.OnMouseButtonWithHit += PaintObject.OnMouseButton;
            }
            else
            {
                InputController.Instance.OnMouseDown -= PaintObject.OnMouseDown;
                InputController.Instance.OnMouseDown += PaintObject.OnMouseDown;
                InputController.Instance.OnMouseButton -= PaintObject.OnMouseButton;
                InputController.Instance.OnMouseButton += PaintObject.OnMouseButton;
            }
            InputController.Instance.OnMouseUp -= PaintObject.OnMouseUp;
            InputController.Instance.OnMouseUp += PaintObject.OnMouseUp;
        }

        private void UnsubscribeInputEvents()
        {
            InputController.Instance.OnMouseHover -= PaintObject.OnMouseHover;
            InputController.Instance.OnMouseHoverWithHit -= PaintObject.OnMouseHover;
            InputController.Instance.OnMouseDown -= PaintObject.OnMouseDown;
            InputController.Instance.OnMouseDownWithHit -= PaintObject.OnMouseDown;
            InputController.Instance.OnMouseButton -= PaintObject.OnMouseButton;
            InputController.Instance.OnMouseButtonWithHit -= PaintObject.OnMouseButton;
            InputController.Instance.OnMouseUp -= PaintObject.OnMouseUp;
        }

        public void UpdatePreviewInput()
        {
            if (PaintController.Instance.Preview)
            {
                if (_renderComponentsHelper.IsMesh())
                {
                    InputController.Instance.OnMouseHoverWithHit -= PaintObject.OnMouseHover;
                    InputController.Instance.OnMouseHoverWithHit += PaintObject.OnMouseHover;
                }
                else
                {
                    InputController.Instance.OnMouseHover -= PaintObject.OnMouseHover;
                    InputController.Instance.OnMouseHover += PaintObject.OnMouseHover;
                }
            }
            else
            {
                if (_renderComponentsHelper.IsMesh())
                {
                    InputController.Instance.OnMouseHoverWithHit -= PaintObject.OnMouseHover;
                }
                else
                {
                    InputController.Instance.OnMouseHover -= PaintObject.OnMouseHover;
                }
            }
        }
        #endregion
    }
}