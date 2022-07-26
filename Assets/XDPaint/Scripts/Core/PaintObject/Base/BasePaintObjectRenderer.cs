﻿using System;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Controllers;
using XDPaint.Core.Materials;
using XDPaint.Tools.Raycast;

namespace XDPaint.Core.PaintObject.Base
{
	public class BasePaintObjectRenderer
	{
		public bool UseNeighborsVertices
		{
			set { _lineDrawer.UseNeighborsVertices = value; }
		}
		
		protected Camera Camera
		{
			set { _lineDrawer.Camera = value; }
		}
		
		protected Paint PaintMaterial;
		protected bool IsPainted;
		protected bool InBounds;
		private BaseLineDrawer _lineDrawer;
		private Mesh _mesh;
		private Mesh _quadMesh;
		private RenderTexture _paintTexture;
		private CommandBuffer _commandBuffer;
		private RenderTargetIdentifier _rti;
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
		private RenderTargetIdentifier _rtiLine;
#endif
		private RenderTargetIdentifier _rtiCombined;
		private readonly Vector3 _upRight = new Vector3(1, 1, 0);

		protected void InitRenderer(Camera camera, IRenderTextureHelper renderTextureHelper, Paint paint)
		{
			_mesh = new Mesh();
			PaintMaterial = paint;
			_lineDrawer = new BaseLineDrawer();
			var sourceTextureSize = new Vector2(paint.SourceTexture.width, paint.SourceTexture.height);
			_lineDrawer.Init(camera, sourceTextureSize, RenderLine);
			_paintTexture = renderTextureHelper.PaintTexture;
			_commandBuffer = new CommandBuffer {name = "XDPaintObject"};
			_rti = new RenderTargetIdentifier(renderTextureHelper.PaintTexture);
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
			_rtiLine = new RenderTargetIdentifier(renderTextureHelper.PaintLine);
#endif
			_rtiCombined = new RenderTargetIdentifier(renderTextureHelper.CombinedTexture);
			InitQuadMesh();
		}

		private void InitQuadMesh()
		{
			_quadMesh = new Mesh
			{
				vertices = new Vector3[4],
				uv = new[] {Vector2.up, Vector2.one, Vector2.right, Vector2.zero,},
				triangles = new[] {0, 1, 2, 2, 3, 0},
				colors = new[] {Color.white, Color.white, Color.white, Color.white}
			};
		}

		protected void Destroy()
		{
			if (_commandBuffer != null)
			{
				_commandBuffer.Release();
			}
			if (_mesh != null)
			{
				UnityEngine.Object.Destroy(_mesh);
			}
			if (_quadMesh != null)
			{
				UnityEngine.Object.Destroy(_quadMesh);
			}
		}

		protected void ClearRenderTexture()
		{
			_commandBuffer.Clear();
			_commandBuffer.SetRenderTarget(_rti);
			_commandBuffer.ClearRenderTarget(false, true, Color.clear);
			Graphics.ExecuteCommandBuffer(_commandBuffer);
			if (PaintMaterial.SourceTexture != null)
			{
				Graphics.Blit(PaintMaterial.SourceTexture, _paintTexture);
			}
		}

		protected void ClearCombined()
		{
			_commandBuffer.Clear();
			_commandBuffer.SetRenderTarget(_rtiCombined);
			_commandBuffer.ClearRenderTarget(false, true, Color.clear);
			Graphics.ExecuteCommandBuffer(_commandBuffer);
		}

		protected void DrawPostProcess()
		{
			if (PaintController.Instance.ToolsManager.CurrentTool.DrawPostProcess)
			{
				PaintController.Instance.ToolsManager.CurrentTool.OnDrawPostProcess(_commandBuffer, _rti, PaintMaterial.Material);
			}
		}

