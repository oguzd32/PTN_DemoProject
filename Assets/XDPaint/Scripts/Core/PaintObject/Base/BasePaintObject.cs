using UnityEngine;
using XDPaint.Controllers;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintObject.States;
using XDPaint.Tools;
using XDPaint.Tools.Raycast;

namespace XDPaint.Core.PaintObject.Base
{
    public abstract class BasePaintObject : BasePaintObjectRenderer
    {
        #region Events
        public delegate void PaintDataHandler(Vector2 paintPosition, float brushSize, float pressure, Color brushColor, PaintTool tool);
        public delegate void PaintHandler(Vector2 paintPosition, float pressure);
        public delegate void MouseUVHandler(Vector2 uv, Vector2 paintPosition, float pressure);
        public delegate void MouseHandler();
        public delegate void TexturesKeeperHandler(object sender);
        public event PaintDataHandler OnPaintDataHandler;
        public event PaintHandler OnPaintHandler;
        public event MouseUVHandler OnMouseHoverHandler;
        public event MouseUVHandler OnMouseDownHandler;
        public event MouseUVHandler OnMouseHandler;
        public event MouseHandler OnMouseUpHandler;
        public event TexturesKeeperHandler OnUndoHandler;
        public event TexturesKeeperHandler OnRedoHandler;
        #endregion

        #region Properties and variables
        public bool IsPainting { get; private set; }
        public bool ProcessInput = true;
        public bool UseSourceTextureAsBackground = true;

        private Camera _camera;
        public new Camera Camera
        {
            protected get { return _camera; }
            set
            {
                _camera = value;
                base.Camera = _camera;
            }
        }
        protected Vector2? PaintPosition { private get; set; }
        protected Transform ObjectTransform { get; private set; }
        
        private float _pressure = 1f;
        private float Pressure
        {
            get { return Mathf.Clamp(_pressure, 0.01f, 10f); }
            set { _pressure = value; }
        }

        public TexturesKeeper TextureKeeper;

        private LineData _lineData;
        private Vector2 _previousPaintPosition;
        private bool _shouldReDraw;
        private bool _shouldClearTexture = true;
        private bool _hasSourceTexture;
        private const float HalfTextureRatio = 0.5f;
        #endregion

        #region Abstract methods
        protected abstract void Init();
        protected abstract void OnPaint(Vector3 position, Vector2? uv = null);
        #endregion

        public void Init(Camera camera, Transform objectTransform, Paint paint, IRenderTextureHelper renderTextureHelper)
        {
            _camera = camera;
            InitRenderer(Camera, renderTextureHelper, paint);
            ObjectTransform = objectTransform;
            PaintMaterial = paint;
            _hasSourceTexture = PaintMaterial.SourceTexture != null;
            _lineData = new LineData();
            InitStateKeeper();
            Init();
        }

        public new void Destroy()
        {
            if (TextureKeeper != null)
            {
                TextureKeeper.Reset();
            }
            base.Destroy();
        }

        private void InitStateKeeper()
        {
            if (TextureKeeper != null)
            {
                TextureKeeper.Reset();
            }
            TextureKeeper = new TexturesKeeper();
            TextureKeeper.Init(OnExtraDraw, Settings.Instance.UndoRedoEnabled);
            TextureKeeper.OnResetState = () => _shouldClearTexture = true;
            TextureKeeper.OnChangeState = () => _shouldReDraw = true;
            TextureKeeper.OnUndo = () =>
            {
                if (OnUndoHandler != null)
                {
                    OnUndoHandler(this);
                }
            };
            TextureKeeper.OnRedo = () =>
            {
                if (OnRedoHandler != null)
                {
                    OnRedoHandler(this);
                }
            };
        }
        
        #region Input
        public void OnMouseHover(Vector3 position, Triangle triangle = null)
        {
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            if (!IsPainting)
            {
                if (triangle != null)
                {
                    OnPaint(position, triangle.UVHit);
                    if (OnMouseHoverHandler != null && PaintPosition != null)
                    {
                        OnMouseHoverHandler(triangle.UVHit, PaintPosition.Value, 1f);
                    }
                }
                else
                {
                    OnPaint(position);
                }
            }
        }

