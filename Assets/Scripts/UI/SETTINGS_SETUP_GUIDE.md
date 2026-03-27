# Settings System - Unity Setup Guide

This guide will help you set up the settings system in your Unity scenes. Follow these steps for both MainMenu and GameplayScene.

## Part 1: MainMenu Scene Setup

### Step 1: Create SettingsManager GameObject
1. Open the **MainMenu** scene
2. Create an empty GameObject: `GameObject > Create Empty`
3. Name it: `SettingsManager`
4. Add the `SettingsManager` component to it
5. This GameObject will automatically persist across scenes (DontDestroyOnLoad)

### Step 2: Create Lobby Music GameObject
1. Create an empty GameObject: `GameObject > Create Empty`
2. Name it: `LobbyMusicManager`
3. Add the `MusicManager` component to it
4. In the Inspector, configure:
   - **Music Type**: Lobby
   - **Music Clip**: Drag `Assets/Audio/BackgroundMusic.mp3` here
   - **Target Scene Name**: `MainMenu`

### Step 3: Create Settings Panel UI

#### Create the Panel Container
1. In your Canvas, create a Panel: `Right-click Canvas > UI > Panel`
2. Name it: `SettingsPanel`
3. Configure the Panel:
   - Set anchor to fill the parent (stretch both ways)
   - Background color: Semi-transparent dark (e.g., RGBA: 0, 0, 0, 200)
4. Add the `SettingsPanelController` script to this panel
5. **Set the panel to inactive** (uncheck the checkbox at the top of Inspector)

#### Create Title Text
1. Inside SettingsPanel, create: `UI > Text - TextMeshPro`
2. Name it: `TitleText`
3. Configure:
   - Text: "Settings"
   - Font Size: 48
   - Alignment: Center
   - Position it at the top of the panel

#### Create Mouse Sensitivity Controls
1. Inside SettingsPanel, create: `UI > Slider`
2. Name it: `MouseSensitivitySlider`
3. Configure:
   - Min Value: 0.5
   - Max Value: 5
   - Value: 2
   - Whole Numbers: OFF
4. Create a label (TextMeshPro): `MouseSensitivityLabel`
   - Text: "Mouse Sensitivity"
   - Position it above the slider
5. Create a value display (TextMeshPro): `MouseSensitivityValue`
   - Text: "2.00"
   - Position it to the right of the slider

#### Create Lobby Music Volume Controls
1. Inside SettingsPanel, create: `UI > Slider`
2. Name it: `LobbyMusicVolumeSlider`
3. Configure:
   - Min Value: 0
   - Max Value: 1
   - Value: 0.7
   - Whole Numbers: OFF
4. Create a label (TextMeshPro): `LobbyMusicVolumeLabel`
   - Text: "Lobby Music Volume"
   - Position it above the slider
5. Create a value display (TextMeshPro): `LobbyMusicVolumeValue`
   - Text: "70%"
   - Position it to the right of the slider

#### Create Game Music Volume Controls
1. Inside SettingsPanel, create: `UI > Slider`
2. Name it: `GameMusicVolumeSlider`
3. Configure:
   - Min Value: 0
   - Max Value: 1
   - Value: 0.7
   - Whole Numbers: OFF
4. Create a label (TextMeshPro): `GameMusicVolumeLabel`
   - Text: "Game Music Volume"
   - Position it above the slider
5. Create a value display (TextMeshPro): `GameMusicVolumeValue`
   - Text: "70%"
   - Position it to the right of the slider

#### Create Close Button
1. Inside SettingsPanel, create: `UI > Button - TextMeshPro`
2. Name it: `CloseButton`
3. Configure:
   - Button text: "Close" or "Back"
   - Position it at the bottom of the panel
4. In the Inspector, under Button > OnClick():
   - Click the `+` button
   - Drag the `SettingsPanel` GameObject into the object field
   - Select: `SettingsPanelController > CloseSettings()`

#### Wire Up the SettingsPanelController
1. Select the `SettingsPanel` GameObject
2. In the `SettingsPanelController` component, drag and drop:
   - **Mouse Sensitivity Slider**: `MouseSensitivitySlider`
   - **Mouse Sensitivity Value Text**: `MouseSensitivityValue`
   - **Lobby Music Volume Slider**: `LobbyMusicVolumeSlider`
   - **Lobby Music Volume Value Text**: `LobbyMusicVolumeValue`
   - **Game Music Volume Slider**: `GameMusicVolumeSlider`
   - **Game Music Volume Value Text**: `GameMusicVolumeValue`

### Step 4: Add Settings Button to Main Menu
1. Find your existing main menu buttons (Host Game, Join Game, etc.)
2. Duplicate one of the existing buttons
3. Rename it: `SettingsButton`
4. Change the button text to: "Settings"
5. Position it appropriately with your other menu buttons
6. In the Inspector, under Button > OnClick():
   - Clear existing events
   - Click the `+` button
   - Drag the `SettingsPanel` GameObject into the object field
   - Select: `GameObject > SetActive(bool)`
   - Check the checkbox to pass `true`

**Alternative if using PanelSwapper:**
1. Add your `SettingsPanel` to the PanelSwapper's panels list
2. Set the Panel Name to: "Settings"
3. In SettingsButton > OnClick():
   - Call `PanelSwapper > SwapPanel(string)`
   - Enter "Settings" as the parameter

---

## Part 2: GameplayScene Setup

