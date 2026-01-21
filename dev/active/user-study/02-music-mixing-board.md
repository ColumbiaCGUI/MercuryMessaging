# Scene 2: Music Mixing Board

## Scene Overview

**Complexity:** Simple
**Estimated Time:** 30-45 minutes
**Priority:** ⭐⭐⭐⭐⭐ (UNIQUE & ENGAGING)

**Description:**
A DJ mixing board with 8 audio tracks, effect rack (reverb, delay, EQ, distortion), master volume control, and preset system. Participants implement real-time audio mixing with hierarchical effect chains demonstrating MercuryMessaging vs Unity Events for continuous state synchronization.

**Why This Scene:**
- **Unique scenario** - Not typical game dev (creative and engaging)
- **Real-time state sync** - Demonstrates Mercury's strength in continuous updates
- **Hierarchical effect chains** - Clear audio signal flow
- **Familiar to musicians** - Anyone who's used a mixer understands the concept
- **Visual feedback** - Volume meters and waveforms make debugging easier

---

## Learning Objectives

Participants will experience:

1. **Hierarchical Signal Flow** - Audio flows through Track → Effects → Master Output
2. **Real-Time State Synchronization** - Volume changes propagate instantly
3. **Tag-Based Muting** - Mute groups (drums, bass, melody) without manual wiring
4. **Preset System** - FSM states for song sections (Intro/Verse/Chorus/Outro)
5. **Effect Chain Management** - Add/remove effects dynamically
6. **Bidirectional Communication** - Tracks report levels to UI

---

## Object Hierarchy

### MercuryMessaging Implementation

```
MixerHub (MmRelaySwitchNode - FSM for presets)
├── MasterOutput (MmBaseResponder)
│   └── EffectRack (MmRelayNode)
│       ├── ReverbEffect (MmBaseResponder)
│       ├── DelayEffect (MmBaseResponder)
│       ├── EQEffect (MmBaseResponder)
│       └── DistortionEffect (MmBaseResponder)
├── TrackGroup_Drums (MmRelayNode - Tag: Drums)
│   ├── Track_Kick (MmBaseResponder - Tag: Drums)
│   ├── Track_Snare (MmBaseResponder - Tag: Drums)
│   └── Track_HiHat (MmBaseResponder - Tag: Drums)
├── TrackGroup_Bass (MmRelayNode - Tag: Bass)
│   └── Track_Bass (MmBaseResponder - Tag: Bass)
├── TrackGroup_Melody (MmRelayNode - Tag: Melody)
│   ├── Track_Synth (MmBaseResponder - Tag: Melody)
│   ├── Track_Piano (MmBaseResponder - Tag: Melody)
│   ├── Track_Guitar (MmBaseResponder - Tag: Melody)
│   └── Track_Vocals (MmBaseResponder - Tag: Melody)
└── UI_MixerPanel (MmBaseResponder)
```

**Component Counts:**
- Total GameObjects: 17
- MmRelaySwitchNode: 1 (MixerHub for presets)
- MmRelayNode: 5 (MasterOutput + EffectRack + 3 TrackGroups)
- MmBaseResponder: 11 (8 tracks + 3 effects + UI)

**Tag Assignments:**
- `MmTag.Tag0` - Drums (3 tracks)
- `MmTag.Tag1` - Bass (1 track)
- `MmTag.Tag2` - Melody (4 tracks)
- `MmTag.Tag3` - Effects (4 effects)

**FSM States (MixerHub):**
- `Intro` - Drums + bass only, effects minimal
- `Verse` - Add melody, moderate effects
- `Chorus` - All tracks full, heavy effects
- `Outro` - Fade out, reverb increase

---

### Unity Events Implementation

```
MixerController (MonoBehaviour - custom state machine)
├── MasterOutputController (MonoBehaviour)
│   └── EffectRackController (MonoBehaviour)
│       ├── ReverbEffect (AudioReverbFilter)
│       ├── DelayEffect (AudioEchoFilter)
│       ├── EQEffect (AudioHighPassFilter + AudioLowPassFilter)
│       └── DistortionEffect (AudioDistortionFilter)
├── Track_Kick (AudioTrack)
├── Track_Snare (AudioTrack)
├── Track_HiHat (AudioTrack)
├── Track_Bass (AudioTrack)
├── Track_Synth (AudioTrack)
├── Track_Piano (AudioTrack)
├── Track_Guitar (AudioTrack)
├── Track_Vocals (AudioTrack)
└── UI_MixerPanel (MixerUI)
```

