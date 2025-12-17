# Wiki Drafts

This folder contains draft versions of wiki tutorials and pages before they are pushed to the GitHub wiki.

## Folder Structure

```
dev/wiki-drafts/
├── tutorials/          # Tutorial markdown files (tutorial-01-*.md to tutorial-14-*.md)
├── pages/              # Index and reference pages
│   ├── home.md         # Wiki homepage
│   ├── tutorials-index.md  # Tutorial listing
│   └── troubleshooting.md  # Debugging guide
└── README.md           # This file
```

## Pushing to Wiki

### Clone the Wiki Repository

```bash
# Clone the wiki (separate from the main repo)
git clone https://github.com/ColumbiaCGUI/MercuryMessaging.wiki.git

# Or if already cloned, pull latest
cd MercuryMessaging.wiki && git pull
```

### Copy Files to Wiki

After review and approval, copy the draft files to the wiki repository:

```bash
# From the wiki repo root
cp ../MercuryMessaging/dev/wiki-drafts/tutorials/*.md ./
cp ../MercuryMessaging/dev/wiki-drafts/pages/home.md ./Home.md
cp ../MercuryMessaging/dev/wiki-drafts/pages/tutorials-index.md ./Tutorials.md
cp ../MercuryMessaging/dev/wiki-drafts/pages/troubleshooting.md ./Troubleshooting.md
```

### Commit and Push

```bash
cd MercuryMessaging.wiki
git add .
git commit -m "Update tutorials 1-14"
git push
```

## File Naming Convention

Wiki files use GitHub wiki naming conventions:
- `Home.md` - Main wiki homepage
- `Tutorial-1:-Introduction.md` - Tutorial pages (spaces become hyphens in filenames)
- Sidebar links auto-generate from page names

## Current Tutorials (New Numbering)

| # | Title | Draft File | Wiki Page |
|---|-------|------------|-----------|
| 1 | Introduction to MercuryMessaging | `tutorial-01-introduction.md` | Tutorial-1:-Introduction |
| 2 | Basic Routing | `tutorial-02-basic-routing.md` | Tutorial-2:-Basic-Routing |
| 3 | Creating Custom Responders | `tutorial-03-custom-responders.md` | Tutorial-3:-Custom-Responders |
| 4 | Creating Custom Messages | `tutorial-04-custom-messages.md` | Tutorial-4:-Custom-Messages |
| 5 | Fluent DSL API | `tutorial-05-fluent-dsl.md` | Tutorial-5:-Fluent-DSL-API |
| 6 | Networking with FishNet | `tutorial-06-fishnet.md` | Tutorial-6:-FishNet-Networking |
| 7 | Networking with Photon Fusion 2 | `tutorial-07-fusion2.md` | Tutorial-7:-Fusion-2-Networking |
| 8 | Switch Nodes & FSM | `tutorial-08-switch-nodes-fsm.md` | Tutorial-8:-Switch-Nodes-FSM |
| 9 | Task Management | `tutorial-09-task-management.md` | Tutorial-9:-Task-Management |
| 10 | Application State Management | `tutorial-10-application-state.md` | Tutorial-10:-Application-State |
| 11 | Advanced Networking | `tutorial-11-advanced-networking.md` | Tutorial-11:-Advanced-Networking |
| 12 | VR Behavioral Experiment | `tutorial-12-vr-experiment.md` | Tutorial-12:-VR-Experiment |
| 13 | Spatial & Temporal Filtering (STUB) | `tutorial-13-spatial-temporal.md` | Tutorial-13:-Spatial-Temporal |
| 14 | Performance Optimization (STUB) | `tutorial-14-performance.md` | Tutorial-14:-Performance |

---

*Last Updated: 2025-12-12*
