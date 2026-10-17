Shader "Hidden/CoopBeam/PixelPalette" {
 Properties { _MainTex("Frame",2D)="white"{} }
 SubShader { Cull Off ZWrite Off ZTest Always
 Pass {
 CGPROGRAM
 #pragma target 3.0
 #pragma vertex vert_img
 #pragma fragment Frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;
 static const float3 palette[32]={
 float3(.075,.04,.12),float3(.125,.078,.176),float3(.22,.125,.247),float3(.341,.192,.282),
 float3(.455,.259,.329),float3(.569,.318,.427),float3(.263,.235,.471),float3(.376,.345,.635),
 float3(.553,.471,.761),float3(.733,.627,.855),float3(.137,.31,.459),float3(.2,.498,.667),
 float3(.365,.71,.816),float3(.608,.894,.922),float3(.149,.353,.361),float3(.227,.549,.471),
 float3(.384,.788,.624),float3(.659,.902,.729),float3(.557,.149,.29),float3(.784,.247,.384),
 float3(.933,.396,.525),float3(.953,.616,.722),float3(1,.816,.8),float3(.38,.227,.196),
 float3(.596,.38,.243),float3(.784,.553,.322),float3(.898,.722,.416),float3(.973,.855,.596),
 float3(1,.941,.8),float3(1,.973,.902),float3(.76,.8,.85),float3(.47,.57,.67)};
 fixed4 Frag(v2f_img i):SV_Target {float3 original=tex2D(_MainTex,i.uv).rgb;float3 source=original;
 #ifndef UNITY_COLORSPACE_GAMMA
 source=LinearToGammaSpace(source);
 #endif
 float best=100;float3 result=source;[unroll]for(int p=0;p<32;p++){float3 d=source-palette[p];float distance=dot(d*d,float3(.9,1.15,1));if(distance<best){best=distance;result=palette[p];}}
 result=lerp(source,result,.28);
 #ifndef UNITY_COLORSPACE_GAMMA
 result=GammaToLinearSpace(result);
 #endif
 return float4(result,1);}
 ENDCG
 }
 }
}
