
bool DiffuseEnabled = true;


////////Matrices/////////////////////////////////////////////
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WVPMatrix;
float4x4 WorldInverseTranspose;

////////Ambient
float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.2;
////////Technique: Pointsprite
float3 CamPos;
float3 CamUp;
float PointSpriteSize;
float4 SpriteColor;

////////Diffuse-Lighting
// Maximum number of Diffuse-LightSources
const static int DiffuseLightSources = 1;
//Normalized directionVector
float3 DiffuseLightDirections[DiffuseLightSources];
// = {
  //  float3(-1,0,0), float3(0,-1,0), float3(0,0,-1), float3(-1,0,0)};
//Color: all values between 0 and 1
float4 DiffuseColors[DiffuseLightSources];
// = {
  //  float4(1, 0, 0, 1),float4(0, 1, 0, 1),float4(0, 0, 1, 1),float4(0, 0, 1, 1)};
//intensity: all values between 0 and 1
float DiffuseIntensities[DiffuseLightSources];
// = {
  //  0, 0, 0, 0};

////////BlockShader
const float4 normalArray[6] = {
	float4(1, 0, 0, 1),
	float4(0, 1, 0, 1),
	float4(0, 0, 1, 1),
	float4(-1, 0, 0, 1),
	float4(0, -1, 0, 1),
	float4(0, 0, -1, 1)
	};

////////TextureSampler
texture Texture;
sampler2D retroTextureSampler = sampler_state
{
    Texture = (Texture);
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
sampler2D linearTextureSampler = sampler_state
{
    Texture = (Texture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};
//////////////////////Structs
struct BlockVertexShaderInput
{
    float4 PositionNormal : POSITION0;
    float TextureCoordinateX : TEXCOORD0;
    float TextureCoordinateY : TEXCOORD1;
};

struct NewVertexShaderInput
{
    float4 PositionNormal : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
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

////////////////////////////////////////////////NEW DIFFUSE-SHADER/////////////////////
VertexShaderOutput NewVertexShaderFunction(NewVertexShaderInput input)
{
	float4 inputPosition  = float4(input.PositionNormal.xyz, 1.0);
	float4 inputNormal = normalArray[input.PositionNormal.w];
    VertexShaderOutput output;
	
	output.Position = mul(inputPosition, WVPMatrix);
    float4 normal = mul(inputNormal, WorldInverseTranspose);
    output.Normal = normal;
	output.TextureCoordinate = input.TextureCoordinate;

	float4 tempColor = (0, 0, 0, 0);
    for (int i = 0; i < DiffuseLightSources; i++)
	{
		if(DiffuseIntensities[i] != 0)
		{
			float lightIntensity = saturate(dot(normal,-DiffuseLightDirections[i]));
			
			tempColor +=  DiffuseColors[i] * DiffuseIntensities[i] * lightIntensity;
		}
    }
	output.Color = saturate(tempColor);
    return output;
}

float4 NewPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 textureColor = tex2D(retroTextureSampler, input.TextureCoordinate);
    textureColor.a = 1;
    textureColor.rgb *= input.Color + AmbientColor * AmbientIntensity;
    return saturate(textureColor);
}

/////////Techniques/////////////////////////////////////////////////
technique NewDiffuse
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 NewVertexShaderFunction();
        PixelShader = compile ps_5_0 NewPixelShaderFunction();
    }
}

///////////JUST DRAW SHADER ////////////////////////////////////////
float4 VertexDrawOnly(float4 inPos : POSITION0 ) : POSITION0
{
return inPos;
}

float4 PixelDrawOnly(float4 inPos : POSITION0) : COLOR0
{
return float4(1,1,1,1);
}

technique JustDraw
{
	pass Pass0
	{   
		VertexShader = compile vs_5_0 VertexDrawOnly();
		PixelShader  = compile ps_5_0 PixelDrawOnly();
	}
}