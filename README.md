# MercuryMessaging

![Basic scene layout](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Images/General/MercuryCollage2.png)

The *Mercury Messaging* toolkit is a new way to handle cross-component communication in the Unity
  game engine. It integrates seamlessly with the Unity Editor, and is both
 robust and expandable. 
 
*It has been tested in Unity 2022 up until 2022.3.18f1. This is the recommended version of Unity to use with Mercury! Networking features have been tested and work for PUN and Fusion in LTS 2022.*
 
The toolkit contains the *Mercury* messaging framework, which is a messaging
  and organizational framework built around the *Mercury Protocol*. 

Unity organizes its rendered scene objects
(known in Unity as
  [GameObjects](https://docs.unity3d.com/ScriptReference/GameObject.html))
using a standard scene graph (known in Unity as the
  [Scene Hierarchy](https://docs.unity3d.com/Manual/Hierarchy.html)).
While Unity is very powerful,
it is fairly difficult to achieve nonspatial communication between
scriptable components of GameObjects (in Unity, known as
  [MonoBehaviours](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html))
.

Consider a visualization in a basketball game where you connect the ball to *each* 
player on a court with individual lines. There will be a control script on each
  line in the visualization.

Normally, to disable a GameObject in Unity, you invoke the
  [SetActive](https://docs.unity3d.com/ScriptReference/GameObject.SetActive.html)
  method.
This will disable the GameObject and its children in the scene hierarchy.
However, in our example, to disable the entire visualization, you would need to
  disable the endpoint spheres and the line objects.
In a script, you would need to get a handle to the GameObjects, and invoke
  SetActive on each of them individually.  

Developed by **Columbia University CGUI Lab**

---

## Overview

The *Mercury* messaging toolkit is a new way to handle cross-component communication in the Unity game engine. It integrates seamlessly with the Unity Editor, and is both robust and expandable.

### What is MercuryMessaging?

MercuryMessaging enables loosely-coupled communication between GameObjects through hierarchical message routing. It eliminates the need for direct component references, making game code more modular, maintainable, and scalable.

### Key Features

- **Hierarchical Message Routing** - Messages flow naturally through Unity's scene graph
- **Multi-Level Filtering** - Target messages by level, active state, tags, and network status
- **Built-in Networking** - Automatic message serialization for multiplayer games
- **FSM Integration** - First-class support for finite state machines
- **Task Management** - System for managing experimental workflows and user studies
- **VR/XR Ready** - Compatible with Unity XR Interaction Toolkit

---

## The Problem

Unity organizes its rendered scene objects (known in Unity as [GameObjects](https://docs.unity3d.com/ScriptReference/GameObject.html)) using a standard scene graph (known in Unity as the [Scene Hierarchy](https://docs.unity3d.com/Manual/Hierarchy.html)). While Unity is very powerful, it is fairly difficult to achieve nonspatial communication between scriptable components of GameObjects (in Unity, known as [MonoBehaviours](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html)).

### Example: Basketball Visualization

Consider a visualization in a basketball game where you connect the ball to *each* player on a court with individual lines. There will be a control script on each line in the visualization.

Normally, to disable a GameObject in Unity, you invoke the [SetActive](https://docs.unity3d.com/ScriptReference/GameObject.SetActive.html) method. This will disable the GameObject and its children in the scene hierarchy. However, in our example, to disable the entire visualization, you would need to disable the endpoint spheres and the line objects. In a script, you would need to get a handle to the GameObjects, and invoke SetActive on each of them individually.

### The Mercury Solution

In this simple example, the Mercury messaging toolkit makes it easy to achieve this.

You first drop an *MmRelayNode* (MercuryMessaging Relay Node) and an *MmResponder* (MercuryMessaging Responder) onto each of the related GameObjects in the visualization.

Each *MmRelayNode* has a *MmRoutingTable*. In the line's root *MmRelayNode*, you'll drag and drop the related components: the endpoint-spheres and line.

In your line control script, you invoke the following method:

```csharp
GetComponent<MmRelayNode>().MmInvoke(MmMethod.SetActive, true,
    new MmMetadataBlock(MmLevelFilter.Default, MmActiveFilter.All,
    MmSelectedFilter.All, MmNetworkFilter.Local));
```

This will trigger a special SetActive message on each of the objects involved in the visualization.

Done!

## IEEE ISMAR 2024

A library of debugging features and UI for Mercury was presented at IEEE ISMAR 2024 with the title "An XR GUI for Visualizing Messages in ECS Architectures". 

The extended abstract for this demo will be available soon in the Adjunct Proceedings of IEEE ISMAR 2024 in IEEE Xplore.

## CHI 2018

## Quick Start

### Installation

**From GitHub:**
You can check-out or download the code from [GitHub](https://github.com/ColumbiaCGUI/MercuryMessaging) directly. If you downloaded the source from GitHub, please drag and drop the root folder of MercuryMessaging, *MercuryMessaging* into the Assets folder of your project.

### Basic Usage

```csharp
// 1. Add MmRelayNode to a GameObject to enable message routing
public class GameManager : MonoBehaviour {
    private MmRelayNode relay;

    void Start() {
        relay = GetComponent<MmRelayNode>();

        // Send a message to all child objects
        relay.MmInvoke(MmMethod.Initialize);
    }
}

// 2. Create a responder to receive messages
public class MyComponent : MmBaseResponder {
    protected override void ReceivedInitialize() {
        Debug.Log("Initialized!");
    }

    protected override void ReceivedMessage(MmMessageString message) {
        Debug.Log("Received: " + message.value);
    }
}
```

### Hierarchy Setup

```
GameManager (MmRelayNode)
  ├── Player (MmRelayNode)
  │   ├── Weapon (MyComponent)
  │   └── Health (MyComponent)
  └── UI (MmRelayNode)
      ├── HUD (MyComponent)
      └── Menu (MyComponent)
```

Messages sent from GameManager will reach all child responders automatically!

---

## Project Structure (Updated November 2025)

**Important:** The project structure was reorganized on November 18, 2025 to improve organization and follow Unity best practices. See [REORGANIZATION_SUMMARY.md](REORGANIZATION_SUMMARY.md) for complete details.

```
Assets/
├── _Project/                    # Custom project assets
│   ├── Scenes/                  # Production scenes
│   ├── Scripts/                 # Custom scripts (organized by category)
│   ├── Prefabs/                 # Reusable prefabs
│   └── Settings/                # Project configuration
│
├── MercuryMessaging/            # Core framework (109 C# scripts)
│   ├── Protocol/                # Core messaging protocol ⭐ MOST IMPORTANT
│   │   ├── MmRelayNode.cs      # Central message router (1,422 lines)
│   │   ├── MmBaseResponder.cs  # Base responder class
│   │   ├── Message/            # Message type definitions
│   │   └── ...
│   ├── AppState/                # Application state management
│   ├── Task/                    # Task management system
│   ├── Support/                 # Utilities and helpers
│   └── Examples/                # Demo scenes and tutorials
│       ├── Demo/                # TrafficLights.unity demo
│       └── Tutorials/           # Tutorial scenes (1-5)
│
├── UserStudy/                   # User study scenes and scripts
│   ├── Scenes/                  # Scenario1.unity (traffic simulation)
│   └── Scripts/                 # User study-specific scripts
│
├── ThirdParty/                  # Third-party assets
│   ├── Plugins/                 # Code libraries (ALINE, Photon, etc.)
│   ├── AssetStore/              # Unity Asset Store packages
│   └── GraphSystem/             # Graph visualization systems
│
└── XRConfiguration/             # VR/XR platform configuration
    ├── Oculus/, MetaXR/, XR/, etc.
```

---

## Documentation

### Quick Navigation Guide

**🚀 New to MercuryMessaging?**
- Start: This README (you are here)
- Learn: [CLAUDE.md](CLAUDE.md) - Complete framework reference
- Practice: [Examples](Assets/MercuryMessaging/Examples/) - Demo scenes and tutorials

**💻 Developer joining the project?**
- Start: [dev/CONTEXT_RESET_READY.md](dev/CONTEXT_RESET_READY.md) - Latest session summary
- Overview: [dev/active/MASTER-SUMMARY.md](dev/active/MASTER-SUMMARY.md) - All active tasks
- Navigate: [dev/DOCUMENTATION_INDEX.md](dev/DOCUMENTATION_INDEX.md) - Complete docs index

**📊 Project manager or contributor?**
- Tasks: [dev/active/](dev/active/) - 5 focused improvement areas
- Timeline: [dev/active/MASTER-SUMMARY.md](dev/active/MASTER-SUMMARY.md) - Effort estimates
- History: [dev/archive/](dev/archive/) - Completed work

---

### Primary Documentation

- **[CLAUDE.md](CLAUDE.md)** - **START HERE** - Comprehensive framework documentation
  - Framework architecture and core concepts
  - Complete API reference with examples
  - Tutorial progression (SimpleScene → Tutorial 1-5)
  - Best practices and common patterns

- **[Official Documentation](https://columbiacgui.github.io/MercuryMessaging/)** - Complete online documentation
- **[Wiki Tutorials](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Tutorials)** - Step-by-step tutorials

### Development Documentation

**Active Work** (1,808-1,926 hours total across all tasks):

- **[dev/active/framework-analysis/](dev/active/framework-analysis/)** - NEW Nov 18
  - Comprehensive codebase analysis (109 C# scripts)
  - 10 optimization opportunities identified
  - Quick wins (38-46h) for 20-30% performance improvement
  - **Start here for performance work**

- **[dev/active/routing-optimization/](dev/active/routing-optimization/)** - CRITICAL Priority
  - Advanced routing patterns (420h, 10-11 weeks)
  - Sibling/cousin routing, custom paths
  - 3-5x performance improvement potential

- **[dev/active/network-performance/](dev/active/network-performance/)** - HIGH Priority
  - Delta state synchronization (500h, 12-13 weeks)
  - 50-80% bandwidth reduction
  - Priority queuing, object pooling

- **[dev/active/visual-composer/](dev/active/visual-composer/)** - MEDIUM Priority
  - Unity GraphView-based network editor (360h, 9 weeks)
  - Hierarchy mirroring, templates, validation

- **[dev/active/standard-library/](dev/active/standard-library/)** - MEDIUM Priority
  - 40+ standardized message types (290h, 7 weeks)
  - UI, AppState, Input, Task libraries
  - Message versioning system

- **[dev/active/user-study/](dev/active/user-study/)** - IN PROGRESS
  - Traffic simulation scene (240+ tasks)
  - 8 intersection network demonstration
  - See also: [Assets/UserStudy/README.md](Assets/UserStudy/README.md)

**Completed Work:**

- **[dev/archive/](dev/archive/)** - Completed tasks archive
  - [reorganization/](dev/archive/reorganization/) - Assets reorganization (Nov 18, 2025)
  - [mercury-improvements-original/](dev/archive/mercury-improvements-original/) - Original master plan

**Project Infrastructure:**

- **[BRANCHES.md](BRANCHES.md)** - Git workflow, branch management, merge strategies
- **[REORGANIZATION_SUMMARY.md](REORGANIZATION_SUMMARY.md)** - Assets reorganization overview (Nov 18, 2025)
- **[dev/DOCUMENTATION_INDEX.md](dev/DOCUMENTATION_INDEX.md)** - Complete documentation index

### Component Documentation

Documentation is organized by component in the Assets/ folder:

- **[Assets/MercuryMessaging/Readme.md](Assets/MercuryMessaging/Readme.md)** - Framework quick-start
- **[Assets/_Project/README.md](Assets/_Project/README.md)** - Custom project assets
- **[Assets/ThirdParty/README.md](Assets/ThirdParty/README.md)** - Third-party plugins
- **[Assets/XRConfiguration/README.md](Assets/XRConfiguration/README.md)** - VR/XR setup
- **[Assets/UserStudy/README.md](Assets/UserStudy/README.md)** - Traffic simulation (11,000 words)

---

## Examples and Tutorials

### Demo Scene
**Location:** `Assets/MercuryMessaging/Examples/Demo/TrafficLights.unity`

A traffic light simulation demonstrating real-world usage of MercuryMessaging for coordinating complex behaviors.

### Tutorial Scenes
**Location:** `Assets/MercuryMessaging/Examples/Tutorials/`

- **SimpleScene** - Basic light switch example (message passing, GUI, VR interaction)
- **Tutorial 1-5** - Progressive tutorial series covering all major features

### User Study Scene
**Location:** `Assets/UserStudy/Scenes/Scenario1.unity`

Complex traffic simulation with multiple intersections, pedestrians, and vehicles. Demonstrates MercuryMessaging at scale for a UIST 2025 research paper.

---

## Unity Version Support

The framework has been tested in:
- **Unity 2020** up until 2020.3.21f1
- **Unity 2019** up until 2019.2.17f1
- **Unity 2018** up until 2018.3.13f1
- **Unity 2017** up until 2017.4f1
- **Unity 5.6**

**Recommended:** Unity 2021.3 LTS, Unity 2022.3 LTS, or Unity 6 (2023.2+)

### Platform Support
- **Editor:** Windows, macOS, Linux
- **Standalone:** Windows, macOS, Linux
- **Mobile:** Android, iOS
- **Web:** WebGL
- **XR:** Quest 2/3, PSVR2, PC VR (OpenXR)

---

## Research and Publications

### CHI 2018

*Mercury* was presented at CHI 2018. The paper is available online at the ACM Digital Library.

Carmine Elvezio, Mengu Sukan, and Steven Feiner. 2018. **Mercury: A Messaging Framework for Modular UI Components.** In Proceedings of the 2018 CHI Conference on Human Factors in Computing Systems (CHI '18). ACM, New York, NY, USA, Paper 588, 12 pages. DOI:https://doi.org/10.1145/3173574.3174162

### Additional Publications

- **An XR GUI for Visualizing Messages in ECS Architectures** - Recent XR visualization work
- **Dissertation Chapters 1-3** - Extensive background, use cases, and validation

### Ongoing Development

**User Study Scene** (In Progress)
- Complex traffic simulation with multiple intersections
- Demonstrates MercuryMessaging at scale
- Autonomous agents with AI behaviors
- Located in `Assets/UserStudy/`

---

## FAQ

### Q. Does the toolkit work in Unity version 5.4.x, 4.x, 3.x, and earlier?

A. Core functions have been tested in Unity 2020 up until 2020.3.21f1, Unity 2019 up until 2019.2.17f1, Unity 2018 up until 2018.3.13f1, Unity 2017 up until 2017.4f1, and 5.6. 

The Framework was originally developed using Unity 5.6. The toolkit requires some features that were added in Unity 5. As such, we provide no support for the toolkit in earlier versions of Unity. That said, it may work in other versions of Unity 5, but we're not sure.

### Q. What is Unity?

A. Unity is a game engine. Please see here: [Unity](https://unity3d.com/).

### Q. Can I use the toolkit with Unreal, CryEngine, etc.?

A. As much as we like those engines, we originally built the toolkit to support us in our work in our lab, where we use Unity.

### Q. UNET Deprecation

A. MercuryMessaging uses a few components of UNET in the framework. Unity is deprecating and removing UNET from Unity post 2019.4 (LTS) and 2018.4 (LTS). We are already transitioning the code away from UNET, but while we are, if you try to use MercuryMessaging in Unity 2019.1.4 and beyond, you may encounter compilation issues. To resolve these, all you need to do is ensure that you have the Multiplayer HLAPI enabled in packages (Unity→Window→Package Manager→Multiplayer HLAPI (Install)).

---

## Contributing

MercuryMessaging is a research project from Columbia University's CGUI Lab. While we're not currently accepting external contributions, we welcome:

- Bug reports and issues
- Feature requests and suggestions
- Research collaborations
- User studies participation

Contact us through the GitHub issues page or the CGUI Lab website.

---

## Support and Contact

### Columbia CGUI Lab
- **Website:** [Columbia CGUI Lab](https://cgui.cs.columbia.edu/)
- **GitHub:** https://github.com/ColumbiaCGUI/MercuryMessaging
- **Issues:** Report bugs and request features via GitHub Issues

### Key People
- **Steven Feiner** - Principal Investigator
- **Carmine Elvezio** - Lead Developer
- **Mercury Development Team** - Columbia CGUI Lab

---

## License

**TBD** - Check with Columbia University CGUI Lab for licensing information.

---

## Acknowledgments

Funded in part by National Science Foundation Grant IIS-1514429.

Any opinions, findings and conclusions or recommendations expressed in this material are those of the authors and do not necessarily reflect the views of the National Science Foundation.

MercuryMessaging was developed at Columbia University's Computer Graphics and User Interfaces (CGUI) Lab with support from various research grants and collaborators.

Special thanks to:
- Columbia University CGUI Lab
- Unity Technologies
- The VR/XR developer community
- All research participants and collaborators

---

**Last Updated:** November 18, 2025
**Version:** 1.x (Unity 2021.3+)
**Status:** Active Research Project
