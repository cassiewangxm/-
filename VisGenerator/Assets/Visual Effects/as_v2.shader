Shader "Hidden/VFX/as_v2/System 1/Output Particle Lit Mesh"
{
	SubShader
	{	
		Tags { "Queue"="Geometry+0" "IgnoreProjector"="False" "RenderType"="Opaque" }
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		ZTest LEqual
		ZWrite On
		Cull Back
		
	
			
		HLSLINCLUDE
		
		#define NB_THREADS_PER_GROUP 64
		#define HAS_ATTRIBUTES 1
		#define VFX_PASSDEPTH_ACTUAL (0)
		#define VFX_PASSDEPTH_MOTION_VECTOR (1)
		#define VFX_PASSDEPTH_SELECTION (2)
		#define VFX_USE_PARTICLEID_CURRENT 1
		#define VFX_USE_SCALEY_CURRENT 1
		#define VFX_USE_POSITION_CURRENT 1
		#define VFX_USE_COLOR_CURRENT 1
		#define VFX_USE_ALPHA_CURRENT 1
		#define VFX_USE_ALIVE_CURRENT 1
		#define VFX_USE_AXISX_CURRENT 1
		#define VFX_USE_AXISY_CURRENT 1
		#define VFX_USE_AXISZ_CURRENT 1
		#define VFX_USE_ANGLEX_CURRENT 1
		#define VFX_USE_ANGLEY_CURRENT 1
		#define VFX_USE_ANGLEZ_CURRENT 1
		#define VFX_USE_PIVOTX_CURRENT 1
		#define VFX_USE_PIVOTY_CURRENT 1
		#define VFX_USE_PIVOTZ_CURRENT 1
		#define VFX_USE_SIZE_CURRENT 1
		#define VFX_USE_SCALEX_CURRENT 1
		#define VFX_USE_SCALEZ_CURRENT 1
		#define VFX_COLORMAPPING_DEFAULT 1
		#define IS_OPAQUE_PARTICLE 1
		#define VFX_SHADERGRAPH 1
		#define HAS_SHADERGRAPH_PARAM_BASECOLOR 1
		#define HAS_SHADERGRAPH_PARAM_ALPHA 1
		#define HAS_SHADERGRAPH_PARAM_METALLIC 1
		#define HAS_SHADERGRAPH_PARAM_SMOOTHNESS 1
		#define HAS_SHADERGRAPH_PARAM_EMISSIVE 1
		#define HAS_SHADERGRAPH_PARAM_NORMAL 1
		#define SHADERGRAPH_NEEDS_NORMAL_GBUFFER 1
		#define SHADERGRAPH_NEEDS_TANGENT_GBUFFER 1
		#define SHADERGRAPH_NEEDS_NORMAL_FORWARD 1
		#define SHADERGRAPH_NEEDS_TANGENT_FORWARD 1
		#define SHADERGRAPH_NEEDS_NORMAL_DEPTHONLY 1
		#define SHADERGRAPH_NEEDS_TANGENT_DEPTHONLY 1
		#define VFX_NEEDS_POSWS_INTERPOLATOR 1
		#define HDRP_LIT 1
		#define IS_OPAQUE_NOT_SIMPLE_LIT_PARTICLE 1
		
		
		
		
		
		
		
		
		
		
		
		
		
		#define VFX_LOCAL_SPACE 1
		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXDefines.hlsl"
		

		CBUFFER_START(parameters)
		    float Size_b;
		    float uniform_c;
		    float Vector1_1FF39C4B;
		    uint PADDING_0;
		CBUFFER_END
		
		struct Attributes
		{
		    uint particleId;
		    float scaleY;
		    float3 position;
		    float3 color;
		    float alpha;
		    bool alive;
		    float3 axisX;
		    float3 axisY;
		    float3 axisZ;
		    float angleX;
		    float angleY;
		    float angleZ;
		    float pivotX;
		    float pivotY;
		    float pivotZ;
		    float size;
		    float scaleX;
		    float scaleZ;
		};
		
		struct SourceAttributes
		{
		};
		
		Texture2D texture_b;
		SamplerState samplertexture_b;
		float4 texture_b_TexelSize;
		Texture2D Texture2D_64C137FF;
		SamplerState samplerTexture2D_64C137FF;
		float4 Texture2D_64C137FF_TexelSize;
		RWTexture2D<float4> RaycastTexture;
		SamplerState samplerRaycastTexture;
		float4 RaycastTexture_TexelSize;
		

		
		#define VFX_NEEDS_COLOR_INTERPOLATOR (VFX_USE_COLOR_CURRENT || VFX_USE_ALPHA_CURRENT)
		#if HAS_STRIPS
		#define VFX_OPTIONAL_INTERPOLATION 
		#else
		#define VFX_OPTIONAL_INTERPOLATION nointerpolation
		#endif
		
		ByteAddressBuffer attributeBuffer;	
		
		#if VFX_HAS_INDIRECT_DRAW
		StructuredBuffer<uint> indirectBuffer;	
		#endif	
		
		#if USE_DEAD_LIST_COUNT
		ByteAddressBuffer deadListCount;
		#endif
		
		#if HAS_STRIPS
		Buffer<uint> stripDataBuffer;
		#endif
		
		#if WRITE_MOTION_VECTOR_IN_FORWARD || USE_MOTION_VECTORS_PASS
		ByteAddressBuffer elementToVFXBufferPrevious;
		#endif
		
		CBUFFER_START(outputParams)
			float nbMax;
			float systemSeed;
		CBUFFER_END
		
		// Helper macros to always use a valid instanceID
		#if defined(UNITY_STEREO_INSTANCING_ENABLED)
			#define VFX_DECLARE_INSTANCE_ID     UNITY_VERTEX_INPUT_INSTANCE_ID
			#define VFX_GET_INSTANCE_ID(i)      unity_InstanceID
		#else
			#define VFX_DECLARE_INSTANCE_ID     uint instanceID : SV_InstanceID;
			#define VFX_GET_INSTANCE_ID(i)      i.instanceID
		#endif
		
		ENDHLSL
		

		Pass
		{		
			Tags { "LightMode"="SceneSelectionPass" }
		
			ZWrite On
			Blend Off
			
			HLSLPROGRAM
			#define VFX_PASSDEPTH VFX_PASSDEPTH_SELECTION
			
			#pragma target 4.5
			#define UNITY_MATERIAL_LIT
			#pragma multi_compile _ WRITE_NORMAL_BUFFER
			
			struct ps_input
			{		
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;	
			    #if VFX_SHADERGRAPH_HAS_UV1
			    float4 uv1 : COLOR2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    float4 uv2 : COLOR3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    float4 uv3 : COLOR4;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    float4 vertexColor : COLOR1;
			    #endif
				#if USE_ALPHA_TEST || USE_FLIPBOOK_INTERPOLATION || VFX_USE_ALPHA_CURRENT
				// x: alpha threshold
				// y: frame blending factor
				// z: alpha
				// w: smoothness
				nointerpolation float4 builtInInterpolants : TEXCOORD1;
				#endif
				#if USE_FLIPBOOK_MOTIONVECTORS
				// x: motion vector scale u
				// y: motion vector scale v
				nointerpolation float2 builtInInterpolants2 : TEXCOORD3;
				#endif
				#if defined(WRITE_NORMAL_BUFFER) || SHADERGRAPH_NEEDS_NORMAL_DEPTHONLY
				float3 normal : TEXCOORD4;
				#if SHADERGRAPH_NEEDS_TANGENT_DEPTHONLY
				float4 tangent : TEXCOORD5;
				#endif
				#endif
				
				#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
				float4 cPosPrevious : TEXCOORD6;
				float4 cPosNonJiterred : TEXCOORD7;
				#endif
			    VFX_OPTIONAL_INTERPOLATION float4 Color_DCF944C2 : NORMAL0;
			    VFX_OPTIONAL_INTERPOLATION float2 RaycastTextureUV : NORMAL1;
			    

				
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			#define VFX_VARYING_PS_INPUTS ps_input
			#define VFX_VARYING_POSCS pos
			#define VFX_VARYING_ALPHA builtInInterpolants.z
			#define VFX_VARYING_ALPHATHRESHOLD builtInInterpolants.x
			#define VFX_VARYING_FRAMEBLEND builtInInterpolants.y
			#define VFX_VARYING_MOTIONVECTORSCALE builtInInterpolants2.xy
			#define VFX_VARYING_UV uv
			
			#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
			#define VFX_VARYING_VELOCITY_CPOS cPosNonJiterred
			#define VFX_VARYING_VELOCITY_CPOS_PREVIOUS cPosPrevious
			#endif
			
			#if defined(WRITE_NORMAL_BUFFER) || SHADERGRAPH_NEEDS_NORMAL_DEPTHONLY
			#define VFX_VARYING_NORMAL normal
			#endif
			#ifdef WRITE_NORMAL_BUFFER
			#define VFX_VARYING_SMOOTHNESS builtInInterpolants.w
			#endif
			#if SHADERGRAPH_NEEDS_TANGENT_DEPTHONLY
			#define VFX_VARYING_TANGENT tangent
			#endif
					
			
					
			#if !(defined(VFX_VARYING_PS_INPUTS) && defined(VFX_VARYING_POSCS))
			#error VFX_VARYING_PS_INPUTS, VFX_VARYING_POSCS and VFX_VARYING_UV must be defined.
			#endif
			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXCommon.hlsl"
			#include "Packages/com.unity.visualeffectgraph/Shaders/VFXCommon.hlsl"
			

			void SetAttribute_65DEC940(inout float pivotX, inout float pivotY, inout float pivotZ, float3 Pivot) /*attribute:pivot Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    pivotX = Pivot.x;
			    pivotY = Pivot.y;
			    pivotZ = Pivot.z;
			}
			void SetAttribute_3278B22F(inout float size, float Size) /*attribute:size Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    size = Size;
			}
			void SetAttribute_D5151640(inout float scaleX, inout float scaleZ, float2 Scale) /*attribute:scale Composition:Overwrite Source:Slot Random:Off channels:XZ */
			{
			    scaleX = Scale.x;
			    scaleZ = Scale.y;
			}
			void SetAttribute_CAC29747(inout float3 position, float3 Position) /*attribute:position Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    position = Position;
			}
			void SetAttribute_D5151645(inout float scaleY, float Scale) /*attribute:scale Composition:Overwrite Source:Slot Random:Off channels:Y */
			{
			    scaleY = Scale.x;
			}
			

			
			struct vs_input
			{
				float3 pos : POSITION;
				float2 uv : TEXCOORD0;
			    #if VFX_SHADERGRAPH_HAS_UV1
			    float4 uv1 : TEXCOORD1;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    float4 uv2 : TEXCOORD2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    float4 uv3 : TEXCOORD3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    float4 vertexColor : COLOR;
			    #endif
				float3 normal : NORMAL;
				#if defined(VFX_VARYING_TANGENT) || SHADERGRAPH_HAS_NORMAL
				float4 tangent : TANGENT;
				#endif
				VFX_DECLARE_INSTANCE_ID
			};
			
			#pragma vertex vert
			VFX_VARYING_PS_INPUTS vert(vs_input i)
			{
			    VFX_VARYING_PS_INPUTS o = (VFX_VARYING_PS_INPUTS)0;
			
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			
				uint index = VFX_GET_INSTANCE_ID(i);	
			
				
						uint deadCount = 0;
						#if USE_DEAD_LIST_COUNT
						deadCount = deadListCount.Load(0);
						#endif	
						if (index >= asuint(nbMax) - deadCount)
						#if USE_GEOMETRY_SHADER
							return; // cull
						#else
							return o; // cull
						#endif
						
						Attributes attributes = (Attributes)0;
						SourceAttributes sourceAttributes = (SourceAttributes)0;
						
						#if VFX_HAS_INDIRECT_DRAW
						index = indirectBuffer[index];
						attributes.particleId = (attributeBuffer.Load((index * 0x2 + 0x0) << 2));
						attributes.scaleY = asfloat(attributeBuffer.Load((index * 0x2 + 0x1) << 2));
						attributes.position = float3(0, 0, 0);
						attributes.color = float3(1, 1, 1);
						attributes.alpha = (float)1;
						attributes.alive = (bool)true;
						attributes.axisX = float3(1, 0, 0);
						attributes.axisY = float3(0, 1, 0);
						attributes.axisZ = float3(0, 0, 1);
						attributes.angleX = (float)0;
						attributes.angleY = (float)0;
						attributes.angleZ = (float)0;
						attributes.pivotX = (float)0;
						attributes.pivotY = (float)0;
						attributes.pivotZ = (float)0;
						attributes.size = (float)0.100000001;
						attributes.scaleX = (float)1;
						attributes.scaleZ = (float)1;
						
				
						#else
						attributes.alive = (bool)true;
						
				
						#if !HAS_STRIPS
						if (!attributes.alive)
							return o;
						#endif
							
						attributes.particleId = (attributeBuffer.Load((index * 0x2 + 0x0) << 2));
						attributes.scaleY = asfloat(attributeBuffer.Load((index * 0x2 + 0x1) << 2));
						attributes.position = float3(0, 0, 0);
						attributes.color = float3(1, 1, 1);
						attributes.alpha = (float)1;
						attributes.axisX = float3(1, 0, 0);
						attributes.axisY = float3(0, 1, 0);
						attributes.axisZ = float3(0, 0, 1);
						attributes.angleX = (float)0;
						attributes.angleY = (float)0;
						attributes.angleZ = (float)0;
						attributes.pivotX = (float)0;
						attributes.pivotY = (float)0;
						attributes.pivotZ = (float)0;
						attributes.size = (float)0.100000001;
						attributes.scaleX = (float)1;
						attributes.scaleZ = (float)1;
						
				
						#endif
						
						// Initialize built-in needed attributes
						#if HAS_STRIPS
						InitStripAttributes(index, attributes, stripData);
						#endif
						
				{
				    SetAttribute_65DEC940( /*inout */attributes.pivotX,  /*inout */attributes.pivotY,  /*inout */attributes.pivotZ, float3(0, -0.5, 0));
				}
				SetAttribute_3278B22F( /*inout */attributes.size, Size_b);
				{
				    SetAttribute_D5151640( /*inout */attributes.scaleX,  /*inout */attributes.scaleZ, float2(0.00999999978, 0.00999999978));
				}
				{
				    int tmp_y = (int)attributes.particleId;
				    int tmp_ba = tmp_y / (int)256;
				    float tmp_bb = (float)tmp_ba;
				    float tmp_bc = floor(tmp_bb);
				    uint tmp_bd = (uint)tmp_y;
				    uint tmp_bf = tmp_bd / (uint)256;
				    uint tmp_bg = tmp_bf * (uint)256;
				    uint tmp_bh = tmp_bd - tmp_bg;
				    float tmp_bi = (float)tmp_bh;
				    float2 tmp_bj = float2(tmp_bc, tmp_bi);
				    float2 tmp_bl = tmp_bj / float2(256, 256);
				    float2 tmp_bn = tmp_bl * float2(64, 64);
				    float2 tmp_bp = tmp_bn + float2(0.125, 0.125);
				    float2 tmp_bq = tmp_bp / float2(64, 64);
				    float2 tmp_bs = tmp_bq * float2(640, 640);
				    float tmp_bt = tmp_bs[0];
				    float tmp_bv = tmp_bs[1];
				    float3 tmp_bw = float3(tmp_bt, (float)0, tmp_bv);
				    SetAttribute_CAC29747( /*inout */attributes.position, tmp_bw);
				}
				{
				    int tmp_y = (int)attributes.particleId;
				    int tmp_ba = tmp_y / (int)256;
				    float tmp_bb = (float)tmp_ba;
				    float tmp_bc = floor(tmp_bb);
				    uint tmp_bd = (uint)tmp_y;
				    uint tmp_bf = tmp_bd / (uint)256;
				    uint tmp_bg = tmp_bf * (uint)256;
				    uint tmp_bh = tmp_bd - tmp_bg;
				    float tmp_bi = (float)tmp_bh;
				    float2 tmp_bj = float2(tmp_bc, tmp_bi);
				    float2 tmp_bl = tmp_bj / float2(256, 256);
				    float2 tmp_bn = tmp_bl * float2(64, 64);
				    float2 tmp_bp = tmp_bn + float2(0.125, 0.125);
				    float2 tmp_bq = tmp_bp / float2(64, 64);
				    float4 tmp_bs = SampleTexture(VFX_SAMPLER(texture_b),tmp_bq,(float)0);
				    float tmp_bt = tmp_bs[0];
				    float tmp_bu = tmp_bt * uniform_c;
				    float tmp_bw = tmp_bu + (float)0.00999999978;
				    SetAttribute_D5151645( /*inout */attributes.scaleY, tmp_bw);
				}
				

						
				if (!attributes.alive)
					return o;
				
				o.VFX_VARYING_UV.xy = i.uv;
			    
			    #if VFX_SHADERGRAPH_HAS_UV1
			    o.uv1 = i.uv1;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    o.uv2 = i.uv2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    o.uv3 = i.uv3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    o.vertexColor = i.vertexColor;
			    #endif
				
				
						float3 size3 = float3(attributes.size,attributes.size,attributes.size);
						#if VFX_USE_SCALEX_CURRENT
						size3.x *= attributes.scaleX;
						#endif
						#if VFX_USE_SCALEY_CURRENT
						size3.y *= attributes.scaleY;
						#endif
						#if VFX_USE_SCALEZ_CURRENT
						size3.z *= attributes.scaleZ;
						#endif
						
				
				float3 inputVertexPosition = i.pos;
				float4x4 elementToVFX = GetElementToVFXMatrix(
					attributes.axisX,
					attributes.axisY,
					attributes.axisZ,
					float3(attributes.angleX,attributes.angleY,attributes.angleZ),
					float3(attributes.pivotX,attributes.pivotY,attributes.pivotZ),
					size3,
					attributes.position);
					
				float3 vPos = mul(elementToVFX,float4(inputVertexPosition,1.0f)).xyz;
				float4 csPos = TransformPositionVFXToClip(vPos);
				o.VFX_VARYING_POSCS = csPos;
				
				float3 normalWS = normalize(TransformDirectionVFXToWorld(mul((float3x3)elementToVFX, i.normal)));
				#ifdef VFX_VARYING_NORMAL // TODO Should use inverse transpose
				o.VFX_VARYING_NORMAL = normalWS;
				#endif
				#ifdef VFX_VARYING_TANGENT
				o.VFX_VARYING_TANGENT = float4(normalize(TransformDirectionVFXToWorld(mul((float3x3)elementToVFX,i.tangent.xyz))),i.tangent.w);
				#endif
			
				
						#if defined(VFX_VARYING_VELOCITY_CPOS) && defined(VFX_VARYING_VELOCITY_CPOS_PREVIOUS)
						float4x4 previousElementToVFX = (float4x4)0;
						previousElementToVFX[3] = float4(0,0,0,1);
						
						UNITY_UNROLL
						for (int itIndexMatrixRow = 0; itIndexMatrixRow < 3; ++itIndexMatrixRow)
						{
							UNITY_UNROLL
							for (int itIndexMatrixCol = 0; itIndexMatrixCol < 4; ++itIndexMatrixCol)
							{
								uint itIndexMatrix = itIndexMatrixCol * 4 + itIndexMatrixRow;
								uint read = elementToVFXBufferPrevious.Load((index * 16 + itIndexMatrix) << 2);
								previousElementToVFX[itIndexMatrixRow][itIndexMatrixCol] = asfloat(read);
							}
						}
						
						uint previousFrameIndex = elementToVFXBufferPrevious.Load((index * 16 + 15) << 2);
						o.VFX_VARYING_VELOCITY_CPOS = o.VFX_VARYING_VELOCITY_CPOS_PREVIOUS = float4(0.0f, 0.0f, 0.0f, 1.0f);
						if (asuint(currentFrameIndex) - previousFrameIndex == 1u)
						{
							float3 oldvPos = mul(previousElementToVFX,float4(inputVertexPosition, 1.0f)).xyz;
							o.VFX_VARYING_VELOCITY_CPOS_PREVIOUS = TransformPositionVFXToPreviousClip(oldvPos);
							o.VFX_VARYING_VELOCITY_CPOS = TransformPositionVFXToNonJitteredClip(vPos);
						}
						#endif
						
				
				
						#if VFX_USE_COLOR_CURRENT && defined(VFX_VARYING_COLOR)
						o.VFX_VARYING_COLOR = attributes.color;
						#endif
						#if VFX_USE_ALPHA_CURRENT && defined(VFX_VARYING_ALPHA) 
						o.VFX_VARYING_ALPHA = attributes.alpha;
						#endif
						
						#ifdef VFX_VARYING_EXPOSUREWEIGHT
						
						o.VFX_VARYING_EXPOSUREWEIGHT = exposureWeight;
						#endif
						
						#if USE_SOFT_PARTICLE && defined(VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE)
						
						o.VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE = invSoftParticlesFadeDistance;
						#endif
						
						#if (USE_ALPHA_TEST || WRITE_MOTION_VECTOR_IN_FORWARD) && (!VFX_SHADERGRAPH || !HAS_SHADERGRAPH_PARAM_ALPHATHRESHOLD) && defined(VFX_VARYING_ALPHATHRESHOLD)
						float alphaThreshold = (float)0;
						{
						    
						    alphaThreshold = (float)0.5;
						}
						
				
						o.VFX_VARYING_ALPHATHRESHOLD = alphaThreshold;
						#endif
						
						#if USE_UV_SCALE_BIAS
						
						
						#if defined (VFX_VARYING_UV)
						o.VFX_VARYING_UV.xy = o.VFX_VARYING_UV.xy * uvScale + uvBias;
						#endif
						#endif
						
						#if defined(VFX_VARYING_POSWS)
						o.VFX_VARYING_POSWS = TransformPositionVFXToWorld(vPos);
						#endif
						
				
				
						#if USE_FLIPBOOK && defined(VFX_VARYING_UV)
						
						
						VFXUVData uvData = GetUVData(flipBookSize, invFlipBookSize, o.VFX_VARYING_UV.xy, attributes.texIndex);
						o.VFX_VARYING_UV.xy = uvData.uvs.xy;
						#if USE_FLIPBOOK_INTERPOLATION && defined(VFX_VARYING_UV) && defined (VFX_VARYING_FRAMEBLEND)
						o.VFX_VARYING_UV.zw = uvData.uvs.zw;
						o.VFX_VARYING_FRAMEBLEND = uvData.blend;
						#if USE_FLIPBOOK_MOTIONVECTORS && defined(VFX_VARYING_MOTIONVECTORSCALE)
						
						o.VFX_VARYING_MOTIONVECTORSCALE = motionVectorScale * invFlipBookSize;
						#endif
						#endif
						#endif
						
			
				
							
							
			    
			    float4 Color_DCF944C2__ = (float4)0;{
			        
			        Color_DCF944C2__ = float4(0, 1, 0.96201396, 0);
			    }
			    o.Color_DCF944C2 = Color_DCF944C2__;float2 RaycastTextureUV__ = (float2)0;{
			        int tmp_y = (int)attributes.particleId;
			        int tmp_ba = tmp_y / (int)256;
			        float tmp_bb = (float)tmp_ba;
			        float tmp_bc = floor(tmp_bb);
			        uint tmp_bd = (uint)tmp_y;
			        uint tmp_bf = tmp_bd / (uint)256;
			        uint tmp_bg = tmp_bf * (uint)256;
			        uint tmp_bh = tmp_bd - tmp_bg;
			        float tmp_bi = (float)tmp_bh;
			        float2 tmp_bj = float2(tmp_bc, tmp_bi);
			        float2 tmp_bl = tmp_bj / float2(256, 256);
			        
			        RaycastTextureUV__ = tmp_bl;
			    }
			    o.RaycastTextureUV = RaycastTextureUV__;

				
				return o;
			}
			
			
			
			#include "Packages/com.unity.visualeffectgraph/Shaders/VFXCommonOutput.hlsl"
			
			
			
				
			#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
			#define SHADERPASS SHADERPASS_MOTION_VECTORS
			#else
			#define SHADERPASS SHADERPASS_DEPTH_ONLY
			#endif
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXLit.hlsl"
			
			#ifndef VFX_SHADERGRAPH
			
			void VFXGetHDRPLitData(out SurfaceData surfaceData, out BuiltinData builtinData, out BSDFData bsdfData, out PreLightData preLightData, VFX_VARYING_PS_INPUTS i, float3 normalWS, const VFXUVData uvData, uint2 tileIndex)
			{	
				#if HDRP_MATERIAL_TYPE_TRANSLUCENT
				 // Loads diffusion profile
				#else
				const uint diffusionProfileHash = 0;
				#endif
				
				float3 posRWS = VFXGetPositionRWS(i);
				float4 posSS = i.VFX_VARYING_POSCS;
				PositionInputs posInput = GetPositionInput(posSS.xy, _ScreenSize.zw, posSS.z, posSS.w, posRWS, tileIndex);
				
				float alpha;
				surfaceData = VFXGetSurfaceData(i,normalWS,uvData,diffusionProfileHash,alpha);	
				bsdfData = ConvertSurfaceDataToBSDFData(posSS.xy, surfaceData);
			
				preLightData = GetPreLightData(GetWorldSpaceNormalizeViewDir(posRWS),posInput,bsdfData);
				
				preLightData.diffuseFGD = 1.0f;
			    //TODO: investigate why this is needed
			    preLightData.coatPartLambdaV = 0;
			    preLightData.coatIblR = 0;
			    preLightData.coatIblF = 0;
			    
				builtinData = VFXGetBuiltinData(i,posInput,surfaceData,uvData,alpha);
			}
			
			void VFXGetHDRPLitData(out SurfaceData surfaceData, out BuiltinData builtinData, VFX_VARYING_PS_INPUTS i, float3 normalWS, const VFXUVData uvData)
			{
				BSDFData bsdfData = (BSDFData)0;
				PreLightData preLightData = (PreLightData)0;
				preLightData.diffuseFGD = 1.0f;
				VFXGetHDRPLitData(surfaceData,builtinData,bsdfData,preLightData,i,normalWS,uvData,uint2(0,0));
			}
			
			#endif
			
			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXLitPixelOutput.hlsl"
			
			
			
						
			
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
					#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinGIUtilities.hlsl"
					#ifndef SHADERPASS
					#error Shaderpass should be defined at this stage.
					#endif
					#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
					
			
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			struct SG_Input_abfd42c13223447bfbe3d7528d18d99d
			{
			};
			
			struct SG_Output_abfd42c13223447bfbe3d7528d18d99d
			{
			    float3 Normal_8;
			    float Alpha_4;
			};
			
			SG_Output_abfd42c13223447bfbe3d7528d18d99d SG_Evaluate_abfd42c13223447bfbe3d7528d18d99d(
			    SG_Input_abfd42c13223447bfbe3d7528d18d99d IN)
			{
			    // Visual Effect Master
			    SG_Output_abfd42c13223447bfbe3d7528d18d99d OUT;
			    OUT.Normal_8 = float3 (0, 0, 1);
			    OUT.Alpha_4 = 1;
			    return OUT;
			}
			

				
			#if VFX_PASSDEPTH == VFX_PASSDEPTH_SELECTION
			int _ObjectId;
			int _PassValue;
			#endif
			
			#pragma fragment frag
			void frag(ps_input i
			#if USE_DOUBLE_SIDED
				, bool frontFace : SV_IsFrontFace
			#endif
			
			#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
				, out float4 outMotionVector : SV_Target0
				#ifdef WRITE_NORMAL_BUFFER
					, out float4 outNormalBuffer : SV_Target1
				#endif
			#else
				#ifdef WRITE_NORMAL_BUFFER
					, out float4 outNormalBuffer : SV_Target0
				#elif VFX_PASSDEPTH == VFX_PASSDEPTH_SELECTION
					, out float4 outColor : SV_Target0
				#endif
			#endif
				)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				VFXTransformPSInputs(i);
				
							#ifdef VFX_VARYING_NORMAL
							#if USE_DOUBLE_SIDED
							const float faceMul = frontFace ? 1.0f : -1.0f;
							#else
							const float faceMul = 1.0f;
							#endif
							
							float3 normalWS = normalize(i.VFX_VARYING_NORMAL * faceMul);
							const VFXUVData uvData = GetUVData(i);
							
							#ifdef VFX_VARYING_TANGENT
							float3 tangentWS = normalize(i.VFX_VARYING_TANGENT.xyz);
							float3 bitangentWS = cross(normalWS,tangentWS) * (i.VFX_VARYING_TANGENT.w * faceMul);
							float3x3 tbn = float3x3(tangentWS,bitangentWS,normalWS);
							
							#if USE_NORMAL_MAP
							float3 n = SampleNormalMap(VFX_SAMPLER(normalMap),uvData);
							float normalScale = 1.0f;
							#ifdef VFX_VARYING_NORMALSCALE
							normalScale = i.VFX_VARYING_NORMALSCALE;
							#endif
							normalWS = normalize(lerp(normalWS,mul(n,tbn),normalScale));
							#endif
							#endif
							#endif
							
				
				#ifdef VFX_SHADERGRAPH
			        float4 Color_DCF944C2 = i.Color_DCF944C2;float2 RaycastTextureUV = i.RaycastTextureUV;
			        //Call Shader Graph
			        SG_Input_abfd42c13223447bfbe3d7528d18d99d INSG = (SG_Input_abfd42c13223447bfbe3d7528d18d99d)0;
			        
			        SG_Output_abfd42c13223447bfbe3d7528d18d99d OUTSG = SG_Evaluate_abfd42c13223447bfbe3d7528d18d99d(INSG);
			        

				
			        float alpha = OUTSG.Alpha_4;
			    #else
				float alpha = VFXGetFragmentColor(i).a;
				#if HDRP_USE_BASE_COLOR_MAP_ALPHA
					alpha *= VFXGetTextureColor(VFX_SAMPLER(baseColorMap),i).a;
				#endif
			    #endif
				VFXClipFragmentColor(alpha,i);
				
				#ifdef WRITE_NORMAL_BUFFER
			        #ifndef VFX_SHADERGRAPH
			            VFXComputePixelOutputToNormalBuffer(i,normalWS,uvData,outNormalBuffer);
			        #else
			           #if HAS_SHADERGRAPH_PARAM_NORMAL
			               float3 n =  OUTSG.Normal_8;
			               normalWS = mul(n,tbn);
			           #endif
			           SurfaceData surface = (SurfaceData)0;
			           
			           surface.normalWS = normalWS;
			           
			           EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surface), i.VFX_VARYING_POSCS.xy, outNormalBuffer);
			        #endif
				#endif
			
				#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
					
							float2 velocity = (i.VFX_VARYING_VELOCITY_CPOS.xy/i.VFX_VARYING_VELOCITY_CPOS.w) - (i.VFX_VARYING_VELOCITY_CPOS_PREVIOUS.xy/i.VFX_VARYING_VELOCITY_CPOS_PREVIOUS.w);
							#if UNITY_UV_STARTS_AT_TOP
								velocity.y = -velocity.y;
							#endif
							float4 encodedMotionVector = 0.0f;
							VFXEncodeMotionVector(velocity * 0.5f, encodedMotionVector);
							
					outMotionVector = encodedMotionVector;
				#elif VFX_PASSDEPTH == VFX_PASSDEPTH_SELECTION
					// We use depth prepass for scene selection in the editor, this code allow to output the outline correctly
					outColor = float4(_ObjectId, _PassValue, 1.0, 1.0);
				#elif VFX_PASSDEPTH == VFX_PASSDEPTH_ACTUAL
					//void
				#else
					#error VFX_PASSDEPTH undefined
				#endif
			}
			
		
		
			ENDHLSL
		}
		

		Pass
		{		
			Tags { "LightMode"="DepthOnly" }
		
			ZWrite On
			Blend Off
			
			HLSLPROGRAM
			#define VFX_PASSDEPTH VFX_PASSDEPTH_ACTUAL
			
			#pragma target 4.5
			#define UNITY_MATERIAL_LIT
			#pragma multi_compile _ WRITE_NORMAL_BUFFER
			
			struct ps_input
			{		
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;	
			    #if VFX_SHADERGRAPH_HAS_UV1
			    float4 uv1 : COLOR2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    float4 uv2 : COLOR3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    float4 uv3 : COLOR4;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    float4 vertexColor : COLOR1;
			    #endif
				#if USE_ALPHA_TEST || USE_FLIPBOOK_INTERPOLATION || VFX_USE_ALPHA_CURRENT
				// x: alpha threshold
				// y: frame blending factor
				// z: alpha
				// w: smoothness
				nointerpolation float4 builtInInterpolants : TEXCOORD1;
				#endif
				#if USE_FLIPBOOK_MOTIONVECTORS
				// x: motion vector scale u
				// y: motion vector scale v
				nointerpolation float2 builtInInterpolants2 : TEXCOORD3;
				#endif
				#if defined(WRITE_NORMAL_BUFFER) || SHADERGRAPH_NEEDS_NORMAL_DEPTHONLY
				float3 normal : TEXCOORD4;
				#if SHADERGRAPH_NEEDS_TANGENT_DEPTHONLY
				float4 tangent : TEXCOORD5;
				#endif
				#endif
				
				#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
				float4 cPosPrevious : TEXCOORD6;
				float4 cPosNonJiterred : TEXCOORD7;
				#endif
			    VFX_OPTIONAL_INTERPOLATION float4 Color_DCF944C2 : NORMAL0;
			    VFX_OPTIONAL_INTERPOLATION float2 RaycastTextureUV : NORMAL1;
			    

				
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			#define VFX_VARYING_PS_INPUTS ps_input
			#define VFX_VARYING_POSCS pos
			#define VFX_VARYING_ALPHA builtInInterpolants.z
			#define VFX_VARYING_ALPHATHRESHOLD builtInInterpolants.x
			#define VFX_VARYING_FRAMEBLEND builtInInterpolants.y
			#define VFX_VARYING_MOTIONVECTORSCALE builtInInterpolants2.xy
			#define VFX_VARYING_UV uv
			
			#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
			#define VFX_VARYING_VELOCITY_CPOS cPosNonJiterred
			#define VFX_VARYING_VELOCITY_CPOS_PREVIOUS cPosPrevious
			#endif
			
			#if defined(WRITE_NORMAL_BUFFER) || SHADERGRAPH_NEEDS_NORMAL_DEPTHONLY
			#define VFX_VARYING_NORMAL normal
			#endif
			#ifdef WRITE_NORMAL_BUFFER
			#define VFX_VARYING_SMOOTHNESS builtInInterpolants.w
			#endif
			#if SHADERGRAPH_NEEDS_TANGENT_DEPTHONLY
			#define VFX_VARYING_TANGENT tangent
			#endif
					
			
					
			#if !(defined(VFX_VARYING_PS_INPUTS) && defined(VFX_VARYING_POSCS))
			#error VFX_VARYING_PS_INPUTS, VFX_VARYING_POSCS and VFX_VARYING_UV must be defined.
			#endif
			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXCommon.hlsl"
			#include "Packages/com.unity.visualeffectgraph/Shaders/VFXCommon.hlsl"
			

			void SetAttribute_65DEC940(inout float pivotX, inout float pivotY, inout float pivotZ, float3 Pivot) /*attribute:pivot Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    pivotX = Pivot.x;
			    pivotY = Pivot.y;
			    pivotZ = Pivot.z;
			}
			void SetAttribute_3278B22F(inout float size, float Size) /*attribute:size Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    size = Size;
			}
			void SetAttribute_D5151640(inout float scaleX, inout float scaleZ, float2 Scale) /*attribute:scale Composition:Overwrite Source:Slot Random:Off channels:XZ */
			{
			    scaleX = Scale.x;
			    scaleZ = Scale.y;
			}
			void SetAttribute_CAC29747(inout float3 position, float3 Position) /*attribute:position Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    position = Position;
			}
			void SetAttribute_D5151645(inout float scaleY, float Scale) /*attribute:scale Composition:Overwrite Source:Slot Random:Off channels:Y */
			{
			    scaleY = Scale.x;
			}
			

			
			struct vs_input
			{
				float3 pos : POSITION;
				float2 uv : TEXCOORD0;
			    #if VFX_SHADERGRAPH_HAS_UV1
			    float4 uv1 : TEXCOORD1;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    float4 uv2 : TEXCOORD2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    float4 uv3 : TEXCOORD3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    float4 vertexColor : COLOR;
			    #endif
				float3 normal : NORMAL;
				#if defined(VFX_VARYING_TANGENT) || SHADERGRAPH_HAS_NORMAL
				float4 tangent : TANGENT;
				#endif
				VFX_DECLARE_INSTANCE_ID
			};
			
			#pragma vertex vert
			VFX_VARYING_PS_INPUTS vert(vs_input i)
			{
			    VFX_VARYING_PS_INPUTS o = (VFX_VARYING_PS_INPUTS)0;
			
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			
				uint index = VFX_GET_INSTANCE_ID(i);	
			
				
						uint deadCount = 0;
						#if USE_DEAD_LIST_COUNT
						deadCount = deadListCount.Load(0);
						#endif	
						if (index >= asuint(nbMax) - deadCount)
						#if USE_GEOMETRY_SHADER
							return; // cull
						#else
							return o; // cull
						#endif
						
						Attributes attributes = (Attributes)0;
						SourceAttributes sourceAttributes = (SourceAttributes)0;
						
						#if VFX_HAS_INDIRECT_DRAW
						index = indirectBuffer[index];
						attributes.particleId = (attributeBuffer.Load((index * 0x2 + 0x0) << 2));
						attributes.scaleY = asfloat(attributeBuffer.Load((index * 0x2 + 0x1) << 2));
						attributes.position = float3(0, 0, 0);
						attributes.color = float3(1, 1, 1);
						attributes.alpha = (float)1;
						attributes.alive = (bool)true;
						attributes.axisX = float3(1, 0, 0);
						attributes.axisY = float3(0, 1, 0);
						attributes.axisZ = float3(0, 0, 1);
						attributes.angleX = (float)0;
						attributes.angleY = (float)0;
						attributes.angleZ = (float)0;
						attributes.pivotX = (float)0;
						attributes.pivotY = (float)0;
						attributes.pivotZ = (float)0;
						attributes.size = (float)0.100000001;
						attributes.scaleX = (float)1;
						attributes.scaleZ = (float)1;
						
				
						#else
						attributes.alive = (bool)true;
						
				
						#if !HAS_STRIPS
						if (!attributes.alive)
							return o;
						#endif
							
						attributes.particleId = (attributeBuffer.Load((index * 0x2 + 0x0) << 2));
						attributes.scaleY = asfloat(attributeBuffer.Load((index * 0x2 + 0x1) << 2));
						attributes.position = float3(0, 0, 0);
						attributes.color = float3(1, 1, 1);
						attributes.alpha = (float)1;
						attributes.axisX = float3(1, 0, 0);
						attributes.axisY = float3(0, 1, 0);
						attributes.axisZ = float3(0, 0, 1);
						attributes.angleX = (float)0;
						attributes.angleY = (float)0;
						attributes.angleZ = (float)0;
						attributes.pivotX = (float)0;
						attributes.pivotY = (float)0;
						attributes.pivotZ = (float)0;
						attributes.size = (float)0.100000001;
						attributes.scaleX = (float)1;
						attributes.scaleZ = (float)1;
						
				
						#endif
						
						// Initialize built-in needed attributes
						#if HAS_STRIPS
						InitStripAttributes(index, attributes, stripData);
						#endif
						
				{
				    SetAttribute_65DEC940( /*inout */attributes.pivotX,  /*inout */attributes.pivotY,  /*inout */attributes.pivotZ, float3(0, -0.5, 0));
				}
				SetAttribute_3278B22F( /*inout */attributes.size, Size_b);
				{
				    SetAttribute_D5151640( /*inout */attributes.scaleX,  /*inout */attributes.scaleZ, float2(0.00999999978, 0.00999999978));
				}
				{
				    int tmp_y = (int)attributes.particleId;
				    int tmp_ba = tmp_y / (int)256;
				    float tmp_bb = (float)tmp_ba;
				    float tmp_bc = floor(tmp_bb);
				    uint tmp_bd = (uint)tmp_y;
				    uint tmp_bf = tmp_bd / (uint)256;
				    uint tmp_bg = tmp_bf * (uint)256;
				    uint tmp_bh = tmp_bd - tmp_bg;
				    float tmp_bi = (float)tmp_bh;
				    float2 tmp_bj = float2(tmp_bc, tmp_bi);
				    float2 tmp_bl = tmp_bj / float2(256, 256);
				    float2 tmp_bn = tmp_bl * float2(64, 64);
				    float2 tmp_bp = tmp_bn + float2(0.125, 0.125);
				    float2 tmp_bq = tmp_bp / float2(64, 64);
				    float2 tmp_bs = tmp_bq * float2(640, 640);
				    float tmp_bt = tmp_bs[0];
				    float tmp_bv = tmp_bs[1];
				    float3 tmp_bw = float3(tmp_bt, (float)0, tmp_bv);
				    SetAttribute_CAC29747( /*inout */attributes.position, tmp_bw);
				}
				{
				    int tmp_y = (int)attributes.particleId;
				    int tmp_ba = tmp_y / (int)256;
				    float tmp_bb = (float)tmp_ba;
				    float tmp_bc = floor(tmp_bb);
				    uint tmp_bd = (uint)tmp_y;
				    uint tmp_bf = tmp_bd / (uint)256;
				    uint tmp_bg = tmp_bf * (uint)256;
				    uint tmp_bh = tmp_bd - tmp_bg;
				    float tmp_bi = (float)tmp_bh;
				    float2 tmp_bj = float2(tmp_bc, tmp_bi);
				    float2 tmp_bl = tmp_bj / float2(256, 256);
				    float2 tmp_bn = tmp_bl * float2(64, 64);
				    float2 tmp_bp = tmp_bn + float2(0.125, 0.125);
				    float2 tmp_bq = tmp_bp / float2(64, 64);
				    float4 tmp_bs = SampleTexture(VFX_SAMPLER(texture_b),tmp_bq,(float)0);
				    float tmp_bt = tmp_bs[0];
				    float tmp_bu = tmp_bt * uniform_c;
				    float tmp_bw = tmp_bu + (float)0.00999999978;
				    SetAttribute_D5151645( /*inout */attributes.scaleY, tmp_bw);
				}
				

						
				if (!attributes.alive)
					return o;
				
				o.VFX_VARYING_UV.xy = i.uv;
			    
			    #if VFX_SHADERGRAPH_HAS_UV1
			    o.uv1 = i.uv1;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    o.uv2 = i.uv2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    o.uv3 = i.uv3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    o.vertexColor = i.vertexColor;
			    #endif
				
				
						float3 size3 = float3(attributes.size,attributes.size,attributes.size);
						#if VFX_USE_SCALEX_CURRENT
						size3.x *= attributes.scaleX;
						#endif
						#if VFX_USE_SCALEY_CURRENT
						size3.y *= attributes.scaleY;
						#endif
						#if VFX_USE_SCALEZ_CURRENT
						size3.z *= attributes.scaleZ;
						#endif
						
				
				float3 inputVertexPosition = i.pos;
				float4x4 elementToVFX = GetElementToVFXMatrix(
					attributes.axisX,
					attributes.axisY,
					attributes.axisZ,
					float3(attributes.angleX,attributes.angleY,attributes.angleZ),
					float3(attributes.pivotX,attributes.pivotY,attributes.pivotZ),
					size3,
					attributes.position);
					
				float3 vPos = mul(elementToVFX,float4(inputVertexPosition,1.0f)).xyz;
				float4 csPos = TransformPositionVFXToClip(vPos);
				o.VFX_VARYING_POSCS = csPos;
				
				float3 normalWS = normalize(TransformDirectionVFXToWorld(mul((float3x3)elementToVFX, i.normal)));
				#ifdef VFX_VARYING_NORMAL // TODO Should use inverse transpose
				o.VFX_VARYING_NORMAL = normalWS;
				#endif
				#ifdef VFX_VARYING_TANGENT
				o.VFX_VARYING_TANGENT = float4(normalize(TransformDirectionVFXToWorld(mul((float3x3)elementToVFX,i.tangent.xyz))),i.tangent.w);
				#endif
			
				
						#if defined(VFX_VARYING_VELOCITY_CPOS) && defined(VFX_VARYING_VELOCITY_CPOS_PREVIOUS)
						float4x4 previousElementToVFX = (float4x4)0;
						previousElementToVFX[3] = float4(0,0,0,1);
						
						UNITY_UNROLL
						for (int itIndexMatrixRow = 0; itIndexMatrixRow < 3; ++itIndexMatrixRow)
						{
							UNITY_UNROLL
							for (int itIndexMatrixCol = 0; itIndexMatrixCol < 4; ++itIndexMatrixCol)
							{
								uint itIndexMatrix = itIndexMatrixCol * 4 + itIndexMatrixRow;
								uint read = elementToVFXBufferPrevious.Load((index * 16 + itIndexMatrix) << 2);
								previousElementToVFX[itIndexMatrixRow][itIndexMatrixCol] = asfloat(read);
							}
						}
						
						uint previousFrameIndex = elementToVFXBufferPrevious.Load((index * 16 + 15) << 2);
						o.VFX_VARYING_VELOCITY_CPOS = o.VFX_VARYING_VELOCITY_CPOS_PREVIOUS = float4(0.0f, 0.0f, 0.0f, 1.0f);
						if (asuint(currentFrameIndex) - previousFrameIndex == 1u)
						{
							float3 oldvPos = mul(previousElementToVFX,float4(inputVertexPosition, 1.0f)).xyz;
							o.VFX_VARYING_VELOCITY_CPOS_PREVIOUS = TransformPositionVFXToPreviousClip(oldvPos);
							o.VFX_VARYING_VELOCITY_CPOS = TransformPositionVFXToNonJitteredClip(vPos);
						}
						#endif
						
				
				
						#if VFX_USE_COLOR_CURRENT && defined(VFX_VARYING_COLOR)
						o.VFX_VARYING_COLOR = attributes.color;
						#endif
						#if VFX_USE_ALPHA_CURRENT && defined(VFX_VARYING_ALPHA) 
						o.VFX_VARYING_ALPHA = attributes.alpha;
						#endif
						
						#ifdef VFX_VARYING_EXPOSUREWEIGHT
						
						o.VFX_VARYING_EXPOSUREWEIGHT = exposureWeight;
						#endif
						
						#if USE_SOFT_PARTICLE && defined(VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE)
						
						o.VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE = invSoftParticlesFadeDistance;
						#endif
						
						#if (USE_ALPHA_TEST || WRITE_MOTION_VECTOR_IN_FORWARD) && (!VFX_SHADERGRAPH || !HAS_SHADERGRAPH_PARAM_ALPHATHRESHOLD) && defined(VFX_VARYING_ALPHATHRESHOLD)
						float alphaThreshold = (float)0;
						{
						    
						    alphaThreshold = (float)0.5;
						}
						
				
						o.VFX_VARYING_ALPHATHRESHOLD = alphaThreshold;
						#endif
						
						#if USE_UV_SCALE_BIAS
						
						
						#if defined (VFX_VARYING_UV)
						o.VFX_VARYING_UV.xy = o.VFX_VARYING_UV.xy * uvScale + uvBias;
						#endif
						#endif
						
						#if defined(VFX_VARYING_POSWS)
						o.VFX_VARYING_POSWS = TransformPositionVFXToWorld(vPos);
						#endif
						
				
				
						#if USE_FLIPBOOK && defined(VFX_VARYING_UV)
						
						
						VFXUVData uvData = GetUVData(flipBookSize, invFlipBookSize, o.VFX_VARYING_UV.xy, attributes.texIndex);
						o.VFX_VARYING_UV.xy = uvData.uvs.xy;
						#if USE_FLIPBOOK_INTERPOLATION && defined(VFX_VARYING_UV) && defined (VFX_VARYING_FRAMEBLEND)
						o.VFX_VARYING_UV.zw = uvData.uvs.zw;
						o.VFX_VARYING_FRAMEBLEND = uvData.blend;
						#if USE_FLIPBOOK_MOTIONVECTORS && defined(VFX_VARYING_MOTIONVECTORSCALE)
						
						o.VFX_VARYING_MOTIONVECTORSCALE = motionVectorScale * invFlipBookSize;
						#endif
						#endif
						#endif
						
			
				
							
							
			    
			    float4 Color_DCF944C2__ = (float4)0;{
			        
			        Color_DCF944C2__ = float4(0, 1, 0.96201396, 0);
			    }
			    o.Color_DCF944C2 = Color_DCF944C2__;float2 RaycastTextureUV__ = (float2)0;{
			        int tmp_y = (int)attributes.particleId;
			        int tmp_ba = tmp_y / (int)256;
			        float tmp_bb = (float)tmp_ba;
			        float tmp_bc = floor(tmp_bb);
			        uint tmp_bd = (uint)tmp_y;
			        uint tmp_bf = tmp_bd / (uint)256;
			        uint tmp_bg = tmp_bf * (uint)256;
			        uint tmp_bh = tmp_bd - tmp_bg;
			        float tmp_bi = (float)tmp_bh;
			        float2 tmp_bj = float2(tmp_bc, tmp_bi);
			        float2 tmp_bl = tmp_bj / float2(256, 256);
			        
			        RaycastTextureUV__ = tmp_bl;
			    }
			    o.RaycastTextureUV = RaycastTextureUV__;

				
				return o;
			}
			
			
			
			#include "Packages/com.unity.visualeffectgraph/Shaders/VFXCommonOutput.hlsl"
			
			
			
				
			#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
			#define SHADERPASS SHADERPASS_MOTION_VECTORS
			#else
			#define SHADERPASS SHADERPASS_DEPTH_ONLY
			#endif
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXLit.hlsl"
			
			#ifndef VFX_SHADERGRAPH
			
			void VFXGetHDRPLitData(out SurfaceData surfaceData, out BuiltinData builtinData, out BSDFData bsdfData, out PreLightData preLightData, VFX_VARYING_PS_INPUTS i, float3 normalWS, const VFXUVData uvData, uint2 tileIndex)
			{	
				#if HDRP_MATERIAL_TYPE_TRANSLUCENT
				 // Loads diffusion profile
				#else
				const uint diffusionProfileHash = 0;
				#endif
				
				float3 posRWS = VFXGetPositionRWS(i);
				float4 posSS = i.VFX_VARYING_POSCS;
				PositionInputs posInput = GetPositionInput(posSS.xy, _ScreenSize.zw, posSS.z, posSS.w, posRWS, tileIndex);
				
				float alpha;
				surfaceData = VFXGetSurfaceData(i,normalWS,uvData,diffusionProfileHash,alpha);	
				bsdfData = ConvertSurfaceDataToBSDFData(posSS.xy, surfaceData);
			
				preLightData = GetPreLightData(GetWorldSpaceNormalizeViewDir(posRWS),posInput,bsdfData);
				
				preLightData.diffuseFGD = 1.0f;
			    //TODO: investigate why this is needed
			    preLightData.coatPartLambdaV = 0;
			    preLightData.coatIblR = 0;
			    preLightData.coatIblF = 0;
			    
				builtinData = VFXGetBuiltinData(i,posInput,surfaceData,uvData,alpha);
			}
			
			void VFXGetHDRPLitData(out SurfaceData surfaceData, out BuiltinData builtinData, VFX_VARYING_PS_INPUTS i, float3 normalWS, const VFXUVData uvData)
			{
				BSDFData bsdfData = (BSDFData)0;
				PreLightData preLightData = (PreLightData)0;
				preLightData.diffuseFGD = 1.0f;
				VFXGetHDRPLitData(surfaceData,builtinData,bsdfData,preLightData,i,normalWS,uvData,uint2(0,0));
			}
			
			#endif
			
			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXLitPixelOutput.hlsl"
			
			
			
						
			
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
					#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinGIUtilities.hlsl"
					#ifndef SHADERPASS
					#error Shaderpass should be defined at this stage.
					#endif
					#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
					
			
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			struct SG_Input_abfd42c13223447bfbe3d7528d18d99d
			{
			};
			
			struct SG_Output_abfd42c13223447bfbe3d7528d18d99d
			{
			    float3 Normal_8;
			    float Alpha_4;
			};
			
			SG_Output_abfd42c13223447bfbe3d7528d18d99d SG_Evaluate_abfd42c13223447bfbe3d7528d18d99d(
			    SG_Input_abfd42c13223447bfbe3d7528d18d99d IN)
			{
			    // Visual Effect Master
			    SG_Output_abfd42c13223447bfbe3d7528d18d99d OUT;
			    OUT.Normal_8 = float3 (0, 0, 1);
			    OUT.Alpha_4 = 1;
			    return OUT;
			}
			

				
			#if VFX_PASSDEPTH == VFX_PASSDEPTH_SELECTION
			int _ObjectId;
			int _PassValue;
			#endif
			
			#pragma fragment frag
			void frag(ps_input i
			#if USE_DOUBLE_SIDED
				, bool frontFace : SV_IsFrontFace
			#endif
			
			#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
				, out float4 outMotionVector : SV_Target0
				#ifdef WRITE_NORMAL_BUFFER
					, out float4 outNormalBuffer : SV_Target1
				#endif
			#else
				#ifdef WRITE_NORMAL_BUFFER
					, out float4 outNormalBuffer : SV_Target0
				#elif VFX_PASSDEPTH == VFX_PASSDEPTH_SELECTION
					, out float4 outColor : SV_Target0
				#endif
			#endif
				)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				VFXTransformPSInputs(i);
				
							#ifdef VFX_VARYING_NORMAL
							#if USE_DOUBLE_SIDED
							const float faceMul = frontFace ? 1.0f : -1.0f;
							#else
							const float faceMul = 1.0f;
							#endif
							
							float3 normalWS = normalize(i.VFX_VARYING_NORMAL * faceMul);
							const VFXUVData uvData = GetUVData(i);
							
							#ifdef VFX_VARYING_TANGENT
							float3 tangentWS = normalize(i.VFX_VARYING_TANGENT.xyz);
							float3 bitangentWS = cross(normalWS,tangentWS) * (i.VFX_VARYING_TANGENT.w * faceMul);
							float3x3 tbn = float3x3(tangentWS,bitangentWS,normalWS);
							
							#if USE_NORMAL_MAP
							float3 n = SampleNormalMap(VFX_SAMPLER(normalMap),uvData);
							float normalScale = 1.0f;
							#ifdef VFX_VARYING_NORMALSCALE
							normalScale = i.VFX_VARYING_NORMALSCALE;
							#endif
							normalWS = normalize(lerp(normalWS,mul(n,tbn),normalScale));
							#endif
							#endif
							#endif
							
				
				#ifdef VFX_SHADERGRAPH
			        float4 Color_DCF944C2 = i.Color_DCF944C2;float2 RaycastTextureUV = i.RaycastTextureUV;
			        //Call Shader Graph
			        SG_Input_abfd42c13223447bfbe3d7528d18d99d INSG = (SG_Input_abfd42c13223447bfbe3d7528d18d99d)0;
			        
			        SG_Output_abfd42c13223447bfbe3d7528d18d99d OUTSG = SG_Evaluate_abfd42c13223447bfbe3d7528d18d99d(INSG);
			        

				
			        float alpha = OUTSG.Alpha_4;
			    #else
				float alpha = VFXGetFragmentColor(i).a;
				#if HDRP_USE_BASE_COLOR_MAP_ALPHA
					alpha *= VFXGetTextureColor(VFX_SAMPLER(baseColorMap),i).a;
				#endif
			    #endif
				VFXClipFragmentColor(alpha,i);
				
				#ifdef WRITE_NORMAL_BUFFER
			        #ifndef VFX_SHADERGRAPH
			            VFXComputePixelOutputToNormalBuffer(i,normalWS,uvData,outNormalBuffer);
			        #else
			           #if HAS_SHADERGRAPH_PARAM_NORMAL
			               float3 n =  OUTSG.Normal_8;
			               normalWS = mul(n,tbn);
			           #endif
			           SurfaceData surface = (SurfaceData)0;
			           
			           surface.normalWS = normalWS;
			           
			           EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surface), i.VFX_VARYING_POSCS.xy, outNormalBuffer);
			        #endif
				#endif
			
				#if VFX_PASSDEPTH == VFX_PASSDEPTH_MOTION_VECTOR
					
							float2 velocity = (i.VFX_VARYING_VELOCITY_CPOS.xy/i.VFX_VARYING_VELOCITY_CPOS.w) - (i.VFX_VARYING_VELOCITY_CPOS_PREVIOUS.xy/i.VFX_VARYING_VELOCITY_CPOS_PREVIOUS.w);
							#if UNITY_UV_STARTS_AT_TOP
								velocity.y = -velocity.y;
							#endif
							float4 encodedMotionVector = 0.0f;
							VFXEncodeMotionVector(velocity * 0.5f, encodedMotionVector);
							
					outMotionVector = encodedMotionVector;
				#elif VFX_PASSDEPTH == VFX_PASSDEPTH_SELECTION
					// We use depth prepass for scene selection in the editor, this code allow to output the outline correctly
					outColor = float4(_ObjectId, _PassValue, 1.0, 1.0);
				#elif VFX_PASSDEPTH == VFX_PASSDEPTH_ACTUAL
					//void
				#else
					#error VFX_PASSDEPTH undefined
				#endif
			}
			
		
		
		
			ENDHLSL
		}
		

		
		Pass
		{		
			Tags { "LightMode"="GBuffer" }
			
			Stencil
			{
				WriteMask 7
				Ref  2
				Comp Always
				Pass Replace
			}	
				
			HLSLPROGRAM
			#pragma target 4.5
			
			#pragma multi_compile _ LIGHT_LAYERS
			#pragma multi_compile _ DEBUG_DISPLAY
			
			#define UNITY_MATERIAL_LIT
			
			#define HDRP_NEEDS_UVS (HDRP_USE_BASE_COLOR_MAP || HDRP_USE_MASK_MAP || USE_NORMAL_MAP || HDRP_USE_EMISSIVE_MAP)
			#define HDRP_USE_EMISSIVE (HDRP_USE_EMISSIVE_MAP || HDRP_USE_EMISSIVE_COLOR || HDRP_USE_ADDITIONAL_EMISSIVE_COLOR)
			
			
			
			
			
			
			
			
			
			struct ps_input
			{
				float4 pos : SV_POSITION;
				
				
							
							#if (VFX_NEEDS_COLOR_INTERPOLATOR && HDRP_USE_BASE_COLOR) || HDRP_USE_ADDITIONAL_BASE_COLOR
							VFX_OPTIONAL_INTERPOLATION float4 color : COLOR0;
							#endif
							#if HDRP_MATERIAL_TYPE_SPECULAR
							VFX_OPTIONAL_INTERPOLATION float3 specularColor : COLOR1;
							#endif
							#if HDRP_USE_EMISSIVE	
							VFX_OPTIONAL_INTERPOLATION float4 emissiveColor : COLOR2;
							#endif
							
							// x: smoothness
							// y: metallic/thickness
							// z: normal scale
							// w: emissive scale
							VFX_OPTIONAL_INTERPOLATION float4 materialProperties : TEXCOORD0;
							
				
				#if USE_FLIPBOOK_INTERPOLATION
				float4 uv : TEXCOORD1;
				#else
				float2 uv : TEXCOORD1;	
				#endif
		        #if VFX_SHADERGRAPH_HAS_UV1
		        float4 uv1 : COLOR2;
		        #endif
		        #if VFX_SHADERGRAPH_HAS_UV2
		        float4 uv2 : COLOR3;
		        #endif
		        #if VFX_SHADERGRAPH_HAS_UV3
		        float4 uv3 : COLOR4;
		        #endif
		        #if VFX_SHADERGRAPH_HAS_COLOR
		        float4 vertexColor : COLOR1;
		        #endif
				float3 normal : TEXCOORD2;
				#if USE_NORMAL_MAP || SHADERGRAPH_NEEDS_TANGENT_GBUFFER
				float4 tangent : TEXCOORD3;
				#endif
				#if USE_SOFT_PARTICLE || USE_ALPHA_TEST || USE_FLIPBOOK_INTERPOLATION
				// x: inverse soft particles fade distance
				// y: alpha threshold
				// z: frame blending factor
				nointerpolation float3 builtInInterpolants : TEXCOORD4;
				#endif
				#if USE_FLIPBOOK_MOTIONVECTORS
				// x: motion vector scale u
				// y: motion vector scale v
				nointerpolation float2 builtInInterpolants2 : TEXCOORD5;
				#endif
		        
		#if VFX_NEEDS_POSWS_INTERPOLATOR
				float3 posWS : TEXCOORD6;
		#endif
		
		        VFX_OPTIONAL_INTERPOLATION float4 Color_DCF944C2 : NORMAL0;
		        VFX_OPTIONAL_INTERPOLATION float2 RaycastTextureUV : NORMAL1;
		        

		    
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			struct ps_output
			{
				float4 color : SV_Target0;
			};
		
		
					#if (VFX_NEEDS_COLOR_INTERPOLATOR && HDRP_USE_BASE_COLOR) || HDRP_USE_ADDITIONAL_BASE_COLOR
					#define VFX_VARYING_COLOR color.rgb
					#define VFX_VARYING_ALPHA color.a
					#endif
					
					#define VFX_VARYING_SMOOTHNESS materialProperties.x
					
					#if HDRP_MATERIAL_TYPE_STANDARD
					#define VFX_VARYING_METALLIC materialProperties.y
					#elif HDRP_MATERIAL_TYPE_SPECULAR
					#define VFX_VARYING_SPECULAR specularColor
					#elif HDRP_MATERIAL_TYPE_TRANSLUCENT
					#define VFX_VARYING_THICKNESS materialProperties.y
					#endif
					
					#if USE_NORMAL_MAP
					#define VFX_VARYING_NORMALSCALE materialProperties.z
					#endif
					
					#if HDRP_USE_EMISSIVE_MAP
					#define VFX_VARYING_EMISSIVESCALE materialProperties.w
					#endif
					
					#if HDRP_USE_EMISSIVE_COLOR || HDRP_USE_ADDITIONAL_EMISSIVE_COLOR
					#define VFX_VARYING_EMISSIVE emissiveColor.rgb
					#endif
					
					#if USE_EXPOSURE_WEIGHT
					#define VFX_VARYING_EXPOSUREWEIGHT emissiveColor.a
					#endif
					
			
		#define VFX_VARYING_PS_INPUTS ps_input
		#define VFX_VARYING_POSCS pos
		#define VFX_VARYING_UV uv
		#define VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE builtInInterpolants.x
		#define VFX_VARYING_ALPHATHRESHOLD builtInInterpolants.y
		#define VFX_VARYING_FRAMEBLEND builtInInterpolants.z
		#define VFX_VARYING_MOTIONVECTORSCALE builtInInterpolants2.xy
		#define VFX_VARYING_NORMAL normal
		
		#if USE_NORMAL_MAP || SHADERGRAPH_NEEDS_TANGENT_GBUFFER
		#define VFX_VARYING_TANGENT tangent
		#endif
		#if VFX_NEEDS_POSWS_INTERPOLATOR
		#define VFX_VARYING_POSWS posWS
		#endif
		
		
		
			#if !(defined(VFX_VARYING_PS_INPUTS) && defined(VFX_VARYING_POSCS))
			#error VFX_VARYING_PS_INPUTS, VFX_VARYING_POSCS and VFX_VARYING_UV must be defined.
			#endif
			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXCommon.hlsl"
			#include "Packages/com.unity.visualeffectgraph/Shaders/VFXCommon.hlsl"
			

			void SetAttribute_65DEC940(inout float pivotX, inout float pivotY, inout float pivotZ, float3 Pivot) /*attribute:pivot Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    pivotX = Pivot.x;
			    pivotY = Pivot.y;
			    pivotZ = Pivot.z;
			}
			void SetAttribute_3278B22F(inout float size, float Size) /*attribute:size Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    size = Size;
			}
			void SetAttribute_D5151640(inout float scaleX, inout float scaleZ, float2 Scale) /*attribute:scale Composition:Overwrite Source:Slot Random:Off channels:XZ */
			{
			    scaleX = Scale.x;
			    scaleZ = Scale.y;
			}
			void SetAttribute_CAC29747(inout float3 position, float3 Position) /*attribute:position Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    position = Position;
			}
			void SetAttribute_D5151645(inout float scaleY, float Scale) /*attribute:scale Composition:Overwrite Source:Slot Random:Off channels:Y */
			{
			    scaleY = Scale.x;
			}
			

			
			struct vs_input
			{
				float3 pos : POSITION;
				float2 uv : TEXCOORD0;
			    #if VFX_SHADERGRAPH_HAS_UV1
			    float4 uv1 : TEXCOORD1;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    float4 uv2 : TEXCOORD2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    float4 uv3 : TEXCOORD3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    float4 vertexColor : COLOR;
			    #endif
				float3 normal : NORMAL;
				#if defined(VFX_VARYING_TANGENT) || SHADERGRAPH_HAS_NORMAL
				float4 tangent : TANGENT;
				#endif
				VFX_DECLARE_INSTANCE_ID
			};
			
			#pragma vertex vert
			VFX_VARYING_PS_INPUTS vert(vs_input i)
			{
			    VFX_VARYING_PS_INPUTS o = (VFX_VARYING_PS_INPUTS)0;
			
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			
				uint index = VFX_GET_INSTANCE_ID(i);	
			
				
						uint deadCount = 0;
						#if USE_DEAD_LIST_COUNT
						deadCount = deadListCount.Load(0);
						#endif	
						if (index >= asuint(nbMax) - deadCount)
						#if USE_GEOMETRY_SHADER
							return; // cull
						#else
							return o; // cull
						#endif
						
						Attributes attributes = (Attributes)0;
						SourceAttributes sourceAttributes = (SourceAttributes)0;
						
						#if VFX_HAS_INDIRECT_DRAW
						index = indirectBuffer[index];
						attributes.particleId = (attributeBuffer.Load((index * 0x2 + 0x0) << 2));
						attributes.scaleY = asfloat(attributeBuffer.Load((index * 0x2 + 0x1) << 2));
						attributes.position = float3(0, 0, 0);
						attributes.color = float3(1, 1, 1);
						attributes.alpha = (float)1;
						attributes.alive = (bool)true;
						attributes.axisX = float3(1, 0, 0);
						attributes.axisY = float3(0, 1, 0);
						attributes.axisZ = float3(0, 0, 1);
						attributes.angleX = (float)0;
						attributes.angleY = (float)0;
						attributes.angleZ = (float)0;
						attributes.pivotX = (float)0;
						attributes.pivotY = (float)0;
						attributes.pivotZ = (float)0;
						attributes.size = (float)0.100000001;
						attributes.scaleX = (float)1;
						attributes.scaleZ = (float)1;
						
				
						#else
						attributes.alive = (bool)true;
						
				
						#if !HAS_STRIPS
						if (!attributes.alive)
							return o;
						#endif
							
						attributes.particleId = (attributeBuffer.Load((index * 0x2 + 0x0) << 2));
						attributes.scaleY = asfloat(attributeBuffer.Load((index * 0x2 + 0x1) << 2));
						attributes.position = float3(0, 0, 0);
						attributes.color = float3(1, 1, 1);
						attributes.alpha = (float)1;
						attributes.axisX = float3(1, 0, 0);
						attributes.axisY = float3(0, 1, 0);
						attributes.axisZ = float3(0, 0, 1);
						attributes.angleX = (float)0;
						attributes.angleY = (float)0;
						attributes.angleZ = (float)0;
						attributes.pivotX = (float)0;
						attributes.pivotY = (float)0;
						attributes.pivotZ = (float)0;
						attributes.size = (float)0.100000001;
						attributes.scaleX = (float)1;
						attributes.scaleZ = (float)1;
						
				
						#endif
						
						// Initialize built-in needed attributes
						#if HAS_STRIPS
						InitStripAttributes(index, attributes, stripData);
						#endif
						
				{
				    SetAttribute_65DEC940( /*inout */attributes.pivotX,  /*inout */attributes.pivotY,  /*inout */attributes.pivotZ, float3(0, -0.5, 0));
				}
				SetAttribute_3278B22F( /*inout */attributes.size, Size_b);
				{
				    SetAttribute_D5151640( /*inout */attributes.scaleX,  /*inout */attributes.scaleZ, float2(0.00999999978, 0.00999999978));
				}
				{
				    int tmp_y = (int)attributes.particleId;
				    int tmp_ba = tmp_y / (int)256;
				    float tmp_bb = (float)tmp_ba;
				    float tmp_bc = floor(tmp_bb);
				    uint tmp_bd = (uint)tmp_y;
				    uint tmp_bf = tmp_bd / (uint)256;
				    uint tmp_bg = tmp_bf * (uint)256;
				    uint tmp_bh = tmp_bd - tmp_bg;
				    float tmp_bi = (float)tmp_bh;
				    float2 tmp_bj = float2(tmp_bc, tmp_bi);
				    float2 tmp_bl = tmp_bj / float2(256, 256);
				    float2 tmp_bn = tmp_bl * float2(64, 64);
				    float2 tmp_bp = tmp_bn + float2(0.125, 0.125);
				    float2 tmp_bq = tmp_bp / float2(64, 64);
				    float2 tmp_bs = tmp_bq * float2(640, 640);
				    float tmp_bt = tmp_bs[0];
				    float tmp_bv = tmp_bs[1];
				    float3 tmp_bw = float3(tmp_bt, (float)0, tmp_bv);
				    SetAttribute_CAC29747( /*inout */attributes.position, tmp_bw);
				}
				{
				    int tmp_y = (int)attributes.particleId;
				    int tmp_ba = tmp_y / (int)256;
				    float tmp_bb = (float)tmp_ba;
				    float tmp_bc = floor(tmp_bb);
				    uint tmp_bd = (uint)tmp_y;
				    uint tmp_bf = tmp_bd / (uint)256;
				    uint tmp_bg = tmp_bf * (uint)256;
				    uint tmp_bh = tmp_bd - tmp_bg;
				    float tmp_bi = (float)tmp_bh;
				    float2 tmp_bj = float2(tmp_bc, tmp_bi);
				    float2 tmp_bl = tmp_bj / float2(256, 256);
				    float2 tmp_bn = tmp_bl * float2(64, 64);
				    float2 tmp_bp = tmp_bn + float2(0.125, 0.125);
				    float2 tmp_bq = tmp_bp / float2(64, 64);
				    float4 tmp_bs = SampleTexture(VFX_SAMPLER(texture_b),tmp_bq,(float)0);
				    float tmp_bt = tmp_bs[0];
				    float tmp_bu = tmp_bt * uniform_c;
				    float tmp_bw = tmp_bu + (float)0.00999999978;
				    SetAttribute_D5151645( /*inout */attributes.scaleY, tmp_bw);
				}
				

						
				if (!attributes.alive)
					return o;
				
				o.VFX_VARYING_UV.xy = i.uv;
			    
			    #if VFX_SHADERGRAPH_HAS_UV1
			    o.uv1 = i.uv1;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    o.uv2 = i.uv2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    o.uv3 = i.uv3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    o.vertexColor = i.vertexColor;
			    #endif
				
				
						float3 size3 = float3(attributes.size,attributes.size,attributes.size);
						#if VFX_USE_SCALEX_CURRENT
						size3.x *= attributes.scaleX;
						#endif
						#if VFX_USE_SCALEY_CURRENT
						size3.y *= attributes.scaleY;
						#endif
						#if VFX_USE_SCALEZ_CURRENT
						size3.z *= attributes.scaleZ;
						#endif
						
				
				float3 inputVertexPosition = i.pos;
				float4x4 elementToVFX = GetElementToVFXMatrix(
					attributes.axisX,
					attributes.axisY,
					attributes.axisZ,
					float3(attributes.angleX,attributes.angleY,attributes.angleZ),
					float3(attributes.pivotX,attributes.pivotY,attributes.pivotZ),
					size3,
					attributes.position);
					
				float3 vPos = mul(elementToVFX,float4(inputVertexPosition,1.0f)).xyz;
				float4 csPos = TransformPositionVFXToClip(vPos);
				o.VFX_VARYING_POSCS = csPos;
				
				float3 normalWS = normalize(TransformDirectionVFXToWorld(mul((float3x3)elementToVFX, i.normal)));
				#ifdef VFX_VARYING_NORMAL // TODO Should use inverse transpose
				o.VFX_VARYING_NORMAL = normalWS;
				#endif
				#ifdef VFX_VARYING_TANGENT
				o.VFX_VARYING_TANGENT = float4(normalize(TransformDirectionVFXToWorld(mul((float3x3)elementToVFX,i.tangent.xyz))),i.tangent.w);
				#endif
			
				
						#if defined(VFX_VARYING_VELOCITY_CPOS) && defined(VFX_VARYING_VELOCITY_CPOS_PREVIOUS)
						float4x4 previousElementToVFX = (float4x4)0;
						previousElementToVFX[3] = float4(0,0,0,1);
						
						UNITY_UNROLL
						for (int itIndexMatrixRow = 0; itIndexMatrixRow < 3; ++itIndexMatrixRow)
						{
							UNITY_UNROLL
							for (int itIndexMatrixCol = 0; itIndexMatrixCol < 4; ++itIndexMatrixCol)
							{
								uint itIndexMatrix = itIndexMatrixCol * 4 + itIndexMatrixRow;
								uint read = elementToVFXBufferPrevious.Load((index * 16 + itIndexMatrix) << 2);
								previousElementToVFX[itIndexMatrixRow][itIndexMatrixCol] = asfloat(read);
							}
						}
						
						uint previousFrameIndex = elementToVFXBufferPrevious.Load((index * 16 + 15) << 2);
						o.VFX_VARYING_VELOCITY_CPOS = o.VFX_VARYING_VELOCITY_CPOS_PREVIOUS = float4(0.0f, 0.0f, 0.0f, 1.0f);
						if (asuint(currentFrameIndex) - previousFrameIndex == 1u)
						{
							float3 oldvPos = mul(previousElementToVFX,float4(inputVertexPosition, 1.0f)).xyz;
							o.VFX_VARYING_VELOCITY_CPOS_PREVIOUS = TransformPositionVFXToPreviousClip(oldvPos);
							o.VFX_VARYING_VELOCITY_CPOS = TransformPositionVFXToNonJitteredClip(vPos);
						}
						#endif
						
				
				
						#if VFX_USE_COLOR_CURRENT && defined(VFX_VARYING_COLOR)
						o.VFX_VARYING_COLOR = attributes.color;
						#endif
						#if VFX_USE_ALPHA_CURRENT && defined(VFX_VARYING_ALPHA) 
						o.VFX_VARYING_ALPHA = attributes.alpha;
						#endif
						
						#ifdef VFX_VARYING_EXPOSUREWEIGHT
						
						o.VFX_VARYING_EXPOSUREWEIGHT = exposureWeight;
						#endif
						
						#if USE_SOFT_PARTICLE && defined(VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE)
						
						o.VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE = invSoftParticlesFadeDistance;
						#endif
						
						#if (USE_ALPHA_TEST || WRITE_MOTION_VECTOR_IN_FORWARD) && (!VFX_SHADERGRAPH || !HAS_SHADERGRAPH_PARAM_ALPHATHRESHOLD) && defined(VFX_VARYING_ALPHATHRESHOLD)
						float alphaThreshold = (float)0;
						{
						    
						    alphaThreshold = (float)0.5;
						}
						
				
						o.VFX_VARYING_ALPHATHRESHOLD = alphaThreshold;
						#endif
						
						#if USE_UV_SCALE_BIAS
						
						
						#if defined (VFX_VARYING_UV)
						o.VFX_VARYING_UV.xy = o.VFX_VARYING_UV.xy * uvScale + uvBias;
						#endif
						#endif
						
						#if defined(VFX_VARYING_POSWS)
						o.VFX_VARYING_POSWS = TransformPositionVFXToWorld(vPos);
						#endif
						
				
				
						#if USE_FLIPBOOK && defined(VFX_VARYING_UV)
						
						
						VFXUVData uvData = GetUVData(flipBookSize, invFlipBookSize, o.VFX_VARYING_UV.xy, attributes.texIndex);
						o.VFX_VARYING_UV.xy = uvData.uvs.xy;
						#if USE_FLIPBOOK_INTERPOLATION && defined(VFX_VARYING_UV) && defined (VFX_VARYING_FRAMEBLEND)
						o.VFX_VARYING_UV.zw = uvData.uvs.zw;
						o.VFX_VARYING_FRAMEBLEND = uvData.blend;
						#if USE_FLIPBOOK_MOTIONVECTORS && defined(VFX_VARYING_MOTIONVECTORSCALE)
						
						o.VFX_VARYING_MOTIONVECTORSCALE = motionVectorScale * invFlipBookSize;
						#endif
						#endif
						#endif
						
			
				
						
									#ifndef VFX_SHADERGRAPH
									#ifdef VFX_VARYING_SMOOTHNESS
									
									o.VFX_VARYING_SMOOTHNESS = smoothness;
									#endif
									#if HDRP_MATERIAL_TYPE_STANDARD
									#ifdef VFX_VARYING_METALLIC
									
									o.VFX_VARYING_METALLIC = metallic;
									#endif
									#elif HDRP_MATERIAL_TYPE_SPECULAR
									#ifdef VFX_VARYING_SPECULAR
									
									o.VFX_VARYING_SPECULAR = specularColor;
									#endif
									#elif HDRP_MATERIAL_TYPE_TRANSLUCENT
									#ifdef VFX_VARYING_THICKNESS
									
									o.VFX_VARYING_THICKNESS = thickness;
									#endif
									#endif
									#if USE_NORMAL_MAP
									#ifdef VFX_VARYING_NORMALSCALE
									
									o.VFX_VARYING_NORMALSCALE = normalScale;
									#endif
									#endif
									#if HDRP_USE_EMISSIVE_MAP
									#ifdef VFX_VARYING_EMISSIVESCALE
									
									o.VFX_VARYING_EMISSIVESCALE = emissiveScale;
									#endif
									#endif
									#ifdef VFX_VARYING_EMISSIVE
									#if HDRP_USE_EMISSIVE_COLOR
									o.VFX_VARYING_EMISSIVE = attributes.color;
									#elif HDRP_USE_ADDITIONAL_EMISSIVE_COLOR
									
									o.VFX_VARYING_EMISSIVE = emissiveColor;
									#endif
									#endif
									#if HDRP_USE_ADDITIONAL_BASE_COLOR
									#ifdef VFX_VARYING_COLOR
									
									o.VFX_VARYING_COLOR = baseColor;
									#endif
									#endif
									#endif
									
						
			    
			    float4 Color_DCF944C2__ = (float4)0;{
			        
			        Color_DCF944C2__ = float4(0, 1, 0.96201396, 0);
			    }
			    o.Color_DCF944C2 = Color_DCF944C2__;float2 RaycastTextureUV__ = (float2)0;{
			        int tmp_y = (int)attributes.particleId;
			        int tmp_ba = tmp_y / (int)256;
			        float tmp_bb = (float)tmp_ba;
			        float tmp_bc = floor(tmp_bb);
			        uint tmp_bd = (uint)tmp_y;
			        uint tmp_bf = tmp_bd / (uint)256;
			        uint tmp_bg = tmp_bf * (uint)256;
			        uint tmp_bh = tmp_bd - tmp_bg;
			        float tmp_bi = (float)tmp_bh;
			        float2 tmp_bj = float2(tmp_bc, tmp_bi);
			        float2 tmp_bl = tmp_bj / float2(256, 256);
			        
			        RaycastTextureUV__ = tmp_bl;
			    }
			    o.RaycastTextureUV = RaycastTextureUV__;

				
				return o;
			}
			
			
			
			#include "Packages/com.unity.visualeffectgraph/Shaders/VFXCommonOutput.hlsl"
			
			
			
			
			#define SHADERPASS SHADERPASS_GBUFFER	
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXLit.hlsl"
			
			#ifndef VFX_SHADERGRAPH
			
			void VFXGetHDRPLitData(out SurfaceData surfaceData, out BuiltinData builtinData, out BSDFData bsdfData, out PreLightData preLightData, VFX_VARYING_PS_INPUTS i, float3 normalWS, const VFXUVData uvData, uint2 tileIndex)
			{	
				#if HDRP_MATERIAL_TYPE_TRANSLUCENT
				 // Loads diffusion profile
				#else
				const uint diffusionProfileHash = 0;
				#endif
				
				float3 posRWS = VFXGetPositionRWS(i);
				float4 posSS = i.VFX_VARYING_POSCS;
				PositionInputs posInput = GetPositionInput(posSS.xy, _ScreenSize.zw, posSS.z, posSS.w, posRWS, tileIndex);
				
				float alpha;
				surfaceData = VFXGetSurfaceData(i,normalWS,uvData,diffusionProfileHash,alpha);	
				bsdfData = ConvertSurfaceDataToBSDFData(posSS.xy, surfaceData);
			
				preLightData = GetPreLightData(GetWorldSpaceNormalizeViewDir(posRWS),posInput,bsdfData);
				
				preLightData.diffuseFGD = 1.0f;
			    //TODO: investigate why this is needed
			    preLightData.coatPartLambdaV = 0;
			    preLightData.coatIblR = 0;
			    preLightData.coatIblF = 0;
			    
				builtinData = VFXGetBuiltinData(i,posInput,surfaceData,uvData,alpha);
			}
			
			void VFXGetHDRPLitData(out SurfaceData surfaceData, out BuiltinData builtinData, VFX_VARYING_PS_INPUTS i, float3 normalWS, const VFXUVData uvData)
			{
				BSDFData bsdfData = (BSDFData)0;
				PreLightData preLightData = (PreLightData)0;
				preLightData.diffuseFGD = 1.0f;
				VFXGetHDRPLitData(surfaceData,builtinData,bsdfData,preLightData,i,normalWS,uvData,uint2(0,0));
			}
			
			#endif
			
			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXLitPixelOutput.hlsl"
			
			
		
				
			
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
					#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinGIUtilities.hlsl"
					#ifndef SHADERPASS
					#error Shaderpass should be defined at this stage.
					#endif
					#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
					
			
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			// Node: Add
			void Unity_Add_float(float A, float B, out float Out)
			{
			    Out = A + B;
			}
			
			// Node: Divide
			void Unity_Divide_float(float A, float B, out float Out)
			{
			    Out = A / B;
			}
			
			// Node: Multiply
			void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
			{
			    Out = A * B;
			}
			
			// Node: Multiply
			void Unity_Multiply_float(float A, float B, out float Out)
			{
			    Out = A * B;
			}
			
			// Node: Sample Gradient
			void Unity_SampleGradient_float(Gradient Gradient, float Time, out float4 Out)
			{
			    float3 color = Gradient.colors[0].rgb;
			    [unroll]
			    for (int c = 1; c < 8; c++)
			    {
			        float colorPos = saturate((Time - Gradient.colors[c-1].w) / (Gradient.colors[c].w - Gradient.colors[c-1].w)) * step(c, Gradient.colorsLength-1);
			        color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
			    }
			#ifndef UNITY_COLORSPACE_GAMMA
			    color = SRGBToLinear(color);
			#endif
			    float alpha = Gradient.alphas[0].x;
			    [unroll]
			    for (int a = 1; a < 8; a++)
			    {
			        float alphaPos = saturate((Time - Gradient.alphas[a-1].y) / (Gradient.alphas[a].y - Gradient.alphas[a-1].y)) * step(a, Gradient.alphasLength-1);
			        alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
			    }
			    Out = float4(color, alpha);
			}
			
			// Node: Add
			void Unity_Add_float4(float4 A, float4 B, out float4 Out)
			{
			    Out = A + B;
			}
			
			// Property: Gradient
			Gradient Gradient_150991D1_Definition()
			{
			    Gradient g;
			    g.type = 0;
			    g.colorsLength = 2;
			    g.alphasLength = 2;
			    g.colors[0] = float4(0.004405484, 0.02539425, 0.08490568, 0.002944991);
			    g.colors[1] = float4(2, 1.294118, 0.6196079, 1);
			    g.colors[2] = float4(0, 0, 0, 0);
			    g.colors[3] = float4(0, 0, 0, 0);
			    g.colors[4] = float4(0, 0, 0, 0);
			    g.colors[5] = float4(0, 0, 0, 0);
			    g.colors[6] = float4(0, 0, 0, 0);
			    g.colors[7] = float4(0, 0, 0, 0);
			    g.alphas[0] = float2(1, 0);
			    g.alphas[1] = float2(1, 1);
			    g.alphas[2] = float2(0, 0);
			    g.alphas[3] = float2(0, 0);
			    g.alphas[4] = float2(0, 0);
			    g.alphas[5] = float2(0, 0);
			    g.alphas[6] = float2(0, 0);
			    g.alphas[7] = float2(0, 0);
			    return g;
			}
			#define Gradient_150991D1 Gradient_150991D1_Definition()
			
			struct SG_Input_abfd42c13223447bfbe3d7528d18d99d
			{
			    float3 ObjectSpacePosition;
			};
			
			struct SG_Output_abfd42c13223447bfbe3d7528d18d99d
			{
			    float3 BaseColor_1;
			    float Metallic_2;
			    float Smoothness_3;
			    float3 Normal_8;
			    float3 Emissive_5;
			    float Alpha_4;
			};
			
			SG_Output_abfd42c13223447bfbe3d7528d18d99d SG_Evaluate_abfd42c13223447bfbe3d7528d18d99d(
			    SG_Input_abfd42c13223447bfbe3d7528d18d99d IN,
			    /* Property: ASHL */ TEXTURE2D_PARAM(Texture2D_64C137FF, samplerTexture2D_64C137FF), float4 Texture2D_64C137FF_TexelSize,
			    /* Property: Vector1 */ float Vector1_1FF39C4B,
			    /* Property: Color */ float4 Color_DCF944C2)
			{
			    // Node: Split
			    float _Split_EC6A98BB_R_1 = IN.ObjectSpacePosition[0];
			    float _Split_EC6A98BB_G_2 = IN.ObjectSpacePosition[1];
			    float _Split_EC6A98BB_B_3 = IN.ObjectSpacePosition[2];
			    float _Split_EC6A98BB_A_4 = 0;
			
			    // Node: Add
			    float _Add_54466F3B_Out_2;
			    Unity_Add_float(0, _Split_EC6A98BB_R_1, _Add_54466F3B_Out_2);
			
			    // Node: Divide
			    float _Divide_B561D354_Out_2;
			    Unity_Divide_float(_Add_54466F3B_Out_2, 640, _Divide_B561D354_Out_2);
			
			    // Node: Add
			    float _Add_2855D0DF_Out_2;
			    Unity_Add_float(0.0001, _Divide_B561D354_Out_2, _Add_2855D0DF_Out_2);
			
			    // Node: Add
			    float _Add_D3D19FE7_Out_2;
			    Unity_Add_float(0, _Split_EC6A98BB_B_3, _Add_D3D19FE7_Out_2);
			
			    // Node: Divide
			    float _Divide_FCE5A996_Out_2;
			    Unity_Divide_float(_Add_D3D19FE7_Out_2, 640, _Divide_FCE5A996_Out_2);
			
			    // Node: Add
			    float _Add_15F593F9_Out_2;
			    Unity_Add_float(0.0001, _Divide_FCE5A996_Out_2, _Add_15F593F9_Out_2);
			
			    // Node: Vector 2
			    float2 _Vector2_799C5B31_Out_0 = float2(_Add_2855D0DF_Out_2, _Add_15F593F9_Out_2);
			
			    // Node: Sample Texture 2D
			    float4 _SampleTexture2D_5EB2E67D_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_64C137FF, samplerTexture2D_64C137FF, _Vector2_799C5B31_Out_0);
			    float _SampleTexture2D_5EB2E67D_R_4 = _SampleTexture2D_5EB2E67D_RGBA_0.r;
			    float _SampleTexture2D_5EB2E67D_G_5 = _SampleTexture2D_5EB2E67D_RGBA_0.g;
			    float _SampleTexture2D_5EB2E67D_B_6 = _SampleTexture2D_5EB2E67D_RGBA_0.b;
			    float _SampleTexture2D_5EB2E67D_A_7 = _SampleTexture2D_5EB2E67D_RGBA_0.a;
			
			    // Node: Property
			    float4 _Property_5AB79687_Out_0 = Color_DCF944C2;
			
			    // Node: Multiply
			    float4 _Multiply_23F0F88D_Out_2;
			    Unity_Multiply_float((_SampleTexture2D_5EB2E67D_R_4.xxxx), _Property_5AB79687_Out_0, _Multiply_23F0F88D_Out_2);
			
			    // Node: Property
			    Gradient _Property_27170F86_Out_0 = Gradient_150991D1;
			
			    // Node: Property
			    float _Property_E377AE6C_Out_0 = Vector1_1FF39C4B;
			
			    // Node: Multiply
			    float _Multiply_16D86C1C_Out_2;
			    Unity_Multiply_float(_Split_EC6A98BB_G_2, _Property_E377AE6C_Out_0, _Multiply_16D86C1C_Out_2);
			
			    // Node: Sample Gradient
			    float4 _SampleGradient_C193F82B_Out_2;
			    Unity_SampleGradient_float(_Property_27170F86_Out_0, _Multiply_16D86C1C_Out_2, _SampleGradient_C193F82B_Out_2);
			
			    // Node: Multiply
			    float4 _Multiply_6C6D466_Out_2;
			    Unity_Multiply_float((_SampleTexture2D_5EB2E67D_G_5.xxxx), _SampleGradient_C193F82B_Out_2, _Multiply_6C6D466_Out_2);
			
			    // Node: Add
			    float4 _Add_D8A1D76E_Out_2;
			    Unity_Add_float4(_Multiply_23F0F88D_Out_2, _Multiply_6C6D466_Out_2, _Add_D8A1D76E_Out_2);
			
			    // Visual Effect Master
			    SG_Output_abfd42c13223447bfbe3d7528d18d99d OUT;
			    OUT.BaseColor_1 = (_Add_D8A1D76E_Out_2.xyz);
			    OUT.Metallic_2 = 0.5;
			    OUT.Smoothness_3 = 0.5;
			    OUT.Normal_8 = float3 (0, 0, 1);
			    OUT.Emissive_5 = float3(0, 0, 0);
			    OUT.Alpha_4 = 1;
			    return OUT;
			}
			

		    
			#pragma fragment frag
			void frag(ps_input i, OUTPUT_GBUFFER(outGBuffer)
		#if USE_DOUBLE_SIDED
			, bool frontFace : SV_IsFrontFace
		#endif
			)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				VFXTransformPSInputs(i);
				
							#ifdef VFX_VARYING_NORMAL
							#if USE_DOUBLE_SIDED
							const float faceMul = frontFace ? 1.0f : -1.0f;
							#else
							const float faceMul = 1.0f;
							#endif
							
							float3 normalWS = normalize(i.VFX_VARYING_NORMAL * faceMul);
							const VFXUVData uvData = GetUVData(i);
							
							#ifdef VFX_VARYING_TANGENT
							float3 tangentWS = normalize(i.VFX_VARYING_TANGENT.xyz);
							float3 bitangentWS = cross(normalWS,tangentWS) * (i.VFX_VARYING_TANGENT.w * faceMul);
							float3x3 tbn = float3x3(tangentWS,bitangentWS,normalWS);
							
							#if USE_NORMAL_MAP
							float3 n = SampleNormalMap(VFX_SAMPLER(normalMap),uvData);
							float normalScale = 1.0f;
							#ifdef VFX_VARYING_NORMALSCALE
							normalScale = i.VFX_VARYING_NORMALSCALE;
							#endif
							normalWS = normalize(lerp(normalWS,mul(n,tbn),normalScale));
							#endif
							#endif
							#endif
							
		        
		        #ifdef VFX_SHADERGRAPH
		            float4 Color_DCF944C2 = i.Color_DCF944C2;float2 RaycastTextureUV = i.RaycastTextureUV;
		            //Call Shader Graph
		            SG_Input_abfd42c13223447bfbe3d7528d18d99d INSG = (SG_Input_abfd42c13223447bfbe3d7528d18d99d)0;
		            float3 posRelativeWS = VFXGetPositionRWS(i.VFX_VARYING_POSWS);
		            float3 posAbsoluteWS = VFXGetPositionAWS(i.VFX_VARYING_POSWS);
		            INSG.ObjectSpacePosition = TransformWorldToObject(posRelativeWS);
		            
		            SG_Output_abfd42c13223447bfbe3d7528d18d99d OUTSG = SG_Evaluate_abfd42c13223447bfbe3d7528d18d99d(INSG,Texture2D_64C137FF, samplerTexture2D_64C137FF, Texture2D_64C137FF_TexelSize, Vector1_1FF39C4B, Color_DCF944C2);
		            

		
		            SurfaceData surface;
		            BuiltinData builtin;
		            surface = (SurfaceData)0;
		            builtin = (BuiltinData)0;
		            
		            surface.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
		            surface.specularOcclusion = 1.0f;
		            surface.ambientOcclusion = 1.0f;
		            surface.subsurfaceMask = 1.0f;
		            
		            #if HAS_SHADERGRAPH_PARAM_ALPHA
		                builtin.opacity = OUTSG.Alpha_4;
		                VFXClipFragmentColor(builtin.opacity,i);
		            #endif
		            
		            #if HAS_SHADERGRAPH_PARAM_SMOOTHNESS
		                surface.perceptualSmoothness = OUTSG.Smoothness_3;
		            #endif
		            #if HAS_SHADERGRAPH_PARAM_METALLIC
		                surface.metallic = OUTSG.Metallic_2;
		            #endif
		            #if HAS_SHADERGRAPH_PARAM_BASECOLOR
		                surface.baseColor = OUTSG.BaseColor_1;
		            #endif
		            
		            #if HAS_SHADERGRAPH_PARAM_NORMAL
		                float3 n =  OUTSG.Normal_8;
		                normalWS = mul(n,tbn);
		            #endif
		            
		            surface.normalWS = normalWS;
		            
		            #if HAS_SHADERGRAPH_PARAM_EMISSIVE
		                builtin.emissiveColor = OUTSG.Emissive_5;
		            #endif
		
		            
		            VFXSetupBuiltin(builtin,surface,builtin.emissiveColor, i);
		            ENCODE_INTO_GBUFFER(surface, builtin, i.VFX_VARYING_POSCS.xy, outGBuffer);
			float depth = LoadCameraDepth(i.VFX_VARYING_POSCS.xy);
			if (depth == i.VFX_VARYING_POSCS.z)
				RaycastTexture[i.VFX_VARYING_POSCS.xy] = float4(i.RaycastTextureUV.x, i.RaycastTextureUV.y, 0, 0);

		        #else
		            VFXComputePixelOutputToGBuffer(i,normalWS,uvData,outGBuffer);
			RaycastTexture[uvData] = float4(i.RaycastTextureUV.x, i.RaycastTextureUV.y, 0, 0);
		        #endif
			}
			ENDHLSL
		}
		

		Pass
		{		
			Tags { "LightMode"="Forward"}
					
			HLSLPROGRAM
			#pragma target 4.5
			
			#define UNITY_MATERIAL_LIT
			#define LIGHTLOOP_TILE_PASS
			#define _ENABLE_FOG_ON_TRANSPARENT
			#define _DISABLE_DECALS
			

			#pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST
			#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH
			#pragma multi_compile _ DEBUG_DISPLAY
			//#pragma enable_d3d11_debug_symbols
			
			#define HDRP_NEEDS_UVS (HDRP_USE_BASE_COLOR_MAP || HDRP_USE_MASK_MAP || USE_NORMAL_MAP || HDRP_USE_EMISSIVE_MAP)
			#define HDRP_USE_EMISSIVE (HDRP_USE_EMISSIVE_MAP || HDRP_USE_EMISSIVE_COLOR || HDRP_USE_ADDITIONAL_EMISSIVE_COLOR)
			
			
			
			
			
			
			
			
			
			struct ps_input
			{
				float4 pos : SV_POSITION;
				
				
							
							#if (VFX_NEEDS_COLOR_INTERPOLATOR && HDRP_USE_BASE_COLOR) || HDRP_USE_ADDITIONAL_BASE_COLOR
							VFX_OPTIONAL_INTERPOLATION float4 color : COLOR0;
							#endif
							#if HDRP_MATERIAL_TYPE_SPECULAR
							VFX_OPTIONAL_INTERPOLATION float3 specularColor : COLOR1;
							#endif
							#if HDRP_USE_EMISSIVE	
							VFX_OPTIONAL_INTERPOLATION float4 emissiveColor : COLOR2;
							#endif
							
							// x: smoothness
							// y: metallic/thickness
							// z: normal scale
							// w: emissive scale
							VFX_OPTIONAL_INTERPOLATION float4 materialProperties : TEXCOORD0;
							
				
				#if USE_FLIPBOOK_INTERPOLATION
				float4 uv : TEXCOORD1;
				#else
				float2 uv : TEXCOORD1;	
				#endif
		        #if VFX_SHADERGRAPH_HAS_UV1
		        float4 uv1 : COLOR2;
		        #endif
		        #if VFX_SHADERGRAPH_HAS_UV2
		        float4 uv2 : COLOR3;
		        #endif
		        #if VFX_SHADERGRAPH_HAS_UV3
		        float4 uv3 : COLOR4;
		        #endif
		        #if VFX_SHADERGRAPH_HAS_COLOR
		        float4 vertexColor : COLOR1;
		        #endif
				float3 normal : TEXCOORD2;
				#if USE_NORMAL_MAP || SHADERGRAPH_NEEDS_TANGENT_FORWARD
				float4 tangent : TEXCOORD3;
				#endif
				#if USE_SOFT_PARTICLE || USE_ALPHA_TEST || USE_FLIPBOOK_INTERPOLATION || WRITE_MOTION_VECTOR_IN_FORWARD
				// x: inverse soft particles fade distance
				// y: alpha threshold
				// z: frame blending factor
				nointerpolation float3 builtInInterpolants : TEXCOORD4;
				#endif
				
				#if USE_FLIPBOOK_MOTIONVECTORS
				// x: motion vector scale u
				// y: motion vector scale v
				nointerpolation float2 builtInInterpolants2 : TEXCOORD5;
				#endif
				
				#if WRITE_MOTION_VECTOR_IN_FORWARD
				float4 cPosPrevious : TEXCOORD6;
				float4 cPosNonJiterred : TEXCOORD7;
				#endif
		
				float3 posWS : TEXCOORD8; // Needed for fog
		        
		        VFX_OPTIONAL_INTERPOLATION float4 Color_DCF944C2 : NORMAL0;
		        VFX_OPTIONAL_INTERPOLATION float2 RaycastTextureUV : NORMAL1;
		        

				
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			struct ps_output
			{
				float4 color : SV_Target0;
			};
		
		
					#if (VFX_NEEDS_COLOR_INTERPOLATOR && HDRP_USE_BASE_COLOR) || HDRP_USE_ADDITIONAL_BASE_COLOR
					#define VFX_VARYING_COLOR color.rgb
					#define VFX_VARYING_ALPHA color.a
					#endif
					
					#define VFX_VARYING_SMOOTHNESS materialProperties.x
					
					#if HDRP_MATERIAL_TYPE_STANDARD
					#define VFX_VARYING_METALLIC materialProperties.y
					#elif HDRP_MATERIAL_TYPE_SPECULAR
					#define VFX_VARYING_SPECULAR specularColor
					#elif HDRP_MATERIAL_TYPE_TRANSLUCENT
					#define VFX_VARYING_THICKNESS materialProperties.y
					#endif
					
					#if USE_NORMAL_MAP
					#define VFX_VARYING_NORMALSCALE materialProperties.z
					#endif
					
					#if HDRP_USE_EMISSIVE_MAP
					#define VFX_VARYING_EMISSIVESCALE materialProperties.w
					#endif
					
					#if HDRP_USE_EMISSIVE_COLOR || HDRP_USE_ADDITIONAL_EMISSIVE_COLOR
					#define VFX_VARYING_EMISSIVE emissiveColor.rgb
					#endif
					
					#if USE_EXPOSURE_WEIGHT
					#define VFX_VARYING_EXPOSUREWEIGHT emissiveColor.a
					#endif
					
		
		#define VFX_VARYING_PS_INPUTS ps_input
		#define VFX_VARYING_POSCS pos
		#define VFX_VARYING_UV uv
		#define VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE builtInInterpolants.x
		#define VFX_VARYING_ALPHATHRESHOLD builtInInterpolants.y
		#define VFX_VARYING_FRAMEBLEND builtInInterpolants.z
		#define VFX_VARYING_MOTIONVECTORSCALE builtInInterpolants2.xy
		#define VFX_VARYING_NORMAL normal
		#if USE_NORMAL_MAP || SHADERGRAPH_NEEDS_TANGENT_FORWARD
		#define VFX_VARYING_TANGENT tangent
		#endif
		#define VFX_VARYING_POSWS posWS
		
		#if WRITE_MOTION_VECTOR_IN_FORWARD
		#define VFX_VARYING_VELOCITY_CPOS cPosNonJiterred
		#define VFX_VARYING_VELOCITY_CPOS_PREVIOUS cPosPrevious
		#endif
		
		
		
			#if !(defined(VFX_VARYING_PS_INPUTS) && defined(VFX_VARYING_POSCS))
			#error VFX_VARYING_PS_INPUTS, VFX_VARYING_POSCS and VFX_VARYING_UV must be defined.
			#endif
			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXCommon.hlsl"
			#include "Packages/com.unity.visualeffectgraph/Shaders/VFXCommon.hlsl"
			

			void SetAttribute_65DEC940(inout float pivotX, inout float pivotY, inout float pivotZ, float3 Pivot) /*attribute:pivot Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    pivotX = Pivot.x;
			    pivotY = Pivot.y;
			    pivotZ = Pivot.z;
			}
			void SetAttribute_3278B22F(inout float size, float Size) /*attribute:size Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    size = Size;
			}
			void SetAttribute_D5151640(inout float scaleX, inout float scaleZ, float2 Scale) /*attribute:scale Composition:Overwrite Source:Slot Random:Off channels:XZ */
			{
			    scaleX = Scale.x;
			    scaleZ = Scale.y;
			}
			void SetAttribute_CAC29747(inout float3 position, float3 Position) /*attribute:position Composition:Overwrite Source:Slot Random:Off channels:XYZ */
			{
			    position = Position;
			}
			void SetAttribute_D5151645(inout float scaleY, float Scale) /*attribute:scale Composition:Overwrite Source:Slot Random:Off channels:Y */
			{
			    scaleY = Scale.x;
			}
			

			
			struct vs_input
			{
				float3 pos : POSITION;
				float2 uv : TEXCOORD0;
			    #if VFX_SHADERGRAPH_HAS_UV1
			    float4 uv1 : TEXCOORD1;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    float4 uv2 : TEXCOORD2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    float4 uv3 : TEXCOORD3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    float4 vertexColor : COLOR;
			    #endif
				float3 normal : NORMAL;
				#if defined(VFX_VARYING_TANGENT) || SHADERGRAPH_HAS_NORMAL
				float4 tangent : TANGENT;
				#endif
				VFX_DECLARE_INSTANCE_ID
			};
			
			#pragma vertex vert
			VFX_VARYING_PS_INPUTS vert(vs_input i)
			{
			    VFX_VARYING_PS_INPUTS o = (VFX_VARYING_PS_INPUTS)0;
			
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			
				uint index = VFX_GET_INSTANCE_ID(i);	
			
				
						uint deadCount = 0;
						#if USE_DEAD_LIST_COUNT
						deadCount = deadListCount.Load(0);
						#endif	
						if (index >= asuint(nbMax) - deadCount)
						#if USE_GEOMETRY_SHADER
							return; // cull
						#else
							return o; // cull
						#endif
						
						Attributes attributes = (Attributes)0;
						SourceAttributes sourceAttributes = (SourceAttributes)0;
						
						#if VFX_HAS_INDIRECT_DRAW
						index = indirectBuffer[index];
						attributes.particleId = (attributeBuffer.Load((index * 0x2 + 0x0) << 2));
						attributes.scaleY = asfloat(attributeBuffer.Load((index * 0x2 + 0x1) << 2));
						attributes.position = float3(0, 0, 0);
						attributes.color = float3(1, 1, 1);
						attributes.alpha = (float)1;
						attributes.alive = (bool)true;
						attributes.axisX = float3(1, 0, 0);
						attributes.axisY = float3(0, 1, 0);
						attributes.axisZ = float3(0, 0, 1);
						attributes.angleX = (float)0;
						attributes.angleY = (float)0;
						attributes.angleZ = (float)0;
						attributes.pivotX = (float)0;
						attributes.pivotY = (float)0;
						attributes.pivotZ = (float)0;
						attributes.size = (float)0.100000001;
						attributes.scaleX = (float)1;
						attributes.scaleZ = (float)1;
						
				
						#else
						attributes.alive = (bool)true;
						
				
						#if !HAS_STRIPS
						if (!attributes.alive)
							return o;
						#endif
							
						attributes.particleId = (attributeBuffer.Load((index * 0x2 + 0x0) << 2));
						attributes.scaleY = asfloat(attributeBuffer.Load((index * 0x2 + 0x1) << 2));
						attributes.position = float3(0, 0, 0);
						attributes.color = float3(1, 1, 1);
						attributes.alpha = (float)1;
						attributes.axisX = float3(1, 0, 0);
						attributes.axisY = float3(0, 1, 0);
						attributes.axisZ = float3(0, 0, 1);
						attributes.angleX = (float)0;
						attributes.angleY = (float)0;
						attributes.angleZ = (float)0;
						attributes.pivotX = (float)0;
						attributes.pivotY = (float)0;
						attributes.pivotZ = (float)0;
						attributes.size = (float)0.100000001;
						attributes.scaleX = (float)1;
						attributes.scaleZ = (float)1;
						
				
						#endif
						
						// Initialize built-in needed attributes
						#if HAS_STRIPS
						InitStripAttributes(index, attributes, stripData);
						#endif
						
				{
				    SetAttribute_65DEC940( /*inout */attributes.pivotX,  /*inout */attributes.pivotY,  /*inout */attributes.pivotZ, float3(0, -0.5, 0));
				}
				SetAttribute_3278B22F( /*inout */attributes.size, Size_b);
				{
				    SetAttribute_D5151640( /*inout */attributes.scaleX,  /*inout */attributes.scaleZ, float2(0.00999999978, 0.00999999978));
				}
				{
				    int tmp_y = (int)attributes.particleId;
				    int tmp_ba = tmp_y / (int)256;
				    float tmp_bb = (float)tmp_ba;
				    float tmp_bc = floor(tmp_bb);
				    uint tmp_bd = (uint)tmp_y;
				    uint tmp_bf = tmp_bd / (uint)256;
				    uint tmp_bg = tmp_bf * (uint)256;
				    uint tmp_bh = tmp_bd - tmp_bg;
				    float tmp_bi = (float)tmp_bh;
				    float2 tmp_bj = float2(tmp_bc, tmp_bi);
				    float2 tmp_bl = tmp_bj / float2(256, 256);
				    float2 tmp_bn = tmp_bl * float2(64, 64);
				    float2 tmp_bp = tmp_bn + float2(0.125, 0.125);
				    float2 tmp_bq = tmp_bp / float2(64, 64);
				    float2 tmp_bs = tmp_bq * float2(640, 640);
				    float tmp_bt = tmp_bs[0];
				    float tmp_bv = tmp_bs[1];
				    float3 tmp_bw = float3(tmp_bt, (float)0, tmp_bv);
				    SetAttribute_CAC29747( /*inout */attributes.position, tmp_bw);
				}
				{
				    int tmp_y = (int)attributes.particleId;
				    int tmp_ba = tmp_y / (int)256;
				    float tmp_bb = (float)tmp_ba;
				    float tmp_bc = floor(tmp_bb);
				    uint tmp_bd = (uint)tmp_y;
				    uint tmp_bf = tmp_bd / (uint)256;
				    uint tmp_bg = tmp_bf * (uint)256;
				    uint tmp_bh = tmp_bd - tmp_bg;
				    float tmp_bi = (float)tmp_bh;
				    float2 tmp_bj = float2(tmp_bc, tmp_bi);
				    float2 tmp_bl = tmp_bj / float2(256, 256);
				    float2 tmp_bn = tmp_bl * float2(64, 64);
				    float2 tmp_bp = tmp_bn + float2(0.125, 0.125);
				    float2 tmp_bq = tmp_bp / float2(64, 64);
				    float4 tmp_bs = SampleTexture(VFX_SAMPLER(texture_b),tmp_bq,(float)0);
				    float tmp_bt = tmp_bs[0];
				    float tmp_bu = tmp_bt * uniform_c;
				    float tmp_bw = tmp_bu + (float)0.00999999978;
				    SetAttribute_D5151645( /*inout */attributes.scaleY, tmp_bw);
				}
				

						
				if (!attributes.alive)
					return o;
				
				o.VFX_VARYING_UV.xy = i.uv;
			    
			    #if VFX_SHADERGRAPH_HAS_UV1
			    o.uv1 = i.uv1;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV2
			    o.uv2 = i.uv2;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_UV3
			    o.uv3 = i.uv3;
			    #endif
			    #if VFX_SHADERGRAPH_HAS_COLOR
			    o.vertexColor = i.vertexColor;
			    #endif
				
				
						float3 size3 = float3(attributes.size,attributes.size,attributes.size);
						#if VFX_USE_SCALEX_CURRENT
						size3.x *= attributes.scaleX;
						#endif
						#if VFX_USE_SCALEY_CURRENT
						size3.y *= attributes.scaleY;
						#endif
						#if VFX_USE_SCALEZ_CURRENT
						size3.z *= attributes.scaleZ;
						#endif
						
				
				float3 inputVertexPosition = i.pos;
				float4x4 elementToVFX = GetElementToVFXMatrix(
					attributes.axisX,
					attributes.axisY,
					attributes.axisZ,
					float3(attributes.angleX,attributes.angleY,attributes.angleZ),
					float3(attributes.pivotX,attributes.pivotY,attributes.pivotZ),
					size3,
					attributes.position);
					
				float3 vPos = mul(elementToVFX,float4(inputVertexPosition,1.0f)).xyz;
				float4 csPos = TransformPositionVFXToClip(vPos);
				o.VFX_VARYING_POSCS = csPos;
				
				float3 normalWS = normalize(TransformDirectionVFXToWorld(mul((float3x3)elementToVFX, i.normal)));
				#ifdef VFX_VARYING_NORMAL // TODO Should use inverse transpose
				o.VFX_VARYING_NORMAL = normalWS;
				#endif
				#ifdef VFX_VARYING_TANGENT
				o.VFX_VARYING_TANGENT = float4(normalize(TransformDirectionVFXToWorld(mul((float3x3)elementToVFX,i.tangent.xyz))),i.tangent.w);
				#endif
			
				
						#if defined(VFX_VARYING_VELOCITY_CPOS) && defined(VFX_VARYING_VELOCITY_CPOS_PREVIOUS)
						float4x4 previousElementToVFX = (float4x4)0;
						previousElementToVFX[3] = float4(0,0,0,1);
						
						UNITY_UNROLL
						for (int itIndexMatrixRow = 0; itIndexMatrixRow < 3; ++itIndexMatrixRow)
						{
							UNITY_UNROLL
							for (int itIndexMatrixCol = 0; itIndexMatrixCol < 4; ++itIndexMatrixCol)
							{
								uint itIndexMatrix = itIndexMatrixCol * 4 + itIndexMatrixRow;
								uint read = elementToVFXBufferPrevious.Load((index * 16 + itIndexMatrix) << 2);
								previousElementToVFX[itIndexMatrixRow][itIndexMatrixCol] = asfloat(read);
							}
						}
						
						uint previousFrameIndex = elementToVFXBufferPrevious.Load((index * 16 + 15) << 2);
						o.VFX_VARYING_VELOCITY_CPOS = o.VFX_VARYING_VELOCITY_CPOS_PREVIOUS = float4(0.0f, 0.0f, 0.0f, 1.0f);
						if (asuint(currentFrameIndex) - previousFrameIndex == 1u)
						{
							float3 oldvPos = mul(previousElementToVFX,float4(inputVertexPosition, 1.0f)).xyz;
							o.VFX_VARYING_VELOCITY_CPOS_PREVIOUS = TransformPositionVFXToPreviousClip(oldvPos);
							o.VFX_VARYING_VELOCITY_CPOS = TransformPositionVFXToNonJitteredClip(vPos);
						}
						#endif
						
				
				
						#if VFX_USE_COLOR_CURRENT && defined(VFX_VARYING_COLOR)
						o.VFX_VARYING_COLOR = attributes.color;
						#endif
						#if VFX_USE_ALPHA_CURRENT && defined(VFX_VARYING_ALPHA) 
						o.VFX_VARYING_ALPHA = attributes.alpha;
						#endif
						
						#ifdef VFX_VARYING_EXPOSUREWEIGHT
						
						o.VFX_VARYING_EXPOSUREWEIGHT = exposureWeight;
						#endif
						
						#if USE_SOFT_PARTICLE && defined(VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE)
						
						o.VFX_VARYING_INVSOFTPARTICLEFADEDISTANCE = invSoftParticlesFadeDistance;
						#endif
						
						#if (USE_ALPHA_TEST || WRITE_MOTION_VECTOR_IN_FORWARD) && (!VFX_SHADERGRAPH || !HAS_SHADERGRAPH_PARAM_ALPHATHRESHOLD) && defined(VFX_VARYING_ALPHATHRESHOLD)
						float alphaThreshold = (float)0;
						{
						    
						    alphaThreshold = (float)0.5;
						}
						
				
						o.VFX_VARYING_ALPHATHRESHOLD = alphaThreshold;
						#endif
						
						#if USE_UV_SCALE_BIAS
						
						
						#if defined (VFX_VARYING_UV)
						o.VFX_VARYING_UV.xy = o.VFX_VARYING_UV.xy * uvScale + uvBias;
						#endif
						#endif
						
						#if defined(VFX_VARYING_POSWS)
						o.VFX_VARYING_POSWS = TransformPositionVFXToWorld(vPos);
						#endif
						
				
				
						#if USE_FLIPBOOK && defined(VFX_VARYING_UV)
						
						
						VFXUVData uvData = GetUVData(flipBookSize, invFlipBookSize, o.VFX_VARYING_UV.xy, attributes.texIndex);
						o.VFX_VARYING_UV.xy = uvData.uvs.xy;
						#if USE_FLIPBOOK_INTERPOLATION && defined(VFX_VARYING_UV) && defined (VFX_VARYING_FRAMEBLEND)
						o.VFX_VARYING_UV.zw = uvData.uvs.zw;
						o.VFX_VARYING_FRAMEBLEND = uvData.blend;
						#if USE_FLIPBOOK_MOTIONVECTORS && defined(VFX_VARYING_MOTIONVECTORSCALE)
						
						o.VFX_VARYING_MOTIONVECTORSCALE = motionVectorScale * invFlipBookSize;
						#endif
						#endif
						#endif
						
			
				
						
									#ifndef VFX_SHADERGRAPH
									#ifdef VFX_VARYING_SMOOTHNESS
									
									o.VFX_VARYING_SMOOTHNESS = smoothness;
									#endif
									#if HDRP_MATERIAL_TYPE_STANDARD
									#ifdef VFX_VARYING_METALLIC
									
									o.VFX_VARYING_METALLIC = metallic;
									#endif
									#elif HDRP_MATERIAL_TYPE_SPECULAR
									#ifdef VFX_VARYING_SPECULAR
									
									o.VFX_VARYING_SPECULAR = specularColor;
									#endif
									#elif HDRP_MATERIAL_TYPE_TRANSLUCENT
									#ifdef VFX_VARYING_THICKNESS
									
									o.VFX_VARYING_THICKNESS = thickness;
									#endif
									#endif
									#if USE_NORMAL_MAP
									#ifdef VFX_VARYING_NORMALSCALE
									
									o.VFX_VARYING_NORMALSCALE = normalScale;
									#endif
									#endif
									#if HDRP_USE_EMISSIVE_MAP
									#ifdef VFX_VARYING_EMISSIVESCALE
									
									o.VFX_VARYING_EMISSIVESCALE = emissiveScale;
									#endif
									#endif
									#ifdef VFX_VARYING_EMISSIVE
									#if HDRP_USE_EMISSIVE_COLOR
									o.VFX_VARYING_EMISSIVE = attributes.color;
									#elif HDRP_USE_ADDITIONAL_EMISSIVE_COLOR
									
									o.VFX_VARYING_EMISSIVE = emissiveColor;
									#endif
									#endif
									#if HDRP_USE_ADDITIONAL_BASE_COLOR
									#ifdef VFX_VARYING_COLOR
									
									o.VFX_VARYING_COLOR = baseColor;
									#endif
									#endif
									#endif
									
						
			    
			    float4 Color_DCF944C2__ = (float4)0;{
			        
			        Color_DCF944C2__ = float4(0, 1, 0.96201396, 0);
			    }
			    o.Color_DCF944C2 = Color_DCF944C2__;float2 RaycastTextureUV__ = (float2)0;{
			        int tmp_y = (int)attributes.particleId;
			        int tmp_ba = tmp_y / (int)256;
			        float tmp_bb = (float)tmp_ba;
			        float tmp_bc = floor(tmp_bb);
			        uint tmp_bd = (uint)tmp_y;
			        uint tmp_bf = tmp_bd / (uint)256;
			        uint tmp_bg = tmp_bf * (uint)256;
			        uint tmp_bh = tmp_bd - tmp_bg;
			        float tmp_bi = (float)tmp_bh;
			        float2 tmp_bj = float2(tmp_bc, tmp_bi);
			        float2 tmp_bl = tmp_bj / float2(256, 256);
			        
			        RaycastTextureUV__ = tmp_bl;
			    }
			    o.RaycastTextureUV = RaycastTextureUV__;

				
				return o;
			}
			
			
			
			#include "Packages/com.unity.visualeffectgraph/Shaders/VFXCommonOutput.hlsl"
			
			
			
		
			#define SHADERPASS SHADERPASS_FORWARD
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXLit.hlsl"
			
			#ifndef VFX_SHADERGRAPH
			
			void VFXGetHDRPLitData(out SurfaceData surfaceData, out BuiltinData builtinData, out BSDFData bsdfData, out PreLightData preLightData, VFX_VARYING_PS_INPUTS i, float3 normalWS, const VFXUVData uvData, uint2 tileIndex)
			{	
				#if HDRP_MATERIAL_TYPE_TRANSLUCENT
				 // Loads diffusion profile
				#else
				const uint diffusionProfileHash = 0;
				#endif
				
				float3 posRWS = VFXGetPositionRWS(i);
				float4 posSS = i.VFX_VARYING_POSCS;
				PositionInputs posInput = GetPositionInput(posSS.xy, _ScreenSize.zw, posSS.z, posSS.w, posRWS, tileIndex);
				
				float alpha;
				surfaceData = VFXGetSurfaceData(i,normalWS,uvData,diffusionProfileHash,alpha);	
				bsdfData = ConvertSurfaceDataToBSDFData(posSS.xy, surfaceData);
			
				preLightData = GetPreLightData(GetWorldSpaceNormalizeViewDir(posRWS),posInput,bsdfData);
				
				preLightData.diffuseFGD = 1.0f;
			    //TODO: investigate why this is needed
			    preLightData.coatPartLambdaV = 0;
			    preLightData.coatIblR = 0;
			    preLightData.coatIblF = 0;
			    
				builtinData = VFXGetBuiltinData(i,posInput,surfaceData,uvData,alpha);
			}
			
			void VFXGetHDRPLitData(out SurfaceData surfaceData, out BuiltinData builtinData, VFX_VARYING_PS_INPUTS i, float3 normalWS, const VFXUVData uvData)
			{
				BSDFData bsdfData = (BSDFData)0;
				PreLightData preLightData = (PreLightData)0;
				preLightData.diffuseFGD = 1.0f;
				VFXGetHDRPLitData(surfaceData,builtinData,bsdfData,preLightData,i,normalWS,uvData,uint2(0,0));
			}
			
			#endif
			
			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/VFXGraph/Shaders/VFXLitPixelOutput.hlsl"
			
			
			
				
		    
		    		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		    		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
		    		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		    		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinGIUtilities.hlsl"
		    		#ifndef SHADERPASS
		    		#error Shaderpass should be defined at this stage.
		    		#endif
		    		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
		    		
		    
		    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
		    // Node: Add
		    void Unity_Add_float(float A, float B, out float Out)
		    {
		        Out = A + B;
		    }
		    
		    // Node: Divide
		    void Unity_Divide_float(float A, float B, out float Out)
		    {
		        Out = A / B;
		    }
		    
		    // Node: Multiply
		    void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
		    {
		        Out = A * B;
		    }
		    
		    // Node: Multiply
		    void Unity_Multiply_float(float A, float B, out float Out)
		    {
		        Out = A * B;
		    }
		    
		    // Node: Sample Gradient
		    void Unity_SampleGradient_float(Gradient Gradient, float Time, out float4 Out)
		    {
		        float3 color = Gradient.colors[0].rgb;
		        [unroll]
		        for (int c = 1; c < 8; c++)
		        {
		            float colorPos = saturate((Time - Gradient.colors[c-1].w) / (Gradient.colors[c].w - Gradient.colors[c-1].w)) * step(c, Gradient.colorsLength-1);
		            color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
		        }
		    #ifndef UNITY_COLORSPACE_GAMMA
		        color = SRGBToLinear(color);
		    #endif
		        float alpha = Gradient.alphas[0].x;
		        [unroll]
		        for (int a = 1; a < 8; a++)
		        {
		            float alphaPos = saturate((Time - Gradient.alphas[a-1].y) / (Gradient.alphas[a].y - Gradient.alphas[a-1].y)) * step(a, Gradient.alphasLength-1);
		            alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
		        }
		        Out = float4(color, alpha);
		    }
		    
		    // Node: Add
		    void Unity_Add_float4(float4 A, float4 B, out float4 Out)
		    {
		        Out = A + B;
		    }
		    
		    // Property: Gradient
		    Gradient Gradient_150991D1_Definition()
		    {
		        Gradient g;
		        g.type = 0;
		        g.colorsLength = 2;
		        g.alphasLength = 2;
		        g.colors[0] = float4(0.004405484, 0.02539425, 0.08490568, 0.002944991);
		        g.colors[1] = float4(2, 1.294118, 0.6196079, 1);
		        g.colors[2] = float4(0, 0, 0, 0);
		        g.colors[3] = float4(0, 0, 0, 0);
		        g.colors[4] = float4(0, 0, 0, 0);
		        g.colors[5] = float4(0, 0, 0, 0);
		        g.colors[6] = float4(0, 0, 0, 0);
		        g.colors[7] = float4(0, 0, 0, 0);
		        g.alphas[0] = float2(1, 0);
		        g.alphas[1] = float2(1, 1);
		        g.alphas[2] = float2(0, 0);
		        g.alphas[3] = float2(0, 0);
		        g.alphas[4] = float2(0, 0);
		        g.alphas[5] = float2(0, 0);
		        g.alphas[6] = float2(0, 0);
		        g.alphas[7] = float2(0, 0);
		        return g;
		    }
		    #define Gradient_150991D1 Gradient_150991D1_Definition()
		    
		    struct SG_Input_abfd42c13223447bfbe3d7528d18d99d
		    {
		        float3 ObjectSpacePosition;
		    };
		    
		    struct SG_Output_abfd42c13223447bfbe3d7528d18d99d
		    {
		        float3 BaseColor_1;
		        float Metallic_2;
		        float Smoothness_3;
		        float3 Normal_8;
		        float3 Emissive_5;
		        float Alpha_4;
		    };
		    
		    SG_Output_abfd42c13223447bfbe3d7528d18d99d SG_Evaluate_abfd42c13223447bfbe3d7528d18d99d(
		        SG_Input_abfd42c13223447bfbe3d7528d18d99d IN,
		        /* Property: ASHL */ TEXTURE2D_PARAM(Texture2D_64C137FF, samplerTexture2D_64C137FF), float4 Texture2D_64C137FF_TexelSize,
		        /* Property: Vector1 */ float Vector1_1FF39C4B,
		        /* Property: Color */ float4 Color_DCF944C2)
		    {
		        // Node: Split
		        float _Split_EC6A98BB_R_1 = IN.ObjectSpacePosition[0];
		        float _Split_EC6A98BB_G_2 = IN.ObjectSpacePosition[1];
		        float _Split_EC6A98BB_B_3 = IN.ObjectSpacePosition[2];
		        float _Split_EC6A98BB_A_4 = 0;
		    
		        // Node: Add
		        float _Add_54466F3B_Out_2;
		        Unity_Add_float(0, _Split_EC6A98BB_R_1, _Add_54466F3B_Out_2);
		    
		        // Node: Divide
		        float _Divide_B561D354_Out_2;
		        Unity_Divide_float(_Add_54466F3B_Out_2, 640, _Divide_B561D354_Out_2);
		    
		        // Node: Add
		        float _Add_2855D0DF_Out_2;
		        Unity_Add_float(0.0001, _Divide_B561D354_Out_2, _Add_2855D0DF_Out_2);
		    
		        // Node: Add
		        float _Add_D3D19FE7_Out_2;
		        Unity_Add_float(0, _Split_EC6A98BB_B_3, _Add_D3D19FE7_Out_2);
		    
		        // Node: Divide
		        float _Divide_FCE5A996_Out_2;
		        Unity_Divide_float(_Add_D3D19FE7_Out_2, 640, _Divide_FCE5A996_Out_2);
		    
		        // Node: Add
		        float _Add_15F593F9_Out_2;
		        Unity_Add_float(0.0001, _Divide_FCE5A996_Out_2, _Add_15F593F9_Out_2);
		    
		        // Node: Vector 2
		        float2 _Vector2_799C5B31_Out_0 = float2(_Add_2855D0DF_Out_2, _Add_15F593F9_Out_2);
		    
		        // Node: Sample Texture 2D
		        float4 _SampleTexture2D_5EB2E67D_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_64C137FF, samplerTexture2D_64C137FF, _Vector2_799C5B31_Out_0);
		        float _SampleTexture2D_5EB2E67D_R_4 = _SampleTexture2D_5EB2E67D_RGBA_0.r;
		        float _SampleTexture2D_5EB2E67D_G_5 = _SampleTexture2D_5EB2E67D_RGBA_0.g;
		        float _SampleTexture2D_5EB2E67D_B_6 = _SampleTexture2D_5EB2E67D_RGBA_0.b;
		        float _SampleTexture2D_5EB2E67D_A_7 = _SampleTexture2D_5EB2E67D_RGBA_0.a;
		    
		        // Node: Property
		        float4 _Property_5AB79687_Out_0 = Color_DCF944C2;
		    
		        // Node: Multiply
		        float4 _Multiply_23F0F88D_Out_2;
		        Unity_Multiply_float((_SampleTexture2D_5EB2E67D_R_4.xxxx), _Property_5AB79687_Out_0, _Multiply_23F0F88D_Out_2);
		    
		        // Node: Property
		        Gradient _Property_27170F86_Out_0 = Gradient_150991D1;
		    
		        // Node: Property
		        float _Property_E377AE6C_Out_0 = Vector1_1FF39C4B;
		    
		        // Node: Multiply
		        float _Multiply_16D86C1C_Out_2;
		        Unity_Multiply_float(_Split_EC6A98BB_G_2, _Property_E377AE6C_Out_0, _Multiply_16D86C1C_Out_2);
		    
		        // Node: Sample Gradient
		        float4 _SampleGradient_C193F82B_Out_2;
		        Unity_SampleGradient_float(_Property_27170F86_Out_0, _Multiply_16D86C1C_Out_2, _SampleGradient_C193F82B_Out_2);
		    
		        // Node: Multiply
		        float4 _Multiply_6C6D466_Out_2;
		        Unity_Multiply_float((_SampleTexture2D_5EB2E67D_G_5.xxxx), _SampleGradient_C193F82B_Out_2, _Multiply_6C6D466_Out_2);
		    
		        // Node: Add
		        float4 _Add_D8A1D76E_Out_2;
		        Unity_Add_float4(_Multiply_23F0F88D_Out_2, _Multiply_6C6D466_Out_2, _Add_D8A1D76E_Out_2);
		    
		        // Visual Effect Master
		        SG_Output_abfd42c13223447bfbe3d7528d18d99d OUT;
		        OUT.BaseColor_1 = (_Add_D8A1D76E_Out_2.xyz);
		        OUT.Metallic_2 = 0.5;
		        OUT.Smoothness_3 = 0.5;
		        OUT.Normal_8 = float3 (0, 0, 1);
		        OUT.Emissive_5 = float3(0, 0, 0);
		        OUT.Alpha_4 = 1;
		        return OUT;
		    }
		    

		
			#pragma fragment frag
			void frag(ps_input i
			, out float4 outColor : SV_Target0
		#if USE_DOUBLE_SIDED
			, bool frontFace : SV_IsFrontFace
		#endif
		#if WRITE_MOTION_VECTOR_IN_FORWARD
			, out float4 outMotionVector : SV_Target1
		#endif
			)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				VFXTransformPSInputs(i);
				
							#ifdef VFX_VARYING_NORMAL
							#if USE_DOUBLE_SIDED
							const float faceMul = frontFace ? 1.0f : -1.0f;
							#else
							const float faceMul = 1.0f;
							#endif
							
							float3 normalWS = normalize(i.VFX_VARYING_NORMAL * faceMul);
							const VFXUVData uvData = GetUVData(i);
							
							#ifdef VFX_VARYING_TANGENT
							float3 tangentWS = normalize(i.VFX_VARYING_TANGENT.xyz);
							float3 bitangentWS = cross(normalWS,tangentWS) * (i.VFX_VARYING_TANGENT.w * faceMul);
							float3x3 tbn = float3x3(tangentWS,bitangentWS,normalWS);
							
							#if USE_NORMAL_MAP
							float3 n = SampleNormalMap(VFX_SAMPLER(normalMap),uvData);
							float normalScale = 1.0f;
							#ifdef VFX_VARYING_NORMALSCALE
							normalScale = i.VFX_VARYING_NORMALSCALE;
							#endif
							normalWS = normalize(lerp(normalWS,mul(n,tbn),normalScale));
							#endif
							#endif
							#endif
							
		        
		        #ifdef VFX_SHADERGRAPH
		            float4 Color_DCF944C2 = i.Color_DCF944C2;float2 RaycastTextureUV = i.RaycastTextureUV;
		            //Call Shader Graph
		            SG_Input_abfd42c13223447bfbe3d7528d18d99d INSG = (SG_Input_abfd42c13223447bfbe3d7528d18d99d)0;
		            float3 posRelativeWS = VFXGetPositionRWS(i.VFX_VARYING_POSWS);
		            float3 posAbsoluteWS = VFXGetPositionAWS(i.VFX_VARYING_POSWS);
		            INSG.ObjectSpacePosition = TransformWorldToObject(posRelativeWS);
		            
		            SG_Output_abfd42c13223447bfbe3d7528d18d99d OUTSG = SG_Evaluate_abfd42c13223447bfbe3d7528d18d99d(INSG,Texture2D_64C137FF, samplerTexture2D_64C137FF, Texture2D_64C137FF_TexelSize, Vector1_1FF39C4B, Color_DCF944C2);
		            

		            
		            SurfaceData surface;
		            BuiltinData builtin;
		            surface = (SurfaceData)0;
		            builtin = (BuiltinData)0;
		            
		            surface.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
		            surface.specularOcclusion = 1.0f;
		            surface.ambientOcclusion = 1.0f;
		            surface.subsurfaceMask = 1.0f;
		            
		            #if HAS_SHADERGRAPH_PARAM_ALPHA
		                builtin.opacity = OUTSG.Alpha_4;
		                VFXClipFragmentColor(builtin.opacity,i);
		            #endif
		            
		            #if HAS_SHADERGRAPH_PARAM_SMOOTHNESS
		                surface.perceptualSmoothness = OUTSG.Smoothness_3;
		            #endif
		            #if HAS_SHADERGRAPH_PARAM_METALLIC
		                surface.metallic = OUTSG.Metallic_2;
		            #endif
		            #if HAS_SHADERGRAPH_PARAM_BASECOLOR
		                surface.baseColor = OUTSG.BaseColor_1;
		            #endif
		            
		            #if HAS_SHADERGRAPH_PARAM_NORMAL
		                float3 n =  OUTSG.Normal_8;
		                normalWS = mul(n,tbn);
		            #endif
		            
		            surface.normalWS = normalWS;
		            
		            #if HAS_SHADERGRAPH_PARAM_EMISSIVE
		                builtin.emissiveColor = OUTSG.Emissive_5;
		            #endif
		
		            
		            outColor = VFXGetPixelOutputForwardShaderGraph(surface, builtin,i);
		        #else
		            outColor = VFXGetPixelOutputForward(i,normalWS,uvData);
		        #endif
				
				#if WRITE_MOTION_VECTOR_IN_FORWARD
					
							float2 velocity = (i.VFX_VARYING_VELOCITY_CPOS.xy/i.VFX_VARYING_VELOCITY_CPOS.w) - (i.VFX_VARYING_VELOCITY_CPOS_PREVIOUS.xy/i.VFX_VARYING_VELOCITY_CPOS_PREVIOUS.w);
							#if UNITY_UV_STARTS_AT_TOP
								velocity.y = -velocity.y;
							#endif
							float4 encodedMotionVector = 0.0f;
							VFXEncodeMotionVector(velocity * 0.5f, encodedMotionVector);
							
					outMotionVector = encodedMotionVector;
					outMotionVector.a = outColor.a < i.VFX_VARYING_ALPHATHRESHOLD ? 0.0f : 1.0f; //Independant clipping for motion vector pass
				#endif
			}
			ENDHLSL
		}
		

		
	}
}
