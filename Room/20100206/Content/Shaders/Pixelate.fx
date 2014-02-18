uniform extern texture ScreenTexture;	

sampler ScreenS = sampler_state
{
	Texture = <ScreenTexture>;	
};


float HorizontalPixelCounts;
float VerticalPixelCounts;

sampler2D implicitInputSampler : register(S0);


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
   float2 brickCounts = { HorizontalPixelCounts, VerticalPixelCounts };
   float2 brickSize = 1.0 / brickCounts;

   // Offset every other row of bricks
   float2 offsetuv = uv;
   bool oddRow = floor(offsetuv.y / brickSize.y) % 2.0 >= 1.0;
   if (oddRow)
   {
       offsetuv.x += brickSize.x / 2.0;
   }
   
   float2 brickNum = floor(offsetuv / brickSize);
   float2 centerOfBrick = brickNum * brickSize + brickSize / 2;
   float4 color = tex2D(implicitInputSampler, centerOfBrick);

   return color;
}



technique
{
	pass P0
	{
		PixelShader = compile ps_2_0 main();
	}
}
