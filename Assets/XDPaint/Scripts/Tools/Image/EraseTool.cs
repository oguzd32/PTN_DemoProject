using XDPaint.Core;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
	public class EraseTool : BasePaintTool
	{
		public override PaintTool Type
		{
			get { return PaintTool.Erase; }
		}
		
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
		public override bool DrawPreview
		{
			get { return true; }
		}
#endif
	}
}