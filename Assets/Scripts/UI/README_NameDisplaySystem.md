# Player Name Display System

A comprehensive system for displaying player names above characters in multiplayer Unity games using Mirror networking.

## 🌟 Features

- **Billboard Behavior**: Names always face the camera, even when players rotate
- **Network Synchronized**: Names are automatically synced across all clients
- **Distance-Based Visibility**: Names fade out at distance and disappear when too far
- **Customizable Appearance**: Colors, fonts, and positioning can be adjusted
- **Input UI**: Simple interface for players to set their names
- **Auto Setup**: Automatically creates UI components when needed
- **Performance Optimized**: Efficient updates and distance checking

## 📁 System Components

### Core Scripts

1. **`PlayerNameDisplay.cs`** - Main component that handles name rendering and billboard behavior
2. **`PlayerNameInputUI.cs`** - UI system for players to input their names
3. **`NameDisplaySetupHelper.cs`** - Helper script for easy scene setup
4. **Enhanced `EnhancedPlayerMovement.cs`** - Integrated with player name synchronization

## 🚀 Quick Setup

### Method 1: Automatic Setup (Recommended)

1. Add `NameDisplaySetupHelper` to any GameObject in your scene
2. The system will automatically setup when players spawn
3. Players can press **N** to open the name input UI

### Method 2: Manual Setup

1. Add `PlayerNameDisplay` component to your player prefab
2. Add `PlayerNameInputUI` to a GameObject in your scene
3. The system will handle the rest automatically

## 🎮 Controls

- **N Key**: Open/close name input UI (configurable)
- **Escape**: Close name input and lock cursor back to gameplay
- **Random Button**: Generate a random name
- **Confirm Button**: Set the entered name

## ⚙️ Configuration

### PlayerNameDisplay Settings

```csharp
[Header("Name Display Settings")]
public float heightOffset = 2.5f;        // Height above player
public float maxDistance = 50f;          // Max visibility distance
public bool hideOwnName = true;          // Hide local player's name

[Header("Visual Settings")]
public Color defaultNameColor = Color.white;
public Color localPlayerColor = Color.cyan;
public Color carriedPlayerColor = Color.yellow;
public float fadeDistance = 30f;         // Distance to start fading
```

### PlayerNameInputUI Settings

```csharp
[Header("Settings")]
public int maxNameLength = 20;           // Maximum name length
public int minNameLength = 2;            // Minimum name length
public bool showOnStart = true;          // Show input on game start
public KeyCode toggleKey = KeyCode.N;    // Key to toggle input
```

## 🔧 Advanced Usage

### Setting Names Programmatically

```csharp
// Server-side only
EnhancedPlayerMovement player = GetComponent<EnhancedPlayerMovement>();
player.SetPlayerName("Custom Name");

// Client command (works from any client)
player.CmdSetPlayerName("Custom Name");
```

### Customizing Name Colors

```csharp
PlayerNameDisplay nameDisplay = GetComponent<PlayerNameDisplay>();

// Temporarily highlight a name
nameDisplay.HighlightName(Color.red, 3f); // Red for 3 seconds

// Change height offset
nameDisplay.SetHeightOffset(3f);

// Change max distance
nameDisplay.SetMaxDistance(100f);
```

### Accessing Player Names

```csharp
// Get a player's display name
string playerName = playerMovement.PlayerDisplayName;

// Check if name is set
if (!string.IsNullOrEmpty(playerName))
{
    Debug.Log($"Player name: {playerName}");
}
```

## 🎨 Visual Customization

### Name Colors

The system automatically applies different colors based on player state:

- **White**: Default players
- **Cyan**: Local player (if `hideOwnName` is false)
- **Yellow**: Players being carried
- **Custom**: Any color you set via `HighlightName()`

### Distance Fading

Names smoothly fade out as players move away:
- **0 to fadeDistance**: Full opacity
- **fadeDistance to maxDistance**: Gradual fade using `distanceFadeCurve`
- **Beyond maxDistance**: Hidden completely

### Font and Styling

The system uses TextMeshPro for high-quality text rendering:
- Bold font style by default
- Automatic outline for better readability
- Configurable font size and materials

## 🌐 Network Architecture

### SyncVar Integration

```csharp
[SyncVar(hook = nameof(OnPlayerNameChanged))]
public string playerDisplayName = "";
```

Names are synchronized using Mirror's SyncVar system, ensuring:
- Automatic synchronization to all clients
- Efficient network usage (only sends when changed)
- Reliable delivery and ordering

### Command Pattern

```csharp
[Command]
public void CmdSetPlayerName(string newName)
{
    // Server validates and sets the name
}
```

Client requests are validated on the server to prevent:
- Invalid names (too short/long, empty)
- Malicious input
- Excessive network traffic

## 🐛 Troubleshooting

### Names Not Appearing

1. **Check Camera**: Ensure there's an active camera with the "MainCamera" tag
2. **Check Components**: Verify `PlayerNameDisplay` is on the player prefab
3. **Check Canvas**: Ensure UI Canvas is properly configured

### Names Not Syncing

1. **Network Authority**: Only the server can set names via `SetPlayerName()`
2. **Use Commands**: Clients should use `CmdSetPlayerName()` to request name changes
3. **Check Logs**: Look for network synchronization messages in console

### Performance Issues

1. **Distance Checking**: Adjust `DISTANCE_CHECK_INTERVAL` for less frequent updates
2. **Max Distance**: Lower `maxDistance` to reduce processing for distant players
3. **UI Updates**: The system automatically optimizes UI updates

### Billboard Not Working

1. **Camera Reference**: Ensure `FindPlayerCamera()` finds the correct camera
2. **Update Frequency**: Billboard rotation updates every frame in `Update()`
3. **Canvas Mode**: Ensure Canvas is set to `WorldSpace` mode

## 🔮 Future Enhancements

Potential improvements that could be added:

- **Clan/Team Tags**: Display team affiliation
- **Status Icons**: Show player status (AFK, typing, etc.)
- **Health Bars**: Integrate health display with names
- **Chat Integration**: Show recent messages above players
- **Rank/Level Display**: Show player progression
- **Custom Fonts**: Per-player font customization

## 📝 Example Usage

### Basic Player Name Setup

```csharp
public class GameManager : NetworkBehaviour
{
    void Start()
    {
        // Find all players and set up names
        EnhancedPlayerMovement[] players = FindObjectsOfType<EnhancedPlayerMovement>();
        
        foreach (var player in players)
        {
            if (player.isServer)
            {
                // Set default names on server
                string defaultName = $"Player {player.netId}";
                player.SetPlayerName(defaultName);
            }
        }
    }
}
```

### Custom Name Input

```csharp
public class CustomNameSetter : MonoBehaviour
{
    public void SetCustomName()
    {
        EnhancedPlayerMovement localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            localPlayer.CmdSetPlayerName("My Custom Name");
        }
    }
    
    EnhancedPlayerMovement FindLocalPlayer()
    {
        EnhancedPlayerMovement[] players = FindObjectsOfType<EnhancedPlayerMovement>();
        return System.Array.Find(players, p => p.isLocalPlayer);
    }
}
```

## 🎯 Integration with Existing Systems

The name display system is designed to work seamlessly with:

- **Mirror Networking**: Full compatibility with Mirror's networking features
- **Animation System**: Names update colors based on carry/throw states
- **UI Systems**: Works with existing Canvas and UI systems
- **Camera Systems**: Automatically finds and works with any camera setup

---

**✅ The system is now fully implemented and ready to use!**

Simply add the components to your scene and players will be able to set and display their names with full networking support and billboard behavior.