		protected void UpdateQuad(Action<Vector2> onDraw, Rect positionRect, bool isUndo = false)
		{
			_quadMesh.vertices = new[]
			{
				new Vector3(positionRect.xMin, positionRect.yMax, 0),
				new Vector3(positionRect.xMax, positionRect.yMax, 0),
				new Vector3(positionRect.xMax, positionRect.yMin, 0),
				new Vector3(positionRect.xMin, positionRect.yMin, 0)
			};
			GL.LoadOrtho();
			RenderToPaintTexture(_quadMesh);
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
			RenderToLineTexture(_quadMesh);
#endif
			if (!isUndo)
			{
				if (onDraw != null)
				{
					onDraw(Vector2.zero);
				}
			}
		}
		
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
		protected void DrawPreview(Rect positionRect)
		{
			if (PaintController.Instance.ToolsManager.CurrentTool.RenderToLineTexture)
			{
				_quadMesh.vertices = new[]
				{
					new Vector3(positionRect.xMin, positionRect.yMax, 0),
					new Vector3(positionRect.xMax, positionRect.yMax, 0),
					new Vector3(positionRect.xMax, positionRect.yMin, 0),
					new Vector3(positionRect.xMin, positionRect.yMin, 0)
				};
				GL.LoadOrtho();
				_commandBuffer.Clear();
				_commandBuffer.SetRenderTarget(_rtiLine);
				_commandBuffer.ClearRenderTarget(false, true, Color.clear);
				_commandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, PaintController.Instance.Brush.Material);
				Graphics.ExecuteCommandBuffer(_commandBuffer);
			}
		}
#endif

		protected void SetDefaultQuad()
		{
			_quadMesh.vertices = new[]
			{
				Vector3.up,
				_upRight,
				Vector3.right,
				Vector3.zero
			};
		}

		protected Vector2[] DrawLine(Vector2 fistPaintPos, Vector2 lastPaintPos, Triangle firstTriangle, Triangle lastTriangle)
		{
			return _lineDrawer.DrawLine(fistPaintPos, lastPaintPos, firstTriangle, lastTriangle);
		}

		protected void DrawMesh(int pass)
		{
			_commandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, PaintMaterial.Material, 0, pass);
		}

		protected void Execute()
		{
			Graphics.ExecuteCommandBuffer(_commandBuffer);
		}

		protected void RenderLine(Action<Vector2> onDraw, Vector2[] drawLine, Texture brushTexture, float brushSizeActual, float[] brushSizes, bool isUndo = false)
		{
			_lineDrawer.RenderLine(onDraw, drawLine, brushTexture, brushSizeActual, brushSizes, isUndo);
		}

		private void RenderToPaintTexture(Mesh mesh)
		{
			if (PaintController.Instance.ToolsManager.CurrentTool.RenderToPaintTexture)
			{
				_commandBuffer.Clear();
				_commandBuffer.SetRenderTarget(_rti);
				_commandBuffer.DrawMesh(mesh, Matrix4x4.identity, PaintController.Instance.Brush.Material);
				Graphics.ExecuteCommandBuffer(_commandBuffer);
			}
		}
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
		protected void ClearLineTexture()
		{
			_commandBuffer.Clear();
			_commandBuffer.SetRenderTarget(_rtiLine);
			_commandBuffer.ClearRenderTarget(false, true, Color.clear);
			Graphics.ExecuteCommandBuffer(_commandBuffer);
		}
#endif

#if XDPAINT_USE_FRAME_PAINT_TEXTURE
		private void RenderToLineTexture(Mesh mesh)
		{
			if (PaintController.Instance.ToolsManager.CurrentTool.RenderToLineTexture)
			{
				_commandBuffer.Clear();
				_commandBuffer.SetRenderTarget(_rtiLine);
				_commandBuffer.ClearRenderTarget(false, true, Color.clear);
				_commandBuffer.DrawMesh(mesh, Matrix4x4.identity, PaintController.Instance.Brush.Material);
				Graphics.ExecuteCommandBuffer(_commandBuffer);
			}
		}
#endif
		
		private void RenderLine(Vector3[] positions, Vector2[] uv, int[] indices, Color[] colors)
		{
			if (_mesh != null)
			{
				_mesh.Clear(false);
			}
			_mesh.vertices = positions;
			_mesh.uv = uv;
			_mesh.triangles = indices;
			_mesh.colors = colors;
			
			GL.LoadOrtho();
			RenderToPaintTexture(_mesh);
#if XDPAINT_USE_FRAME_PAINT_TEXTURE
			RenderToLineTexture(_mesh);
#endif
		}
	}
}