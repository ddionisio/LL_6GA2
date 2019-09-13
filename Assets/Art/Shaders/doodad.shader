Shader "Game/doodad"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}

		ZWrite On // on might hide behind pixels, off might miss order
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB
		Lighting Off Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : POSITION;
				fixed4 color : COLOR;
            };

			fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

				o.color = _Color * v.color;
                
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.texcoord);
				//fixed3 colScroll = tex2D(_ScrollTex, i.texcoord2) * _ScrollColor;

				//col = fixed4(lerp(colScroll.rgb, col.rgb, col.a), 1) * i.color;

                //fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
				
				//col = fixed4(lerp(col.rgb, i.color2.rgb, i.color2.a), col.a);
                
                return i.color;
            }
            ENDCG
        }
    }
}