**Component Counts:**
- Total GameObjects: 14
- MixerController: 1 (central controller)
- AudioTrack scripts: 8
- Effect scripts: 4

**Inspector Wiring Required:**
- MixerController → All 8 tracks: 8 references
- MixerController → MasterOutput: 1 reference
- MixerController → EffectRack: 1 reference
- MixerController → UI: 1 reference
- Each track → MixerController (level reporting): 8 UnityEvent connections
- UI sliders → MixerController: 8 UnityEvent connections
- **Total: ~35 manual connections**

---

## Communication Patterns

### Pattern 1: Master Volume Broadcasting

**Scenario:** User adjusts master volume slider

**MercuryMessaging:**
```csharp
// In UI_MixerPanel.cs
public void OnMasterVolumeChanged(float value) {
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.MessageFloat,
        value,
        new MmMetadataBlock(MmLevelFilter.Parent) // Up to hub, then down to all tracks
    );
}

// In AudioTrack.cs
protected override void ReceivedMessage(MmMessageFloat message) {
    audioSource.volume = trackVolume * message.value; // Track vol * Master vol
}
```
- **Single message** reaches all 8 tracks
- **No references** to tracks needed
- **Automatic propagation** through hierarchy

**Unity Events:**
```csharp
// In MixerController.cs
[SerializeField] private List<AudioTrack> allTracks; // 8 references

public void OnMasterVolumeChanged(float value) {
    foreach (var track in allTracks) {
        track.SetMasterVolume(value);
    }
}

// In AudioTrack.cs
public void SetMasterVolume(float master) {
    audioSource.volume = trackVolume * master;
}
```
- **8 manual Inspector connections** required
- **Must iterate all tracks**
- **Adding track requires Inspector update**

---

### Pattern 2: Mute Groups (Tag-Based)

**Scenario:** User presses "Mute Drums" button

**MercuryMessaging:**
```csharp
// In UI_MixerPanel.cs
public void OnMuteDrumsButton() {
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.MessageBool,
        true, // muted = true
        new MmMetadataBlock(
            MmLevelFilter.Parent,
            MmActiveFilter.All,
            MmSelectedFilter.All,
            MmNetworkFilter.Local,
            MmTag.Tag0 // Drums only
        )
    );
}

// In AudioTrack.cs
protected override void ReceivedMessage(MmMessageBool message) {
    audioSource.mute = message.value;
}
```
- **Tag filter automatically selects** only drum tracks
- **Works with any number of drum tracks**
- **No drum-specific code** in controller

**Unity Events:**
```csharp
// In MixerController.cs
[SerializeField] private List<AudioTrack> drumTracks; // 3 references
[SerializeField] private List<AudioTrack> bassTracks; // 1 reference
[SerializeField] private List<AudioTrack> melodyTracks; // 4 references

public void OnMuteDrumsButton() {
    foreach (var track in drumTracks) {
        track.SetMuted(true);
    }
}
```
- **Separate list for each group** (drums, bass, melody)
- **8+ additional Inspector connections** for grouping
- **Adding track to group requires Inspector update**

---

### Pattern 3: Effect Chain (Hierarchical Audio Processing)

**Scenario:** Audio signal flows through effect chain: Track → Reverb → Delay → Master

