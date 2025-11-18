# Mercury Messaging Framework - Implementation Context

**Last Updated:** 2025-11-18

---

## SESSION PROGRESS

### Latest Session (2025-11-18 Evening - Dev Docs Update)

**✅ COMPLETED:**
- ✅ Created `/dev/active/` directory structure for active task tracking
- ✅ **Assets reorganization documentation** (reorganization-context.md, reorganization-tasks.md)
  - Comprehensive documentation of Nov 18 reorganization
  - 29+ folders → 10 organized folders
  - Phase 1 complete, Phase 2 (Unity verification) pending
- ✅ **User study documentation** (user-study-context.md, user-study-tasks.md)
  - Traffic simulation scene development status
  - 11 implemented scripts (TrafficLightController, Pedestrian, CarController, etc.)
  - 1 of 8 intersections complete, 7 pending
  - Unity Events comparison implementation not started (100 hours - CRITICAL PATH)
- ✅ **UIST 2025 paper documentation** (uist-paper-context.md, uist-paper-tasks.md)
  - Full paper structure planned (10 pages, ~9,000 words)
  - Timeline mapped (20 weeks to April 9, 2025 deadline)
  - Resource requirements ($2,300 budget, 1,080 hours effort)
  - **CRITICAL DECISION NEEDED:** Commit to UIST 2025 by Nov 25
- ✅ Updated this context file with session notes

**🟡 STATUS UPDATES:**
- Phase 0 (reorganization) - ✅ COMPLETE (awaiting Unity verification)
- Phase 1 (UIST Paper) - 🔨 IN ACTIVE PLANNING
  - Scene implementation: 1/8 intersections done
  - Unity Events implementation: Not started (blocker)
  - Participant recruitment: Not started
  - Paper writing: Not started
  - **Deadline:** April 9, 2025 (4.5 months away)

**⚠️ BLOCKERS:**
- Unity verification of reorganization (must be done next session)
- UIST 2025 commitment decision (needed by Nov 25)
- IRB requirement determination (urgent)
- Unity Events implementation resource allocation

**📝 NEXT ACTIONS:**
1. **CRITICAL:** Decide on UIST 2025 pursuit (by Nov 25)
2. **CRITICAL:** Check IRB requirements immediately
3. Open Unity and verify reorganization (Phase 2 of reorganization)
4. Continue with user study scene implementation (7 more intersections)
5. Allocate resources for Unity Events implementation
6. Begin task design for user study

---

### Original Session (2025-11-18 Morning - Strategic Planning)

**✅ COMPLETED:**
- Master strategic plan created with 6 phases
- Resource requirements estimated ($329k budget, 12-18 months)
- Risk assessment completed
- Success metrics defined
- Phase breakdown with effort estimates
- Detailed task breakdown (500+ tasks) created
- Quick-start guide and README written

**🟡 IN PROGRESS (Completed Later That Day):**
- ~~Creating detailed sub-plans for each phase~~ → **NOW: Phase 1 detailed docs created**
- ~~Setting up directory structure for tracking~~ → **NOW: /dev/active/ created**

---

## Project Overview

### What We're Building
Comprehensive enhancement of Mercury Messaging Framework for Unity, addressing:
1. Architectural limitations (routing flexibility)
2. Performance optimization (10K+ nodes)
3. Developer experience (70% reduction in debug time)
4. Ecosystem & reusability (marketplace-ready)
5. Research validation (UIST paper)

### Why It Matters
- Mercury is already used in multiple research projects
- Current limitations prevent adoption at scale
- Performance improvements enable new applications
- Better tooling dramatically improves developer productivity
- Standardization enables component reuse

### Timeline
- **Total:** 12-18 months
- **Phase 1 (Critical):** 3 months - UIST paper preparation
- **Phases 2-3:** 5 months - Core improvements
- **Phases 4-6:** 12+ months - Tooling and ecosystem

---

## Key Files & Directories

### Current Codebase (Unity)
**Location:** `/mnt/project/` (documentation only, actual code in separate repo)

