
////////OPTIONS////////////////
bool DiffuseEnabled = true;
bool BumpMapsEnabled = true;

////////MATRICES/////////////////////////////////////////////
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

////////BUMPMAPPING
float BumpConstant = 2;
texture NormalMap;
sampler2D bumpSampler = sampler_state
{
	Texture = (NormalMap);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

////////VertexNormals
const float4 normalArray[6] = {
	float4(1, 0, 0, 1),//
	float4(0, 1, 0, 1),//
	float4(0, 0, 1, 1),//
	float4(-1, 0, 0, 1),//
	float4(0, -1, 0, 1),
	float4(0, 0, -1, 1)//
};
//VertexTangents
const float4 tangentArray[6] = {
	float4(0, 0, 1, 1),
	float4(-1, 0, 0, 1),
	float4(-1, 0, 0, 1),
	float4(0, 0, -1, 1),
	float4(-1, 0, 0, 1),
	float4(1, 0, 0, 1)
};
const float4 binormalArray[6] = {
	float4(0, 1, 0, 1),
	float4(0, 0, -1, 1),
	float4(0, 1, 0, 1),
	float4(0, 1, 0, 1),
	float4(0, 0, 1, 1),
	float4(0, 1, 0, 1)
};

////////TextureSampler
texture Texture;

sampler2D retroTextureSampler = sampler_state
{
	Texture = (Texture);
	MagFilter = Point;
	MinFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};
sampler2D linearTextureSampler = sampler_state
{
	Texture = (Texture);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Linear;
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
	float2 TextureCoordinate : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 Tangent : TEXCOORD2;
	float3 Binormal : TEXCOORD3;
};

///////////////////Process Diffuse-Color
float4 DiffuseColor(float4 normal)
{
	float4 tempColor;
	if (!DiffuseEnabled)
	{
		tempColor = (1, 1, 1, 1);
	}
	else
	{
		tempColor = (0, 0, 0, 0);
		for (int i = 0; i < DiffuseLightSources; i++)
		{
			if (DiffuseIntensities[i] != 0)
			{
				float lightIntensity = saturate(dot(normal, -DiffuseLightDirections[i]));

				tempColor += DiffuseColors[i] * DiffuseIntensities[i] * lightIntensity;
			}
		}
	}
	return saturate(tempColor);
}

////////////////////////////////////////////////NEW DIFFUSE-SHADER/////////////////////
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 inputPosition = float4(input.PositionNormal.xyz, 1.0);
	float4 inputNormal = normalArray[input.PositionNormal.w];
	float4 inputTangent = tangentArray[input.PositionNormal.w];
	float4 inputBinormal = binormalArray[input.PositionNormal.w];

	output.Position = mul(inputPosition, WVPMatrix);

	output.Normal = normalize(mul(inputNormal, WorldInverseTranspose));
	output.Tangent = normalize(mul(inputTangent, WorldInverseTranspose));
	output.Binormal = normalize(mul(inputBinormal, WorldInverseTranspose));

	output.TextureCoordinate = input.TextureCoordinate;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 normal;
	if (BumpMapsEnabled)
	{
		// Calculate the normal, including the information in the bump map
		float3 bump = BumpConstant * (tex2D(bumpSampler, input.TextureCoordinate) - (0.5, 0.5, 0.5));
		float3 bumpNormal = input.Normal + (bump.x * input.Tangent + bump.y * input.Binormal);
		bumpNormal = normalize(bumpNormal);
		normal = float4(bumpNormal.x, bumpNormal.y, bumpNormal.z, 1);
	}
	else
		normal = float4(input.Normal.x, input.Normal.y, input.Normal.z, 1);

	float4 textureColor = tex2D(linearTextureSampler, input.TextureCoordinate);
	float4 colorMod = DiffuseColor(normal);//Diffusecolor and -intensity

	textureColor.a = 1;
	textureColor.rgb *= colorMod + AmbientColor * AmbientIntensity;
	return saturate(textureColor);
}

/////////Techniques/////////////////////////////////////////////////
technique BlockShader
{
	pass Pass1
	{
		VertexShader = compile vs_5_0  VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}