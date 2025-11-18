# Git Branch Documentation

**Last Updated:** November 18, 2025

---

## Branch Overview

This document describes the purpose and status of all active branches in the MercuryMessaging repository.

---

## Main Branches

### `master` (Production)

**Purpose:** Main production branch containing stable, tested code

**Status:** Stable

**Description:**
- Default branch for the repository
- Contains the latest stable release of MercuryMessaging
- All merges to master should be thoroughly tested
- Protected branch (requires review before merge)

**Last Major Update:** Prior to reorganization (see below for recent changes on user_study)

**Merge Strategy:**
- Merge from feature branches via pull requests
- Require code review and testing
- Squash commits for clean history (optional)

---

## Active Development Branches

### `user_study` (Current Active Branch)

**Purpose:** Traffic simulation scene development for UIST 2025 paper

**Status:** 🔨 Active Development

**Description:**
- Complex traffic simulation with 8-12 intersections
- Autonomous pedestrians with fear-factor AI
- Autonomous vehicles with recklessness meters
- Cross-intersection coordination
- User study preparation for UIST 2025 submission (deadline: April 9, 2025)

**Key Features:**
- 11 implemented scripts (TrafficLightController, Pedestrian, CarController, etc.)
- Scenario1.unity scene in `Assets/UserStudy/`
- MercuryMessaging-based message routing
- Object pooling for performance

**Recent Activity:**
- November 18, 2025: Assets reorganization completed
  - Commit `235d134` - "Major refactoring changes"
  - 29+ folders → 10 organized folders
  - Project structure improved following Unity best practices
- Earlier commits: Progressive feature additions
  - Traffic light system
  - Pedestrian/vehicle AI
  - Multi-intersection setup
  - Cross-intersection dependencies
  - Sentiment tracking

**Pending Work:**
- ⚠️ Unity verification after reorganization (CRITICAL NEXT STEP)
- 7 more intersections (currently 1 of 8 complete)
- Unity Events comparison implementation (100 hours - CRITICAL PATH)
- Performance benchmarking
- User study tasks and data collection system

**Merge to Master:**
- **When:** After UIST paper submission (post April 9, 2025) OR if UIST not pursued
- **Requires:** Full testing of reorganized structure, user study completion
- **Strategy:** Create pull request with comprehensive description

### `editor_ui`

**Purpose:** Editor UI improvements and enhancements

**Status:** Unknown (older branch)

**Description:**
- Work on improving Unity Editor integration
- Custom inspectors and editor tools
- Visual debugging enhancements

**Merge to Master:**
- **When:** After testing and code review
- **Strategy:** Standard pull request process

---

## Remote Branches

### `MercuryNetworkingUpdates`

**Purpose:** Networking-related updates and improvements

**Status:** Unknown

**Description:** Work on improving Mercury's networking capabilities (Photon integration, etc.)

### `Unity6Upgrade`

**Purpose:** Upgrading framework to Unity 6 compatibility

**Status:** Unknown

**Description:** Compatibility testing and fixes for Unity 6 (2023.2+)

### `xichen0418`

**Purpose:** Developer-specific branch (contributor: xichen)

**Status:** Unknown

### `yangb-work-dict`

**Purpose:** Developer-specific branch (contributor: yangb)

**Status:** Unknown

---

## Branch Relationships

```
master (stable production)
  ├── user_study (active - traffic simulation)
  ├── editor_ui (UI improvements)
  ├── MercuryNetworkingUpdates (networking)
  ├── Unity6Upgrade (Unity 6 compatibility)
  ├── xichen0418 (contributor branch)
  └── yangb-work-dict (contributor branch)
```

---

## Branching Strategy

### Creating a New Branch

```bash
# From master
git checkout master
git pull origin master
git checkout -b feature/your-feature-name

# From user_study (if building on that work)
git checkout user_study
git pull origin user_study
git checkout -b feature/your-feature-name
```

### Naming Conventions

- **Feature branches:** `feature/feature-name` (e.g., `feature/green-wave-coordination`)
- **Bug fixes:** `bugfix/bug-description` (e.g., `bugfix/traffic-light-timing`)
- **User work:** `username-description` (e.g., `yangb-work-dict`)
- **Version upgrades:** `UnityXUpgrade` (e.g., `Unity6Upgrade`)

### Merging Strategy

**Option 1: Pull Request (Recommended)**
```bash
# Push branch to remote
git push origin feature/your-feature-name

# Create pull request on GitHub
# Request code review
# Merge after approval
```

**Option 2: Direct Merge (Small Changes)**
```bash
# Merge feature to master
git checkout master
git pull origin master
git merge feature/your-feature-name
git push origin master
```

---

## Current Status (November 18, 2025)

### Recently Active: `user_study`

**Last Commit:** `235d134` - "Major refactoring changes" (2025-11-18)

**Changes:**
- Complete assets reorganization
- 29+ folders consolidated to 10 organized directories
- All git history preserved (used `git mv`)
- CLAUDE.md updated with new structure
- Comprehensive reorganization documentation created

**Untracked Changes:**
- `dev/mercury-improvements/` directory (strategic planning docs)
  - 5 files, ~65,000 words of strategic planning
  - Not yet committed (decision needed: commit or keep local)

**Next Steps:**
1. **CRITICAL:** Open Unity and verify reorganization
2. Test Scenario1.unity scene
3. Fix any broken references (unlikely - GUIDs preserved)
4. Continue user study development

