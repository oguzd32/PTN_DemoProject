﻿using UnityEngine;

namespace XDPaint.Core
{
	public class RenderTextureHelper : IRenderTextureHelper
	{
		public RenderTexture PaintTexture { get; private set; }
		public RenderTexture CombinedTexture { get; private set; }
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
		public RenderTexture PaintLine { get; private set; }
#endif
		
		/// <summary>
		/// Creates 3 RenderTextures:
		/// PaintTexture - for painting;
		/// CombinedTexture - for combining source texture with paint texture and for brush preview.
		/// PaintLine - for current frame paint result storing (used by Tools);
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void Init(int width, int height)
		{
			ReleaseTextures();
			if (PaintTexture == null)
			{
				PaintTexture = CreateRenderTexture(width, height);
			}
			if (CombinedTexture == null)
			{
				CombinedTexture = CreateRenderTexture(width, height);
			}
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
			if (PaintLine == null)
			{
				PaintLine = CreateRenderTexture(width, height);
			}
#endif
		}

		private RenderTexture CreateRenderTexture(int width, int height)
		{
			var renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
			{
				filterMode = FilterMode.Point,
				autoGenerateMips = false,
				wrapMode = TextureWrapMode.Clamp,
				anisoLevel = 0,
				useMipMap = false
			};
			renderTexture.Create();
			return renderTexture;
		}

		/// <summary>
		/// Releases RenderTextures
		/// </summary>
		public void ReleaseTextures()
		{
			ReleaseTexture(PaintTexture);
			PaintTexture = null;
			ReleaseTexture(CombinedTexture);
			CombinedTexture = null;
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
			ReleaseTexture(PaintLine);
			PaintLine = null;
#endif
		}

		private void ReleaseTexture(RenderTexture renderTexture)
		{
			if (renderTexture != null && renderTexture.IsCreated())
			{
				if (RenderTexture.active == renderTexture)
				{
					RenderTexture.active = null;
				}
				renderTexture.Release();
				Object.Destroy(renderTexture);
			}
		}
	}
}