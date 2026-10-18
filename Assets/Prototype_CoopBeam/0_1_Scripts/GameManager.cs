using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace CoopBeam {
public class GameManager:MonoBehaviour {
 public static GameManager Instance{get;private set;} public PlayerHealth[] players;public EnemyBase[] enemyPrefabs;public Sprite particle;
 public CandyLevelDefinition level;
 public bool explorationOnly;
 public bool Practice{get;private set;} public bool Running{get;private set;} public bool Started{get;private set;} public bool Victory{get;private set;}
 public int Wave{get;private set;}public int Score{get;private set;}public int Combo{get;private set;}public int maxCombo=12;public float comboWindow=3;
 public string Announcement{get;private set;}="";public readonly List<EnemyBase> enemies=new List<EnemyBase>();float lastKill=-100;AudioSource audioSource;AudioClip tone;
 void Awake(){Instance=this;BeamFX.particle=particle;audioSource=gameObject.AddComponent<AudioSource>();audioSource.volume=.12f;if(explorationOnly)return;if(!GetComponent<LinkPowers>())gameObject.AddComponent<LinkPowers>();if(!GetComponent<CoopPresentation>())gameObject.AddComponent<CoopPresentation>();if(level)CandyCampaign.SelectRealm(level.Realm);}
 void OnEnable(){Instance=this;BeamFX.particle=particle;if(!audioSource)audioSource=GetComponent<AudioSource>();}
 void Start(){Physics2D.IgnoreCollision(players[0].GetComponent<Collider2D>(),players[1].GetComponent<Collider2D>(),true);}
 void Update(){if(GameDuoCinema.Busy)return;if(explorationOnly)return;var k=Keyboard.current;if(k!=null){if(!Started&&k.pKey.wasPressedThisFrame)BeginPractice();else if(!Started&&k.enterKey.wasPressedThisFrame)Begin();else if(Started&&k.rKey.wasPressedThisFrame)Restart();if(level&&!Running&&k.mKey.wasPressedThisFrame)CandyCampaign.Map();if(level&&Victory&&level.number<5&&k.nKey.wasPressedThisFrame)CandyCampaign.Load(level.number+1);}if(Time.time-lastKill>comboWindow)Combo=0;}
 public void BeginExploration(){if(!explorationOnly)return;Started=true;Running=true;}
 public void Begin(){if(Started||GetComponent<CandyLevelExit>()&&GetComponent<CandyLevelExit>().Arriving)return;Started=true;Running=true;if(!GetComponent<CoopInteractionDirector>())gameObject.AddComponent<CoopInteractionDirector>();StartCoroutine(Waves());}
 public void BeginPractice(){if(Started)return;Started=true;Running=true;Practice=true;if(!GetComponent<CoopInteractionDirector>())gameObject.AddComponent<CoopInteractionDirector>();GetComponent<LinkPowers>().AddCharge(LinkPowers.ChargeRequired);}
 public void Restart(){if(!Practice)LinkPowers.ResetStoredCharge();SceneManager.LoadScene(SceneManager.GetActiveScene().path);}
 IEnumerator Waves(){var cooperation=GetComponent<CoopInteractionDirector>();yield return cooperation.Opening();for(int w=1;w<=3;w++){Wave=w;Announcement="OLEADA "+w;yield return new WaitForSeconds(1.8f);if(!Running)yield break;
 int[] kinds=level?level.waves[w-1].enemyTypes:w==1?new[]{0,0,0}:w==2?new[]{0,0,1,1}:new[]{2,2,3,0,0};
 for(int i=0;i<kinds.Length;i++){Vector2 pos=level&&level.waves[w-1].spawnPositions.Length>i?level.waves[w-1].spawnPositions[i]:new Vector2((i-(kinds.Length-1)*.5f)*3, i%2==0?-.5f:2.4f);var e=Instantiate(enemyPrefabs[kinds[i]],pos,Quaternion.identity);foreach(var other in enemies)if(other&&other.GetComponent<Collider2D>().enabled)Physics2D.IgnoreCollision(e.GetComponent<Collider2D>(),other.GetComponent<Collider2D>(),true);enemies.Add(e);BeamFX.Burst(pos,new Color(.8f,.5f,1),10);}
 Announcement="";while(enemies.Count>0&&Running)yield return null;if(!Running)yield break;Announcement="¡BUEN ENLACE!";yield return new WaitForSeconds(2);}
 Victory=true;Running=false;if(level)CandyCampaign.Complete(level.number);Announcement="¡VICTORIA!";Tone(880,.35f);}
 public void EnemyKilled(EnemyBase enemy){if(!enemies.Remove(enemy))return;var powers=GetComponent<LinkPowers>();if(powers)powers.OnKill(enemy.transform.position);Combo=Time.time-lastKill<=comboWindow?Mathf.Min(maxCombo,Combo+1):1;lastKill=Time.time;Score+=100*Combo;Tone(440+Combo*70,.08f);}
 public void CheckDefeat(){if(players[0].Dead&&players[1].Dead){Running=false;Announcement="FIN DE LA PARTIDA";}}
 public void Tone(float frequency,float duration){if(!audioSource)return;int count=(int)(22050*duration);var samples=new float[count];for(int i=0;i<count;i++)samples[i]=Mathf.Sin(2*Mathf.PI*frequency*i/22050)*(.25f*(1-(float)i/count));var clip=AudioClip.Create("Arcade tone",count,1,22050,false);clip.SetData(samples,0);audioSource.PlayOneShot(clip);Destroy(clip,duration+.2f);}
 void OnDestroy(){if(Instance==this)Instance=null;BeamFX.particle=null;}
}}

