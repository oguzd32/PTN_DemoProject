Shader "XD Paint/Brush"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _SrcColorBlend ("__srcC", Int) = 5
        _DstColorBlend ("__dstC", Int) = 10
        _SrcAlphaBlend ("__srcA", Int) = 1
        _DstAlphaBlend ("__dstA", Int) = 1
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Cull Off Lighting Off ZWrite Off ZTest Off Fog { Color (0,0,0,0) }
        Blend [_SrcColorBlend] [_DstColorBlend], [_SrcAlphaBlend] [_DstAlphaBlend]
        Pass
        {
            SetTexture [_MainTex]
            {
                combine texture
            }
        }
    }
}