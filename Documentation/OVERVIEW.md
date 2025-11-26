# Project Overview

**MercuryMessaging** is a hierarchical message routing framework for Unity developed by Columbia University's CGUI lab. It enables loosely-coupled communication between GameObjects through a message-based architecture, eliminating the need for direct component references.

## Key Features

- **Hierarchical Message Routing**: Messages flow through Unity's scene graph structure
- **Multi-Level Filtering**: Target messages by level (parent/child), active state, tags, and network status
- **Built-in Networking**: Automatic message serialization for networked applications
- **FSM Integration**: First-class support for finite state machines
- **Task Management**: System for managing experimental workflows and user studies
- **VR/XR Ready**: Compatible with Unity XR Interaction Toolkit

## External Dependencies

**IMPORTANT:** The core MercuryMessaging framework (AppState, Protocol, Support/Data, Task) has **ZERO** third-party dependencies as of 2025-11-19.

### Optional Dependencies (Properly Isolated)
- **Photon Unity Networking** (optional) - Wrapped in `#if PHOTON_AVAILABLE` for network features

**Policy:** The core framework must remain dependency-free to ensure maximum portability and minimal breaking changes.

---

## Directory Structure

### Project Root Structure (Assets/)

```
Assets/
├── _Project/                    # Custom project-specific assets (NEW - organized)
│   ├── Scenes/                  # Production scenes
│   ├── Scripts/                 # All custom scripts (organized by category)
│   │   ├── Core/                # Core application logic
│   │   ├── UI/                  # UI-related scripts
│   │   ├── VR/                  # VR/XR initialization scripts
│   │   ├── Utilities/           # General utility scripts
│   │   ├── Responders/          # Custom MercuryMessaging responders
│   │   ├── TrafficLights/       # Traffic light system scripts
│   │   └── Tutorials/           # Tutorial scripts (from original Script/)
│   ├── Prefabs/                 # Reusable prefab assets
│   │   ├── UI/                  # UI prefabs
│   │   └── Environment/         # Environment prefabs
│   ├── Materials/               # Project-specific materials
│   ├── Resources/               # Runtime-loadable assets
│   └── Settings/                # Project configuration files
│
├── MercuryMessaging/            # Core messaging framework (109 C# scripts)
│   ├── AppState/                # Application state management with FSM
│   ├── Protocol/                # Core messaging protocol (MOST IMPORTANT)
│   │   └── Message/             # Message type definitions
│   ├── Support/                 # Supporting utilities and systems
│   │   ├── Data/                # Data collection and CSV/XML handling
│   │   ├── Editor/              # Custom Unity editor utilities
│   │   ├── Extensions/          # C# extension methods
│   │   ├── FiniteStateMachine/  # Generic FSM implementation
│   │   ├── GUI/                 # GUI utilities and responders
│   │   ├── Input/               # Input handling (keyboard)
│   │   ├── Interpolators/       # Animation/interpolation utilities
│   │   └── ThirdParty/          # Third-party integration utilities
│   ├── Task/                    # Task management system for experiments
│   │   └── Transformation/      # Transform-specific task implementations
│   └── Examples/                # Demo and tutorial content (REORGANIZED)
│       ├── Demo/                # Demo scenes (TrafficLights.unity)
│       └── Tutorials/           # Tutorial scenes and examples
│           ├── SimpleScene/     # Basic light switch example
│           ├── SimpleTutorial_Alternative/
│           └── Tutorial1-5/     # Progressive tutorial series
│
├── UserStudy/                   # User study scenes, scripts, and assets
│
├── ThirdParty/                  # All third-party assets (NEW - consolidated)
│   ├── Plugins/                 # Third-party plugins and libraries
│   │   ├── ALINE/               # Debug drawing library
│   │   ├── EasyPerformantOutline/  # Outline effect system
│   │   ├── QuickOutline/        # Quick outline effect
│   │   ├── MKGlowFree/          # Glow effect system (formerly _MK/)
│   │   ├── Photon/              # Photon Fusion networking
│   │   ├── Android/             # Android-specific plugins
│   │   └── Vuplex/              # WebView plugin
│   ├── AssetStore/              # Unity Asset Store packages
│   │   ├── ModularCityProps/    # City building props (formerly MCP/)
│   │   ├── PBR_TrafficLightsEU/  # PBR traffic lights
│   │   ├── TrafficLightsSystem/  # Traffic light system with tools
│   │   └── Skybox/              # Skybox assets
│   └── GraphSystem/             # Graph visualization systems
│       ├── NewGraph/            # Base graph framework (formerly NewGraph-master/)
│       └── MercuryGraph/        # Mercury-specific graph implementation
│
├── XRConfiguration/             # VR/XR platform configuration (NEW - consolidated)
│   ├── Oculus/                  # Oculus/Meta platform config
│   ├── MetaXR/                  # Meta XR SDK configuration
│   ├── XR/                      # Unity XR Plugin Management
│   ├── XRI/                     # XR Interaction Toolkit settings
│   └── CompositionLayers/       # Composition layers config
│
├── Editor/                      # Unity editor scripts
├── Resources/                   # Unity Resources folder
├── Settings/                    # Project settings
├── Samples/                     # Unity Package Manager samples
└── TextMesh Pro/                # TextMesh Pro package
```

**Note**: The project structure was reorganized on 2025-11-18 to improve organization and follow Unity best practices. See `dev/ASSETS_REORGANIZATION_PLAN.md` for historical context.

---

## Important Files Reference

For a complete list of important files with descriptions, see [FILE_REFERENCE.md](../FILE_REFERENCE.md).

**Key Files:**
- `Protocol/MmRelayNode.cs` (1422 lines) - Central message router
- `Protocol/MmBaseResponder.cs` (383 lines) - Base responder with method routing
- `Protocol/MmExtendableResponder.cs` - Registration-based custom method handling
- `Protocol/MmRelaySwitchNode.cs` (188 lines) - FSM-enabled relay node
- See [FILE_REFERENCE.md](../FILE_REFERENCE.md) for complete list