        public void OnMouseDown(Vector3 position, float pressure = 1f, Triangle triangle = null)
        {
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            if (triangle != null && triangle.Transform != ObjectTransform)
                return;
            IsPainted = false;
            InBounds = false;
            Pressure = pressure;
            if (PaintPosition != null)
            {
                if (OnMouseDownHandler != null)
                {
                    if (triangle == null)
                    {
                        var uv = new Vector2(PaintPosition.Value.x / PaintMaterial.SourceTexture.width,  PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                        OnMouseDownHandler(uv, PaintPosition.Value, Pressure);
                    }
                    else
                    {
                        OnMouseDownHandler(triangle.UVHit, PaintPosition.Value, Pressure);
                    }
                }
            }
        }

        public void OnMouseButton(Vector3 position, float pressure = 1f, Triangle triangle = null)
        {
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            if (triangle == null)
            {
                IsPainting = true;
                _lineData.AddBrush(pressure * PaintController.Instance.Brush.Size);
                OnPaint(position);
                Pressure = pressure;
                if (PaintPosition != null)
                {
                    IsPainting = true;
                }
                if (InBounds && PaintPosition != null && OnMouseHandler != null)
                {
                    var uv = new Vector2(PaintPosition.Value.x / PaintMaterial.SourceTexture.width, PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                    OnMouseHandler(uv, PaintPosition.Value, Pressure);
                }
            }
            else if (triangle.Transform == ObjectTransform)
            {
                IsPainting = true;
                _lineData.AddTriangleBrush(triangle, pressure * PaintController.Instance.Brush.Size);
                Pressure = pressure;
                OnPaint(position, triangle.UVHit);
                if (OnMouseHandler != null && PaintPosition != null)
                {
                    OnMouseHandler(triangle.UVHit, PaintPosition.Value, Pressure);
                }
            }
            else
            {
                PaintPosition = null;
                _lineData.Clear();
            }
        }

        public void OnMouseUp()
        {
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            FinishPainting();
            if (OnMouseUpHandler != null)
            {
                OnMouseUpHandler();
            }
        }
        #endregion

        #region DrawFromCode
        /// <summary>
        /// Draws point with pressure
        /// </summary>
        /// <param name="position"></param>
        /// <param name="pressure"></param>
        public void DrawPoint(Vector2 position, float pressure = 1f)
        {
            Pressure = pressure;
            PaintPosition = position;
            IsPainting = true;
            IsPainted = true;
            if (OnPaintDataHandler != null)
            { 
                OnPaintDataHandler(position, PaintController.Instance.Brush.Size, Pressure, PaintController.Instance.Brush.Color, PaintController.Instance.ToolsManager.CurrentTool.Type);
            }
            _lineData.Clear();
            _lineData.AddPosition(position);
            OnRender();
            RenderCombined();
            FinishPainting();
        }
        
        /// <summary>
        /// Draws line with pressure
        /// </summary>
        /// <param name="positionStart"></param>
        /// <param name="positionEnd"></param>
        /// <param name="pressureStart"></param>
        /// <param name="pressureEnd"></param>
        public void DrawLine(Vector2 positionStart, Vector2 positionEnd, float pressureStart = 1f, float pressureEnd = 1f)
        {
            Pressure = pressureEnd;
            PaintPosition = positionEnd;
            IsPainting = true;
            IsPainted = true;
            if (OnPaintDataHandler != null)
            { 
                OnPaintDataHandler(positionStart, PaintController.Instance.Brush.Size, pressureStart, PaintController.Instance.Brush.Color, PaintController.Instance.ToolsManager.CurrentTool.Type);
                OnPaintDataHandler(positionEnd, PaintController.Instance.Brush.Size, Pressure, PaintController.Instance.Brush.Color, PaintController.Instance.ToolsManager.CurrentTool.Type);
            }
            _lineData.AddBrush(pressureStart * PaintController.Instance.Brush.Size);
            _lineData.AddBrush(Pressure * PaintController.Instance.Brush.Size);
            _lineData.AddPosition(positionStart);
            _lineData.AddPosition(positionEnd);
            OnRender();
            RenderCombined();
            FinishPainting();
        }
        #endregion

        /// <summary>
        /// Resets all states, save paint progress to TextureKeeper
        /// </summary>
        public void FinishPainting()
        {
            if (IsPainting)
            {
                Pressure = 1f;
                IsPainting = false;
                if (IsPainted && PaintController.Instance.ToolsManager.CurrentTool.RenderToTextures && Settings.Instance.UndoRedoEnabled)
                {
                    var texture = PaintMaterial.Material.GetTexture(Paint.PaintTextureShaderParam) as RenderTexture;
                    var state = RenderTexture.GetTemporary(texture.width, texture.height, 0, texture.format);
                    Graphics.Blit(texture, state);
                    TextureKeeper.OnMouseUp(state);
                }
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
                ClearLineTexture();
#endif
                _lineData.Clear();
            }
            PaintMaterial.SetPaintPreviewVector(Vector4.zero);
            PaintPosition = null;
            IsPainted = false;
            InBounds = false;
            _previousPaintPosition = default(Vector2);
        }

        /// <summary>
        /// Renders Points and Lines, restoring textures when Undo/Redo invoking
        /// </summary>
        public void OnRender()
        {
            if (_shouldClearTexture)
            {
                ClearRenderTexture();
                _shouldClearTexture = false;
            }

            if (_shouldReDraw)
            {
                TextureKeeper.OnReDraw();
                _shouldReDraw = false;
            }
            
            if (IsPainting && PaintPosition != null && _previousPaintPosition != PaintPosition.Value && PaintController.Instance.ToolsManager.CurrentTool.AllowRender)
            {
                if (_lineData.HasOnePosition())
                {
                    DrawPoint();
                    _previousPaintPosition = PaintPosition.Value;
                }
                else
                {
                    if (_lineData.HasNotSameTriangles())
                    {
                        DrawLine(false);
                    }
                    else
                    {
                        DrawLine(true);
                    }
                    _previousPaintPosition = PaintPosition.Value;
                }
            }
        }
        
        /// <summary>
        /// Combines textures, render preview
        /// </summary>
        public void RenderCombined()
        {
            ClearCombined();
            SetDefaultQuad();
            
            GL.LoadOrtho();
            var firstPass = _hasSourceTexture && UseSourceTextureAsBackground ? 0 : 1;
            var passCount = IsPainting && !PaintController.Instance.ToolsManager.CurrentTool.ShowPreview ? PaintMaterial.Material.passCount - 1 : PaintMaterial.Material.passCount;
            for (var i = firstPass; i < passCount; i++)
            {
                DrawMesh(i);
            }
            Execute();
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
            if (PaintController.Instance.Preview && !IsPainting && PaintPosition != null && PaintController.Instance.ToolsManager.CurrentTool.DrawPreview)
            {
                var positionRect = GetPosition(PaintPosition.Value, PaintController.Instance.Brush.Size * Pressure);
                DrawPreview(positionRect);
            }
#endif
            DrawPostProcess();
        }
        
        /// <summary>
        /// Restores texture when Undo/Redo invoking
        /// </summary>
        /// <param name="renderTexture"></param>
        private void OnExtraDraw(RenderTexture renderTexture)
        {
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
            ClearLineTexture();
#endif
            if (renderTexture != null)
            {
                var paintTexture = PaintMaterial.Material.GetTexture(Paint.PaintTextureShaderParam) as RenderTexture;
                Graphics.Blit(renderTexture, paintTexture);
            }
        }

        /// <summary>
        /// Gets position for draw point
        /// </summary>
        /// <param name="holePosition"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private Rect GetPosition(Vector2 holePosition, float scale)
        {
            var positionX = (int) holePosition.x;
            var positionY = (int) holePosition.y;
            var positionRect = new Rect(
                (positionX - HalfTextureRatio * PaintController.Instance.Brush.RenderTexture.width * scale) /
                PaintMaterial.SourceTexture.width,
                (positionY - HalfTextureRatio * PaintController.Instance.Brush.RenderTexture.width * scale) /
                PaintMaterial.SourceTexture.height,
                PaintController.Instance.Brush.RenderTexture.width / (float)PaintMaterial.SourceTexture.width * scale,
                PaintController.Instance.Brush.RenderTexture.height / (float)PaintMaterial.SourceTexture.height * scale
            );
            return positionRect;
        }

        /// <summary>
        /// Marks RenderTexture to be cleared
        /// </summary>
        public void ClearTexture()
        {
            _shouldClearTexture = true;
        }
        
        /// <summary>
        /// Renders quad(point)
        /// </summary>
        private void DrawPoint()
        {
            var positionRect = GetPosition(PaintPosition.Value, PaintController.Instance.Brush.Size * Pressure);
            UpdateQuad(OnPaintPointOrLine, positionRect);
        }

        /// <summary>
        /// Renders a few quads(line)
        /// </summary>
        /// <param name="interpolate"></param>
        private void DrawLine(bool interpolate)
        {
            Vector2[] positions;
            if (interpolate)
            {
                positions = _lineData.GetPositions();
            }
            else
            {
                var paintPositions = _lineData.GetPositions();
                var triangles = _lineData.GetTriangles();
                positions = DrawLine(paintPositions[0], paintPositions[1], triangles[0], triangles[1]);
            }
            if (positions.Length > 0)
            {
                var brushes = _lineData.GetBrushes();
                if (brushes.Length != 2)
                {
                    Debug.LogWarning("Incorrect length of brushes sizes array!");
                }
                else
                {
                    RenderLine(OnPaintPointOrLine, positions, PaintController.Instance.Brush.RenderTexture, PaintController.Instance.Brush.Size, brushes);
                }
            }
        }

        /// <summary>
        /// Invokes Paint Handler after drawing point or line
        /// </summary>
        /// <param name="paintPosition"></param>
        private void OnPaintPointOrLine(Vector2 paintPosition)
        {
            if (OnPaintHandler != null)
            {
                OnPaintHandler(paintPosition, _pressure);
            }
        }
        
        /// <summary>
        /// Post paint method, used by OnPaint method
        /// </summary>
        protected void OnPostPaint()
        {
            if (PaintPosition != null && IsPainting)
            {
                if (OnPaintDataHandler != null)
                {
                    OnPaintDataHandler(PaintPosition.Value, PaintController.Instance.Brush.Size, Pressure, PaintMaterial.Material.color, PaintController.Instance.ToolsManager.CurrentTool.Type);
                }
                _lineData.AddPosition(PaintPosition.Value);
                
                if (PaintController.Instance.Preview)
                {
                    var brushOffset = GetPreviewVector();
                    PaintMaterial.SetPaintPreviewVector(brushOffset);
                }
            }
            else if (PaintPosition == null)
            {
                _lineData.Clear();
            }
            
            if (PaintController.Instance.Preview)
            {
                if (PaintPosition != null)
                {
                    var brushOffset = GetPreviewVector();
                    PaintMaterial.SetPaintPreviewVector(brushOffset);
                }
                else
                {
                    PaintMaterial.SetPaintPreviewVector(Vector4.zero);
                }
            }
        }

        /// <summary>
        /// Returns Vector4 for brush preview 
        /// </summary>
        /// <returns></returns>
        private Vector4 GetPreviewVector()
        {
            var brushRatio = new Vector2(
                                 PaintMaterial.SourceTexture.width / (float) PaintController.Instance.Brush.RenderTexture.width,
                                 PaintMaterial.SourceTexture.height / (float) PaintController.Instance.Brush.RenderTexture.height) / PaintController.Instance.Brush.Size / Pressure;
            var brushOffset = new Vector4(
                PaintPosition.Value.x / PaintMaterial.SourceTexture.width * brushRatio.x,
                PaintPosition.Value.y / PaintMaterial.SourceTexture.height * brushRatio.y,
                1f / brushRatio.x, 1f / brushRatio.y);
            return brushOffset;
        }
    }
}