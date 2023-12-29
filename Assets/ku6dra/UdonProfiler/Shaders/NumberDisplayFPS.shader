Shader "ku6dra/UdonProfiler/NumberDisplayFPS"
{
    Properties
    {
        _MainTex("Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "PreviewType" = "Plane"
            "ForceNoShadowCasting" = "True"
            "IgnoreProjector" = "True"
            "VRCFallback" = "Hidden"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#pragma multi_compile_instancing
            //#pragma instancing_options nolightprobe nolightmap

            #include "UnityCG.cginc"

            #define CHR_SCALE_X 0.1875
            #define CHR_SCALE_Y 0.25

            #define DISP_SCALE_X .3333333
            #define DISP_SCALE_Y 1

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 drawCharacter(v2f i, fixed4 baseTex, min12int drawPosX, min12int drawPosY, min12int chrPosX, min12int chrPosY)
            {
                fixed2 pos = fixed2(i.uv.x * CHR_SCALE_X / DISP_SCALE_X + CHR_SCALE_X * (chrPosX - drawPosX),
                (1 - CHR_SCALE_Y) + i.uv.y * CHR_SCALE_Y / DISP_SCALE_Y - CHR_SCALE_Y * (chrPosY - drawPosY));

                float2 uv = TRANSFORM_TEX(pos, _MainTex);

                fixed4 addTex = tex2D(_MainTex, uv);

                fixed condition = step(i.uv.x, (drawPosX + 1) * DISP_SCALE_X)
                    * step(i.uv.y, 1 - drawPosY * DISP_SCALE_Y)
                    * step(drawPosX * DISP_SCALE_X, i.uv.x)
                    * step(1 - (drawPosY + 1) * DISP_SCALE_Y, i.uv.y);

                return baseTex * (1 - condition) + addTex * condition;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(0,0,0,0);

                min12int loopCnt;
                bool skipZero = true;

                min12int digit[3];

                min16uint number = round(unity_DeltaTime.w);

                digit[0] = fmod(number / 100, 10);
                digit[1] = fmod(number / 10, 10);
                digit[2] = fmod(number, 10);

                for (loopCnt = 0; loopCnt < 3; loopCnt++) {
                    if (skipZero && (digit[loopCnt] != 0 || loopCnt == 2)) {
                        skipZero = false;
                    }
                    if (!skipZero) {
                        col = drawCharacter(i, col, loopCnt, 0, digit[loopCnt] & 3, digit[loopCnt] >> 2);
                    }
                }
                clip(col.r - 0.5);
               return col;
            }
            ENDCG
        }
    }
}
