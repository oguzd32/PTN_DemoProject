using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
	public class EyedropperTool : BasePaintTool
	{
		public override PaintTool Type
		{
			get { return PaintTool.Eyedropper; }
		}

#if XDPAINT_USE_FRAME_PAINT_TEXTURE  
		public override bool RenderToLineTexture
		{
			get { return false; }
		}
#endif

		public override bool ShowPreview
		{
			get { return false; }
		}
		
		private Material _material;
		private RenderTexture _brushTexture;
		private Mesh _quadMesh;
		private CommandBuffer _commandBuffer;
		private RenderTargetIdentifier _brushRti;
		private const string MainTexParam = "_MainTex";
		private const string BrushTexParam = "_BrushTex";
		private const string BrushOffsetShaderParam = "_BrushOffset";

		public override void Enter()
		{
			base.Enter();
			RenderToPaintTexture = false;
			_commandBuffer = new CommandBuffer {name = "BrushSamplerToolBuffer"};
			InitMaterial();
			InitQuadMesh();
		}

		public override void UpdatePress(Vector2 uv, Vector2 paintPosition, float pressure)
		{
			base.UpdatePress(uv, paintPosition, pressure);

			var activePainters = PaintController.Instance.ActivePaintManagers();
			foreach (var paint in activePainters)
			{
				var brushOffset = GetPreviewVector(paint, paintPosition, pressure);
				_material.SetTexture(MainTexParam, paint.GetResultRenderTexture());
				_material.SetVector(BrushOffsetShaderParam, brushOffset);
				UpdateRenderTexture();
				Render(paint);
			}
		}

		public override void OnUndo(object sender)
		{
			base.OnUndo(sender);
			RenderPaintObject(sender);
		}

		public override void OnRedo(object sender)
		{
			base.OnRedo(sender);
			RenderPaintObject(sender);
		}

		private void RenderPaintObject(object sender)
		{
			var paintObject = sender as BasePaintObject;
			if (paintObject == null) 
				return;
			RenderToPaintTexture = true;
			paintObject.OnRender();
			paintObject.RenderCombined();
			RenderToPaintTexture = false;
		}

		private Vector4 GetPreviewVector(PaintManager paintManager, Vector2 paintPosition, float pressure)
		{
			var brushRatio = new Vector2(
				paintManager.Material.SourceTexture.width,
				paintManager.Material.SourceTexture.height) / PaintController.Instance.Brush.Size / pressure;
			var brushOffset = new Vector4(
				paintPosition.x / paintManager.Material.SourceTexture.width * brushRatio.x,
				paintPosition.y / paintManager.Material.SourceTexture.height * brushRatio.y,
				1f / brushRatio.x, 1f / brushRatio.y);
			return brushOffset;
		}

		private void InitMaterial()
		{
			if (_material == null)
			{
				_material = new Material(Settings.Instance.EyedropperShader);
			}
		}
		
		private void InitQuadMesh()
		{
			if (_quadMesh == null)
			{
				_quadMesh = new Mesh
				{
					vertices = new[] {Vector3.up, new Vector3(1, 1, 0), Vector3.right, Vector3.zero},
					uv = new[] {Vector2.up, Vector2.one, Vector2.right, Vector2.zero,},
					triangles = new[] {0, 1, 2, 2, 3, 0},
					colors = new[] {Color.white, Color.white, Color.white, Color.white}
				};
			}
		}

		private RenderTexture CreateRenderTexture(int width, int height)
		{
			var renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
			{
				filterMode = FilterMode.Point,
				autoGenerateMips = false
			};
			renderTexture.Create();
			return renderTexture;
		}

		/// <summary>
		/// Renders pixel to RenderTexture and set a new brush color
		/// </summary>
		/// <param name="paintManager"></param>
		private void Render(PaintManager paintManager)
		{
			GL.LoadOrtho();
			_commandBuffer.Clear();
			_commandBuffer.SetRenderTarget(_brushRti);
			_commandBuffer.ClearRenderTarget(false, true, Color.clear);
			_commandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, _material);
			Graphics.ExecuteCommandBuffer(_commandBuffer);
			
			var previousRenderTexture = RenderTexture.active;
			var texture2D = new Texture2D(_brushTexture.width, _brushTexture.height, TextureFormat.ARGB32, false);
			RenderTexture.active = _brushTexture;
			texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0, false);
			RenderTexture.active = previousRenderTexture;

			var pixelColor = texture2D.GetPixel(0, 0);
			PaintController.Instance.Brush.SetColor(pixelColor);
		}

		/// <summary>
		/// Creates 1x1 texture
		/// </summary>
		private void UpdateRenderTexture()
		{
			if (_brushTexture != null)
				return;
			_brushTexture = CreateRenderTexture(1, 1);
			_material.SetTexture(BrushTexParam, _brushTexture);
			_brushRti = new RenderTargetIdentifier(_brushTexture);
		}
	}
}