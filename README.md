# MenuToggler

[日本語](README.ja.md)

A Unity Editor tool for hiding and restoring menu items in the Unity menu bar.

## Concept

MenuToggler was created to make navigating the Unity Editor more convenient by reducing clutter in the menu bar.

As more tools and packages are installed, the number of menu items can increase, making it harder to find frequently used commands.

This tool allows you to hide individual menu items or entire top-level menu groups without removing the corresponding tools or modifying their files.

Hidden menu settings are saved locally and automatically reapplied when Unity rebuilds its menus.

## Overview

- **Unity Version:** 2022.3.22f1
- **Menu:** `nur_ieren > Menu Toggler`
- **Settings Location:** `UserSettings/nur_ieren/MenuToggler.asset`

## Features

- **Individual Menu Hiding** — Hides or restores individual menu items.
- **Group-Based Hiding** — Hides or restores all menu items under a top-level menu group, such as `Tools`.
- **Menu Search** — Filters menu items by their paths using a search field.
- **Menu Reload** — Rescans registered menu items to detect newly added menus.
- **Show All** — Restores all hidden menu items and clears the saved visibility settings.
- **Persistent Settings** — Saves hidden menu paths and groups locally using Unity's `ScriptableSingleton`.
- **Automatic Reapplication** — Reapplies hidden settings after assembly reloads, project changes, and returning to Edit Mode.
- **Repeated Reapplication** — Continues applying hidden settings once per second for 10 seconds after certain Unity events to catch menus rebuilt in multiple stages.
- **Protected Menu Group** — Excludes the `nur_ieren` menu group from the list and prevents it from being hidden.
- **Shortcut Normalization** — Removes shortcut key notation from menu paths when displaying and matching them.

## Requirements

- Unity 2022.3.22f1 (the version this tool was developed for)

## Installation

Copy the `Assets/nur_ieren/MenuToggler/` folder into your Unity project.

```text
Assets/
└─ nur_ieren/
   └─ MenuToggler/
      ├─ nur_ieren.MenuToggler.asmdef
      ├─ MenuToggleCore.cs
      ├─ MenuToggleSettings.cs
      └─ MenuToggleWindow.cs
```

The tool does not need to be placed inside `Assets/Editor/`. The included Editor-only assembly definition ensures that it is excluded from player builds.

## How to use

1. Open `nur_ieren > Menu Toggler` from the Unity menu bar.
2. Browse the registered menu groups.
3. Uncheck a top-level menu group to hide all its menu items, or expand the group and uncheck individual items.
4. Use the search field to filter menu items by name or path.
5. If you have added new scripts or tools, click `Reload` to refresh the menu list.
6. Click `Apply` to apply the selected visibility settings.

   - Newly hidden menu items are removed immediately.
   - If any previously hidden items are restored, Unity reloads the scripts to rebuild the menu structure.

To restore all hidden menu items and reset the settings, click `Show All`.

The `nur_ieren` menu group is excluded from the list and cannot be hidden.

## How it works

Unity does not provide an official public API for hiding registered menu items. This tool uses reflection to invoke Unity's internal `Menu.RemoveMenuItem` method.

The tool collects menu paths registered through `[MenuItem]` attributes, groups them by their top-level menu names, and removes the selected paths from the currently registered menu structure.

Since menu removal only affects the menus currently in memory, Unity may recreate them during domain reloads or other editor events.

To handle this, the tool automatically reapplies the saved settings:

- **Editor startup:** Applies all saved hidden settings after initialization.
- **Assembly reload:** Reapplies hidden settings after Unity rebuilds its assemblies and menus.
- **Project changes:** Starts the reapplication process when the project changes, such as after asset imports.
- **Returning to Edit Mode:** Reapplies hidden settings when exiting Play Mode and returning to Edit Mode.
- **Repeated application:** After these events, the tool attempts to remove hidden menu items once per second for 10 seconds to catch menus that are rebuilt asynchronously or in multiple stages.

Settings are stored in:

```text
UserSettings/nur_ieren/MenuToggler.asset
```

The settings file is stored outside `Assets/`, so saving visibility settings does not trigger asset reimports.

## Important notes

- This tool changes the visibility of Unity Editor menu items only. It does not delete or modify the tools, scripts, or assets associated with them.
- Only menus registered through `[MenuItem]` attributes are collected. Internal Unity menu items such as `File > New Scene` are not supported.
- Validation menu items, `CONTEXT/` menu items, and internal menu paths are excluded from the list.
- The `nur_ieren` menu group is protected and cannot be hidden.
- Menu removal relies on Unity's internal `Menu.RemoveMenuItem` method. Changes to Unity's internal implementation may affect compatibility with other Unity versions.
- Hidden menu items may briefly reappear after a reload or import while Unity rebuilds its menu structure.
- Menu items with shortcut notation are displayed without their shortcut keys. Not every shortcut-based menu item has been verified to disappear correctly.
- Settings are stored locally in `UserSettings/`. Consider excluding this folder from version control if you do not want to share menu visibility settings with other project users.

## Author

- nur_ieren
- vor_ges

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

[MIT License](https://opensource.org/license/mit)