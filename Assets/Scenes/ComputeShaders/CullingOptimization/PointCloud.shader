Shader "Custom/PointCloud"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct PointData
            {
                float3 position;
                float size;
                float4 color;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END
            
            StructuredBuffer<PointData> _AllPoints;
            StructuredBuffer<uint> _VisiblePoints;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            Varyings vert(Attributes input, uint instanceID : SV_InstanceID)
            {
                Varyings output = (Varyings)0;
                
                // Get the index of the visible point
                uint pointIndex = _VisiblePoints[instanceID];
                
                // Get point data
                PointData pointData = _AllPoints[pointIndex];
                
                // Scale mesh by point size
                float3 positionOS = input.positionOS.xyz * pointData.size;
                
                // Transform from object to world space (use point position as world origin)
                float3 positionWS = pointData.position + positionOS;
                
                // Transform from world to clip space
                output.positionCS = mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
                output.uv = input.uv;
                output.color = pointData.color * _BaseColor;
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
        
        // Add a shadow caster pass if needed
    }
} 