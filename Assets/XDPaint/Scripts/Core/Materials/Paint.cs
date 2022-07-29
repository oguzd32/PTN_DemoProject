using System;
using UnityEngine;
using UnityEngine.Serialization;
using XDPaint.Controllers;
using XDPaint.Tools;
using Object = UnityEngine.Object;

namespace XDPaint.Core.Materials
{
	[Serializable]
	public class Paint
	{
		#region Properties and variables
		private Material _material;
		public Material Material
		{
			get { return _material; }
		}

		[SerializeField] private string shaderTextureName = "_MainTex";
		public string ShaderTextureName
		{
			get { return shaderTextureName; }
			set { shaderTextureName = value; }
		}

		public Texture SourceTexture { get; private set; }

		[FormerlySerializedAs("sourceMaterial")] public Material SourceMaterial;
		private IRenderComponentsHelper _renderComponentsHelper;
		private Material _objectMaterial;
		
		private bool _initialized;

		public const string BrushOffsetShaderParam = "_BrushOffset";
		public const string BrushTextureShaderParam = "_BrushTex";
		public const string PaintTextureShaderParam = "_MaskTex";
		#endregion

		public void Init(IRenderComponentsHelper renderComponentsHelper)
		{
			_renderComponentsHelper = renderComponentsHelper;
			Destroy();
			if (SourceMaterial != null)
 			{
	            _objectMaterial = Object.Instantiate(SourceMaterial);
			}
			else if (_renderComponentsHelper.Material != null)
			{
				_objectMaterial = Object.Instantiate(_renderComponentsHelper.Material);
			}
			SourceTexture = _renderComponentsHelper.GetSourceTexture(_objectMaterial, shaderTextureName);
			_material = new Material(Settings.Instance.PaintShader) {mainTexture = SourceTexture};
			SetPreviewTexture(PaintController.Instance.Brush.RenderTexture);
			_initialized = true;
		}

		public void InitMaterial(Material material)
		{
			if (SourceMaterial == null)
			{
				if (material != null)
				{
					SourceMaterial = material;
				}
			}
		}

		public void Destroy()
		{
			if (_objectMaterial != null)
			{
				Object.Destroy(_objectMaterial);
			}
			if (_material != null)
			{
				Object.Destroy(_material);
			}
		}

		public void RestoreTexture()
		{
			if (!_initialized)
				return;
			if (SourceTexture != null)
			{
				_objectMaterial.SetTexture(shaderTextureName, SourceTexture);
			}
			else
			{
				_renderComponentsHelper.Material = SourceMaterial;
			}
		}

		public void SetObjectMaterialTexture(Texture texture)
		{
			if (!_initialized)
				return;
			_objectMaterial.SetTexture(shaderTextureName, texture);
			_renderComponentsHelper.SetSourceMaterial(_objectMaterial);
		}

		public void SetPreviewTexture(Texture texture)
		{
			if (!_initialized)
				return;
			_material.SetTexture(BrushTextureShaderParam, texture);
		}

		public void SetPaintTexture(Texture texture)
		{
			if (!_initialized)
				return;
			_material.SetTexture(PaintTextureShaderParam, texture);
		}

		public void SetPaintPreviewVector(Vector4 brushOffset)
		{
			if (!_initialized)
				return;
			_material.SetVector(BrushOffsetShaderParam, brushOffset);
		}
	}
}