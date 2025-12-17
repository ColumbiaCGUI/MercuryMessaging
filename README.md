# MercuryMessaging

![MercuryMessaging](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Images/General/MercuryCollage2.png)

**Hierarchical Message Routing for Unity**

![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black?logo=unity)
![License](https://img.shields.io/badge/License-BSD--3--Clause-blue)
[![DOI](https://img.shields.io/badge/DOI-10.1145%2F3173574.3174162-blue)](https://doi.org/10.1145/3173574.3174162)

Developed by **[Columbia University CGUI Lab](https://cgui.cs.columbia.edu/)**

---

## What is MercuryMessaging?

MercuryMessaging enables loosely-coupled communication between Unity GameObjects through hierarchical message routing. Instead of managing direct component references, send messages that automatically propagate through your scene hierarchy.

---

## Quick Start

```csharp
using MercuryMessaging;

// 1. Add MmRelayNode to GameObjects to enable message routing
// 2. Create responders to handle messages
public class MyComponent : MmBaseResponder
{
    protected override void ReceivedInitialize()
    {
        Debug.Log("Initialized!");
    }
}

// 3. Send messages with the Fluent DSL API
relay.BroadcastInitialize();                              // Initialize all children
relay.Send("hello").ToDescendants().Execute();            // Send to all descendants
relay.Send(42).ToChildren().WithTag(MmTag.Tag0).Execute(); // Filtered messaging
```

**Traditional Unity vs MercuryMessaging:**
```csharp
// Traditional: Direct references, tight coupling
player.GetComponent<Health>().TakeDamage(10);
enemy.GetComponent<AI>().OnPlayerHit();
ui.GetComponent<HUD>().UpdateHealth();

// MercuryMessaging: One message, automatic routing
relay.BroadcastValue(10).ToDescendants().Execute();
```

---

## Key Features

- **Hierarchical Message Routing** - Messages flow through Unity's scene graph
- **Fluent DSL API** - Modern chainable API with 77% code reduction
- **Multi-Level Filtering** - Target by level, active state, tags, and network status
- **Built-in Networking** - FishNet and Photon Fusion 2 support
- **FSM Integration** - First-class finite state machine support
- **VR/XR Ready** - Compatible with Unity XR Interaction Toolkit
- **Zero Dependencies** - Core framework has no external dependencies

---

## Installation

**Unity Package Manager (Git URL):**
```
https://github.com/ColumbiaCGUI/MercuryMessaging.git?path=Assets/MercuryMessaging
```

**Manual Installation:**
Download or clone the repository and copy `Assets/MercuryMessaging/` into your project's Assets folder.

---

## Compatibility

| Unity Version | Status |
|---------------|--------|
| Unity 6 (6000.x) | Tested |
| Unity 2022.3 LTS | Recommended |
| Unity 2021.3 LTS | Supported |
| Unity 2020.3 | Legacy |

**Platforms:** Windows, macOS, Linux, Android, iOS, WebGL, Quest 2/3, PC VR (OpenXR)

**Optional Networking:** FishNet, Photon Fusion 2

---

## Documentation

- **[Wiki Tutorials](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Tutorials)** - Step-by-step learning path
- **[API Reference](https://columbiacgui.github.io/MercuryMessaging/)** - Complete API documentation
- **[Examples](Assets/MercuryMessaging/Examples/)** - Demo scenes and sample code

---

## Research & Citation

MercuryMessaging is a research project from Columbia University. If you use it in your work, please cite:

### CHI 2018 (Primary Citation)

Carmine Elvezio, Mengu Sukan, and Steven Feiner. 2018. **Mercury: A Messaging Framework for Modular UI Components.** In *Proceedings of the 2018 CHI Conference on Human Factors in Computing Systems* (CHI '18). ACM, Paper 588, 12 pages. https://doi.org/10.1145/3173574.3174162

<details>
<summary>BibTeX</summary>

```bibtex
@inproceedings{elvezio2018mercury,
  author = {Elvezio, Carmine and Sukan, Mengu and Feiner, Steven K.},
  title = {Mercury: A Messaging Framework for Modular UI Components},
  booktitle = {Proceedings of the 2018 CHI Conference on Human Factors in Computing Systems},
  series = {CHI '18},
  year = {2018},
  pages = {1--12},
  doi = {10.1145/3173574.3174162},
  publisher = {ACM}
}
```
</details>

### IEEE ISMAR 2024

Ben Yang, Xichen He, Jace Li, Carmine Elvezio, and Steven Feiner. 2024. **An XR GUI for Visualizing Messages in ECS Architectures.** In *2024 IEEE International Symposium on Mixed and Augmented Reality Adjunct* (ISMAR-Adjunct). IEEE, 878-879. https://doi.org/10.1109/ISMAR-Adjunct64951.2024.00189

### Dissertation

Carmine Elvezio. 2021. **XR Development with the Relay and Responder Pattern.** Ph.D. Dissertation, Columbia University. https://doi.org/10.7916/D8-K5VE-HK48

---

## Support

- **Bug Reports:** [GitHub Issues](https://github.com/ColumbiaCGUI/MercuryMessaging/issues)
- **Questions:** [GitHub Discussions](https://github.com/ColumbiaCGUI/MercuryMessaging/discussions)
- **Lab Website:** [Columbia CGUI Lab](https://cgui.cs.columbia.edu/)

This is a research project. We welcome bug reports and feature requests, but are not currently accepting external pull requests.

---

## License

BSD 3-Clause License - Copyright (c) 2017, Columbia University

## Acknowledgments

Funded in part by National Science Foundation Grants IIS-1514429 and CMMI-2037101.

Any opinions, findings and conclusions or recommendations expressed in this material are those of the authors and do not necessarily reflect the views of the National Science Foundation.