### Step 1: Create Game Music GameObject (Placeholder)
1. Open the **GameplayScene** scene
2. Create an empty GameObject: `GameObject > Create Empty`
3. Name it: `GameMusicManager`
4. Add the `MusicManager` component to it
5. In the Inspector, configure:
   - **Music Type**: Game
   - **Music Clip**: Leave empty for now (or add a placeholder track)
   - **Target Scene Name**: `GameplayScene`

### Step 2: Create Pause Menu GameObject
1. In your Canvas, create an empty GameObject: `GameObject > Create Empty`
2. Name it: `PauseMenuManager`
3. Add the `PauseMenuController` script to it
4. Configure in Inspector:
   - **Main Menu Scene Name**: `MainMenu`

### Step 3: Create Pause Menu Panel

#### Create the Pause Panel Container
1. Inside Canvas, create: `UI > Panel`
2. Name it: `PauseMenuPanel`
3. Configure:
   - Set anchor to fill the parent
   - Background color: Semi-transparent dark (e.g., RGBA: 0, 0, 0, 200)
4. **Set to inactive** initially

#### Create Title
1. Inside PauseMenuPanel, create: `UI > Text - TextMeshPro`
2. Name it: `PauseTitle`
3. Configure:
   - Text: "Paused"
   - Font Size: 48
   - Alignment: Center
   - Position at top

#### Create Resume Button
1. Inside PauseMenuPanel, create: `UI > Button - TextMeshPro`
2. Name it: `ResumeButton`
3. Configure:
   - Text: "Resume"
   - Position in center-top area
4. In Button > OnClick():
   - Drag `PauseMenuManager` into the object field
   - Select: `PauseMenuController > Resume()`

#### Create Respawn Button
1. Inside PauseMenuPanel, create: `UI > Button - TextMeshPro`
2. Name it: `RespawnButton`
3. Configure:
   - Text: "Respawn"
   - Position below Resume button
4. In Button > OnClick():
   - Drag `PauseMenuManager` into the object field
   - Select: `PauseMenuController > Respawn()`

#### Create Settings Button
1. Inside PauseMenuPanel, create: `UI > Button - TextMeshPro`
2. Name it: `SettingsButton`
3. Configure:
   - Text: "Settings"
   - Position below Respawn button
4. In Button > OnClick():
   - Drag `PauseMenuManager` into the object field
   - Select: `PauseMenuController > OpenSettings()`

#### Create Exit Button
1. Inside PauseMenuPanel, create: `UI > Button - TextMeshPro`
2. Name it: `ExitButton`
3. Configure:
   - Text: "Exit to Main Menu"
   - Position below Settings button
4. In Button > OnClick():
   - Drag `PauseMenuManager` into the object field
   - Select: `PauseMenuController > ExitToMainMenu()`

### Step 4: Create Settings Panel (Same as MainMenu)
1. Follow the same steps as MainMenu Scene's "Step 3: Create Settings Panel UI"
2. Create the panel with all sliders and controls
3. Add a Back button that calls `PauseMenuController > CloseSettings()` instead

**Quick Tip:** You can copy the SettingsPanel from MainMenu scene and paste it into GameplayScene, then just update the Back button to call `PauseMenuController.CloseSettings()`.

### Step 5: Wire Up PauseMenuController
1. Select the `PauseMenuManager` GameObject
2. In the `PauseMenuController` component:
   - **Pause Menu Panel**: Drag `PauseMenuPanel` here
   - **Settings Panel**: Drag `SettingsPanel` here
   - **Main Menu Scene Name**: Enter `MainMenu`

---

## Part 3: Testing

### Test in MainMenu:
1. Play the scene
2. Click the Settings button
3. Adjust the mouse sensitivity slider - note the value changes
4. Adjust the lobby music volume slider - music volume should change in real-time
5. Close settings and reopen - values should persist
6. Start a game to test scene transition

### Test in GameplayScene:
1. Play the scene (after connecting/hosting)
2. Press ESC - pause menu should appear
3. Game should pause (Time.timeScale = 0)
4. Click Settings - settings panel should open
5. Adjust mouse sensitivity - when you resume, camera should use new sensitivity
6. Adjust game music volume (when you add music later)
7. Press ESC again to close settings, then Resume
8. Press ESC to pause, then Exit - should return to MainMenu

### Test Persistence:
1. Change all settings to non-default values
2. Exit Unity Play Mode
3. Start Play Mode again
4. Open settings - all your changes should be saved

---

## Troubleshooting

**Music doesn't play:**
- Check that MusicManager has the audio clip assigned
- Check that Target Scene Name matches the exact scene name
- Look for errors in the Console

**Settings don't save:**
- Make sure SettingsManager is in the scene
- Check Console for errors
- PlayerPrefs are saved to registry/local files automatically

**Cursor doesn't lock/unlock properly:**
- Check that SimpleThirdPersonCamera is attached to the player
- Ensure the player has `isLocalPlayer` set to true
- Settings panel should unlock cursor when opened

**Pause menu doesn't appear:**
- Make sure PauseMenuController is in the scene
- Check that PauseMenuPanel is assigned in the Inspector
- Panels should start as inactive

---

## Optional Enhancements

### Add a Reset to Defaults Button:
1. In SettingsPanel, add a button
2. OnClick: Call `SettingsManager.Instance.ResetToDefaults()`

### Add visual polish:
- Add hover effects to buttons
- Add transitions to panels (fade in/out)
- Add icons to sliders
- Add sound effects to button clicks

### Master Volume:
- Add a master volume slider that affects all music
- Multiply individual volumes by master volume
