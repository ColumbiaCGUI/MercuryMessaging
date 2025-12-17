# MercuryMessaging Wiki Publisher Script
# Usage: .\publish-wiki.ps1 [-DryRun]
#
# This script automates copying tutorial drafts to the GitHub wiki repo

param(
    [switch]$DryRun = $false,
    [string]$WikiRepoUrl = "https://github.com/ColumbiaCGUI/MercuryMessaging.wiki.git"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$WikiDraftsPath = Join-Path $ProjectRoot "dev\wiki-drafts\tutorials"
$WikiClonePath = Join-Path $ProjectRoot "..\MercuryMessaging.wiki"

Write-Host "=== MercuryMessaging Wiki Publisher ===" -ForegroundColor Cyan
Write-Host "Project Root: $ProjectRoot"
Write-Host "Wiki Drafts: $WikiDraftsPath"
Write-Host "Wiki Clone: $WikiClonePath"
if ($DryRun) { Write-Host "[DRY RUN MODE]" -ForegroundColor Yellow }
Write-Host ""

# Step 1: Clone or update wiki repo
if (Test-Path $WikiClonePath) {
    Write-Host "Wiki repo exists, pulling latest..." -ForegroundColor Green
    if (-not $DryRun) {
        Push-Location $WikiClonePath
        git pull origin master
        Pop-Location
    }
} else {
    Write-Host "Cloning wiki repo..." -ForegroundColor Green
    if (-not $DryRun) {
        git clone $WikiRepoUrl $WikiClonePath
    }
}

# Step 2: Define file mappings (source -> wiki name)
$FileMappings = @{
    "tutorial-01-introduction.md" = "Tutorial-1:-Introduction.md"
    "tutorial-02-basic-routing.md" = "Tutorial-2:-Basic-Routing.md"
    "tutorial-03-custom-responders.md" = "Tutorial-3:-Custom-Responders.md"
    "tutorial-04-custom-messages.md" = "Tutorial-4:-Custom-Messages.md"
    "tutorial-05-fluent-dsl-api.md" = "Tutorial-5:-Fluent-DSL-API.md"
    "tutorial-06-fishnet-networking.md" = "Tutorial-6:-FishNet-Networking.md"
    "tutorial-07-fusion2-networking.md" = "Tutorial-7:-Fusion-2-Networking.md"
    "tutorial-08-switch-nodes-fsm.md" = "Tutorial-8:-Switch-Nodes-&-FSM.md"
    "tutorial-09-task-management.md" = "Tutorial-9:-Task-Management.md"
    "tutorial-10-application-state.md" = "Tutorial-10:-Application-State.md"
    "tutorial-11-advanced-networking.md" = "Tutorial-11:-Advanced-Networking.md"
    "tutorial-12-vr-experiment.md" = "Tutorial-12:-VR-Experiment.md"
}

# Step 3: Copy files
Write-Host "`nCopying tutorial files..." -ForegroundColor Green
$CopiedCount = 0
foreach ($mapping in $FileMappings.GetEnumerator()) {
    $SourceFile = Join-Path $WikiDraftsPath $mapping.Key
    $DestFile = Join-Path $WikiClonePath $mapping.Value

    if (Test-Path $SourceFile) {
        Write-Host "  $($mapping.Key) -> $($mapping.Value)"
        if (-not $DryRun) {
            Copy-Item $SourceFile $DestFile -Force
        }
        $CopiedCount++
    } else {
        Write-Host "  [MISSING] $($mapping.Key)" -ForegroundColor Yellow
    }
}
Write-Host "Copied $CopiedCount files"

# Step 4: Generate _Sidebar.md
Write-Host "`nGenerating _Sidebar.md..." -ForegroundColor Green
$SidebarContent = @"
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
"@

$SidebarPath = Join-Path $WikiClonePath "_Sidebar.md"
if (-not $DryRun) {
    $SidebarContent | Out-File -FilePath $SidebarPath -Encoding utf8
}
Write-Host "  Created _Sidebar.md"

# Step 5: Commit and push
Write-Host "`nCommitting changes..." -ForegroundColor Green
if (-not $DryRun) {
    Push-Location $WikiClonePath
    git add -A
    $CommitMsg = "docs: Update tutorials for v4.0.0 release"
    git commit -m $CommitMsg
    Write-Host "Pushing to origin..." -ForegroundColor Green
    git push origin master
    Pop-Location
}

Write-Host "`n=== Wiki Publishing Complete ===" -ForegroundColor Cyan
if ($DryRun) {
    Write-Host "Run without -DryRun to execute changes" -ForegroundColor Yellow
}
