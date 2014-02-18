uniform extern texture ScreenTexture;	

sampler ScreenS = sampler_state
{
	Texture = <ScreenTexture>;	
};

float2 Center;
float BlurAmount;

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 c = 0;    
    uv -= Center;

	for(int i=0; i<15; i++)
    {
        float scale = 1.0 + BlurAmount * (i / 14.0);
        c += tex2D(ScreenS, uv * scale + Center );
    }
   
    c /= 15;
    return c;
}

technique
{
	pass P0
	{
		PixelShader = compile ps_2_0 main();
	}
}
