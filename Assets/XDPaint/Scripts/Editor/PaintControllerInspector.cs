using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Tools;

namespace XDPaint.Editor
{
	[CustomEditor(typeof(PaintController))]
	public class PaintControllerInspector : UnityEditor.Editor
	{
		private SerializedProperty _brushProperty;
		private SerializedProperty _brushSizeProperty;
		private SerializedProperty _brushHardnessProperty;
		private SerializedProperty _brushColorProperty;
		private SerializedProperty _paintToolProperty;
		private SerializedProperty _previewProperty;

		private PaintController _paintController;
		private string[] _paintTools;
		private GUIContent[] _paintToolsContent;
		private int _paintToolId;
		private PaintTool _paintTool;

		void OnEnable()
		{
			_brushProperty = serializedObject.FindProperty("brush.sourceTexture");
			_brushColorProperty = serializedObject.FindProperty("brush.color");
			_brushSizeProperty = serializedObject.FindProperty("brush.size");
			_brushHardnessProperty = serializedObject.FindProperty("brush.hardness");
			_paintToolProperty = serializedObject.FindProperty("paintTool");
			_previewProperty = serializedObject.FindProperty("preview");
		}

		void Awake()
		{
			_paintTools = Enum.GetNames(typeof(PaintTool));
			_paintToolsContent = new GUIContent[_paintTools.Length];
			for (var i = 0; i < _paintToolsContent.Length; i++)
			{
				_paintToolsContent[i] = new GUIContent(_paintTools[i]);
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawBrushBlock();
			DrawPaintToolBlock();
			serializedObject.ApplyModifiedProperties();
		}
		
		private void DrawBrushBlock()
		{
			_paintController = target as PaintController;
			Undo.RecordObject(_paintController, "PaintController edit");

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_brushProperty, new GUIContent("Brush", PaintManagerHelper.BrushTooltip));
			if (EditorGUI.EndChangeCheck())
			{
				_paintController.Brush.SetTexture(_brushProperty.objectReferenceValue as Texture);
			}
			if (_brushProperty.objectReferenceValue == null)
			{
				_paintController.Brush.SetTexture(Settings.Instance.DefaultBrush);
			}
			var brush = _paintController.Brush.RenderTexture != null ? _paintController.Brush.RenderTexture : _paintController.Brush.SourceTexture;
			var width = Mathf.Clamp(brush.width, 1f, 96f);
			var height = Mathf.Clamp(brush.height, 1f, 96f);
			var rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(true));
			GUI.DrawTexture(rect, brush, ScaleMode.ScaleToFit);
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Slider(_brushSizeProperty, PaintManagerHelper.MinValue, PaintManagerHelper.MaxValue, new GUIContent("Brush Size", PaintManagerHelper.BrushSizeTooltip));
			EditorGUI.EndChangeCheck();
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Slider(_brushHardnessProperty, PaintManagerHelper.MinHardnessValue, PaintManagerHelper.MaxHardnessValue, new GUIContent("Brush Hardness", PaintManagerHelper.BrushHardnessTooltip));
			if (EditorGUI.EndChangeCheck())
			{
				_paintController.Brush.Hardness = _brushHardnessProperty.floatValue;
			}
			
			EditorGUI.BeginChangeCheck();
			_brushColorProperty.colorValue = EditorGUILayout.ColorField(new GUIContent("Brush Color", PaintManagerHelper.BrushColorTooltip), _brushColorProperty.colorValue);
			if (EditorGUI.EndChangeCheck())
			{
				_paintController.Brush.SetColor(_brushColorProperty.colorValue);
			}
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_previewProperty, new GUIContent("Preview", PaintManagerHelper.PreviewTooltip));
			if (EditorGUI.EndChangeCheck())
			{
				_paintController.Preview = _previewProperty.boolValue;
				MarkAsDirty();
			}
		}

		private void DrawPaintToolBlock()
		{
			EditorGUI.BeginChangeCheck();
			_paintToolId = EditorGUILayout.Popup(new GUIContent("Paint Tool", PaintManagerHelper.PaintingModeTooltip), _paintToolProperty.enumValueIndex, _paintToolsContent);
			if (EditorGUI.EndChangeCheck())
			{
				_paintTool = (PaintTool)Enum.Parse(typeof(PaintTool), _paintTools[_paintToolId]);
				_paintController.Tool = _paintTool;
			}
		}
		
		private void MarkAsDirty()
		{
			if (!Application.isPlaying)
			{
				EditorUtility.SetDirty(_paintController);
				EditorSceneManager.MarkSceneDirty(_paintController.gameObject.scene);
			}
		}
	}
}