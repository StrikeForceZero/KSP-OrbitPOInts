# Changelog

## 2023-10-10 v0.6
- Added:
    - Option to Use LeftClick to toggle / RightClick for settings
    - Button for force redraw visual components on the map
- Fixed:
    - Any known performance issues when interacting with the GUI (resolves #9)
    - Various undocumented bugs
- Under the Hood:
    - Rewrote the Manager and state machine

## 2023-10-02 v0.5
- Added:
    - Persistence
    - Unlimited POIs
    - Per Body configuration
    - Various UI tweaks
    - Options menu
    - Display each POI's color
- Under the Hood:
    - Added Tests
    - Organized most of the code

## 2023-09-25 v0.4
- Added:
    - Ability to draw POIs for all bodies
    - Ability to change colors for all POIs
    - Toggle for debug logging, otherwise hides majority of the existing logging by default

## 2023-09-25 v0.3
- Added:
    - close button (resolves #7)
    - Scaling for wire spheres
- Fixed:
    - #4 Graphical artifacts appearing after quicksave load 
    - #5 Toolbar button shows in flight view when quicksave loading
    - #6 Toolbar GUI remains up when exiting map view

## 2023-09-25 v0.2.1
- Fixed #3 settings not being honored on quick-save load

## 2023-09-24 v0.2
- Fixed:
    - #1 Graphical artifacts in flight view
    - #2 Race condition leaving orphaned drawings
- Added:
    - 3 Custom POIs

## 2023-09-21 v0.1
- Initial Release
