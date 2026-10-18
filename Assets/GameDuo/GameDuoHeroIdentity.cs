using UnityEngine;
namespace CoopBeam {
// One proportion contract for combat, exploration and every hero animation.
[DefaultExecutionOrder(1100)]
public sealed class GameDuoHeroIdentity:MonoBehaviour {
 public const float LargeScale=1.35f, SmallScale=.8f;
 public const float LargeHeight=1.755f, SmallHeight=1.04f;
 PlayerController2D player; CapsuleCollider2D shape; float referenceHeight;
 public static float Scale(int player)=>player==0?LargeScale:SmallScale;
 public static float Height(int player)=>player==0?LargeHeight:SmallHeight;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Install(){foreach(var player in FindObjectsByType<PlayerController2D>())if(!player.GetComponent<GameDuoHeroIdentity>())player.gameObject.AddComponent<GameDuoHeroIdentity>();}
 void Start(){
  player=GetComponent<PlayerController2D>();shape=GetComponent<CapsuleCollider2D>();
  float oldFoot=shape.bounds.min.y;
  transform.localScale=Vector3.one*Scale(player.playerIndex);
  Physics2D.SyncTransforms();
  float correction=oldFoot-shape.bounds.min.y;
  transform.position+=Vector3.up*correction;
  if(player.health)player.health.spawn+=Vector2.up*correction;
  var animator=player.sprite.GetComponent<PixelSpriteAnimator>();
  var rest=animator&&animator.animationSet&&animator.animationSet.frames.Length>0?animator.animationSet.frames[0]:player.sprite.sprite;
  referenceHeight=rest?rest.bounds.size.y:1.3f;
  gameObject.name=player.playerIndex==0?"Mimo / principe mayor":"Pipro / principe menor";
 }
 void LateUpdate(){
  if(!player||!player.visual||!player.sprite.sprite)return;
  // The controller resets the pose every frame; apply a constant size factor afterward.
  float factor=Height(player.playerIndex)/(Scale(player.playerIndex)*referenceHeight);
  var pose=player.visual.localScale;pose.x*=factor;pose.y*=factor;player.visual.localScale=pose;
  var position=player.visual.localPosition;
  var bounds=player.sprite.sprite.bounds;
  position.y=shape.offset.y-shape.size.y*.5f+(bounds.extents.y-bounds.center.y)*pose.y;
  player.visual.localPosition=position;
 }
}
}
