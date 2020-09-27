Shader "Unlit/Gnomonic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100


        // Before antipode
        Pass
        {
            // Somehow my triangles are ending up reversed.
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            static const float TWOPI = 6.28318530718f;

            float4x4 lqtom (float4 q) // left quaternion to matrix
            {
                return float4x4 (
                         q.w, -q.z,  q.y,  q.x,
                         q.z,  q.w, -q.x,  q.y,
                        -q.y,  q.x,  q.w,  q.z,
                        -q.x, -q.y, -q.z,  q.w);
            }

            float4x4 rqtom (float4 q) // right quaternion to matrix
            {
                return float4x4 (
                         q.w,  q.z, -q.y,  q.x,
                        -q.z,  q.w,  q.x,  q.y,
                         q.y, -q.x,  q.w,  q.z,
                        -q.x, -q.y, -q.z,  q.w);
            }
            // fun fact: left isoclinic matrices commute with right isoclinic matrices
            float4 qprod (float4 q, float4 p) // quaternion product
            {
                return mul(lqtom(q), p);
            }

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 pos : TEXCOORD1;
                float4 nor : TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_Position;
                float bright : COLOR0;
                float fog : TEXCOORD1;
            };

            struct target
            {
                fixed4 col : SV_Target;
                float depth : SV_Depth;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float4x4 _View;
            uniform float4x4 _Model;
            uniform float4x4 _Projection;
            uniform fixed4 _Color;

            v2f vert (appdata v) {
                v2f o;

                float4 pos = mul(_Model, v.pos); // world space
                float4 nor = mul(_Model, v.nor);
                float4 light = qprod(float4(0, 1, 0, 0), pos); // isoclinic lighting
                o.bright = (dot(light, nor) + 2) / 3;

                pos = mul(_View, pos); // camera space

                // sign flipping
                pos.x *= -1;
                pos.z *= -1;
                o.vertex = mul(_Projection, pos); // projected space

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // I don't understand Unity's fog system, so I replaced it with a manual fog. I assume you understand it, so I'll let you re-add it, if applicable.
                float dist = acos(normalize(pos).w);
                o.fog = exp(-dist * 0.3);

                return o;
            }

            target frag (v2f i)
            {
                target o;
                o.col = _Color * i.bright * tex2D(_MainTex, i.uv);
                o.col = o.col * i.fog + fixed4(0.5, 0.5, 0.5, 1) * (1 - i.fog);

                #if defined(UNITY_REVERSED_Z)
                float near_plane = 1.0;
                #else
                float near_plane = UNITY_NEAR_CLIP_VALUE;
                #endif

                // Shove this to the front half of the depth buffer.
                o.depth = (i.vertex.z + 2.0 * near_plane) / 3.0;
                o.depth = i.vertex.z;

                return o;
            }

            ENDCG
        }

        // Beyond antipode
        Pass
        {
            // Somehow my triangles are ending up reversed.
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            static const float TWOPI = 6.28318530718f;

            float4x4 lqtom (float4 q) // left quaternion to matrix
            {
                return float4x4 (
                         q.w, -q.z,  q.y,  q.x,
                         q.z,  q.w, -q.x,  q.y,
                        -q.y,  q.x,  q.w,  q.z,
                        -q.x, -q.y, -q.z,  q.w);
            }

            float4x4 rqtom (float4 q) // right quaternion to matrix
            {
                return float4x4 (
                         q.w,  q.z, -q.y,  q.x,
                        -q.z,  q.w,  q.x,  q.y,
                         q.y, -q.x,  q.w,  q.z,
                        -q.x, -q.y, -q.z,  q.w);
            }
            // fun fact: left isoclinic matrices commute with right isoclinic matrices
            float4 qprod (float4 q, float4 p) // quaternion product
            {
                return mul(lqtom(q), p);
            }

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 pos : TEXCOORD1;
                float4 nor : TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_Position;
                float bright : COLOR0;
                float fog : TEXCOORD1;
            };

            struct target
            {
                fixed4 col : SV_Target;
                float depth : SV_Depth;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float4x4 _View;
            uniform float4x4 _Model;
            uniform float4x4 _Projection;
            uniform fixed4 _Color;

            v2f vert (appdata v) {
                v2f o;

                float4 pos = mul(_Model, v.pos); // world space
                float4 nor = mul(_Model, v.nor);
                float4 light = qprod(float4(0, 1, 0, 0), pos); // isoclinic lighting
                o.bright = (dot(light, nor) + 2) / 3;

                pos = -mul(_View, pos); // camera space

                // sign flipping
                pos.x *= -1;
                pos.z *= -1;
                o.vertex = mul(_Projection, pos); // projected space

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // I don't understand Unity's fog system, so I replaced it with a manual fog. I assume you understand it, so I'll let you re-add it, if applicable.
                float dist = 3.14159265358979f + acos(normalize(pos).w);
                o.fog = exp(-dist * 0.3);

                return o;
            }

            target frag (v2f i)
            {
                target o;
                o.col = _Color * i.bright * tex2D(_MainTex, i.uv);
                o.col = o.col * i.fog + fixed4(0.5, 0.5, 0.5, 1) * (1 - i.fog);

                #if defined(UNITY_REVERSED_Z)
                float far_plane = UNITY_NEAR_CLIP_VALUE;
                #else
                float far_plane = 1.0;
                #endif

                // Shove this to the back half of the depth buffer.
                o.depth = (i.vertex.z + 2.0 * far_plane) / 3.0;

                return o;
            }

            ENDCG
        }
    }
}
