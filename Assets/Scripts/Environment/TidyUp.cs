using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TidyUp : MonoBehaviour
{
    [SerializeField] private Renderer groundRenderer;
    
    private void Start()
    {
        groundRenderer.material.mainTextureScale = new Vector2(transform.localScale.z * 1.5f, 1);
    }
}