**MercuryMessaging:**
```csharp
// Audio flows naturally through hierarchy
// Track.Update() sends audio data up the hierarchy

// In AudioTrack.cs
void Update() {
    float[] audioData = GetAudioData();

    // Send to parent (EffectRack or directly to MasterOutput)
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.MessageFloat,
        audioData[0], // Simplified: send current sample
        new MmMetadataBlock(MmLevelFilter.Parent)
    );
}

// In ReverbEffect.cs
protected override void ReceivedMessage(MmMessageFloat message) {
    float processed = ApplyReverb(message.value);

    // Send to next in chain (parent)
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.MessageFloat,
        processed,
        new MmMetadataBlock(MmLevelFilter.Parent)
    );
}

// In MasterOutput.cs
protected override void ReceivedMessage(MmMessageFloat message) {
    outputAudioSource.PlayOneShot(message.value);
}
```
- **Hierarchy defines signal flow**
- **Adding effect = parent it in hierarchy**
- **Reorder effects = reorder in hierarchy**
- **No manual wiring**

**Unity Events:**
```csharp
// In MixerController.cs
[SerializeField] private EffectRackController effectRack;

void Update() {
    foreach (var track in allTracks) {
        float audioData = track.GetAudioData();

        // Manually route through effects
        audioData = effectRack.ApplyReverb(audioData);
        audioData = effectRack.ApplyDelay(audioData);
        audioData = effectRack.ApplyEQ(audioData);
        audioData = effectRack.ApplyDistortion(audioData);

        masterOutput.PlaySample(audioData);
    }
}

// Or: Each track has reference to effect rack
// In AudioTrack.cs
[SerializeField] private EffectRackController effectRack; // 8 references

void Update() {
    float audioData = GetAudioData();
    float processed = effectRack.ProcessAudio(audioData);
    masterOutput.PlaySample(processed);
}
```
- **Manual routing** through effects
- **Effect order hardcoded** or requires complex setup
- **8 additional references** (tracks → effect rack)
- **Reordering effects = code changes**

---

### Pattern 4: Preset System (FSM)

**Scenario:** User selects "Chorus" preset - all tracks to full volume, heavy effects

**MercuryMessaging:**
```csharp
// In UI_MixerPanel.cs
public void OnChorusPresetButton() {
    GetComponent<MmRelaySwitchNode>().RespondersFSM.JumpTo("Chorus");

    // Broadcast preset change
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.Switch,
        2, // Chorus preset index
        new MmMetadataBlock(MmLevelFilter.Child)
    );
}

// In AudioTrack.cs
protected override void ReceivedSwitch(int presetIndex) {
    switch (presetIndex) {
        case 0: // Intro
            if (Tag == MmTag.Tag0 || Tag == MmTag.Tag1) { // Drums or Bass
                trackVolume = 1.0f;
            } else {
                trackVolume = 0f; // Melody muted
            }
            break;
        case 1: // Verse
            trackVolume = 0.7f; // All tracks moderate
            break;
        case 2: // Chorus
            trackVolume = 1.0f; // All tracks full
            break;
        case 3: // Outro
            StartFadeOut(); // Gradual fade
            break;
    }
    UpdateVolume();
}

// In ReverbEffect.cs
protected override void ReceivedSwitch(int presetIndex) {
    switch (presetIndex) {
        case 0: // Intro
            reverbLevel = 0.2f;
            break;
        case 2: // Chorus
            reverbLevel = 0.8f; // Heavy reverb
            break;
        case 3: // Outro
            reverbLevel = 1.0f; // Max reverb
            break;
    }
}
```
- **FSM built-in** (MmRelaySwitchNode)
- **Single broadcast** reaches all tracks and effects
- **Each component handles** preset change appropriately
- **~30 lines** for preset implementation

**Unity Events:**
```csharp
// In MixerController.cs
public enum MixerPreset { Intro, Verse, Chorus, Outro }
private MixerPreset currentPreset = MixerPreset.Intro;

public void OnChorusPresetButton() {
    SetPreset(MixerPreset.Chorus);
}

private void SetPreset(MixerPreset preset) {
    currentPreset = preset;

    switch (preset) {
        case MixerPreset.Intro:
            // Set drums and bass to full
            foreach (var track in drumTracks) track.SetVolume(1.0f);
            foreach (var track in bassTracks) track.SetVolume(1.0f);
            // Mute melody
            foreach (var track in melodyTracks) track.SetVolume(0f);
            // Minimal effects
            effectRack.SetReverbLevel(0.2f);
            break;

        case MixerPreset.Verse:
            // All tracks moderate
            foreach (var track in allTracks) track.SetVolume(0.7f);
            effectRack.SetReverbLevel(0.5f);
            break;

        case MixerPreset.Chorus:
            // All tracks full
            foreach (var track in allTracks) track.SetVolume(1.0f);
            // Heavy effects
            effectRack.SetReverbLevel(0.8f);
            effectRack.SetDelayLevel(0.6f);
            break;

        case MixerPreset.Outro:
            // Fade out all
            foreach (var track in allTracks) track.StartFadeOut();
            effectRack.SetReverbLevel(1.0f);
            break;
    }
}
```
- **Custom state enum** required
- **60+ lines** for preset implementation
- **Manual iteration** over all tracks and effects
- **Adding new track/effect requires controller update**

