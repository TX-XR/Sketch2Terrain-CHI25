Shader "Custom/ReliefShader"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _HeightMap("Height Map", 2D) = "black" {}
        _LightDir("Light Direction", Vector) = (1, 1, 0, 0)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Lambert

            sampler2D _MainTex;
            sampler2D _HeightMap;
            float4 _LightDir;

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_HeightMap;
            };

            void surf(Input IN, inout SurfaceOutput o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
                float h = tex2D(_HeightMap, IN.uv_HeightMap).r;

                // Compute normal from height map
                float hL = tex2D(_HeightMap, IN.uv_HeightMap + float2(-1.0, 0)).r;
                float hR = tex2D(_HeightMap, IN.uv_HeightMap + float2(1.0, 0)).r;
                float hD = tex2D(_HeightMap, IN.uv_HeightMap + float2(0, -1.0)).r;
                float hU = tex2D(_HeightMap, IN.uv_HeightMap + float2(0, 1.0)).r;

                float3 normal = normalize(float3(hL - hR, 2.0, hD - hU));

                // Lighting
                float3 lightDir = normalize(_LightDir.xyz);
                float diff = max(dot(normal, lightDir), 0.0);

                o.Albedo = c.rgb * diff;
                o.Alpha = 1.0;  // Ensure alpha is fully opaque
            }
            ENDCG
        }
            FallBack "Diffuse"
}