**Critical Files:**
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` (1422 lines)
  - Central message router
  - Manages routing table, filters, hierarchy
  - **Most Important Class**

- `Assets/MercuryMessaging/Protocol/MmBaseResponder.cs` (383 lines)
  - Base responder with method routing
  - Switch statements for message handling

- `Assets/MercuryMessaging/Protocol/MmRoutingTable.cs`
  - Collection of responders with filtering
  - Current implementation: simple List<MmRoutingTableItem>
  - **Target for Phase 3 optimization**

- `Assets/MercuryMessaging/Protocol/MmMetadataBlock.cs`
  - Routing control parameters
  - Contains all filter types
  - **Target for Phase 2 enhancement**

### Documentation
- `/mnt/project/CLAUDE.md` - Comprehensive framework documentation
- `/mnt/project/README.md` - Dev docs pattern guidelines
- `/mnt/project/3173574_3174162.pdf` - CHI 2018 paper
- `/mnt/project/An_XR_GUI_for_Visualizing_Messages_in_ECS_Architectures.pdf` - XR GUI paper
- `/mnt/project/mercurydissertation*.pdf` - Dissertation chapters (1-3)

### Planning Documents
- `/home/claude/dev/active/mercury-improvements/mercury-improvements-master-plan.md`
  - Master strategic plan (THIS DOCUMENT'S COMPANION)
  - 6 phases, 12-18 months
  - ~2,500 hours total estimated effort

---

## Technical Architecture

### Current State

**Message Flow:**
```
User Action
  ↓
MmRelayNode.MmInvoke(message)
  ↓
Apply Filters (Level, Active, Tag, Network)
  ↓
Iterate RoutingTable
  ↓
Deliver to Matching Responders
```

**Routing Filters:**
1. **Level Filter:** Direction (Parent/Child/Self/Bidirectional)
2. **Active Filter:** GameObject active state
3. **Tag Filter:** 8 flags (Tag0-Tag7)
4. **Network Filter:** Local/Network/All

### Known Limitations

1. **Routing Restrictions:**
   - Cannot send to siblings (parent's other children)
   - Cannot send to cousins (parent's sibling's children)
   - "All" messages convert to "self-and-parents" upward
   - Message history tracking disabled (performance concerns)

2. **Performance Issues:**
   - Generic List<T> routing table not optimized
   - No specialized structures for hierarchical/flat/mesh networks
   - Network ID checking overhead with many networked objects
   - No message batching or pooling

3. **Developer Experience:**
   - Manual network construction
   - Limited debugging capabilities
   - No message history replay
   - No performance profiler

### Proposed Enhancements

**Phase 2: Advanced Routing**
```csharp
// New routing options
public class MmRoutingOptions {
    public bool EnableHistoryTracking = false;
    public int HistoryCacheSizeMs = 100;
    public int MaxRoutingHops = 50;
    public bool AllowLateralRouting = false;
    public Func<MmRelayNode, bool> CustomFilter = null;
}

// Extended level filters
public enum MmLevelFilter {
    // Existing...
    Self, Child, Parent, SelfAndChildren, SelfAndBidirectional,
    
    // NEW:
    Siblings,      // Same parent, different node
    Cousins,       // Parent's siblings' children
    Descendants,   // Recursive children
    Ancestors,     // Recursive parents
    Custom        // User-defined predicate
}
```

**Phase 3: Routing Table Optimization**
```csharp
// Specialized implementations
public interface IMmRoutingTable {
    void Add(MmRoutingTableItem item);
    IEnumerable<MmRoutingTableItem> GetRecipients(MmMessage message);
    void InvalidateCache();
}

public class FlatNetworkRoutingTable : IMmRoutingTable {
    private Dictionary<int, MmRoutingTableItem> _nodeMap;  // O(1)
}

public class HierarchicalRoutingTable : IMmRoutingTable {
    private TreeNode<MmRoutingTableItem> _root;  // O(log n)
}

