# 🏷️ Player Name Display System - Setup Instructions

## 📋 What You Need to Do

### 1. **Add Setup Helper to Your Scene** (Easiest Method)

1. In your main game scene, create an empty GameObject
2. Name it "NameDisplayManager" 
3. Add the `NameDisplaySetupHelper` script to it
4. The system will automatically setup when you play!

### 2. **Configure Font Size** (In Unity Inspector)

1. **Select your MainPlayer prefab**
2. **Find the PlayerNameDisplay component**
3. **Adjust the Font Size** field (default is 24, make it smaller like 16-20)
4. **Test in play mode** to see the size change

### 3. **Test the System**

1. **Start your game** (Host/Server mode)
2. **Steam names will automatically appear** above all players
3. **Names always face the camera** (billboard effect for everyone)
4. **Other players will see your Steam name** and you'll see theirs

### 4. **Verify Everything Works**

✅ **Steam names appear above players**  
✅ **Names always face the camera** (billboard effect for all players)  
✅ **Names fade out at distance**  
✅ **Names sync across network** (other players see your Steam name)  
✅ **Font size is controllable** from Unity inspector  
✅ **No manual name input needed** (uses Steam names automatically)  

## 🎮 How It Works

- **Steam Integration**: Automatically detects and uses Steam player names
- **Billboard Effect**: All player names face the camera for easy reading
- **Font Control**: Adjust font size directly in Unity inspector
- **Network Sync**: Names automatically sync across all players

## 🔧 Steam Integration (Optional)

The system automatically tries to get Steam names through:
1. **Mirror's authentication data** (works with most Steam integrations)
2. **Steamworks.NET** (if you have it integrated)
3. **Facepunch.Steamworks** (if you have it integrated)
4. **Fallback to connection ID** if Steam names aren't available

### For Better Steam Integration:
- Add `SteamNameHelper` component to your scene for enhanced Steam detection
- The system will work with or without Steam SDK integration

## ⚙️ Customization Options

If you want to customize the system, you can:

### Adjust Font Size
```csharp
// In PlayerNameDisplay component
public float fontSize = 16f; // Smaller font size (default is 24)
```

### Adjust Name Height
```csharp  
// In PlayerNameDisplay component
public float heightOffset = 3f; // Higher above player
```

### Change Colors
```csharp
// In PlayerNameDisplay component  
public Color defaultNameColor = Color.yellow; // Change default color
public Color localPlayerColor = Color.red;    // Change local player color
public Color carriedPlayerColor = Color.green; // Change carried player color
```

### Hide Own Name Setting
```csharp
// In PlayerNameDisplay component
public bool hideOwnName = false; // Set to false to see your own name
```

### Distance Settings
```csharp
// In PlayerNameDisplay component
public float maxDistance = 50f; // Maximum distance to show names
public float fadeDistance = 30f; // Distance where names start fading
```

## 🔧 Advanced Setup (Optional)

If you want more control, you can manually add components:

### To Your MainPlayer Prefab:
1. Add `PlayerNameDisplay` component
2. Configure the settings in the inspector

### To Your Scene:
1. Create a UI GameObject
2. Add `PlayerNameInputUI` component  
3. It will auto-create the UI if needed

## 🐛 Troubleshooting

**Names not showing?**
- Make sure you have a Camera with "MainCamera" tag
- Check that PlayerNameDisplay component is on your player prefab

**Names not syncing?**
- Make sure you're testing with multiple players (Host + Client)
- Check the console for networking messages

**UI not appearing?**
- Press N key to toggle the name input UI
- Check that PlayerNameInputUI component exists in your scene

**Names not facing camera?**
- The billboard effect requires an active camera
- Make sure your camera is enabled and active

## 🎉 You're Done!

The name display system is now fully integrated with your player animation and carry/throw system. Players can:

- Set custom names that appear above their characters
- See other players' names with perfect billboard behavior  
- Have names that change color based on game state (carrying, being carried)
- Experience smooth distance-based fading
- Use a simple UI to change names anytime

**Everything works seamlessly with your existing multiplayer game!** 🚀
