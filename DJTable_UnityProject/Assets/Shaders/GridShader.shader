Shader "Custom/GridShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_LineGap("LineGap", Float) = 1.0
	}
		SubShader
		{
			Tags { 
				"RenderType" = "Opaque" 
				"ForceNoShadowCasting" = "True"
			}
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input
			{
				float2 uv_MainTex;
				float3 worldPos;
				float3 worldNormal;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			float _LineGap;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				float3 localPos = IN.worldPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;


				// Albedo comes from a texture tinted by color
				// fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

				fixed4 c;
				float extrudeProportion = clamp((IN.worldPos.z - 5.0) / (-1.0), 0.0, 1.0);

				if (frac(IN.worldPos.x / _LineGap) < 0.05f || frac(IN.worldPos.y / _LineGap) < 0.05f) {
					c = ((1.f - extrudeProportion) * fixed4(0,0.7,0.7,1)) + (extrudeProportion * fixed4(1, 0, 0.82, 1));
				}
				else {
					c = fixed4(0, 0, 0, 0);
				}

			  o.Albedo = c.rgb;
			  // Metallic and smoothness come from slider variables
			  o.Metallic = _Metallic;
			  o.Smoothness = _Glossiness;
			  o.Alpha = c.a;
			}
		ENDCG
		}
		FallBack "Diffuse"
}
