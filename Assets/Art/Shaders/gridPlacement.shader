Shader "Game/gridPlacement"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_PulseColor ("Pulse Color", Color) = (1,1,1,1)
		_PulseStart("Pulse Start Alpha", Float) = 0
		_PulseEnd("Pulse End Alpha", Float) = 1
		_PulseSpeed ("Pulse Speed", Float) = 1
		_PulseScale ("Pulse Scale", Float) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}

		//ZWrite Off // on might hide behind pixels, off might miss order
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color1 : COLOR0;
				fixed4 color2 : COLOR1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			
			fixed4 _Color;
			
			fixed4 _PulseColor;
			float _PulseStart;
			float _PulseEnd;
			float _PulseSpeed;
			float _PulseScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.uv, _MainTex);
				o.color1 = _Color * v.color;
				o.color2 = fixed4(_PulseColor.rgb, _PulseScale * lerp(_PulseStart, _PulseEnd, sin(_PulseSpeed*_Time.y)));
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color1;
				
				col = fixed4(lerp(col.rgb, i.color2.rgb, i.color2.a), col.a);
                
                return col;
            }
            ENDCG
        }
    }
}
