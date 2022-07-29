//render each-frame paint result to RenderTexture, will be using in future updates for a few new tools
//#define XDPAINT_USE_FRAME_PAINT_TEXTURE

using UnityEngine;

namespace XDPaint.Tools
{
    [CreateAssetMenu(fileName = "XDPaintSettings", menuName = "XDPaint Settings", order = 100)]
    public class Settings : SingletonScriptableObject<Settings>
    {
        public Shader BrushShader;
        public Shader BrushRenderShader;
        public Shader EyedropperShader;
        public Shader BrushSamplerShader;
        public Shader PaintShader;
        public Texture DefaultBrush;
        public Texture DefaultCircleBrush;
        public bool UndoRedoEnabled = true;
        public uint UndoRedoMaxActionsCount = 20;
        public bool PressureEnabled = true;
        public uint BrushDuplicatePartWidth = 4;
        public uint DefaultTextureWidth = 1024;
        public uint DefaultTextureHeight = 1024;
        public float PixelPerUnit = 100f;
        public string ContainerGameObjectName = "[XDPaintContainer]";
    }
}