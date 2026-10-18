Shader "GameDuo/SootDissolve" {
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _Hit("Damage",Float)=0 _Dissolve("Dissolve",Float)=0 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float _Hit,_Dissolve;
 V vert(A v){V o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.color=v.color;return o;}
 half4 frag(V i):SV_Target{
  half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
  clip(c.a-.045);
  float2 cell=floor(i.uv*155);float noise=frac(sin(dot(cell,float2(127.1,311.7)))*43758.5453);
  clip(noise-_Dissolve*1.05);
  c.rgb=lerp(c.rgb,float3(1,.065,.10)*(.6+dot(c.rgb,float3(.2,.6,.2))*.4),saturate(_Hit)*.88);
  float rim=_Dissolve>0?1-smoothstep(0,.09,noise-_Dissolve*1.05):0;
  c.rgb=lerp(c.rgb,float3(.8,.27,.19),rim*.5);return c;
 }
 ENDHLSL }
 }
}
