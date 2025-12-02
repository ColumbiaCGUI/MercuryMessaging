# Wiki Tutorials Context

**Last Updated:** 2025-12-01

---

## Key Decisions

### Wiki Location
- **Correct URL:** https://github.com/ColumbiaCGUI/MercuryMessaging/wiki
- **Clone URL:** `git clone https://github.com/ColumbiaCGUI/MercuryMessaging.wiki.git`
- **Note:** Wiki is at ColumbiaCGUI org, not CGUI-Lab

### Documentation Architecture
- **Wiki:** Human-written tutorials and guides
- **docs/ folder:** Doxygen-generated API reference (keep unchanged)

### Tutorial Numbering
- Keep existing 1-10 + 5a/5b numbering
- Add 5c for FishNet (parallel to 5a/5b)
- Add 11-14 for new features
- Add Topic pages for advanced concepts

---

## New Features Requiring Documentation

### Priority 1 (Immediate)
| Feature | Source Files |
|---------|--------------|
| FishNet Networking | `Protocol/Network/Backends/FishNetBackend.cs`, `FishNetResolver.cs` |
| Fluent DSL API | `Protocol/DSL/DSL_API_GUIDE.md`, all DSL/*.cs files |
| MmExtendableResponder | `Protocol/MmExtendableResponder.cs` |

### Priority 2 (Next)
| Feature | Source Files |
|---------|--------------|
| Spatial Filtering | `Protocol/DSL/MmFluentMessage.cs` (Within, InCone, InBounds) |
| Temporal Extensions | `Protocol/DSL/MmTemporalExtensions.cs` |
| Hierarchy Query DSL | `Protocol/DSL/MmQuery.cs`, `MmQueryExtensions.cs` |
| Message Listeners | `Protocol/DSL/MmListener.cs`, `MmListenerExtensions.cs` |

### Priority 3 (Later)
| Feature | Source Files |
|---------|--------------|
| PerformanceMode | `Protocol/MmRelayNode.cs` (PerformanceMode flag) |
| Source Generators | `SourceGenerators/MercuryMessaging.Generators/` |
| Delegate Dispatch | `Protocol/MmRoutingTableItem.cs` (Handler property) |

---

## Existing Tutorial Content Analysis

### Tutorial 1: Introduction
- Scene controller listens to keyboard input
- Shows/hides spheres using MmRelayNode + MmBaseResponder
- Uses MmMethod.SetActive

### Tutorial 2: Basic Routing
- Parent/child message routing
- MmLevelFilter concepts

### Tutorial 3: Creating Responders
- Custom MmBaseResponder subclasses
- Override ReceivedMessage methods

### Tutorial 4: Creating Messages
- Custom message types
- Serialization

### Tutorial 5a/5b: Networking
- Photon Fusion V2 and PUN2 integration
- Network message routing

### Tutorial 6-10: Advanced Topics
- Switch nodes, task management, app state, advanced networking, VR experiment

---

## Content to Reference

### For Tutorial 5c (FishNet)
```
Tests/Network/README.md
dev/active/networking/networking-context.md
Protocol/Network/Backends/FishNetBackend.cs
Protocol/Network/Backends/FishNetResolver.cs
```

### For Tutorial 11 (Fluent DSL)
```
Protocol/DSL/DSL_API_GUIDE.md
Protocol/DSL/README.md
Examples/Tutorials/DSL/README.md
Examples/Tutorials/DSL/FluentDslExample.cs
```

### For Tutorial 12 (Spatial/Temporal)
```
Protocol/DSL/MmFluentMessage.cs
Protocol/DSL/MmTemporalExtensions.cs
Examples/Tutorials/DSL/DSLTemporalDemo.cs
```

### For Tutorial 13 (MmExtendableResponder)
```
Protocol/MmExtendableResponder.cs
Protocol/DSL/MmResponderExtensions.cs
Project/Scripts/Tutorials/Tutorial4_ColorChanging/README.md
```

### For Tutorial 14 (Performance)
```
Documentation/PERFORMANCE.md
Documentation/Performance/OPTIMIZATION_RESULTS.md
SourceGenerators/README.md
```

---

## Session Notes

### 2025-12-01: Initial Planning
- Discovered wiki at ColumbiaCGUI (not CGUI-Lab)
- Confirmed existing tutorials 1-10 structure
- Planned new tutorials 5c, 11-14
- Planned new Topic pages

---

*Context file for seamless continuation after context reset*
