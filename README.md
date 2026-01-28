# Lilin's Additions - SCP:SL Plugin

A comprehensive plugin for SCP: Secret Laboratory that adds a point economy system, Mystery Boxes, Vending Machines, and various gameplay enhancements.

## Overview

Lilin's Additions is a feature-rich plugin built on the Exiled framework that introduces custom content systems including an integrated point economy, interactive world objects, and various gameplay mechanics. The plugin enhances gameplay with Mystery Boxes, Vending Machines, and utility systems.

## Main Features

### 💰 Point Economy System

Integrated economy with multiple features:

- **Point Earning:** Players start with 400 points, earn 200 for kills, 100 over time
- **Mystery Boxes:** Cost 800 points, spawn random items
- **Vending Machines:** Cost 200 points, dispense consumables
- **Credit Cards:** Drop from dead players containing their points

### 🎁 Mystery Box System

Interactive world objects that provide randomized rewards:

- Spawn in predefined locations across all zones 
- Weighted random selection system
- Animated opening with sound effects
- Single-use objects that are consumed after interaction

### 🍬 Vending Machine System

Strategic dispensers for consumable items:

- Limited usage (10 uses before self-destruct)
- Spawn in strategic locations
- Usage tracking per machine
- Audio feedback and visual effects

### 🔧 Additional Features

- **Hidden Coins:** Collectible objects worth 1500 points
- **Random Guard Spawn:** Teleports guards to random rooms
- **SCP-914 Teleport:** Special interactions with SCP-914
- **Anti-SCP Suicide:** Prevents SCPs from killing themselves via void damage
- **Credit Card Drops:** Players drop credit cards containing their points on death

### Custom Roles

Adds a handful of custom roles.
> [!WARNING]
> Custom Roles is a seperate plugin.
> 
> Custom Roles uses [Snivy's Ultimate Plugin Package](https://github.com/SnivyFilms/SnivysUltimatePackage) as a dependency. You will need to have `VVUP.Base` and `VVUP.CR` present to have this Custom Role plugin to load.

## Configuration

The plugin uses a comprehensive config system with the following main sections:

### Feature Toggles
```csharp
public bool EnableFortunaFizz { get; set; } = true;     // Vending Machines
public bool EnableMysteryBox { get; set; } = true;      // Mystery Boxes
public bool EnableHiddenCoins { get; set; } = false;    // Credit Cards
public bool EnableRandomGuardSpawn { get; set; } = false;
public bool Enable914Teleport { get; set; } = false;
public bool EnableCreditCardDrop { get; set; } = false;
public bool EnableAntiSCPSuicide { get; set; } = false;
```

### Economy Settings
- Starting points, kill rewards, time bonuses
- Mystery Box and Vending Machine costs
- Usage limits and spawn counts

### Spawn Configuration
- Predefined spawn locations for Mystery Boxes, Vending Machines, and Coins
- Configurable maximum counts for each object type
- Zone-specific placement rules

## Commands

### Points Management
Available to Remote Admin users:

```
points (get|add|set|remove) <player> [amount]
```

Examples:
- `points get PlayerName` - Show current points
- `points add PlayerName 500` - Add 500 points
- `points set PlayerName 1000` - Set to 1000 points
- `points remove PlayerName 200` - Remove 200 points

### Point System Integration

- Points are tracked in `PlayerHandler.PlayerPoints` dictionary
- Display via RueI HUD system for eligible players
- Automatic cleanup when players leave or change roles
- Persistent across rounds while players remain connected

## Notes

- All custom items are distributed through the Mystery Box and Vending Machine systems
- Point display is shown to eligible players (ClassD, Scientists, MTF, CI)
- The plugin includes comprehensive logging and error handling
- Schematic spawning system handles world object placement
- Audio system supports custom sound files for enhanced experience
- Features are modular and can be enabled/disabled independently

The plugin is designed to be highly configurable and modular, allowing server administrators to enable/disable specific features based on their server's needs.
