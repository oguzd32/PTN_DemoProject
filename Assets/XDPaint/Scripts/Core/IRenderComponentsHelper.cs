using UnityEngine;

namespace XDPaint.Core
{
	public interface IRenderComponentsHelper
	{
		ObjectComponentType ComponentType { get; }
		Component PaintComponent { get; }
		Component RendererComponent { get; }
		Material Material { get; set; }

		void Init(GameObject gameObject, out ObjectComponentType componentType);
		bool IsMesh();
		void SetSourceMaterial(Material material);
		Texture GetSourceTexture(Material material, string shaderTextureName);
		Mesh GetMesh();
	}
}