Shader "CoopBeam/PixelSprite" {
 Properties {
  [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
  _Color ("Tint", Color) = (1,1,1,1)
  _Flash ("Hit flash", Range(0,1)) = 0
 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
  Blend SrcAlpha OneMinusSrcAlpha
  Cull Off
  ZWrite Off
  Pass {
   Tags { "LightMode"="SRPDefaultUnlit" }
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
   struct Attributes { float3 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct Varyings { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
   TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
   float4 _Color;float _Flash;
   CBUFFER_END
   Varyings Vert(Attributes v){UNITY_SETUP_INSTANCE_ID(v);SetUpSpriteInstanceProperties();Varyings o;float3 position=UnityFlipSprite(v.positionOS,unity_SpriteProps.xy);o.positionCS=TransformObjectToHClip(position);o.uv=v.uv;o.color=v.color*_Color*unity_SpriteColor;return o;}
   float4 Frag(Varyings i):SV_Target {float4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;c.rgb=lerp(c.rgb,float3(1,1,1),_Flash);return c;}
   ENDHLSL
  }
 }
}
