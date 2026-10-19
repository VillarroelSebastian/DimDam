using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoopBeam {
[DefaultExecutionOrder(1800)]
public sealed class GameDuoAudio : MonoBehaviour {
    public static GameDuoAudio Instance { get; private set; }

    public static readonly string[] CampaignScenes = {
        "Assets/GameDuo/Scenes/01_Tutorial_Chicle.unity",
        "Assets/GameDuo/Scenes/02_ReinoNeon.unity",
        "Assets/GameDuo/Scenes/03_ReyHielo.unity",
        "Assets/GameDuo/Scenes/04_RescatePinguinos.unity",
        "Assets/GameDuo/Scenes/05_JefeChicle.unity"
    };

    // AudioSources
    private AudioSource musicA;
    private AudioSource musicB;
    private bool usingA = true;
    private AudioSource damageSource;
    private readonly AudioSource[] voices = new AudioSource[10];

    // Clips
    private AudioClip musicTutorial;
    private AudioClip musicNeon;
    private AudioClip musicWinter;
    private AudioClip musicPenguins;
    private AudioClip musicFinalBattle;

    private AudioClip clipJump;
    private AudioClip clipDamage;
    private AudioClip clipEnemyDeleted;
    private AudioClip clipPowerup;

    // State tracking
    private AudioClip targetMusicClip;
    private float musicFadeSpeed = 1.2f;
    private float targetMusicVolume = 0.38f;
    private float damageVolume = 0f;
    private CoopBeam cachedBeam;

    private float lastJumpTime = -1f;
    private float lastDeathTime = -1f;
    private float lastPowerupTime = -1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoInstall() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitialCheck() {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        EnsureInstance();
        if (Instance != null) {
            Instance.HandleScene(scene);
        }
    }

    private static void EnsureInstance() {
        if (Instance != null) return;
        var existing = FindAnyObjectByType<GameDuoAudio>();
        if (existing != null) {
            Instance = existing;
            return;
        }
        var go = new GameObject("GameDuoAudio / Gestor de Audio");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<GameDuoAudio>();
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeSources();
        LoadAllClips();
    }

    private void InitializeSources() {
        musicA = CreateSource("Music_A", loop: true);
        musicB = CreateSource("Music_B", loop: true);
        damageSource = CreateSource("Damage_Beam", loop: true);

        for (int i = 0; i < voices.Length; i++) {
            voices[i] = CreateSource("Voice_" + i, loop: false);
            voices[i].priority = 90;
        }
    }

    private AudioSource CreateSource(string name, bool loop) {
        var child = new GameObject(name);
        child.transform.SetParent(transform, false);
        var source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D Sound
        source.loop = loop;
        source.volume = 0f;
        source.dopplerLevel = 0f;
        return source;
    }

    private void LoadAllClips() {
        musicTutorial = LoadClip("backg-tutorial");
        musicNeon = LoadClip("backg-neon");
        musicWinter = LoadClip("backg-winter");
        musicPenguins = LoadClip("back-pinguinos");
        musicFinalBattle = LoadClip("final battle");

        clipJump = LoadClip("jump");
        clipDamage = LoadClip("makingDamage");
        clipEnemyDeleted = LoadClip("enemy-deleted");
        clipPowerup = LoadClip("dance ended");

        if (damageSource != null && clipDamage != null) {
            damageSource.clip = clipDamage;
        }
    }

    private static AudioClip LoadClip(string name) {
        string[] paths = {
            "FinalAudio/my ones/" + name,
            "FinalAudio/" + name,
            "my ones/" + name,
            name
        };
        foreach (var path in paths) {
            var clip = Resources.Load<AudioClip>(path);
            if (clip != null) {
                clip.LoadAudioData();
                return clip;
            }
        }
        Debug.LogWarning("[GameDuoAudio] No se encontro el archivo de audio para: " + name);
        return null;
    }

    private void HandleScene(Scene scene) {
        int stage = -1;
        for (int i = 0; i < CampaignScenes.Length; i++) {
            if (scene.path == CampaignScenes[i] || scene.name == System.IO.Path.GetFileNameWithoutExtension(CampaignScenes[i])) {
                stage = i;
                break;
            }
        }

        AudioClip nextMusic = null;
        switch (stage) {
            case 0: nextMusic = musicTutorial; break;
            case 1: nextMusic = musicNeon; break;
            case 2: nextMusic = musicWinter; break;
            case 3: nextMusic = musicPenguins; break;
            case 4: nextMusic = musicFinalBattle; break;
        }

        cachedBeam = null;

        if (nextMusic != null) {
            SwitchMusic(nextMusic);
        } else if (scene.name.Contains("Menu") || scene.path.Contains("00_Menu")) {
            // El menu tiene su propia pista en GameDuoMainMenu
            targetMusicClip = null;
        }
    }

    private void SwitchMusic(AudioClip newClip) {
        if (targetMusicClip == newClip && (musicA.isPlaying || musicB.isPlaying)) {
            return;
        }
        targetMusicClip = newClip;

        // Alternar fuentes para crossfade
        var active = usingA ? musicA : musicB;
        var next = usingA ? musicB : musicA;
        usingA = !usingA;

        next.clip = newClip;
        next.volume = 0f;
        if (newClip != null) {
            next.Play();
        }
    }

    private void Update() {
        UpdateMusicVolume();
        UpdateBeamDamageAudio();
    }

    private void UpdateMusicVolume() {
        bool busy = GameDuoCinema.Busy || AudioListener.pause;
        float desiredMax = busy ? 0f : targetMusicVolume;

        var current = usingA ? musicA : musicB;
        var previous = usingA ? musicB : musicA;

        // La pista activa sube al volumen objetivo
        float goal = (targetMusicClip != null && current.clip == targetMusicClip) ? desiredMax : 0f;
        current.volume = Mathf.MoveTowards(current.volume, goal, Time.unscaledDeltaTime * musicFadeSpeed);

        // La pista anterior baja a 0 y se detiene
        previous.volume = Mathf.MoveTowards(previous.volume, 0f, Time.unscaledDeltaTime * musicFadeSpeed * 1.5f);
        if (previous.volume <= 0.001f && previous.isPlaying) {
            previous.Stop();
        }
    }

    private void UpdateBeamDamageAudio() {
        if (!cachedBeam) {
            cachedBeam = FindAnyObjectByType<CoopBeam>();
        }

        bool busy = GameDuoCinema.Busy || AudioListener.pause || Time.timeScale <= 0f;
        bool isDamaging = !busy && cachedBeam != null && cachedBeam.IsDamaging;

        float targetVol = isDamaging ? 0.35f : 0f;
        damageVolume = Mathf.MoveTowards(damageVolume, targetVol, Time.unscaledDeltaTime * (isDamaging ? 3.5f : 4f));

        if (damageSource != null) {
            damageSource.volume = damageVolume;
            if (damageVolume > 0.005f) {
                if (!damageSource.isPlaying && clipDamage != null) {
                    damageSource.Play();
                }
            } else if (damageSource.isPlaying) {
                damageSource.Pause();
            }
        }
    }

    // --- API Publica de Efectos de Sonido ---

    public static void PlayJump() {
        if (!Instance) EnsureInstance();
        if (Instance) Instance.TriggerJump();
    }

    private void TriggerJump() {
        if (Time.unscaledTime - lastJumpTime < 0.04f) return;
        lastJumpTime = Time.unscaledTime;
        PlayOneShot(clipJump, 0.45f, Random.Range(0.96f, 1.04f));
    }

    public static void PlayEnemyDeath() {
        if (!Instance) EnsureInstance();
        if (Instance) Instance.TriggerEnemyDeath();
    }

    private void TriggerEnemyDeath() {
        if (Time.unscaledTime - lastDeathTime < 0.03f) return;
        lastDeathTime = Time.unscaledTime;
        PlayOneShot(clipEnemyDeleted, 0.55f, Random.Range(0.94f, 1.06f));
    }

    public static void PlayPowerup() {
        if (!Instance) EnsureInstance();
        if (Instance) Instance.TriggerPowerup();
    }

    private void TriggerPowerup() {
        if (Time.unscaledTime - lastPowerupTime < 0.35f) return;
        lastPowerupTime = Time.unscaledTime;
        PlayOneShot(clipPowerup, 0.65f, 1.0f);
    }

    private void PlayOneShot(AudioClip clip, float volume, float pitch) {
        if (clip == null || GameDuoCinema.Busy || AudioListener.pause) return;

        foreach (var v in voices) {
            if (!v.isPlaying) {
                v.clip = clip;
                v.volume = volume;
                v.pitch = pitch;
                v.Play();
                return;
            }
        }
        // Si todas las voces estan ocupadas, reutilizar la primera
        voices[0].clip = clip;
        voices[0].volume = volume;
        voices[0].pitch = pitch;
        voices[0].Play();
    }
}
}
