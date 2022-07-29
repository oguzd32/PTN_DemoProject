using UnityEngine;

namespace XDPaint.Core
{
	public interface IRenderTextureHelper
	{
		RenderTexture PaintTexture { get; }
		RenderTexture CombinedTexture { get; }
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
		RenderTexture PaintLine { get; }
#endif
		
		void Init(int width, int height);
		void ReleaseTextures();
	}
}