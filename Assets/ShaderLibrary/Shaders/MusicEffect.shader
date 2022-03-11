Shader "Custom/Unlit/MusicEffect"
{
	Properties
	{
		_Color("Color",Color) = (1,1,1,1)
		_MusicData("MusicData(Alpha)",2D) = "black"{}  //音乐数据输入
		_Divides("Divides",int) = 64                   //将圆弧分成多少段
		_Antialias("_Antialias",Range(0,1)) = 0.1      //背景平滑度
		_Radius("Radius",Range(0,1)) = 0.2             //圆半径
		_RayOffset("RayOffset",Range(0,1)) = 0.1       //变化参数
		_Brightness("Brightness",Range(0,2)) = 1       //亮度
	}

	SubShader
	{
		Tags 
		{
			"RenderType"="Transparency"
			"IgnoreProjection" = "True"
			"RenderQueue" = "Transparency"
		}

		Blend SrcAlpha One
		ZWrite Off
		
		Pass
		{
			CGPROGRAM
            #pragma vertex   vert   //顶点着色器
            #pragma fragment frag   //片段着色器
            #include "UnityCG.cginc"

			struct appdata
		    {
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
		    };

		    struct v2f
			{
				float2 uv     : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D  _MusicData;
			float4     _MusicData_ST;
			int        _Divides;
			fixed4     _Color;
			fixed      _Antialias;
			fixed      _OutAntialias;
			fixed      _Radius;
			fixed      _RayOffset;
			fixed      _Brightness;

			float2x2 rotate2d(float angle)
			{
				return float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
			}

			fixed2 effectShape(float2 uv, fixed2 center, float radius, float antialias, float offset, fixed arc, int divides)
			{
				float2 radiusVector = center - uv;
				float len = length(radiusVector);

				float radiusIn = radius - offset;
				float radiusOut = radius + offset + 0.005;

				fixed arcIn = step(len, radiusOut) - step(len, radiusIn);
				float Arc = step(distance(0.5, frac(arc * divides)), 0.46);
				arcIn *= Arc;

				fixed arcOut= step(len, radiusOut - 0.004) - step(len, radiusIn - 0.004);
				fixed arcgradient = smoothstep(radiusOut + antialias + 0.05, radiusOut, len) - smoothstep(radiusIn - 0.05 + antialias, radiusIn - 0.05, len);
				fixed arc_col = abs(arcOut - arcIn);
				arc_col = clamp(arc_col, 0.1, 1);
				return fixed2(arc_col, arcgradient);
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MusicData);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//将坐标转换为圆圈的弧度坐标，然后用这和坐标去采样音乐图，获得音乐数据去改变圆的半径等
                fixed arcU = atan2(i.uv.x * 2 - 1,i.uv.y * 2 - 1) / UNITY_TWO_PI;
                //将弧度范围从-1~1转换到0~1
                arcU = arcU * 0.5 + 0.5;
                //取样音乐图的数据，将UV分为格子，取格子起始点的值，因为我们的音乐流数据很怪，所以前面加了1-，正常的话去掉
                fixed rayoffset = tex2D(_MusicData, float2(floor((arcU)*_Divides) /  _Divides, 0)).r;
                //圆圈中心点设在坐标中心
                fixed2 center = fixed2(0.5, 0.5);
                //平滑范围最大不超过0.125
                fixed innantialias = _Antialias * 0.125;
                //以时间为旋转速度因子变色的函数
                fixed2 v = mul(rotate2d(_Time.y), i.uv);
                //绘制
                fixed2 shape = effectShape(i.uv,center,_Radius, innantialias,  rayoffset* _RayOffset,arcU,_Divides);
                fixed4 final;
                //分拆很熟的输出，分别作为背景和线条叠加起来
                final.rgb = shape.y * fixed3(v.x, v.y, 0.7 - v.y * v.x) + shape.x;
                final.rgb *= _Color * _Brightness;
                final.a = shape.x * 0.5 + shape.y * 0.2;
                return final;
			}
			ENDCG
        }
	}
}
