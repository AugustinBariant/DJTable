Shader "Unlit/GridShaderUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_LineGap("LineGap", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _LineGap;

			int _NumObjects = 0;
            float4x4 _ObjectPositions1;
            float4x4 _ObjectPositions2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
				
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

				for (int i = 0; i < _NumObjects; i++) {
					float3 dif;
                    if (i < 4) {
                        dif = _ObjectPositions1[i].xyz - worldPos;
                    } else {
                        dif = _ObjectPositions2[i - 4].xyz - worldPos;
                    }
					float dist = sqrt((dif.x * dif.x) + (dif.y * dif.y));

					if (dist < 3.5) {
                        float sinValue = sin(1.79 * dist);
						// float xDif = sign(dif.x) * clamp(sqrt(0.02 / dist) * sin(dist * 2), 0, 1.2);
						// float yDif = sign(dif.y) * clamp(sqrt(0.02 / dist) * sin(dist * 2), 0, 1.2);
                        float xDif = sign(dif.x) * 0.07 * sinValue;
                        float yDif = sign(dif.y) * 0.07 * sinValue;

						o.vertex.xy -= mul(unity_WorldToObject, float4(xDif, yDif, 0, 0)).xy;

                        if (dist < 1.5) {
                            worldPos.z -= sqrt(0.2 / dist);
                        }
					}
					/*o.vertex.xy += mul(unity_WorldToObject, float4(sin(dif.x * 2.0) / 4.0, sin(dif.y * 2.0) / 4.0, 0, 0)).xy;*/
					/*if (dist < 1.2) {
						o.vertex.xy += mul(unity_WorldToObject, float4(sin(dif.x * 2.0) / 4.0, sin(dif.y * 2.0) / 4.0, 0, 0)).xy;
					}*/
				}
				o.vertex = UnityObjectToClipPos(o.vertex);
				o.worldPos = worldPos;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);

				fixed4 c;
				float extrudeProportion = 5.0 - clamp(i.worldPos.z, 4.0, 5.0);

				float xFrac = frac(i.worldPos.x / _LineGap);
				float yFrac = frac(i.worldPos.y / _LineGap);
				if (xFrac < 0.05f || yFrac < 0.05f) {
					c = ((1.f - extrudeProportion) * fixed4(0, 0.5, 0.5, 1)) + (extrudeProportion * fixed4(1, 0, 0.82, 1));
				}
				else {
					c = fixed4(0, 0, 0, 0) + (0.1 * extrudeProportion * fixed4(1, 0, 0.82, 1));
				}


                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, c);
                return c;
            }
            ENDCG
        }
    }
}
