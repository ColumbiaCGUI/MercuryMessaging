# Project Assets

This folder contains all custom project-specific assets for the MercuryMessaging research project.

## Structure

- **Scenes/** - Production scenes for the project
- **Scripts/** - All custom C# scripts organized by category:
  - Core/ - Core application logic
  - UI/ - UI-related scripts
  - VR/ - VR/XR initialization and management
  - Utilities/ - General utility scripts
  - Responders/ - Custom MercuryMessaging responders
  - TrafficLights/ - Traffic light system scripts
  - Tutorials/ - Tutorial-related scripts (from original Script folder)
- **Prefabs/** - Reusable prefab assets
  - UI/ - UI prefabs
  - Environment/ - Environment prefabs
- **Materials/** - Project-specific materials
- **Resources/** - Runtime-loadable assets
- **Settings/** - Project configuration files (input actions, etc.)

## Notes

- All custom project code should go in this folder
- This folder is prefixed with "_" to sort to the top in Unity's Project window
- For MercuryMessaging framework files, see the `MercuryMessaging/` folder in Assets root
- For third-party assets, see the `ThirdParty/` folder in Assets root
