# Accessibility Framework - Technical Context

This document provides technical context for implementing accessibility-first game development patterns using MercuryMessaging.

---

## Core Concept: Message-Based Accessibility

### Traditional Game Accessibility
```csharp
// Scattered accessibility checks throughout code
void UpdateHealthUI()
{
    healthBar.value = currentHealth;

    if (Settings.HighContrast)
        healthBar.color = highContrastColor;

    if (Settings.ScreenReader)
        ScreenReader.Announce($"Health: {currentHealth}%");

    if (Settings.Haptics)
        Haptics.Pulse(healthPulsePattern);
}
```

### Mercury Accessibility Pattern
```csharp
// Single message, multiple accessibility handlers
void UpdateHealthUI()
{
    relay.MmInvoke(new HealthUpdateMessage { Value = currentHealth });
    // Accessibility responders handle their own modes
}

// Separate responders for each accessibility mode
public class HighContrastHealthResponder : MmAccessibilityResponder
{
    protected override void OnHealthUpdate(HealthUpdateMessage msg) { /* ... */ }
}

public class ScreenReaderHealthResponder : MmAccessibilityResponder
{
    protected override void OnHealthUpdate(HealthUpdateMessage msg)
    {
        ScreenReader.Announce($"Health: {msg.Value}%");
    }
}
```

---

## Accessibility Responder Base Class

```csharp
public abstract class MmAccessibilityResponder : MmBaseResponder
{
    [Header("Accessibility Settings")]
    public AccessibilityMode Mode = AccessibilityMode.Standard;
    public bool AlwaysActive = false;

    protected override void Awake()
    {
        base.Awake();
        Tag = GetTagForMode(Mode);
        TagCheckEnabled = true;
    }

    private MmTag GetTagForMode(AccessibilityMode mode)
    {
        return mode switch {
            AccessibilityMode.Visual => AccessibilityTags.Visual,
            AccessibilityMode.Auditory => AccessibilityTags.Auditory,
            AccessibilityMode.Motor => AccessibilityTags.Motor,
            AccessibilityMode.Cognitive => AccessibilityTags.Cognitive,
            _ => MmTag.Everything
        };
    }

    // Override in subclasses for specific accessibility handling
    protected virtual void OnHealthUpdate(HealthUpdateMessage msg) { }
    protected virtual void OnUIFocus(UIFocusMessage msg) { }
    protected virtual void OnGameEvent(GameEventMessage msg) { }
}
```

---

## Standardized Accessibility Messages

### UI Messages

```csharp
public class MmAccessibilityUIMessage : MmMessage
{
    public string ElementId;
    public string ElementType;  // "button", "slider", "text"
    public string Label;
    public string Value;
    public string State;        // "focused", "pressed", "disabled"
    public int TabIndex;
}
```

### Navigation Messages

```csharp
public class MmAccessibilityNavMessage : MmMessage
{
    public NavigationAction Action; // Next, Previous, Enter, Back
    public string CurrentElement;
    public string TargetElement;
    public Vector2 Direction;
}
```

### Feedback Messages

```csharp
public class MmAccessibilityFeedbackMessage : MmMessage
{
    public FeedbackType Type;       // Success, Error, Warning, Info
    public string Text;
    public float Duration;
    public int Priority;            // For queuing announcements
}
```

---

## Alternative Input Bridges

### Voice Command Bridge

```csharp
public class MmVoiceCommandBridge : MonoBehaviour
{
    [System.Serializable]
    public class VoiceMapping
    {
        public string[] Phrases;
        public MmMethod TargetMethod;
        public string StringValue;
    }

    public List<VoiceMapping> Mappings;

    public void OnSpeechRecognized(string phrase)
    {
        foreach (var mapping in Mappings)
        {
            if (mapping.Phrases.Any(p => phrase.Contains(p)))
            {
                var message = CreateMessage(mapping);
                _relay.MmInvoke(message);
                return;
            }
        }

        // Unknown command - provide feedback
        _relay.MmInvoke(new MmAccessibilityFeedbackMessage {
            Type = FeedbackType.Info,
            Text = $"Command not recognized: {phrase}"
        });
    }
}
```

### Switch Control Bridge

```csharp
public class MmSwitchControlBridge : MonoBehaviour
{
    public enum ScanMode { Row, Column, Item }
    public ScanMode Mode = ScanMode.Item;
    public float ScanInterval = 1.5f;

    private int _currentIndex;
    private List<MmAccessibilityResponder> _scanItems;

    public void OnSwitchActivate()
    {
        if (Mode == ScanMode.Item)
        {
            // Select current item
            var item = _scanItems[_currentIndex];
            _relay.MmInvoke(new MmAccessibilityNavMessage {
                Action = NavigationAction.Enter,
                TargetElement = item.name
            });
        }
        else
        {
            // Move to next scan level
            AdvanceScanLevel();
        }
    }

    public void OnSwitchNextPress()
    {
        _currentIndex = (_currentIndex + 1) % _scanItems.Count;
        HighlightCurrentItem();
    }
}
```

---

## Screen Reader Integration

### Announcement Queue

```csharp
public class MmScreenReaderResponder : MmAccessibilityResponder
{
    private Queue<string> _announcementQueue = new();
    private bool _isAnnouncing;

    protected override void OnAccessibilityFeedback(MmAccessibilityFeedbackMessage msg)
    {
        _announcementQueue.Enqueue(msg.Text);
        if (!_isAnnouncing)
            StartCoroutine(ProcessQueue());
    }

    IEnumerator ProcessQueue()
    {
        _isAnnouncing = true;
        while (_announcementQueue.Count > 0)
        {
            var text = _announcementQueue.Dequeue();
            yield return ScreenReader.Announce(text);
            yield return new WaitForSeconds(0.2f);
        }
        _isAnnouncing = false;
    }
}
```

---

## Mode Switching

### Accessibility Mode FSM

```csharp
public class AccessibilityModeController : MonoBehaviour
{
    private MmRelaySwitchNode _modeSwitch;

    public void SetMode(AccessibilityMode mode)
    {
        string stateName = mode switch {
            AccessibilityMode.Standard => "StandardMode",
            AccessibilityMode.HighContrast => "HighContrastMode",
            AccessibilityMode.ScreenReader => "ScreenReaderMode",
            AccessibilityMode.Motor => "MotorAccessibilityMode",
            _ => "StandardMode"
        };

        _modeSwitch.RespondersFSM.JumpTo(stateName);

        // Announce mode change
        _relay.MmInvoke(new MmAccessibilityFeedbackMessage {
            Text = $"Accessibility mode: {mode}",
            Type = FeedbackType.Info
        });
    }
}
```

---

*Last Updated: 2025-12-17*
