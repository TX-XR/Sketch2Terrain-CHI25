Shader "Custom/UnlitTransparentTexture"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _BaseOpacity("Base Opacity", Range(0, 1)) = 1.0
        _Color("Color", Color) = (1,1,1,1)
        _ZFightingOffset("Z Fighting Offset", Float) = 0.0001
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            ZWrite Off
            ZTest LEqual
            LOD 200
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                fixed4 _Color;
                sampler2D _MainTex;
                float _BaseOpacity;
                float _ZFightingOffset;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.vertex.z = o.vertex.z - _ZFightingOffset; // prevent z fighting between patch and stroke
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Sample the main texture
                    fixed4 texColor = tex2D(_MainTex, i.uv);
                // Multiply the texture color with the _Color and apply _BaseOpacity
                fixed4 c = texColor * _Color;
                c.a = texColor.a * _BaseOpacity;

                return c;
            }
            ENDCG
        }
        }
}