using UnityEngine;
using UnityEngine.InputSystem;
namespace CoopBeam {
// All offensive bonuses modify the cooperative segment, never an individual weapon.
public class LinkPowers:MonoBehaviour {
 public float Charge{get;private set;} public const float ChargeRequired=240;
 public bool Ultimate=>Time.time<ultimateUntil; public bool Empowered=>Ultimate||Time.time<prismUntil;
 public float DamageMultiplier=>Ultimate?2.4f:Time.time<prismUntil?1.5f:1;
 public bool ChainActive=>Time.time<chainUntil;public bool SolarActive=>Time.time<solarUntil;public int ChainHits{get;private set;}public int SolarHits{get;private set;}
 public string Status{get;private set;}="DANO DEL LAZO CARGA RESONANCIA";
 public int UltimatesUsed{get;private set;} public int PickupsCollected{get;private set;}
 GameManager game;float ultimateUntil,prismUntil,shieldUntil,chainUntil,solarUntil,chainAt,solarAt,moraReady=-10,limaReady=-10,fxAt,statusUntil;int kills,dropIndex;bool[] shields=new bool[2];
 void Awake(){game=GetComponent<GameManager>();}
 void Start(){var beam=FindAnyObjectByType<CoopBeam>();if(beam&&!beam.GetComponent<ResonanceVisual>())beam.gameObject.AddComponent<ResonanceVisual>();}
 public void AddCharge(float damage){if(game.Running&&!Ultimate&&damage>0)Charge=Mathf.Min(ChargeRequired,Charge+damage);}
 public void OnKill(Vector3 position){kills++;var cycle=new[]{LinkPickup.Kind.Prism,LinkPickup.Kind.Chain,LinkPickup.Kind.Shield,LinkPickup.Kind.Solar,LinkPickup.Kind.Heart};var p=LinkPickup.Create(position,cycle[dropIndex++%cycle.Length]);if(p)p.owner=this;}
 public bool Absorb(PlayerHealth player){if(Time.time>=shieldUntil)return false;for(int i=0;i<game.players.Length&&i<2;i++)if(game.players[i]==player&&shields[i]){shields[i]=false;BeamFX.Burst(player.transform.position,Color.cyan,14);return true;}return false;}
 public bool HasShield(int i)=>Time.time<shieldUntil&&shields[i];
 public void Collect(LinkPickup.Kind kind){if(!game.Running)return;PickupsCollected++;if(kind==LinkPickup.Kind.Prism){prismUntil=Time.time+8;Message("PRISMA / DANO DEL LAZO X1.5");}else if(kind==LinkPickup.Kind.Shield){shieldUntil=Time.time+15;shields[0]=shields[1]=true;Message("BURBUJA / PROTECCION PARA AMBOS");}else if(kind==LinkPickup.Kind.Chain){chainUntil=Time.time+10;Message("CHISPA / EL LAZO ENCADENA ENEMIGOS");}else if(kind==LinkPickup.Kind.Solar){solarUntil=Time.time+10;solarAt=Time.time+.4f;Message("ANILLO / PULSOS ALREDEDOR DEL LAZO");}else{foreach(var h in game.players)if(!h.Dead)h.Heal();AddCharge(30);Message("CORAZON / +1 VIDA Y RESONANCIA");}game.Tone(1080,.18f);}
 bool Linked=>game&&game.Running&&!game.players[0].Dead&&!game.players[1].Dead&&((Vector2)(game.players[0].transform.position-game.players[1].transform.position)).sqrMagnitude>.0025f;
 public static Vector2 TargetPoint(EnemyBase enemy){var fire=enemy.GetComponent<FireForgeBoss>();if(fire)return fire.core.transform.position;var ice=enemy.GetComponent<IceKingBoss>();if(ice)return ice.core.transform.position;var candy=enemy.GetComponent<CandyBoss>();return candy?(Vector2)candy.weakSpot.transform.position:(Vector2)enemy.transform.position;}
 public void OnBeamHit(EnemyBase source,float acceptedDamage){if(!ChainActive||!Linked||acceptedDamage<=0||Time.time<chainAt)return;chainAt=Time.time+.8f;Vector2 point=TargetPoint(source);int count=0;foreach(var enemy in game.enemies.ToArray()){if(!enemy||enemy==source||enemy.Dead||Vector2.Distance(point,TargetPoint(enemy))>3.5f)continue;float hp=enemy.Health;enemy.Damage(10);LinkArcFX.Spawn(point,TargetPoint(enemy),new Color(1,.7f,.12f));if(enemy.Health<hp)ChainHits++;if(++count>=2)break;}}
 void SolarPulse(){if(!SolarActive||!Linked||Time.time<solarAt)return;solarAt=Time.time+1.4f;Vector2 a=game.players[0].transform.position,b=game.players[1].transform.position;LinkArcFX.Pulse(a,b);foreach(var enemy in game.enemies.ToArray()){if(!enemy||enemy.Dead||Distance(TargetPoint(enemy),a,b)>.95f)continue;float hp=enemy.Health;enemy.Damage(18);if(enemy.Health<hp)SolarHits++;}}
 public void Message(string text){Status=text;statusUntil=Time.time+4;}
 public bool Confirm(int player){if(!game.Running||Charge<ChargeRequired||game.players[0].Dead||game.players[1].Dead)return false;if(player==0)moraReady=Time.time;else limaReady=Time.time;
  if(Time.time-moraReady>2||Time.time-limaReady>2)return false;Charge=0;ultimateUntil=Time.time+4.5f;UltimatesUsed++;moraReady=limaReady=-10;foreach(var h in game.players){h.Protect(.75f);BeamFX.Burst(h.transform.position,Color.cyan,24);}Message("RESONANCIA! CRUCEN EL LAZO");game.Tone(1320,.3f);return true;}
 void Update(){if(!game.Running){moraReady=limaReady=-10;return;}var k=Keyboard.current;if(k!=null){if(k.qKey.wasPressedThisFrame)Confirm(0);if(k.rightShiftKey.wasPressedThisFrame)Confirm(1);}if(Time.time>statusUntil)Status=Ultimate?"RESONANCIA ACTIVA":Charge>=ChargeRequired?(Time.time-moraReady<2?"MORA LISTA / LIMA: SHIFT DER":Time.time-limaReady<2?"LIMA LISTA / MORA: Q":"ULTI LISTA / Q + SHIFT DER / 2 S"):"DANO DEL LAZO CARGA RESONANCIA";
  SolarPulse();if(!Ultimate||!Linked||Time.time<fxAt)return;fxAt=Time.time+.07f;Vector3 a=game.players[0].transform.position,b=game.players[1].transform.position;BeamFX.Burst(Vector3.Lerp(a,b,Random.value),Random.value>.5f?Color.cyan:new Color(.8f,.5f,1),3);
  foreach(var p in FindObjectsByType<CandyProjectile>())if(Distance(p.transform.position,a,b)<.45f){BeamFX.Burst(p.transform.position,Color.white,3);Destroy(p.gameObject);}foreach(var p in FindObjectsByType<EncounterHazard>())if(p.clearable&&p.Active&&Distance(p.transform.position,a,b)<.45f)p.Clear();
 }
 public static float Distance(Vector2 point,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(point,a+d*Mathf.Clamp01(Vector2.Dot(point-a,d)/Mathf.Max(.0001f,d.sqrMagnitude)));}
 void OnGUI(){if(!game||!game.Running)return;GUI.depth=-1;PixelFont.Begin();PixelFont.Panel(new Rect(8,40,238,33),PixelFont.Ink,Empowered?PixelFont.Mint:PixelFont.Pink);PixelFont.Fill(new Rect(16,47,220,5),new Color(.22f,.19f,.35f));PixelFont.Fill(new Rect(16,47,220*Charge/ChargeRequired,5),Charge>=ChargeRequired?Color.white:PixelFont.Mint);PixelFont.Text(16,59,"RESONANCIA "+Mathf.RoundToInt(100*Charge/ChargeRequired)+" / Q + SHIFT DER",1,PixelFont.Mint);PixelFont.Panel(new Rect(394,40,238,33),PixelFont.Ink,PixelFont.Mint);string message=Status.Length>37?Status.Substring(0,37):Status;PixelFont.Text(402,47,message,1,PixelFont.Cream);float x=402;EffectIcon(ref x,LinkPickup.Kind.Prism,prismUntil);EffectIcon(ref x,LinkPickup.Kind.Chain,chainUntil);EffectIcon(ref x,LinkPickup.Kind.Solar,solarUntil);if(HasShield(0)||HasShield(1))EffectIcon(ref x,LinkPickup.Kind.Shield,shieldUntil);}
 void EffectIcon(ref float x,LinkPickup.Kind kind,float until){if(Time.time>=until)return;var icon=ExpansionSprites.Icon((int)kind);GUI.color=Color.white;GUI.DrawTexture(new Rect(x,58,12,12),icon.texture);PixelFont.Text(x+15,61,Mathf.CeilToInt(until-Time.time)+"S",1,LinkPickup.ColorFor(kind));x+=55;}
}
}
