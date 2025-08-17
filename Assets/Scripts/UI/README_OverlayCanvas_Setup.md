# Overlay Canvas Setup Guide

## 🎯 Overview
This approach creates a separate Canvas specifically for overlays (notifications, invitations, etc.) that appears on top of all other UI and is independent of the PanelSwapper system.

## 🛠️ Quick Setup Steps

### Step 1: Create Overlay Canvas
1. **Right-click in Hierarchy** → UI → Canvas
2. **Rename it** to "OverlayCanvas"
3. **Add the `OverlayCanvasManager` script** to it
4. **Set Canvas properties:**
   - Render Mode: Screen Space - Overlay
   - Sort Order: 100 (high value to appear on top)

### Step 2: Auto-Create Basic Structure
1. **Select the OverlayCanvas** GameObject
2. **Right-click the `OverlayCanvasManager` component**
3. **Select "Create Basic Overlay Structure"**
4. This will automatically create:
   - NotificationPanel with basic styling
   - InvitationOverlay with Accept/Decline buttons
   - All necessary components and references

### Step 3: Test the Setup
1. **Right-click the `OverlayCanvasManager` component**
2. **Select "Test All Overlays"**
3. You should see:
   - A green notification appear at the top
   - An invitation overlay with fake data

## 🎨 Manual Setup (Alternative)

If you prefer to create your own UI design:

### Create Notification Panel
1. **Create Empty GameObject** under OverlayCanvas → "NotificationPanel"
2. **Add Image component** for background
3. **Add TextMeshPro component** for text
4. **Position at top of screen**
5. **Assign references** in OverlayCanvasManager

### Create Invitation Overlay
1. **Create Empty GameObject** under OverlayCanvas → "InvitationOverlay"
2. **Make it full screen** (anchor to fill)
3. **Add semi-transparent background**
4. **Create child objects:**
   - InviterNameText (TextMeshPro)
   - InvitationMessageText (TextMeshPro)
   - AcceptButton (Button)
   - DeclineButton (Button)
5. **Assign references** in OverlayCanvasManager

## 🔧 Configuration

### OverlayCanvasManager Settings
- `sortingOrder`: Higher values appear on top (default: 100)
- `invitationOverlay`: Reference to your invitation overlay GameObject
- `notificationPanel`: Reference to your notification panel GameObject

### Canvas Hierarchy Example
```
OverlayCanvas (OverlayCanvasManager + PersistentNotificationManager)
├── NotificationPanel
│   └── NotificationText (TextMeshPro)
└── InvitationOverlay (InvitationOverlayManager)
    ├── Background (Image - semi-transparent)
    └── Content
        ├── InviterNameText (TextMeshPro)
        ├── InvitationMessageText (TextMeshPro)
        ├── AcceptButton (Button)
        └── DeclineButton (Button)
```

## ✅ Benefits of This Approach

1. **Independent of PanelSwapper** - Overlays work regardless of which panel is active
2. **Always on top** - High sorting order ensures visibility
3. **Persistent** - DontDestroyOnLoad keeps it across scenes
4. **Clean separation** - Game UI vs Overlay UI are separate
5. **Easy to manage** - All overlays in one place

## 🧪 Testing

### Test Notifications
- Use invitation system → Should show success/error notifications
- Right-click OverlayCanvasManager → "Test All Overlays"

### Test Invitation Overlay
- Send invitation to friend → They should see overlay
- Right-click InvitationOverlayManager → "Test Show Overlay"

## 🎯 Integration with Existing System

The system automatically falls back to PersistentNotificationManager when the regular NotificationManager is unavailable, so your existing invitation code will work without changes.

## 🔍 Troubleshooting

**Overlays not appearing:**
- Check Canvas Sort Order (should be high, like 100)
- Verify OverlayCanvas is active
- Check console for setup messages

**Notifications not working:**
- Ensure PersistentNotificationManager is on OverlayCanvas GameObject
- Check that notificationPanel reference is assigned
- Look for error messages in console

**Invitation overlay not showing:**
- Verify InvitationOverlayManager is on the overlay GameObject
- Check that all UI references are assigned
- Test with "Test Show Overlay" context menu option