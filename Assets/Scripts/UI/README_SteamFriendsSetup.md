# Steam Friends Invite System Setup Guide

## Overview
This system adds an "Invite Friends" button to the LobbyPanel that opens a popup showing Steam friends who can be invited to the current lobby.

## Components Added
1. `SteamFriendsUIManager.cs` - Manages the friends popup and invitation logic
2. `FriendItem.cs` - Component for individual friend list items
3. `InvitationOverlayManager.cs` - Shows invitation overlay when receiving invites
4. `NotificationManager.cs` - Shows success/error notifications
5. Updated `LobbyUIManager.cs` - Added invite friends button functionality
6. Updated `SteamLobby.cs` - Modified to show invitation overlay instead of auto-joining

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
- **Invitation overlay** - Shows "Invitation from [Player]" when receiving invites
- **Accept/Decline buttons** - Players can choose to accept or decline invitations
- **Auto-decline timer** - Invitations auto-decline after 10 seconds
- **Notification system** - Shows success/error messages for invitations
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
### 6. In
vitation Overlay Setup
Create a new GameObject called "InvitationOverlay" with:
- Canvas Group component (for fade animations)
- Panel background with semi-transparent overlay
- TextMeshPro components:
  - "InviterNameText" - Shows inviter's name
  - "InvitationMessageText" - Shows invitation message
- Button components:
  - "AcceptButton" - Accept invitation
  - "DeclineButton" - Decline invitation
- InvitationOverlayManager component

### 7. Notification System Setup
Create a new GameObject called "NotificationPanel" with:
- Canvas Group component (for fade animations)
- Image component for background color
- TextMeshPro component for notification text
- NotificationManager component

### 8. Additional Configuration
**InvitationOverlayManager:**
- `invitationOverlay` - The main overlay GameObject
- `inviterNameText` - TextMeshPro for inviter name
- `invitationMessageText` - TextMeshPro for message
- `acceptButton` - Accept button reference
- `declineButton` - Decline button reference

**NotificationManager:**
- `notificationPanel` - The notification panel GameObject
- `notificationText` - TextMeshPro for notification message
- `notificationBackground` - Image for background color
- Configure colors for success/error/info notifications

## Invitation Flow
1. **Sending Invitations:**
   - Player A clicks "Invite Friends" in lobby
   - Selects friend and clicks "Invite"
   - Steam sends invitation to Player B
   - Player A sees success notification

2. **Receiving Invitations:**
   - Player B receives Steam invitation
   - Invitation overlay appears: "PlayerA wants you to join their lobby"
   - Player B can Accept or Decline
   - If no action taken, auto-declines after 10 seconds
   - On Accept: Joins the lobby automatically
   - On Decline: Overlay disappears

## Advanced Features
- **DontDestroyOnLoad** - Invitation overlay persists across scenes
- **Singleton pattern** - Global access to managers
- **Smooth animations** - Fade in/out effects for better UX
- **Auto-disconnect** - Safely leaves current lobby before joining new one
- **Error handling** - Graceful fallbacks if managers are missing