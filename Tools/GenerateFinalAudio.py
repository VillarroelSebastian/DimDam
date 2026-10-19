"""Original candy-king battle score and cues; deterministic, no external assets."""
from pathlib import Path
import array, math, random, wave, uuid

OUT = Path('Assets/GameDuo/Resources/FinalAudio')
OUT.mkdir(parents=True, exist_ok=True)
SR = 22050
BEAT = 60 / 138
LENGTH = BEAT * 64
rng = random.Random(418)

def note(buf, at, duration, midi, gain, voice='bell'):
    frequency = 440 * 2 ** ((midi - 69) / 12)
    start = round(at * SR)
    for i in range(round(duration * SR)):
        t = i / SR
        envelope = min(1, t / .008) * min(1, (duration - t) / .04)
        phase = 2 * math.pi * frequency * t
        if voice == 'bass':
            sound = math.sin(phase) + .25 * math.sin(phase * 2)
        elif voice == 'bell':
            sound = (math.sin(phase) + .28 * math.sin(phase * 3)) * math.exp(-t * 7)
        elif voice == 'kick':
            sound = math.sin(2 * math.pi * (48*t + 6*(1-math.exp(-t*25)))) * math.exp(-t*18)
        else:
            sound = rng.uniform(-1, 1) * math.exp(-t * 38)
        # Wrap tails to make the musical loop continuous.
        buf[(start + i) % len(buf)] += gain * envelope * sound

def save(name, buf, ceiling):
    peak = max(abs(v) for v in buf)
    scale = min(1, ceiling / max(peak, .00001))
    pcm = array.array('h', (round(v * scale * 32767) for v in buf))
    path = OUT / (name + '.wav')
    with wave.open(str(path), 'wb') as f:
        f.setparams((1, 2, SR, 0, 'NONE', 'not compressed'))
        f.writeframes(pcm.tobytes())
    meta = path.with_suffix('.wav.meta')
    if not meta.exists():
        meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nAudioImporter:\n  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 0\n    sampleRateSetting: 0\n    sampleRateOverride: 22050\n    compressionFormat: 0\n    quality: 1\n    conversionMode: 0\n  forceToMono: 1\n  normalize: 0\n  preloadAudioData: 1\n  loadInBackground: 0\n  ambisonic: 0\n')
    rms = math.sqrt(sum((v/32768)**2 for v in pcm)/len(pcm))
    print(f'{name}: {len(buf)/SR:.2f}s peak={peak*scale:.3f} RMS={rms:.3f}')

base = [0.] * round(LENGTH * SR)
tension = [0.] * len(base)
# C minor / Ab / F minor / G: playful celesta above a tense driving bass.
roots = [36, 32, 29, 31]
motif = [0, 7, 12, 3, 10, 7, 3, 2]
for bar in range(16):
    root = roots[bar % 4]
    for beat in range(4):
        at = (bar*4 + beat)*BEAT
        note(base, at, BEAT*.8, root + (12 if beat%2 else 0), .14, 'bass')
        note(base, at, .22, 36, .16, 'kick')
        if beat%2: note(base, at, .13, 60, .09, 'noise')
        for half in range(2):
            when = at + half*BEAT/2
            note(base, when, .25, root+36+motif[(beat*2+half+bar//4)%8], .12)
            note(tension, when, .12, 60, .045, 'noise')
            note(tension, when, BEAT*.43, root+24+[0,7,12,7][(beat*2+half)%4], .09, 'bass')
save('Battle', base, .65)
save('Tension', tension, .4)
for name, start, end, duration in [('Warning',440,660,.24), ('Attack',250,85,.22), ('Hurt',190,70,.13), ('Defeat',440,55,.55), ('Bats',850,220,.32), ('Rain',900,300,.36), ('Phase',220,880,.65)]:
    buf = []
    for i in range(round(duration*SR)):
        t = i/SR
        sweep = start*t + (end-start)*t*t/(2*duration)
        env = min(1,t/.01) * (1-t/duration)**1.5
        buf.append(env * (.5*math.sin(2*math.pi*sweep)+.12*rng.uniform(-1,1)))
    save(name, buf, .65)
