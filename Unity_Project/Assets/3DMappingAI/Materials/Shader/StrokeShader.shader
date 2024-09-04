Shader "Custom/StrokeShader"
{
    Properties
    {
        _BaseOpacity("Base Opacity", Range(0, 1)) = 1.0
        _BlockedBrightness("Blocked Brightness", Range(0, 1)) = 0.5
        _Color("Color", Color) = (1,1,1,1)
        _ZFightingOffset("Z Fighting Offset", Float) = 0.0001
    }
        SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" }
        ZWrite Off
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            fixed4 _Color;
            float _BaseOpacity;
            float _BlockedBrightness;
            float _ZFightingOffset;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float depth : TEXCOORD0;
            };

            sampler2D _CameraDepthTexture;
            float4 _CameraDepthTexture_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.depth = o.vertex.z;
                o.vertex.z = o.vertex.z - _ZFightingOffset; // prevent z fighting between patch and stroke
                return o;
            }

            float CustomLinear01Depth(float z)
            {
                return (2.0 * _ProjectionParams.z) / (_ProjectionParams.y + _ProjectionParams.z - z * (_ProjectionParams.y - _ProjectionParams.z));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Convert screen position to UV coordinates for depth texture sampling
                float2 uv = i.vertex.xy / i.vertex.w;
                uv = uv * 0.5 + 0.5;

                // Sample the depth buffer at the current pixel
                float sceneDepth = CustomLinear01Depth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.vertex))));

                // Calculate opacity based on whether the stroke is blocked
                // Adjust brightness based on whether the stroke is behind the assigned object
                float brightnessFactor = (i.depth > sceneDepth) ? _BlockedBrightness : 1.0;
                fixed4 c = _Color * brightnessFactor;
                c.a = _BaseOpacity;

                return c;
            }
            ENDCG
        }
    }
}
