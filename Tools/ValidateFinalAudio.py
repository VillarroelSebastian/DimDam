from pathlib import Path
import array, math, wave

peaks, sizes, report = {}, [], []
paths = list(Path('Assets/GameDuo/Resources/FinalAudio').glob('*.wav'))
assert len(paths) == 9
for path in paths:
    with wave.open(str(path)) as f:
        pcm = array.array('h', f.readframes(f.getnframes()))
        peak = max(abs(x) for x in pcm) / 32768
        rms = math.sqrt(sum((x/32768)**2 for x in pcm)/len(pcm))
        assert 0 < peak < .66 and rms > .01
        peaks[path.stem] = peak
        report.append(f'PASS {path.stem}: peak={peak:.3f} RMS={rms:.3f}')
        if path.stem in ('Battle', 'Tension'):
            sizes.append((len(pcm), f.getframerate()))
assert sizes[0] == sizes[1]
bound = peaks['Battle']*.32 + peaks['Tension']*.17 + max(v for k,v in peaks.items() if k not in ('Battle','Tension'))*.198*6
assert bound < .9
report.append(f'PASS conservative added-audio peak bound: {bound:.3f}')
Path('Logs/FinalAudioPCMValidation.txt').write_text('\n'.join(report)+'\n')
print('\n'.join(report))
