﻿Shader "XD Paint/Brush Sampler" {
    Properties {
        _MainTex ("Main", 2D) = "white" {}
        _BrushTex ("Brush", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _BrushOffset ("Brush offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Cull Off Lighting Off ZTest Off ZWrite Off Fog { Color (0,0,0,0) }
        Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            uniform float4 _BrushTex_TexelSize;
            float4 _BrushOffset;
            float4 _Color;

            float4 frag (v2f_img i) : SV_Target
            {
                float2 uv = float2(
                    i.uv.x * _BrushOffset.z + _BrushOffset.x * _BrushOffset.z - _BrushOffset.z / 2,
                    i.uv.y * _BrushOffset.w + _BrushOffset.y * _BrushOffset.w - _BrushOffset.w / 2 
                );
                float4 color = tex2D(_MainTex, uv) * _Color;
                if (i.uv.x <= _BrushTex_TexelSize.x || i.uv.x >= 1.0f - _BrushTex_TexelSize.x || 
                    i.uv.y <= _BrushTex_TexelSize.y || i.uv.y >= 1.0f - _BrushTex_TexelSize.x)
                {
                    color.a = 0;
                }
			    return color;
            }
            ENDCG
        }
    }
}