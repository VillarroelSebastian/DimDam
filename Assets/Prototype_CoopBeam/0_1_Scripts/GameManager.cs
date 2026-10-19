using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace CoopBeam {
public class GameManager:MonoBehaviour {
 public static GameManager Instance{get;private set;} public PlayerHealth[] players;public EnemyBase[] enemyPrefabs;public Sprite particle;
 public CandyLevelDefinition level;
 public bool explorationOnly;public readonly CoopPromptUI.StartGate startGate=new CoopPromptUI.StartGate();
 public bool Practice{get;private set;} public bool Running{get;private set;} public bool Started{get;private set;} public bool Victory{get;private set;}
 public int Wave{get;private set;}public int Score{get;private set;}public int Combo{get;private set;}public int maxCombo=12;public float comboWindow=3;
 public string Announcement{get;private set;}="";public readonly List<EnemyBase> enemies=new List<EnemyBase>();float lastKill=-100;AudioSource audioSource;AudioClip tone;
 void Awake(){Instance=this;BeamFX.particle=particle;audioSource=gameObject.AddComponent<AudioSource>();audioSource.volume=.12f;if(explorationOnly)return;if(!GetComponent<LinkPowers>())gameObject.AddComponent<LinkPowers>();if(!GetComponent<CoopPresentation>())gameObject.AddComponent<CoopPresentation>();if(level)CandyCampaign.SelectRealm(level.Realm);}
 void OnEnable(){Instance=this;BeamFX.particle=particle;if(!audioSource)audioSource=GetComponent<AudioSource>();}
 void Start(){Physics2D.IgnoreCollision(players[0].GetComponent<Collider2D>(),players[1].GetComponent<Collider2D>(),true);}
 void Update(){if(GameDuoCinema.Busy)return;if(explorationOnly)return;var k=Keyboard.current;if(k!=null){if(!Started&&k.pKey.wasPressedThisFrame)BeginPractice();else if(!Started&&startGate.Poll())Begin();else if(Started&&k.rKey.wasPressedThisFrame)Restart();if(level&&!Running&&k.mKey.wasPressedThisFrame)CandyCampaign.Map();if(level&&Victory&&level.number<5&&k.nKey.wasPressedThisFrame)CandyCampaign.Load(level.number+1);}if(Time.time-lastKill>comboWindow)Combo=0;}
 public void BeginExploration(){if(!explorationOnly)return;Started=true;Running=true;}
 public void Begin(){if(Started||GetComponent<CandyLevelExit>()&&GetComponent<CandyLevelExit>().Arriving)return;Started=true;Running=true;if(!GetComponent<CoopInteractionDirector>())gameObject.AddComponent<CoopInteractionDirector>();StartCoroutine(Waves());}
 public void BeginPractice(){if(Started)return;Started=true;Running=true;Practice=true;if(!GetComponent<CoopInteractionDirector>())gameObject.AddComponent<CoopInteractionDirector>();GetComponent<LinkPowers>().AddCharge(LinkPowers.ChargeRequired);}
 public void Restart(){if(!Practice)LinkPowers.ResetStoredCharge();SceneManager.LoadScene(SceneManager.GetActiveScene().path);}
 IEnumerator Waves(){var cooperation=GetComponent<CoopInteractionDirector>();yield return cooperation.Opening();for(int w=1;w<=3;w++){Wave=w;Announcement="OLEADA "+w;yield return new WaitForSeconds(1.8f);if(!Running)yield break;
 int[] kinds=level?level.waves[w-1].enemyTypes:w==1?new[]{0,0,0}:w==2?new[]{0,0,1,1}:new[]{2,2,3,0,0};
 int first=InitialBatchSize(kinds.Length),second=first+(kinds.Length-first)/2;
 int emitted=0;
 foreach(int target in new[]{first,second,kinds.Length}){
  if(target<=emitted)continue;
  if(emitted>0){Announcement="REFUERZOS EN CAMINO";yield return new WaitForSeconds(3.5f);}
  if(!Running)yield break;
  for(;emitted<target;emitted++){
   Vector2 preferred=level&&level.waves[w-1].spawnPositions.Length>emitted?level.waves[w-1].spawnPositions[emitted]:new Vector2((emitted-(kinds.Length-1)*.5f)*3,emitted%2==0?-.5f:2.4f);
   StartCoroutine(SpawnAnnounced(kinds[emitted],preferred));
  }
  Announcement="";while(PendingSpawns>0&&Running)yield return null;
 }

 Announcement="";while(enemies.Count>0&&Running)yield return null;if(!Running)yield break;Announcement="¡BUEN ENLACE!";yield return new WaitForSeconds(2);}
 Victory=true;Running=false;if(level)CandyCampaign.Complete(level.number);Announcement="¡VICTORIA!";Tone(880,.35f);}
 public const float SpawnWarning=.9f,SpawnPlayerGap=3f;
 public int PendingSpawns{get;private set;}
 readonly List<Vector2> spawnReservations=new List<Vector2>();
 public static int InitialBatchSize(int total)=>total<=1?total:total==6?4:Mathf.CeilToInt(total*.5f);
 static void SpawnGeometry(EnemyBase prefab,out Vector2 half,out Vector2 offset){
  var shape=prefab.GetComponent<Collider2D>();Vector2 scale=prefab.transform.lossyScale;scale=new Vector2(Mathf.Abs(scale.x),Mathf.Abs(scale.y));
  Vector2 size=shape is BoxCollider2D box?box.size:shape is CapsuleCollider2D capsule?capsule.size:shape is CircleCollider2D circle?Vector2.one*circle.radius*2:Vector2.one;
  half=Vector2.Scale(size,scale)*.5f;offset=shape?Vector2.Scale(shape.offset,scale):Vector2.zero;
 }
 public bool IsSpawnSafe(int kind,Vector2 point){
  SpawnGeometry(enemyPrefabs[kind],out var half,out var offset);Vector2 center=point+offset;
  foreach(var player in players)if(player&&!player.Dead){var shape=player.GetComponent<Collider2D>();Vector2 nearest=shape?shape.bounds.ClosestPoint(center):(Vector3)player.transform.position;if(Vector2.Distance(center,nearest)<SpawnPlayerGap+half.magnitude)return false;}
  foreach(var other in enemies)if(other&&!other.Dead){var shape=other.GetComponent<Collider2D>();if(shape&&shape.enabled&&shape.bounds.Intersects(new Bounds(center,(half+Vector2.one*.2f)*2)))return false;}
  return !Physics2D.OverlapBox(center,half*2,0,(1<<8)|(1<<9));
 }
 public bool TrySpawnPosition(int kind,Vector2 preferred,out Vector2 point){
  var prefab=enemyPrefabs[kind];SpawnGeometry(prefab,out var half,out var offset);
  var ice=prefab.GetComponent<IceEnemyBrain>();var fire=prefab.GetComponent<FireEnemyBrain>();
  bool flying=prefab.kind==EnemyKind.Flyer||ice&&ice.role==IceEnemyBrain.Role.Owl||fire&&fire.role==FireEnemyBrain.Role.Phoenix;
  point=preferred;float best=float.PositiveInfinity;bool found=false;
  for(int i=0;i<22;i++)for(int height=0;height<(flying?3:1);height++){
   float x=i==0?Mathf.Clamp(preferred.x,-9.5f,9.5f):-9.5f+(i-1)*.95f;
   Vector2 candidate=new Vector2(x,flying?Mathf.Clamp(preferred.y+height*1.5f,-1,4.5f):preferred.y);
   if(!flying){bool supported=false;foreach(var hit in Physics2D.RaycastAll(new Vector2(x,5.5f),Vector2.down,14,(1<<8)|(1<<9))){if(!hit.collider||hit.collider.isTrigger||hit.normal.y<.7f||hit.collider.bounds.size.x<half.x*2+.2f||x+offset.x-half.x<hit.collider.bounds.min.x+.05f||x+offset.x+half.x>hit.collider.bounds.max.x-.05f)continue;
     var rest=new Vector2(x,hit.point.y+half.y-offset.y+.08f);if(!IsSpawnSafe(kind,rest))continue;candidate=rest;supported=true;break;}if(!supported)continue;}
   if(!IsSpawnSafe(kind,candidate))continue;
   bool reserved=false;foreach(var reservation in spawnReservations)if(Vector2.Distance(candidate,reservation)<half.magnitude*2+.5f){reserved=true;break;}if(reserved)continue;
   float score=(candidate-preferred).sqrMagnitude;if(score>=best)continue;best=score;point=candidate;found=true;
  }
  return found;
 }
 public IEnumerator SpawnAnnounced(int kind,Vector2 preferred,MonoBehaviour owner=null,System.Action<EnemyBase> configure=null){
  PendingSpawns++;GameObject marker=null;Vector2 reserved=Vector2.zero;bool hasReservation=false;bool owned=owner;
  try{
   while(Running){
    if(owned&&(!owner||!owner.isActiveAndEnabled||owner.GetComponent<EnemyBase>()&&owner.GetComponent<EnemyBase>().Health<=0))yield break;
    if(!TrySpawnPosition(kind,preferred,out var point)){yield return new WaitForSeconds(.25f);continue;}
    reserved=point;spawnReservations.Add(point);hasReservation=true;
    marker=new GameObject("Aviso de aparicion / sin dano");marker.transform.position=point;var art=marker.AddComponent<SpriteRenderer>();art.sprite=ExpansionSprites.Square;art.sortingOrder=15;marker.transform.localScale=new Vector3(1.4f,.13f,1);
    float age=0;
    while(age<SpawnWarning&&Running&&IsSpawnSafe(kind,point)){
     if(owned&&(!owner||!owner.isActiveAndEnabled||owner.GetComponent<EnemyBase>()&&owner.GetComponent<EnemyBase>().Health<=0))yield break;
     age+=Time.deltaTime;art.color=new Color(1,.75f,.2f,.45f+Mathf.PingPong(age*3,.45f));yield return null;
    }
    Destroy(marker);marker=null;spawnReservations.Remove(reserved);hasReservation=false;
    if(!Running)yield break;
    if(age<SpawnWarning||!IsSpawnSafe(kind,point))continue;
    var enemy=Instantiate(enemyPrefabs[kind],point,Quaternion.identity);configure?.Invoke(enemy);
    foreach(var other in enemies)if(other&&other.GetComponent<Collider2D>().enabled)Physics2D.IgnoreCollision(enemy.GetComponent<Collider2D>(),other.GetComponent<Collider2D>(),true);
    enemies.Add(enemy);BeamFX.Burst(point,new Color(.8f,.5f,1),14);yield break;
   }
  }finally{if(marker)Destroy(marker);if(hasReservation)spawnReservations.Remove(reserved);PendingSpawns--;}
 }
 public void EnemyKilled(EnemyBase enemy){if(!enemies.Remove(enemy))return;var powers=GetComponent<LinkPowers>();if(powers)powers.OnKill(enemy.transform.position);Combo=Time.time-lastKill<=comboWindow?Mathf.Min(maxCombo,Combo+1):1;lastKill=Time.time;Score+=100*Combo;Tone(440+Combo*70,.08f);}
 public void CheckDefeat(){if(players[0].Dead&&players[1].Dead){Running=false;Announcement="FIN DE LA PARTIDA";}}
 public void Tone(float frequency,float duration){if(!audioSource)return;int count=(int)(22050*duration);var samples=new float[count];for(int i=0;i<count;i++)samples[i]=Mathf.Sin(2*Mathf.PI*frequency*i/22050)*(.25f*(1-(float)i/count));var clip=AudioClip.Create("Arcade tone",count,1,22050,false);clip.SetData(samples,0);audioSource.PlayOneShot(clip);Destroy(clip,duration+.2f);}
 void OnDestroy(){if(Instance==this)Instance=null;BeamFX.particle=null;}
}}

