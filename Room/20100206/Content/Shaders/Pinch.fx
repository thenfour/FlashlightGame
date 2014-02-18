uniform extern texture ScreenTexture;	

sampler ScreenS = sampler_state
{
	Texture = <ScreenTexture>;	
};

float2 Center;
float Radius;
float Amount;

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float2 displace = Center - uv;
    float range = saturate(1 - (length(displace) / (abs(-sin(Radius * 8) * Radius) + 0.00000001F)));
    return tex2D(ScreenS, uv + displace * range * Amount);
}

technique
{
	pass P0
	{
		PixelShader = compile ps_2_0 main();
	}
}
