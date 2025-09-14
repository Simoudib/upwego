# 🏷️ Player Name Display System - Setup Instructions

## 📋 What You Need to Do

### 1. **Add Setup Helper to Your Scene** (Easiest Method)

1. In your main game scene, create an empty GameObject
2. Name it "NameDisplayManager" 
3. Add the `NameDisplaySetupHelper` script to it
4. The system will automatically setup when you play!

### 2. **Test the System**

1. **Start your game** (Host/Server mode)
2. **Press N key** to open the name input UI
3. **Type a name** and click "Confirm" (or click "Random" for a random name)
4. **Your name should appear above your character**
5. **Other players will see your name** and you'll see theirs

### 3. **Verify Everything Works**

✅ **Names appear above players**  
✅ **Names always face the camera** (billboard effect)  
✅ **Names fade out at distance**  
✅ **Names sync across network** (other players see your name)  
✅ **Local player name is hidden** (you don't see your own name)  
✅ **Press N to change name anytime**  

## 🎮 Controls

- **N Key**: Open/close name input UI
- **Random Button**: Generate a random name  
- **Confirm Button**: Set your name
- **Escape**: Close UI and return to game

## ⚙️ Customization Options

If you want to customize the system, you can:

### Change the Toggle Key
```csharp
// In PlayerNameInputUI component
public KeyCode toggleKey = KeyCode.T; // Change to T key instead of N
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
```

### Hide Own Name Setting
```csharp
// In PlayerNameDisplay component
public bool hideOwnName = false; // Set to false to see your own name
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
