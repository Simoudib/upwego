# 🏷️ Complete Player Name Display Setup Guide

## 📋 Step-by-Step Setup Instructions

### **STEP 1: Add PlayerNameDisplay to Your MainPlayer Prefab**

1. **Open your MainPlayer prefab** in the Unity Inspector
2. **Click "Add Component"** at the bottom of the Inspector
3. **Search for "PlayerNameDisplay"** and add it
4. **The component will appear with these fields to configure:**

#### **PlayerNameDisplay Component Fields:**

```
📁 Name Display Settings:
├── Name Canvas: (Leave empty - auto-created)
├── Name Text: (Leave empty - auto-created)  
├── Height Offset: 2.5 (How high above player)
├── Max Distance: 50 (Max visibility distance)
└── Hide Own Name: ✓ (Hide local player's name)

📁 Visual Settings:
├── Default Name Color: White
├── Local Player Color: Cyan
├── Carried Player Color: Yellow
├── Fade Distance: 30 (Distance to start fading)
├── Distance Fade Curve: Linear (0,1) to (1,0)
└── Font Size: 24 (MAKE THIS SMALLER - try 16-20)

📁 Auto Setup:
├── Auto Create UI: ✓ (Creates UI automatically)
└── Fallback Font: (Leave empty)
```

### **STEP 2: Configure the Font Size**

1. **In the PlayerNameDisplay component**, find the **"Font Size"** field
2. **Change it from 24 to 16** (or whatever size you prefer)
3. **This controls how big the names appear above players**

### **STEP 3: Add Steam Integration to Your Scene**

1. **Create an empty GameObject** in your scene
2. **Name it "SteamPlayerNameManager"**
3. **Add the `SteamPlayerNameManager` component** to it
4. **This will automatically detect Steam names using your existing Steam integration**

### **STEP 4: Test the System**

1. **Start your game** (Host/Server mode)
2. **Steam names should automatically appear** above all players
3. **Names will face the camera** (billboard effect)
4. **Names will fade out at distance**

## 🎯 **What Each Field Does:**

### **Name Display Settings:**
- **Height Offset (2.5)**: How high above the player the name appears
- **Max Distance (50)**: Maximum distance to show names (performance)
- **Hide Own Name (✓)**: Whether to hide your own name above your character

### **Visual Settings:**
- **Default Name Color (White)**: Color for normal players
- **Local Player Color (Cyan)**: Color for your own name (if not hidden)
- **Carried Player Color (Yellow)**: Color for players being carried
- **Font Size (24)**: **IMPORTANT: Make this smaller (16-20)**
- **Fade Distance (30)**: Distance where names start fading out

### **Auto Setup:**
- **Auto Create UI (✓)**: Automatically creates the name display UI
- **Fallback Font**: Font to use if no other font is available

## 🔧 **Advanced Configuration:**

### **To Make Names Smaller:**
1. Select your MainPlayer prefab
2. Find PlayerNameDisplay component
3. Change "Font Size" from 24 to 16 (or smaller)
4. Save the prefab

### **To Change Name Colors:**
1. Select your MainPlayer prefab
2. Find PlayerNameDisplay component
3. Change the color fields in "Visual Settings"
4. Save the prefab

### **To Adjust Name Height:**
1. Select your MainPlayer prefab
2. Find PlayerNameDisplay component
3. Change "Height Offset" (try 3.0 for higher, 2.0 for lower)
4. Save the prefab

## 🎮 **How It Works:**

1. **Steam Integration**: Uses your existing `SteamFriends.GetFriendPersonaName()` calls
2. **Automatic Setup**: PlayerNameDisplay component creates its own UI
3. **Billboard Effect**: Names always face the camera for easy reading
4. **Network Sync**: Names automatically sync across all players
5. **Performance**: Names fade out at distance to maintain performance

## ✅ **Verification Checklist:**

- [ ] PlayerNameDisplay component added to MainPlayer prefab
- [ ] Font Size set to 16-20 (not 24)
- [ ] SteamPlayerNameManager added to scene
- [ ] Game starts without errors
- [ ] Steam names appear above players
- [ ] Names face the camera (billboard effect)
- [ ] Names fade out at distance
- [ ] Names sync across network

## 🐛 **Troubleshooting:**

**Names not appearing?**
- Check that PlayerNameDisplay component is on MainPlayer prefab
- Check that SteamPlayerNameManager is in your scene
- Check console for Steam initialization messages

**Names too big?**
- Change Font Size from 24 to 16 in PlayerNameDisplay component

**Names not facing camera?**
- Make sure there's an active camera with "MainCamera" tag
- Check console for camera detection messages

**Steam names not working?**
- Check that Steam is initialized in your game
- Check console for Steam integration messages

## 🎉 **You're Done!**

Once you complete these steps, you'll have:
- ✅ Steam names above all players
- ✅ Perfect billboard effect (names face camera)
- ✅ Controllable font size
- ✅ Network synchronization
- ✅ Distance-based fading
- ✅ Integration with your existing Steam system

**The system will work automatically with your existing friend invite system!** 🚀

