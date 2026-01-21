#!/bin/bash
# MercuryMessaging Wiki Publisher Script
# Usage: ./publish-wiki.sh [--dry-run]

set -e

DRY_RUN=false
if [ "$1" == "--dry-run" ]; then
    DRY_RUN=true
    echo "[DRY RUN MODE]"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
WIKI_DRAFTS="$PROJECT_ROOT/dev/wiki-drafts/tutorials"
WIKI_CLONE="$PROJECT_ROOT/../MercuryMessaging.wiki"
WIKI_URL="https://github.com/ColumbiaCGUI/MercuryMessaging.wiki.git"

echo "=== MercuryMessaging Wiki Publisher ==="
echo "Project Root: $PROJECT_ROOT"
echo "Wiki Drafts: $WIKI_DRAFTS"
echo "Wiki Clone: $WIKI_CLONE"
echo ""

# Step 1: Clone or update wiki repo
if [ -d "$WIKI_CLONE" ]; then
    echo "Wiki repo exists, pulling latest..."
    if [ "$DRY_RUN" == "false" ]; then
        cd "$WIKI_CLONE" && git pull origin master && cd -
    fi
else
    echo "Cloning wiki repo..."
    if [ "$DRY_RUN" == "false" ]; then
        git clone "$WIKI_URL" "$WIKI_CLONE"
    fi
fi

# Step 2: Copy files with proper wiki naming
echo ""
echo "Copying tutorial files..."

declare -A MAPPINGS=(
    ["tutorial-01-introduction.md"]="Tutorial-1:-Introduction.md"
    ["tutorial-02-basic-routing.md"]="Tutorial-2:-Basic-Routing.md"
    ["tutorial-03-custom-responders.md"]="Tutorial-3:-Custom-Responders.md"
    ["tutorial-04-custom-messages.md"]="Tutorial-4:-Custom-Messages.md"
    ["tutorial-05-fluent-dsl.md"]="Tutorial-5:-Fluent-DSL-API.md"
    ["tutorial-06-fishnet.md"]="Tutorial-6:-FishNet-Networking.md"
    ["tutorial-07-fusion2.md"]="Tutorial-7:-Fusion-2-Networking.md"
    ["tutorial-08-switch-nodes-fsm.md"]="Tutorial-8:-Switch-Nodes-&-FSM.md"
    ["tutorial-09-task-management.md"]="Tutorial-9:-Task-Management.md"
    ["tutorial-10-application-state.md"]="Tutorial-10:-Application-State.md"
    ["tutorial-11-advanced-networking.md"]="Tutorial-11:-Advanced-Networking.md"
    ["tutorial-12-vr-experiment.md"]="Tutorial-12:-VR-Experiment.md"
)

for src in "${!MAPPINGS[@]}"; do
    dest="${MAPPINGS[$src]}"
    if [ -f "$WIKI_DRAFTS/$src" ]; then
        echo "  $src -> $dest"
        if [ "$DRY_RUN" == "false" ]; then
            cp "$WIKI_DRAFTS/$src" "$WIKI_CLONE/$dest"
        fi
    else
        echo "  [MISSING] $src"
    fi
done

# Step 3: Generate _Sidebar.md
echo ""
echo "Generating _Sidebar.md..."

if [ "$DRY_RUN" == "false" ]; then
cat > "$WIKI_CLONE/_Sidebar.md" << 'EOF'
## MercuryMessaging

* [[Home]]
* [[Getting Started]]

## Tutorials

* [[Tutorial 1: Introduction|Tutorial-1:-Introduction]]
* [[Tutorial 2: Basic Routing|Tutorial-2:-Basic-Routing]]
* [[Tutorial 3: Custom Responders|Tutorial-3:-Custom-Responders]]
* [[Tutorial 4: Custom Messages|Tutorial-4:-Custom-Messages]]
* [[Tutorial 5: Fluent DSL API|Tutorial-5:-Fluent-DSL-API]]
* [[Tutorial 6: FishNet Networking|Tutorial-6:-FishNet-Networking]]
* [[Tutorial 7: Fusion 2 Networking|Tutorial-7:-Fusion-2-Networking]]
* [[Tutorial 8: Switch Nodes & FSM|Tutorial-8:-Switch-Nodes-&-FSM]]
* [[Tutorial 9: Task Management|Tutorial-9:-Task-Management]]
* [[Tutorial 10: Application State|Tutorial-10:-Application-State]]
* [[Tutorial 11: Advanced Networking|Tutorial-11:-Advanced-Networking]]
* [[Tutorial 12: VR Experiment|Tutorial-12:-VR-Experiment]]

## Reference

* [[API Reference]]
* [[Performance]]
* [[Contributing]]
EOF
fi
echo "  Created _Sidebar.md"

# Step 4: Commit and push
echo ""
echo "Committing changes..."
if [ "$DRY_RUN" == "false" ]; then
    cd "$WIKI_CLONE"
    git add -A
    git commit -m "docs: Update tutorials for v4.0.0 release"
    echo "Pushing to origin..."
    git push origin master
    cd -
fi

echo ""
echo "=== Wiki Publishing Complete ==="
if [ "$DRY_RUN" == "true" ]; then
    echo "Run without --dry-run to execute changes"
fi
