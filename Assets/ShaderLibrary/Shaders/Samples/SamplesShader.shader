Shader "Custom/SamplesShader"
{
    Properties
    {
        _Int("Int Type",int) = 1
        _Float("Float Type",float) = 1.0
        _Range("Range Type(实际上是Float)",Range(1,10)) = 5
        _Color("Color Type",Color) = (1,1,1,1)
        _Vector("Vector Type",Vector) = (1,1,1,1)
        _2D("2D Type",2D) = "white"{}
        _Cube("Cube Type",Cube) = ""{}
        _3D("3D Type",3D) = "black"{}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Geometry"
            "RenderType"="Opaque"
            "IgnoreProjector"="True"
            "DisableBatching"="True"
            "ForeNoShadowCasting"="True"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            CGPROGRAM
            int _Int;
            float _Float;
            float _Range;
            float4 _Color;
            float4 _Vector;
            sampler2D _2D;
            samplerCUBE _Cube;
            sampler3D _3D;

            #pragma vertex vert 
            #pragma fragment frag

            struct a2v
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float color : COLOR;
            };

            v2f vert(a2v v)
            {
                v2f o;
                o.color = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                return i.color;
            }

            
            ENDCG
        }
    }
    FallBack "Diffuse"
}
