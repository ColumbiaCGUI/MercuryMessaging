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

### Project Root Structure (Assets/) - 6 Folders

The project was reorganized on 2025-11-25 from 14 folders to 6 clean top-level folders.

```
Assets/
├── Framework/                   # MercuryMessaging framework (portable, zero dependencies)
│   └── MercuryMessaging/        # Core messaging framework
│       ├── AppState/            # Application state management with FSM
│       ├── Protocol/            # Core messaging protocol (MOST IMPORTANT)
│       │   ├── Core/            # Hot-path optimized code (pooling, dispatch)
│       │   ├── DSL/             # Fluent API (MmFluentMessage, extensions)
│       │   ├── Message/         # Message type definitions
│       │   └── Network/         # Networking backends (IMmNetworkBackend)
│       ├── StandardLibrary/     # Typed message handlers
│       │   ├── UI/              # UI messages (Click, Hover, Drag, etc.)
│       │   └── Input/           # VR input messages (6DOF, Gesture, Button)
│       ├── Support/             # Supporting utilities and systems
│       ├── Task/                # Task management system for experiments
│       ├── Tests/               # Unit and integration tests
│       └── Examples/            # Demo and tutorial content
│
├── Project/                     # Project-specific code (feature-based organization)
│   ├── Scripts/                 # All custom scripts
│   │   ├── Core/                # Core application logic
│   │   ├── Responders/          # Custom MercuryMessaging responders
│   │   ├── TrafficLights/       # Traffic light system scripts
│   │   ├── Tutorials/           # Tutorial scripts
│   │   ├── UI/                  # UI-related scripts
│   │   └── VR/                  # VR/XR initialization scripts
│   ├── Prefabs/                 # Reusable prefab assets
│   ├── Materials/               # Project-specific materials
│   └── Settings/                # Project configuration files
│
├── Research/                    # User studies & experiments
│   └── UserStudy/               # User study scenes, scripts, and assets
│
├── Plugins/                     # Third-party dependencies
│   ├── AssetStore/              # Unity Asset Store packages
│   ├── GraphSystem/             # Graph visualization systems
│   └── Plugins/                 # Third-party plugins (ALINE, Photon, etc.)
│
├── Platform/                    # XR/VR configuration (consolidated)
│   └── XR/                      # All XR-related configuration
│       ├── Settings/            # XR settings (MetaXR, Oculus, XRI, etc.)
│       ├── Oculus/              # Oculus-specific assets
│       │   └── ControllerArt/   # Controller models (~494MB, OUT of Resources)
│       └── Samples/             # XR samples
│
└── Unity/                       # Unity-managed folders
    ├── Editor/                  # Unity editor scripts
    ├── Resources/               # Runtime-loadable assets (minimal, ~6KB)
    ├── Settings/                # Project settings
    ├── StreamingAssets/         # Streaming assets
    └── TextMesh Pro/            # TextMesh Pro package
```

**Key Improvements (2025-11-25 Reorganization):**
- **57% folder reduction**: From 14 to 6 top-level folders
- **~500MB build size reduction**: Controller art moved out of Resources
- **Framework isolation**: MercuryMessaging now portable as a package
- **XR consolidation**: 4 XR locations merged into Platform/XR/

---

## Important Files Reference

For a complete list of important files with descriptions, see [FILE_REFERENCE.md](../FILE_REFERENCE.md).

**Key Files:**
- `Protocol/MmRelayNode.cs` (1422 lines) - Central message router
- `Protocol/MmBaseResponder.cs` (383 lines) - Base responder with method routing
- `Protocol/MmExtendableResponder.cs` - Registration-based custom method handling
- `Protocol/MmRelaySwitchNode.cs` (188 lines) - FSM-enabled relay node
- See [FILE_REFERENCE.md](../FILE_REFERENCE.md) for complete list
