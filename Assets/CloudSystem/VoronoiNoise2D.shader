Shader  "Custom/Voronoi2D"
{
	Properties
	{
		_TimeStep("Time Step", float) = 1.0
		_ScaleFactor("Scale Factor", float) = 1.0
		//_DotSize("Dot Size", float) = 1.0
		_Minkowski("Minkowski factor", Range(1,2)) = 1.5
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			half _TimeStep;
			half _ScaleFactor;
			//half _DotSize;
			half _Minkowski;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			float2 random2(float2 p)
			{
				return frac(sin(float2(dot(p,float2(117.12,341.7)),dot(p,float2(269.5,123.3)))) * 43458.5453);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = fixed4(0,0,0,1);
				float2 uv = i.uv;
				uv *= _ScaleFactor;		//Scaling amount (larger number more cells can be seen)
				float2 iuv = floor(uv); //gets integer values no floating point
				float2 fuv = frac(uv);	// gets only the fractional part
				float minDist = 1.0;	// minimun distance
				for (int y = -1; y <= 1; y++)
				{
					for (int x = -1; x <= 1; x++)
					{
						// Position of neighbour on the grid
						float2 neighbour = float2(float(x), float(y));
						// Random position from current + neighbour place in the grid
						float2 pointv = random2(iuv + neighbour);
						// Move the point with time
						//pointv = 0.1 + 0.6 * sin((_Time.z * _TimeStep)+ 6.2236 * pointv);	//each point moves in a certain way
						
						float2 diff = neighbour + pointv - fuv; // Vector between the pixel and the point										
						
                        
						// Distance to the point
                        /*
						float dist = 0;
						if (_DistCalcMethod == 1) {
							
							dist = length(diff);
						}
						else if (_DistCalcMethod == 2) {
							dist = distance_manhattan(diff);
						}
						else if (_DistCalcMethod == 3) {
							dist = distance_minkowski(diff);
						}
                        */

                        float dist = length(diff);
						// Keep the closer distance
						minDist = min(minDist, dist);
					}
				}
				// Draw the min distance (distance field)
				col += minDist;
				//col += minDist * minDist; // squared it to to make edges look sharper
				
				// Increase the dot Size
				//col = pow(col, _DotSize);

				return col;
			}
		ENDCG
		}
	}
}