### Inactive Branches

The following branches have not seen recent activity:
- `editor_ui`
- `MercuryNetworkingUpdates`
- `Unity6Upgrade`
- `xichen0418`
- `yangb-work-dict`

**Recommendation:** Review these branches for:
- Outdated code (may conflict with reorganization)
- Work that should be merged
- Branches that can be deleted (if work completed or abandoned)

---

## Merging `user_study` to `master`

### Prerequisites

Before merging `user_study` → `master`:

**✅ Must Complete:**
1. Unity verification after reorganization
2. All tests passing
3. No broken references or errors
4. Code review completed
5. Documentation updated

**Recommended to Complete:**
1. User study scene fully functional (8 intersections)
2. Unity Events comparison implemented
3. Performance benchmarks documented
4. UIST paper submitted (or decision made not to pursue)

### Merge Process

```bash
# 1. Ensure user_study is clean and pushed
git checkout user_study
git status  # Should be clean
git push origin user_study

# 2. Update master
git checkout master
git pull origin master

# 3. Create merge commit (preserves history)
git merge user_study --no-ff -m "Merge user_study: Traffic simulation and reorganization"

# 4. Resolve conflicts if any
# (Unlikely given reorganization was on user_study)

# 5. Test thoroughly
# Open Unity, test all scenes, run tests

# 6. Push to master
git push origin master
```

### Alternative: Pull Request

For a more controlled merge:

1. Create pull request on GitHub: `user_study` → `master`
2. Request review from team members
3. Address feedback
4. Approve and merge via GitHub interface

---

## Reorganization Impact on Branches

### What Changed (November 18, 2025)

The `user_study` branch underwent a major reorganization:

**Moved:**
- `Assets/Demo/` → `Assets/MercuryMessaging/Examples/Demo/`
- `Assets/Tutorials/` → `Assets/MercuryMessaging/Examples/Tutorials/`
- Third-party assets → `Assets/ThirdParty/`
- VR/XR config → `Assets/XRConfiguration/`
- Custom assets → `Assets/_Project/`

**Impact on Other Branches:**

- ⚠️ **High Risk:** Branches with file path dependencies may have merge conflicts
- ⚠️ **Medium Risk:** Branches modifying moved files will have conflicts
- ✅ **Low Risk:** Branches only modifying MercuryMessaging core (Protocol/, AppState/, etc.)

**Mitigation:**

When merging other branches after reorganization:

```bash
# 1. Update your feature branch from master (after user_study merged)
git checkout feature/your-feature
git fetch origin
git merge origin/master

# 2. Resolve path-related conflicts
# - Update file paths to new locations
# - Check Unity scene references
# - Verify prefab references

# 3. Test in Unity
# - Open Unity
# - Let it reimport
# - Fix any broken references
# - Test your feature still works

# 4. Commit resolved merge
git commit
```

---

## Best Practices

### Before Creating a Branch

1. **Sync with base branch**
   ```bash
   git checkout master  # or user_study
   git pull origin master
   ```

2. **Choose descriptive name** following conventions

3. **Document purpose** (in this file or in PR)

### While Working on a Branch

1. **Commit frequently** with clear messages
2. **Push regularly** to backup work
3. **Keep up to date** with base branch (rebase or merge)
4. **Test thoroughly** before merging

### Before Merging

1. **Update from base** to catch conflicts early
2. **Run all tests**
3. **Code review** (self-review minimum, peer review ideal)
4. **Update documentation** if needed
5. **Clean up** commit history if desired (squash/rebase)

---

## Emergency Procedures

### Accidentally Broke Master

```bash
# Option 1: Revert specific commit
git revert <bad-commit-hash>
git push origin master

# Option 2: Reset to previous commit (DANGEROUS - only if not pushed or coordinated)
git reset --hard <good-commit-hash>
git push --force origin master  # DANGEROUS - use with caution
```

### Lost Work on Branch

```bash
# Check reflog for lost commits
git reflog

# Restore from reflog
git checkout <lost-commit-hash>
git checkout -b recovery-branch
```

### Merge Conflict Panic

```bash
# Abort merge
git merge --abort

# Start fresh
git status  # Check current state
git fetch origin
# Try merge again more carefully
```

---

## Questions & Answers

**Q: Which branch should I base my work on?**

A: Depends on the feature:
- General framework improvements → `master`
- User study related work → `user_study`
- Ask team lead if unsure

**Q: Should I squash commits before merging?**

A: For `master`: Yes, for cleaner history (optional)
For `user_study`: No, preserve detailed history

**Q: How do I update my branch with latest master changes?**

A:
```bash
git checkout your-branch
git fetch origin
git merge origin/master
# Or use rebase for linear history:
git rebase origin/master
```

**Q: Can I delete old branches?**

A: Yes, if:
- Work is merged or abandoned
- No one is using it
- Coordinated with team

```bash
# Delete local branch
git branch -d branch-name

# Delete remote branch
git push origin --delete branch-name
```

---

## Related Documentation

- **Git Workflow:** (TBD - create if needed)
- **Code Review Process:** (TBD - create if needed)
- **Release Process:** (TBD - create if needed)

---

**Status:** Active Documentation
**Maintainer:** MercuryMessaging Development Team
**Last Updated:** November 18, 2025
