
////////OPTIONS////////////////
bool DiffuseEnabled = true;

////////MATRICES/////////////////////////////////////////////
//float4x4 World;
//float4x4 View;
//float4x4 Projection;
float4x4 WVPMatrix;
float4x4 WorldInverseTranspose;

////////AMBIENT VARIABLES
float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.2;

/////////DIFFUSE VARIABLES
const static int DiffuseLightSources = 4;
float3 DiffuseLightDirections[DiffuseLightSources];//Normalized directionVector
float4 DiffuseColors[DiffuseLightSources];//lightcolor
float DiffuseIntensities[DiffuseLightSources];//intensity

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

//////////Structs
struct VertexShaderInput
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

///////////////////Process Diffuse-Color
float4 DiffuseColor(float4 normal)
{
	float4 tempColor;
	if(DiffuseEnabled)
		tempColor = (0, 0, 0, 0);
	else
		tempColor = (1, 1, 1, 1);

    for (int i = 0; i < DiffuseLightSources; i++)
	{
		if(DiffuseIntensities[i] != 0)
		{
			float lightIntensity = saturate(dot(normal,-DiffuseLightDirections[i]));
			
			tempColor +=  DiffuseColors[i] * DiffuseIntensities[i] * lightIntensity;
		}
    }
	return saturate(tempColor);
}

////////////////////////////////////////////////DIFFUSE-SHADER/////////////////////
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)//VERTEX
{
	//Set Position
	float4 inputPosition  = float4(input.PositionNormal.xyz, 1.0);
	float4 inputNormal = normalArray[input.PositionNormal.w];
    VertexShaderOutput output;
	//Set Normal
	output.Position = mul(inputPosition, WVPMatrix);
    float4 normal = mul(inputNormal, WorldInverseTranspose);
    output.Normal = normal;
	output.TextureCoordinate = input.TextureCoordinate;

	output.Color = DiffuseColor(normal);
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 //PIXEL
{
    float4 textureColor = tex2D(retroTextureSampler, input.TextureCoordinate);
    textureColor.a = 1;
    textureColor.rgb *= input.Color + AmbientColor * AmbientIntensity;
    return saturate(textureColor);
}

/////////Techniques/////////////////////////////////////////////////
technique CharacterShader
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}