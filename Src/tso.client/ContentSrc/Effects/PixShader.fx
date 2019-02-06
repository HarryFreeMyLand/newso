﻿//Vertex shader output structure
struct VertexToPixel
{
	float4 VertexPosition : SV_Position0;
	
	float2 ATextureCoord : TEXCOORD0;
	float2 BTextureCoord : TEXCOORD1;
	float2 CTextureCoord : TEXCOORD2;
	float2 BlendTextureCoord : TEXCOORD3;
	float2 RoadTextureCoord : TEXCOORD4;
	float2 RoadCTextureCoord : TEXCOORD5;
	float2 vPos: TEXCOORD6;
	float Depth: TEXCOORD7;
};

struct VertexToShad
{
	float4 Position : SV_Position0;
    float Depth : TEXCOORD0;
};

texture2D VertexColorTex;
sampler2D USampler = sampler_state
{
	Texture = <VertexColorTex>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

texture2D ShadowMap;
sampler2D ShadSampler = sampler_state
{
	Texture = <ShadowMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

texture2D TextureAtlasTex;
sampler2D USamplerTex = sampler_state
{
	Texture = <TextureAtlasTex>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};

texture2D TransAtlasTex;
sampler2D USamplerBlend = sampler_state
{
	Texture = <TransAtlasTex>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};

texture2D RoadAtlasTex;
sampler2D RSamplerTex = sampler_state
{
	Texture = <RoadAtlasTex>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};

texture2D RoadAtlasCTex;
sampler2D RCSamplerTex = sampler_state
{
	Texture = <RoadAtlasCTex>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};
float4 LightCol;
float2 ShadSize;
float ShadowMult;

float4 GetCityColor(VertexToPixel Input)
{
	float4 BlendA = tex2D(USamplerBlend, Input.BlendTextureCoord);
	float4 Base = tex2D(USamplerTex, Input.BTextureCoord);
	float4 Blend = tex2D(USamplerTex, Input.CTextureCoord);
	float4 Road = tex2D(RSamplerTex, Input.RoadTextureCoord);
	float4 RoadC = tex2D(RCSamplerTex, Input.RoadCTextureCoord);
	
	float A = BlendA.x;
	float InvA = 1.0 - A;
	
	Base = Base*InvA + Blend*A;
	Base *= tex2D(USampler, Input.ATextureCoord);
	
	Base = Base*(1.0-Road.w) + Road*Road.w;
	Base = Base*(1.0-RoadC.w) + RoadC*RoadC.w;
	return Base * LightCol;
}

float shadowCompare(sampler2D map, float2 pos, float compare) {
	float depth = (float)tex2D(map, pos);
	return step(depth, compare);
}

float shadowLerp(sampler2D depths, float2 size, float2 uv, float compare){
	float2 texelSize = float2(1.0, 1.0)/size;
	float2 f = frac(uv*size+0.5);
	float2 centroidUV = floor(uv*size+0.5)/size;

	float lb = shadowCompare(depths, centroidUV+texelSize*float2(0.0, 0.0), compare);
	float lt = shadowCompare(depths, centroidUV+texelSize*float2(0.0, 1.0), compare);
	float rb = shadowCompare(depths, centroidUV+texelSize*float2(1.0, 0.0), compare);
	float rt = shadowCompare(depths, centroidUV+texelSize*float2(1.0, 1.0), compare);
	float a = lerp(lb, lt, f.y);
	float b = lerp(rb, rt, f.y);
	float c = lerp(a, b, f.x);
	return c;
}

float4 CityPS(VertexToPixel Input) : COLOR0
{

	float4 BCol = GetCityColor(Input);
	float depth = Input.Depth.x;

	return float4(BCol.xyz*lerp(ShadowMult, 1, shadowLerp(ShadSampler, ShadSize, Input.vPos, depth+0.003*(2048.0/ShadSize.x))), 1);

}

float4 CityPSNoShad(VertexToPixel Input) : COLOR0
{
	return GetCityColor(Input);
}

float4 ShadowMapPS(VertexToShad Input) : COLOR0
{
	return float4(Input.Depth.x, 0, 0, 1);
}

technique RenderCity
{
	pass Final
	{
#if SM4
        PixelShader = compile ps_4_0_level_9_1 CityPS();
#else
        PixelShader = compile ps_3_0 CityPS();
#endif;
	}
	
	pass ShadowMap
	{
#if SM4
        PixelShader = compile ps_4_0_level_9_1 ShadowMapPS();
#else
        PixelShader = compile ps_3_0 ShadowMapPS();
#endif;
	}
	
	pass FinalNoShadow
	{
#if SM4
        PixelShader = compile ps_4_0_level_9_1 CityPSNoShad();
#else
        PixelShader = compile ps_3_0 CityPSNoShad();
#endif;
	}
}
