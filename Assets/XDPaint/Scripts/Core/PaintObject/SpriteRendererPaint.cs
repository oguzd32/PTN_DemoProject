﻿using UnityEngine;
using XDPaint.Controllers;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Core.PaintObject
{
    public sealed class SpriteRendererPaint : BasePaintObject
    {
        private Renderer _renderer;
        private Vector2 _scratchBoundsSize;

        protected override void Init()
        {
            _renderer = ObjectTransform.GetComponent<Renderer>();
            GetScratchBounds();
            if (!Camera.orthographic)
            {
                Debug.LogWarning("Camera is not orthographic!");
            }
        }

        private bool IsInBounds(Vector3 position)
        {
            var clickPosition = Camera.ScreenToWorldPoint(position);
            var bounds = _renderer.bounds;
            var offset = new Vector3(
                PaintController.Instance.Brush.RenderTexture.width * PaintController.Instance.Brush.Size / PaintMaterial.SourceTexture.width * bounds.size.x,
                PaintController.Instance.Brush.RenderTexture.height * PaintController.Instance.Brush.Size/ PaintMaterial.SourceTexture.height * bounds.size.y);
            bounds.size += offset;
            clickPosition.z = bounds.center.z;
            var inBounds = bounds.Contains(clickPosition);
            return inBounds;
        }

        private void GetScratchBounds()
        {
            if (_renderer != null)
            {
                _scratchBoundsSize = _renderer.bounds.size;
            }
        }

        protected override void OnPaint(Vector3 position, Vector2? uv = null)
        {
            InBounds = IsInBounds(position);
            if (InBounds)
            {
                IsPainted = true;
            }

            var clickPosition = Camera.ScreenToWorldPoint(position);
            var surfaceLocalClickPosition = ObjectTransform.InverseTransformPoint(clickPosition);
            var lossyScale = ObjectTransform.lossyScale;
            var clickLocalPosition = new Vector2(surfaceLocalClickPosition.x * lossyScale.x, surfaceLocalClickPosition.y * lossyScale.y);
            GetScratchBounds();
            var bottomLeftLocalPosition = (Vector2)ObjectTransform.InverseTransformPoint(ObjectTransform.position) - _scratchBoundsSize / 2f;
            var scratchSurfaceClickLocalPosition = clickLocalPosition - bottomLeftLocalPosition;
            var ppi = new Vector2(
                PaintMaterial.SourceTexture.width / _scratchBoundsSize.x / lossyScale.x, 
                PaintMaterial.SourceTexture.height / _scratchBoundsSize.y / lossyScale.y);
            PaintPosition = new Vector2( 
                scratchSurfaceClickLocalPosition.x * lossyScale.x * ppi.x, 
                scratchSurfaceClickLocalPosition.y * lossyScale.y * ppi.y);
            OnPostPaint();
        }
    }
}