---

### Pattern 5: Volume Metering (Parent Notification)

**Scenario:** Tracks report audio levels to UI for volume meters

**MercuryMessaging:**
```csharp
// In AudioTrack.cs
void Update() {
    float currentLevel = GetAudioLevel();

    // Report to UI via parent
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.MessageString,
        $"{gameObject.name}:{currentLevel}",
        new MmMetadataBlock(MmLevelFilter.Parent)
    );
}

// In UI_MixerPanel.cs
protected override void ReceivedMessage(MmMessageString message) {
    string[] parts = message.value.Split(':');
    string trackName = parts[0];
    float level = float.Parse(parts[1]);

    UpdateVolumeMeter(trackName, level);
}
```
- **Parent notification** automatic
- **No reference to UI** needed
- **8 tracks reporting independently**

**Unity Events:**
```csharp
// In AudioTrack.cs
[System.Serializable]
public class LevelEvent : UnityEvent<string, float> { }
public LevelEvent OnLevelUpdate;

void Update() {
    float currentLevel = GetAudioLevel();
    OnLevelUpdate?.Invoke(gameObject.name, currentLevel);
}

// In MixerController.cs (wired in Inspector)
public void HandleTrackLevel(string trackName, float level) {
    uiPanel.UpdateVolumeMeter(trackName, level);
}
```
- **UnityEvent on each track** (8 events)
- **Manual Inspector wiring** (8 connections)
- **Controller acts as intermediary**

---

## MercuryMessaging Implementation Details

### Required Scripts

#### 1. MixerHub.cs
```csharp
using UnityEngine;
using MercuryMessaging;

public class MixerHub : MonoBehaviour
{
    private MmRelaySwitchNode switchNode;

    void Start() {
        switchNode = GetComponent<MmRelaySwitchNode>();
        switchNode.RespondersFSM.JumpTo("Intro");
    }

    public void SetPreset(string presetName) {
        switchNode.RespondersFSM.JumpTo(presetName);

        int presetIndex = presetName == "Intro" ? 0 :
                          presetName == "Verse" ? 1 :
                          presetName == "Chorus" ? 2 : 3;

        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.Switch,
            presetIndex,
            new MmMetadataBlock(MmLevelFilter.Child)
        );
    }
}
```

#### 2. AudioTrack.cs
```csharp
using UnityEngine;
using MercuryMessaging;

public class AudioTrack : MmBaseResponder
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MmTag trackGroup; // Tag0=Drums, Tag1=Bass, Tag2=Melody

    private float trackVolume = 1.0f;
    private float masterVolume = 1.0f;
    private bool isMuted = false;

    protected override void Awake() {
        base.Awake();
        Tag = trackGroup;
        TagCheckEnabled = true;
    }

    void Update() {
        // Update actual volume
        audioSource.volume = isMuted ? 0 : (trackVolume * masterVolume);

        // Report level to UI
        float level = audioSource.isPlaying ? audioSource.volume : 0;
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.MessageString,
            $"{gameObject.name}:{level:F2}",
            new MmMetadataBlock(MmLevelFilter.Parent)
        );
    }

    // Master volume changed
    protected override void ReceivedMessage(MmMessageFloat message) {
        masterVolume = message.value;
    }

    // Mute/unmute
    protected override void ReceivedMessage(MmMessageBool message) {
        isMuted = message.value;
    }

    // Preset changed
    protected override void ReceivedSwitch(int presetIndex) {
        switch (presetIndex) {
            case 0: // Intro - drums/bass only
                if (Tag == MmTag.Tag0 || Tag == MmTag.Tag1) {
                    trackVolume = 1.0f;
                } else {
                    trackVolume = 0f;
                }
                break;
            case 1: // Verse - all moderate
                trackVolume = 0.7f;
                break;
            case 2: // Chorus - all full
                trackVolume = 1.0f;
                break;
            case 3: // Outro - fade out
                StartCoroutine(FadeOut());
                break;
        }
    }

    private System.Collections.IEnumerator FadeOut() {
        float startVol = trackVolume;
        for (float t = 0; t < 2f; t += Time.deltaTime) {
            trackVolume = Mathf.Lerp(startVol, 0, t / 2f);
            yield return null;
        }
        trackVolume = 0;
    }
}
```

