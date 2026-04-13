Shader "Custom/MeleeSlash"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _Alpha ("Global Alpha", Range(0,1)) = 1
        _Progress ("Slash Progress", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _MainColor;
            float _Progress;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 利用生成的 UV.y (0-1 代表角度) 实现溶解
                if (i.uv.y > _Progress) discard;
                
                // 边缘羽化：越靠近圆心(uv.x=0)或边缘越透明
                float alpha = i.uv.x * (1.0 - i.uv.x) * 4.0;
                return fixed4(_MainColor.rgb, alpha * _MainColor.a);
            }
            ENDCG
        }
    }
}