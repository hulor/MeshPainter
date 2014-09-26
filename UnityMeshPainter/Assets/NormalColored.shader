Shader "MeshPainter/NormalColored" {
	Properties {
      _MainTex ("Main", 2D) = "white" {}
	}
	SubShader {
		Pass {
			Tags { "RenderType"="Opaque" }
			Lighting Off
			LOD 200
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;

			struct VertexInputs
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float3 color : COLOR;
			};

			VertexOutput vert(VertexInputs input)
			{
				VertexOutput output;

				output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
				output.color = (input.normal + 1.0) * 0.5;
				return (output);
			}

			float4 frag(VertexOutput input) : COLOR
			{
				return (float4(input.color, 1.0));
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
