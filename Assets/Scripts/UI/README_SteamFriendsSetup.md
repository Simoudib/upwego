# Steam Friends Invite System Setup Guide

## Overview
This system adds an "Invite Friends" button to the LobbyPanel that opens a popup showing Steam friends who can be invited to the current lobby.

## Components Added
1. `SteamFriendsUIManager.cs` - Manages the friends popup and invitation logic
2. `FriendItem.cs` - Component for individual friend list items
3. Updated `LobbyUIManager.cs` - Added invite friends button functionality

## Unity Setup Instructions

### 1. LobbyPanel Setup
In your LobbyPanel GameObject:
1. Add an "Invite Friends" button
2. In the LobbyUIManager component, assign:
   - `inviteFriendsButton` - Reference to the new button
   - `steamFriendsUIManager` - Reference to the SteamFriendsUIManager component

### 2. Friends Panel Setup
Create a new GameObject called "FriendsPanel" with:
- Panel component with PanelName = "FriendsPanel"
- Canvas Group component (optional, for fade effects)
- Panel background image
- Close button (X button)
- Scroll View for friends list
- SteamFriendsUIManager component

**Important**: Add this FriendsPanel to your PanelSwapper's panels list in the inspector.

### 3. Friends List Structure
Inside the Scroll View's Content:
- Create a Vertical Layout Group
- Set appropriate spacing and padding

### 4. Friend Item Prefab
Create a prefab called "FriendItemPrefab" with:
- Horizontal Layout Group
- TextMeshPro components:
  - "NameText" - Friend's name
  - "StatusText" - Online status
- Button component:
  - "InviteButton" - Invite button
- FriendItem component attached

### 5. SteamFriendsUIManager Configuration
Assign in the inspector:
- `friendsPopup` - The main FriendsPanel GameObject
- `friendsListParent` - The Content GameObject of the Scroll View
- `friendItemPrefab` - The FriendItemPrefab
- `closeButton` - The close button
- `panelSwapper` - Reference to your PanelSwapper component

## Features
- Shows only online Steam friends
- Displays friend status (Online, Busy, Away, etc.)
- One-click invitation to current lobby
- Button feedback (shows "Invited" after clicking)
- Automatic cleanup when popup is closed

## Usage
1. Player clicks "Invite Friends" button in lobby
2. Popup opens showing online Steam friends
3. Player clicks "Invite" next to friend's name
4. Steam invitation is sent to the friend
5. Friend receives Steam notification to join lobby

## Notes
- Requires Steam to be initialized and running
- Only works when player is in an active lobby
- Friends must be online to appear in the list
- Uses Steam's built-in invitation system