#### 3. UI_MixerPanel.cs
```csharp
using UnityEngine;
using UnityEngine.UI;
using MercuryMessaging;
using System.Collections.Generic;

public class UI_MixerPanel : MmBaseResponder
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Dictionary<string, Image> volumeMeters = new Dictionary<string, Image>();

    void Start() {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
    }

    public void OnMasterVolumeChanged(float value) {
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.MessageFloat,
            value,
            new MmMetadataBlock(MmLevelFilter.Parent)
        );
    }

    public void OnMuteDrumsButton() {
        SendMuteMessage(MmTag.Tag0, true);
    }

    public void OnMuteBassButton() {
        SendMuteMessage(MmTag.Tag1, true);
    }

    public void OnMuteMelodyButton() {
        SendMuteMessage(MmTag.Tag2, true);
    }

    private void SendMuteMessage(MmTag tag, bool muted) {
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.MessageBool,
            muted,
            new MmMetadataBlock(
                MmLevelFilter.Parent,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local,
                tag
            )
        );
    }

    // Preset buttons
    public void OnIntroButton() => GetComponent<MixerHub>().SetPreset("Intro");
    public void OnVerseButton() => GetComponent<MixerHub>().SetPreset("Verse");
    public void OnChorusButton() => GetComponent<MixerHub>().SetPreset("Chorus");
    public void OnOutroButton() => GetComponent<MixerHub>().SetPreset("Outro");

    // Receive level updates from tracks
    protected override void ReceivedMessage(MmMessageString message) {
        string[] parts = message.value.Split(':');
        if (parts.Length == 2) {
            string trackName = parts[0];
            float level = float.Parse(parts[1]);
            UpdateVolumeMeter(trackName, level);
        }
    }

    private void UpdateVolumeMeter(string trackName, float level) {
        if (volumeMeters.ContainsKey(trackName)) {
            volumeMeters[trackName].fillAmount = level;
        }
    }
}
```

### Total Code (MercuryMessaging)

**Estimated Lines of Code:**
- MixerHub.cs: ~30 lines
- AudioTrack.cs: ~90 lines
- AudioEffect.cs: ~40 lines (base class for effects)
- ReverbEffect.cs, DelayEffect.cs, etc.: ~20 lines each (4 × 20 = 80)
- UI_MixerPanel.cs: ~80 lines
- MasterOutput.cs: ~30 lines

**Total: ~350 lines**

**Inspector Connections:** 0

---

## Unity Events Implementation Details

### Total Code (Unity Events)

**Estimated Lines of Code:**
- MixerController.cs: ~200 lines (large controller with all logic)
- AudioTrack.cs: ~80 lines
- EffectRackController.cs: ~100 lines
- ReverbEffect.cs, DelayEffect.cs, etc.: ~30 lines each (4 × 30 = 120)
- MixerUI.cs: ~100 lines
- MasterOutput.cs: ~40 lines

**Total: ~640 lines**

**Inspector Connections:** ~35

---

## User Study Tasks

### Task 1: Add a New Synth Track (SIMPLE)
**Estimated Time:** 5-8 minutes

**Instructions:**
"Add a new synthesizer track to the Melody group. It should:
- Respond to master volume changes
- Be part of the 'Mute Melody' group
- Report audio levels to UI
- Follow preset volume changes"

