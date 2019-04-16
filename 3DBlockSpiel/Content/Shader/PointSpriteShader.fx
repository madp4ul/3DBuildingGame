float4x4 World;
float4x4 View;
float4x4 Projection;

////////Technique: Pointsprite
float3 CamPos;
float3 CamUp;
float PointSpriteSize;
float4 SpriteColor;

////////TextureSampler
texture Texture;

sampler2D linearTextureSampler = sampler_state
{
    Texture = (Texture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
};

////////////////////////////////////////////////POINTSPRITE-SHADER/////////////////////

VertexShaderOutput PointSpriteVS(float3 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
    VertexShaderOutput Output = (VertexShaderOutput)0;

    float3 center = mul(inPos, World);
    float3 eyeVector = center - CamPos;

    float3 sideVector = cross(eyeVector,CamUp);
    sideVector = normalize(sideVector);
    float3 upVector = cross(sideVector,eyeVector);
    upVector = normalize(upVector);

    float3 finalPosition = center;
    finalPosition += (inTexCoord.x-0.5f)*sideVector*0.5f*PointSpriteSize;
    finalPosition += (0.5f-inTexCoord.y)*upVector*0.5f*PointSpriteSize;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul (View, Projection);
    Output.Position = mul(finalPosition4, preViewProjection);
	Output.Color = SpriteColor;
    Output.TextureCoordinate = inTexCoord;

    return Output;
}

float4 PointSpritePS(VertexShaderOutput PSIn) : COLOR0
{
    float4 color = tex2D(linearTextureSampler, PSIn.TextureCoordinate);

	color *= PSIn.Color;
    return color;
}

technique PointSprite
{
	pass Pass0
	{   
		VertexShader = compile vs_5_0 PointSpriteVS();
		PixelShader  = compile ps_5_0 PointSpritePS();
	}
}
