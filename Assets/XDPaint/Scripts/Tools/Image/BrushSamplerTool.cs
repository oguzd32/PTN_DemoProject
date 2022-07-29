using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
	public class BrushSamplerTool : BasePaintTool
	{
		public override PaintTool Type
		{
			get { return PaintTool.BrushSampler; }
		}

		public override bool ShowPreview
		{
			get { return PaintController.Instance.Preview; }
		}

		public override bool RenderToPaintTexture
		{
			get { return false; }
		}
        
#if XDPAINT_USE_FRAME_PAINT_TEXTURE  
		public override bool RenderToLineTexture
		{
			get { return false; }
		}
#endif

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
			_commandBuffer = new CommandBuffer {name = "BrushSamplerToolBuffer"};
			InitMaterial();
			InitQuadMesh();

			PaintController.Instance.Brush.SetColor(new Color(1, 1, 1, PaintController.Instance.Brush.Color.a), false, false); 
			PaintController.Instance.Brush.SetTexture(Settings.Instance.DefaultBrush, true, false);
			//set new preview texture
			var activePainters = PaintController.Instance.ActivePaintManagers();
			foreach (var paintManager in activePainters)
			{
				paintManager.Material.Material.SetTexture(Paint.BrushTextureShaderParam, Settings.Instance.DefaultCircleBrush);
			}
		}

		public override void UpdatePress(Vector2 uv, Vector2 paintPosition, float pressure)
		{
			base.UpdatePress(uv, paintPosition, pressure);
			var activePainters = PaintController.Instance.ActivePaintManagers();
			foreach (var paintManager in activePainters)
			{
				var brushOffset = GetPreviewVector(paintManager, paintPosition, pressure);
				_material.SetVector(BrushOffsetShaderParam, brushOffset);
				var width = PaintController.Instance.Brush.RenderTexture.width;
				var height = PaintController.Instance.Brush.RenderTexture.height;
				UpdateRenderTexture(width, height);
				Render(paintManager);
			}
		}

		private Vector4 GetPreviewVector(PaintManager paintManager, Vector2 paintPosition, float pressure)
		{
			var brushRatio = new Vector2(
				paintManager.Material.SourceTexture.width / (float) PaintController.Instance.Brush.RenderTexture.width,
				paintManager.Material.SourceTexture.height / (float) PaintController.Instance.Brush.RenderTexture.height) / PaintController.Instance.Brush.Size / pressure;
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
				_material = new Material(Settings.Instance.BrushSamplerShader);
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
		/// Renders part of Result texture into RenderTexture, set new brush
		/// </summary>
		/// <param name="paintManager"></param>
		private void Render(PaintManager paintManager)
		{
			//set preview to false
			PaintController.Instance.Preview = false;
			paintManager.Render();
			
			_material.SetTexture(MainTexParam, paintManager.GetResultRenderTexture());
			GL.LoadOrtho();
			_commandBuffer.Clear();
			_commandBuffer.SetRenderTarget(_brushRti);
			_commandBuffer.ClearRenderTarget(false, true, Color.clear);
			_commandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, _material);
			Graphics.ExecuteCommandBuffer(_commandBuffer);
			PaintController.Instance.Brush.SetTexture(_brushTexture, true, false);
			//restore preview
			PaintController.Instance.Preview = Preview;
		}

		/// <summary>
		/// Creates new brush texture
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void UpdateRenderTexture(int width, int height)
		{
			if (_brushTexture != null && _brushTexture.width == width && _brushTexture.height == height)
				return;
			_brushTexture = CreateRenderTexture(width, height);
			_material.SetTexture(BrushTexParam, _brushTexture);
			_brushRti = new RenderTargetIdentifier(_brushTexture);
		}
	}
}