**MercuryMessaging Solution:**
1. Duplicate existing Track_Synth GameObject
2. Rename to Track_Synth2
3. Parent to TrackGroup_Melody
4. Ensure Tag = MmTag.Tag2 (Melody)
5. Done!

**Expected Code Changes:** 0 lines
**Inspector Changes:** 2 (duplicate, parent)

**Unity Events Solution:**
1. Duplicate Track_Synth GameObject
2. Parent to TrackGroup_Melody
3. Add to MixerController.allTracks list
4. Add to MixerController.melodyTracks list
5. Wire OnLevelUpdate event to controller
6. Add volume meter UI element and wire it

**Expected Code Changes:** 0 lines
**Inspector Changes:** 6

---

### Task 2: Implement "Solo" Button (MEDIUM)
**Estimated Time:** 10-15 minutes

**Instructions:**
"Add a 'Solo Drums' button that:
- Mutes all non-drum tracks
- Does NOT mute drums
- Can be toggled off to unmute all"

**MercuryMessaging Solution:**
```csharp
// In UI_MixerPanel.cs
private bool drumsSoloed = false;

public void OnSoloDrumsButton() {
    drumsSoloed = !drumsSoloed;

    // Mute everything except drums
    SendMuteMessage(MmTag.Tag1, drumsSoloed); // Bass
    SendMuteMessage(MmTag.Tag2, drumsSoloed); // Melody
}
```

**Expected Code Changes:** ~10 lines
**Inspector Changes:** 1 (wire button)

**Unity Events Solution:**
```csharp
// In MixerController.cs
private bool drumsSoloed = false;

public void OnSoloDrumsButton() {
    drumsSoloed = !drumsSoloed;

    foreach (var track in bassTracks) {
        track.SetMuted(drumsSoloed);
    }
    foreach (var track in melodyTracks) {
        track.SetMuted(drumsSoloed);
    }
}
```

**Expected Code Changes:** ~15 lines
**Inspector Changes:** 1 (wire button)

---

### Task 3: Add Compression Effect (COMPLEX)
**Estimated Time:** 15-20 minutes

**Instructions:**
"Add a new 'Compression' effect to the effect chain:
- Should be after Reverb, before Delay
- Compresses loud signals, boosts quiet signals
- Has adjustable threshold parameter
- Responds to preset changes (more compression in Chorus)"

**MercuryMessaging Solution:**
1. Create CompressionEffect.cs (inherit MmBaseResponder)
2. Implement ReceivedMessage(MmMessageFloat) for audio processing
3. Implement ReceivedSwitch(int) for preset handling
4. Create CompressionEffect GameObject
5. Parent between ReverbEffect and DelayEffect in hierarchy
6. Done! (Automatically inserted into signal chain)

**Expected Code Changes:** ~40 lines (new effect script)
**Inspector Changes:** 3 (create, parent, order)

**Unity Events Solution:**
1. Create CompressionEffect.cs
2. Update EffectRackController to include compression
3. Update ProcessAudio() to call compression between reverb and delay
4. Update preset logic to adjust compression threshold
5. Create CompressionEffect GameObject and add to rack
6. Add reference in Inspector

**Expected Code Changes:** ~60 lines (new effect + controller updates)
**Inspector Changes:** 4 (create, add component, wire reference, order)

---

### Task 4: Implement "Bridge" Preset (COMPLEX)
**Estimated Time:** 12-18 minutes

**Instructions:**
"Add a new 'Bridge' preset that:
- Bass and drums at 80% volume
- Melody at full volume
- Heavy delay effect
- Moderate reverb
Add a 'Bridge' button to the UI"