public class MeshRoutingTable : IMmRoutingTable {
    private Graph<MmRelayNode> _graph;  // Dijkstra caching
}
```

---

## Important Design Decisions

### 1. Backward Compatibility First
**Decision:** All new features must maintain 100% backward compatibility
**Rationale:** Existing projects use Mercury; breaking changes unacceptable
**Implementation:** 
- New features opt-in via flags
- Deprecation warnings for 2 versions before removal
- Compatibility shims for old APIs

### 2. Performance Budget
**Decision:** Each new feature limited to < 5% performance overhead
**Rationale:** Mercury's advantage is speed; cannot sacrifice this
**Implementation:**
- Continuous benchmarking
- Feature flags to disable expensive features
- Profile-guided optimization

### 3. Phased Rollout
**Decision:** 6 phases over 12-18 months, not big-bang release
**Rationale:** Continuous value delivery, early feedback, risk mitigation
**Implementation:**
- Mercury 2.0 Alpha after Phase 2
- Mercury 2.0 Beta after Phase 4
- Mercury 2.0 Stable after Phase 6

### 4. UIST Paper Priority
**Decision:** Phase 1 (UIST paper) gets highest priority
**Rationale:** Research publication validates approach, enables funding
**Timeline:** Months 1-3, deadline April 9
**Dependencies:** None - can start immediately

### 5. Cross-Platform as Long-Term Goal
**Decision:** Defer Unreal/Godot/JS ports to Phase 5 (months 6-12)
**Rationale:** Core improvements more important; cross-platform is marketing
**Risk:** Delayed cross-platform may slow adoption

---

## Technical Constraints

### Unity Version Support
- **Minimum:** Unity 2021.3 LTS
- **Recommended:** Unity 2022.3 LTS
- **Target:** Unity 6 (2023.2+)
- **Reason:** LTS versions ensure stability, but test on latest

### C# Version
- **Current:** C# 9.0 (Unity 2021.3)
- **Target:** C# 10.0+ (Unity 2023+)
- **Reason:** Modern language features, but maintain compatibility

### Performance Targets
- **Routing Latency:** < 0.01ms for 95th percentile
- **Memory Per Node:** < 500 bytes overhead
- **Network Bandwidth:** 50-80% reduction with delta sync
- **GC Allocations:** Zero in steady state
- **Scale:** 10,000 nodes, 10,000 messages/second

### Platform Support
- **Primary:** Windows, macOS, Linux (editor + standalone)
- **Secondary:** Android, iOS, WebGL (runtime only)
- **XR:** Quest 2/3, PSVR2, PC VR (OpenXR)

---

## Dependencies

### Internal Dependencies
- Unity XR Interaction Toolkit (optional, for VR examples)
- Unity UNET (legacy) or Mirror/Netcode (modern networking)
- EPOOutline (optional, for visual debugging)
- ALINE (optional, for path drawing)
- NewGraph (optional, for graph visualization)

### External Dependencies
- Photon Unity Networking (optional, for Photon adapter)
- TextMeshPro (UI components)
- Unity UI (default UI system)

### Development Dependencies
- Unity Test Framework (unit testing)
- Rider or Visual Studio (IDE)
- Git LFS (for large assets)
- GitHub Actions (CI/CD)

---

## Testing Strategy

### Unit Tests
- **Target:** 80%+ code coverage for core classes
- **Focus:** MmRelayNode, routing logic, message delivery
- **Tools:** Unity Test Framework, NUnit
- **Location:** `Assets/MercuryMessaging/Tests/`

### Integration Tests
- **Scope:** Full message flows, multi-node networks
- **Scenarios:** Hierarchical, flat, mesh topologies
- **Tools:** PlayMode tests in Unity
- **Location:** `Assets/MercuryMessaging/Tests/Integration/`

### Performance Tests
- **Metrics:** Latency, throughput, memory, GC pressure
- **Scale:** 10, 100, 1K, 10K nodes
- **Tools:** Unity Profiler, custom benchmarks
- **Location:** `Assets/MercuryMessaging/Tests/Performance/`

### User Studies
- **Phase 1:** UIST paper (20-30 participants)
- **Methodology:** Within-subjects, counterbalanced
- **Metrics:** Time-on-task, code complexity, cognitive load
- **IRB:** Required for human subjects research

### Continuous Integration
- **Platform:** GitHub Actions
- **Triggers:** Pull request, merge to main
- **Checks:** Build, unit tests, integration tests
- **Artifacts:** Test reports, performance benchmarks

---

## Risk Mitigation Strategies

### Performance Degradation Risk (HIGH)
**Mitigation:**
- Continuous benchmarking on every PR
- Performance regression alerts
- Feature flags for expensive features
- Regular profiling and optimization sprints

### Scope Creep Risk (HIGH)
**Mitigation:**
- Strict prioritization (MoSCoW: Must/Should/Could/Won't)
- Fixed feature freeze dates
- Defer non-critical features to 2.1/2.2 releases
- Weekly scope reviews

### User Study Recruitment Risk (MEDIUM)
**Mitigation:**
- Early recruitment (2 months ahead)
- Competitive compensation ($50-100 per participant)
- Flexible scheduling (evenings/weekends)
- Online remote option
- Backup plan: smaller N, qualitative focus

### Network Synchronization Complexity (HIGH)
**Mitigation:**
- Extensive testing (packet loss, latency, jitter)
- Reference implementations for common patterns
- Conservative defaults (opt-in advanced features)
- Clear documentation of tradeoffs
- Community feedback via beta testing

---

## Communication & Collaboration

### Weekly Sync Meetings
- **When:** Monday mornings
- **Duration:** 30-60 minutes
- **Agenda:** Progress updates, blockers, decisions needed
- **Participants:** Core team + stakeholders

### Bi-Weekly Design Reviews
- **When:** Every other Thursday
- **Duration:** 60-90 minutes
- **Agenda:** Architecture decisions, API design, performance reviews
- **Participants:** Core team + external reviewers

### Monthly Community Updates
- **Format:** Blog post + Discord announcement
- **Content:** Progress report, upcoming features, calls for feedback
- **Platform:** GitHub Discussions, Discord

### Documentation Updates
- **Frequency:** Continuous (update as we go)
- **Review:** Before each release
- **Responsibility:** Dedicated technical writer (part-time)

---

## Quick Resume

### To Continue Implementation:
1. **Read this context file** to understand current state
2. **Review master plan** for overall strategy
3. **Check tasks file** for next actionable items
4. **Consult phase-specific plans** for detailed implementation

### Current Focus:
- Creating detailed phase plans (in progress)
- Setting up project infrastructure
- Preparing for Phase 1 (UIST paper) start

### What's Blocked:
- None currently

### What Needs Decision:
- Approval of master plan
- Budget allocation
- Timeline confirmation
- Team staffing

---

## Key Contacts

### Research Team
- **PI:** [To be filled - likely Steven Feiner]
- **Lead Developer:** [To be filled - likely Carmine Elvezio]
- **Lab:** Columbia University CGUI Lab

### External Collaborators
- Unity Technologies (potential partnership)
- Photon/PUN team (networking integration)
- XR developer community (beta testers)

---

## Resources & References

### Academic Papers
1. **Mercury: A Messaging Framework for Modular UI Components**
   - Elvezio, Sukan, Feiner (CHI 2018)
   - Introduces Mercury protocol and architecture

2. **An XR GUI for Visualizing Messages in ECS Architectures**
   - Recent XR visualization work
   - Foundation for Phase 4 dev tools

3. **Dissertation Chapters**
   - Chapters 1-3 provide extensive background
   - Use cases and validation

### External Resources
- Unity ECS Documentation: https://docs.unity3d.com/Packages/com.unity.entities@latest
- Observer Pattern: https://refactoring.guru/design-patterns/observer
- Component-Based Architecture: https://gameprogrammingpatterns.com/component.html

### Code Repositories
- **Main Repo:** https://github.com/ColumbiaCGUI/MercuryMessaging
- **Unity Asset Store:** [To be published]
- **Documentation Site:** [To be created]

---

**Document Version:** 1.0
**Status:** Active Development
**Next Review:** Weekly during active development
**Maintained By:** Mercury Development Team
