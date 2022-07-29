using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Controllers;
using XDPaint.Core;

namespace XDPaint.Tools.Image.Base
{
    public class BasePaintTool
    {
        /// <summary>
        /// Type of the tool
        /// </summary>
        public virtual PaintTool Type
        {
            get { return PaintTool.Brush; }
        }
        
        public virtual bool ShowPreview
        {
            get { return PaintController.Instance.Preview; }
        }

#if XDPAINT_USE_FRAME_PAINT_TEXTURE
        public virtual bool DrawPreview
        {
            get { return false; }
        }
#endif
        
        public virtual bool RenderToPaintTexture
        {
            get;
            protected set;
        }
        
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
        public virtual bool RenderToLineTexture
        {
            get;
            protected set;
        }
#endif

        public virtual bool AllowRender
        {
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
            get { return RenderToPaintTexture || RenderToLineTexture || ShowPreview || DrawPreview; }
#else
            get { return RenderToPaintTexture || ShowPreview; }
#endif
        }
        
        public virtual bool RenderToTextures
        {
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
            get { return RenderToPaintTexture || RenderToLineTexture; }
#else
            get { return RenderToPaintTexture; }
#endif
        }
        
        public virtual bool DrawPostProcess
        {
            get;
            protected set;
        }

        protected bool Preview;

        /// <summary>
        /// Enter the tool
        /// </summary>
        public virtual void Enter()
        {
            DrawPostProcess = false;
            RenderToPaintTexture = true;
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
            RenderToLineTexture = true;
#endif
            Preview = PaintController.Instance.Preview;
            PaintController.Instance.Preview = ShowPreview;
        }

        /// <summary>
        /// Exit from the tool
        /// </summary>
        public virtual void Exit()
        {
            PaintController.Instance.Preview = Preview;
        }

        /// <summary>
        /// On Mouse Hover handler
        /// </summary>
        /// <param name="uv"></param>
        /// <param name="paintPosition"></param>
        /// <param name="pressure"></param>
        public virtual void UpdateHover(Vector2 uv, Vector2 paintPosition, float pressure)
        {
        }
        
        /// <summary>
        /// On Mouse Down handler
        /// </summary>
        /// <param name="uv"></param>
        /// <param name="paintPosition"></param>
        /// <param name="pressure"></param>
        public virtual void UpdateDown(Vector2 uv, Vector2 paintPosition, float pressure)
        {
        }
        
        /// <summary>
        /// On Mouse Press handler
        /// </summary>
        /// <param name="uv"></param>
        /// <param name="paintPosition"></param>
        /// <param name="pressure"></param>
        public virtual void UpdatePress(Vector2 uv, Vector2 paintPosition, float pressure)
        {
        }

        /// <summary>
        /// On Paint Line handler (BasePaintObject.OnPaintLineHandler)
        /// </summary>
        /// <param name="paintPosition"></param>
        /// <param name="pressure"></param>
        public virtual void OnPaint(Vector2 paintPosition, float pressure)
        {
        }
        
        /// <summary>
        /// On Mouse Up handler
        /// </summary>
        public virtual void UpdateUp()
        {
        }

        /// <summary>
        /// Post Process handler
        /// </summary>
        /// <param name="commandBuffer"></param>
        /// <param name="rti"></param>
        /// <param name="material"></param>
        public virtual void OnDrawPostProcess(CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material)
        {
            DrawPostProcess = false;
        }
        
        /// <summary>
        /// On Undo handler
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnUndo(object sender)
        {
        }
        
        /// <summary>
        /// On Redo handler
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnRedo(object sender)
        {
        }
    }
}