**MercuryMessaging Solution:**
```csharp
// In MixerHub.cs - Add Bridge to FSM
public void SetPreset(string presetName) {
    // ... existing code
    int presetIndex = presetName == "Intro" ? 0 :
                      presetName == "Verse" ? 1 :
                      presetName == "Chorus" ? 2 :
                      presetName == "Bridge" ? 3 : 4; // Outro becomes 4

    // ... broadcast
}

// In AudioTrack.cs
protected override void ReceivedSwitch(int presetIndex) {
    // ... existing presets
    case 3: // Bridge
        if (Tag == MmTag.Tag0 || Tag == MmTag.Tag1) {
            trackVolume = 0.8f; // Drums/Bass 80%
        } else {
            trackVolume = 1.0f; // Melody full
        }
        break;
}

// In DelayEffect.cs
protected override void ReceivedSwitch(int presetIndex) {
    // ... existing presets
    case 3: // Bridge
        delayLevel = 0.9f; // Heavy delay
        break;
}

// In UI_MixerPanel.cs
public void OnBridgeButton() => GetComponent<MixerHub>().SetPreset("Bridge");
```

**Expected Code Changes:** ~25 lines (updates to 3 scripts)
**Inspector Changes:** 1 (wire button)

**Unity Events Solution:**
```csharp
// In MixerController.cs
public enum MixerPreset { Intro, Verse, Chorus, Bridge, Outro } // Update enum

private void SetPreset(MixerPreset preset) {
    // ... existing presets

    case MixerPreset.Bridge:
        // Bass and drums 80%
        foreach (var track in drumTracks) track.SetVolume(0.8f);
        foreach (var track in bassTracks) track.SetVolume(0.8f);
        // Melody full
        foreach (var track in melodyTracks) track.SetVolume(1.0f);
        // Heavy delay
        effectRack.SetDelayLevel(0.9f);
        effectRack.SetReverbLevel(0.5f);
        break;
}

public void OnBridgeButton() {
    SetPreset(MixerPreset.Bridge);
}
```

**Expected Code Changes:** ~35 lines (controller update)
**Inspector Changes:** 1 (wire button)

---

### Task 5: Debug Task (DIAGNOSTIC)
**Estimated Time:** 5-10 minutes

**Instructions:**
"The guitar track volume meter shows 0 even when audio is playing. Debug and fix the issue."

**Planted Bug (MercuryMessaging):**
- Track_Guitar missing MmRelayNode component (can't send messages)

**Solution:**
1. Check if volume meter updates for other tracks (yes)
2. Check Track_Guitar hierarchy (has AudioTrack script)
3. Check if MmRelayNode present (missing!)
4. Add MmRelayNode component
5. Test

**Time: ~5 minutes** (missing component obvious in Inspector)

**Planted Bug (Unity Events):**
- Track_Guitar's OnLevelUpdate event not wired to controller

**Solution:**
1. Check if volume meter updates for other tracks (yes)
2. Check MixerController code (looks correct)
3. Check Track_Guitar Inspector (OnLevelUpdate event has no listeners!)
4. Wire event to MixerController.HandleTrackLevel
5. Test

**Time: ~8 minutes** (need to check event wiring in Inspector)

---

## Metrics to Collect

Same as Smart Home scene (LOC, time, connections, etc.)

---

## Expected Results

**Lines of Code:**
- Mercury: ~350 lines
- Events: ~640 lines
- **Reduction: 45% fewer lines** (more dramatic than Smart Home!)

**Inspector Connections:**
- Mercury: 0
- Events: ~35
- **Reduction: 100% fewer connections**

**Task 1 (Add Track):**
- Mercury: ~5 min
- Events: ~8 min
- **37% faster**

**Task 3 (Add Effect):**
- Mercury: ~15 min (just parent in hierarchy!)
- Events: ~20 min (update controller code)
- **25% faster**

---

## Conclusion

The Music Mixing Board scene is **unique, engaging, and demonstrates Mercury's hierarchical signal flow** beautifully. The audio domain is familiar to musicians and the effect chain concept is very clear.

**Recommended for:**
- ✅ Creative/audio-focused developers
- ✅ Demonstrating hierarchical processing
- ✅ Real-time state synchronization examples

**Scales well for:**
- Adding more tracks
- Adding more effects
- Complex routing (sends, buses)
- Automation and sequencing

---

*Planning document created: 2025-11-21*
*Scene complexity: Simple*
*Priority: ⭐⭐⭐⭐⭐ (UNIQUE)*
