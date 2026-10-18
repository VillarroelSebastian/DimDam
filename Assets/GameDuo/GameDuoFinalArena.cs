using UnityEngine;
using UnityEngine.SceneManagement;
namespace CoopBeam {
public sealed class GameDuoFinalArena : MonoBehaviour {
 SpriteRenderer backdrop;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 static void Install(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Initial(){Loaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
 static void Loaded(Scene scene,LoadSceneMode mode){
  if(scene.path!="Assets/GameDuo/Scenes/05_JefeChicle.unity")return;
  var game=FindAnyObjectByType<GameManager>();if(game&&!game.GetComponent<GameDuoFinalArena>())game.gameObject.AddComponent<GameDuoFinalArena>();
 }
 void Start(){
  foreach(var renderer in FindObjectsByType<SpriteRenderer>())if(renderer.name=="Salon del trono / final.png")backdrop=renderer;
  Fit();
 }
 void LateUpdate(){Fit();}
 void Fit(){if(!backdrop||!backdrop.sprite||!Camera.main)return;var cam=Camera.main;float h=cam.orthographicSize*2;float scale=Mathf.Max(h/backdrop.sprite.bounds.size.y,h*cam.aspect/backdrop.sprite.bounds.size.x);backdrop.transform.localScale=new Vector3(scale,scale,1);backdrop.transform.position=new Vector3(cam.transform.position.x,cam.transform.position.y,0);}
}
}
