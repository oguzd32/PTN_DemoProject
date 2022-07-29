using System.Collections.Generic;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Tools.Image;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools
{
	public class ToolsManager
	{
		public BasePaintTool CurrentTool
		{
			get { return _currentTool; }
		}

		private List<BasePaintTool> _allTools = new List<BasePaintTool>();
		private BasePaintTool _currentTool;

		public ToolsManager()
		{
			var firstTool = InitializeTool<BasePaintTool>();
			InitializeTool<BrushSamplerTool>();
			InitializeTool<EraseTool>();
			InitializeTool<EyedropperTool>();
			_currentTool = firstTool;
			_currentTool.Enter();
		}

		public void Init(PaintManager paintManager)
		{
			paintManager.PaintObject.OnPaintHandler -= Paint;
			paintManager.PaintObject.OnPaintHandler += Paint;
			paintManager.PaintObject.OnMouseHoverHandler -= OnMouseHover;
			paintManager.PaintObject.OnMouseHoverHandler += OnMouseHover;
			paintManager.PaintObject.OnMouseDownHandler -= OnMouseDown;
			paintManager.PaintObject.OnMouseDownHandler += OnMouseDown;
			paintManager.PaintObject.OnMouseHandler -= OnMouse;
			paintManager.PaintObject.OnMouseHandler += OnMouse;
			paintManager.PaintObject.OnMouseUpHandler -= OnMouseUp;
			paintManager.PaintObject.OnMouseUpHandler += OnMouseUp;
			paintManager.PaintObject.OnUndoHandler -= OnUndo;
			paintManager.PaintObject.OnUndoHandler += OnUndo;
			paintManager.PaintObject.OnRedoHandler -= OnRedo;
			paintManager.PaintObject.OnRedoHandler += OnRedo;
		}
		
		public void SetTool(PaintTool newTool)
		{
			foreach (var tool in _allTools)
			{
				if (tool.Type == newTool)
				{
					_currentTool.Exit();
					_currentTool = tool;
					_currentTool.Enter();
					break;
				}
			}
		}

		private T InitializeTool<T>() where T : BasePaintTool, new()
		{
			var tool = new T();
			_allTools.Add(tool);
			return tool;
		}
		
		private void Paint(Vector2 paintPosition, float pressure)
		{			
			_currentTool.OnPaint(paintPosition, pressure);
		}
		
		private void OnMouseHover(Vector2 uv, Vector2 paintPosition, float pressure)
		{
			_currentTool.UpdateHover(uv, paintPosition, pressure);
		}

		private void OnMouseDown(Vector2 uv, Vector2 paintPosition, float pressure)
		{
			_currentTool.UpdateDown(uv, paintPosition, pressure);
		}
		
		private void OnMouse(Vector2 uv, Vector2 paintPosition, float pressure)
		{
			_currentTool.UpdatePress(uv, paintPosition, pressure);
		}
		
		private void OnMouseUp()
		{
			_currentTool.UpdateUp();
		}
		
		private void OnUndo(object sender)
		{
			_currentTool.OnUndo(sender);
		}
		
		private void OnRedo(object sender)
		{
			_currentTool.OnRedo(sender);
		}
	}
}