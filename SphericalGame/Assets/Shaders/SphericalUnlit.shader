Shader "Unlit/SphericalUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct appdata members rquat)
#pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float4 lquat : TEXCOORD1;
                float4 rquat : TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float4x4 _View;
            uniform float4x4 _Model;

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

            v2f vert (appdata v)
            {
                v2f o;

                float4 pos = qprod(v.lquat, v.rquat); // world space
                float4 nor = qprod(qprod(v.lquat, float4(0, 0, 1, 0)), v.rquat); // the point to which the normal is facing
                pos = mul(_View, mul(_Model, pos)); // camera space
                nor = mul(_View, mul(_Model, nor));
                float len = length(pos.xyz); // length of q.xyz pre-projection
                float dis = acos(pos.w); // distance from camera in tangent space
                float3 tpos = pos.xyz / len * dis; // tangent space
                float3 tnor = nor.xyz / len * dis - pos.xyz / len / len * (nor.w + dot(pos.xyz, nor.xyz) * dis / len); // there's some ugly calculus behind this, let's hope it's right

                o.vertex = mul(UNITY_MATRIX_P, float4(tpos, 1.0f)); // projected space
                o.normal = normalize(tnor);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                // fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col = fixed4(i.normal, 1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
