# nur_ieren Menu Toggler
[日本語](README.ja.md)

A Unity Editor tool that lets you toggle the visibility of cluttered menu items on and off.

## Requirements

- Unity 2022.3.22f1 (the version this tool was written for)

## Installation

Copy the `Assets/nur_ieren/MenuToggler/` folder into your project.

```
Assets/
└─ nur_ieren/
   └─ MenuToggler/
      ├─ nur_ieren.MenuToggler.asmdef   (Editor-only assembly)
      ├─ MenuToggleCore.cs
      ├─ MenuToggleSettings.cs
      └─ MenuToggleWindow.cs
```

The tool works under `Assets/nur_ieren/` and does not need `Assets/Editor/`. The Editor-only asmdef keeps it out of player builds.

## Usage

1. Open **`nur_ieren > Menu Toggler`** from the menu bar.
2. Uncheck a top-level menu (e.g. `Tools`) to hide the whole group, or expand it and uncheck individual items.
3. Use the search field to filter, and **Reload** to rescan menus after adding scripts.
4. Press **Apply**.
   - Newly hidden items disappear immediately.
   - If you restored anything, scripts are reloaded so Unity can rebuild the menus.
5. **Show All** clears every setting.

The `nur_ieren` menu itself is never listed and cannot be hidden.

## How it works

Unity has no official API for hiding menu items, so this tool calls the internal `Menu.RemoveMenuItem` via reflection.

- Removal only affects the menus currently in memory. Unity rebuilds all menus on every domain reload (e.g. after importing assets), which brings hidden items back.
- To handle this, the tool re-applies the settings after assembly reloads, project changes, and when returning to Edit Mode. After each trigger it keeps re-applying once per second for 10 seconds, to catch menus that are rebuilt in several steps.
- Settings are stored in `UserSettings/nur_ieren/MenuToggler.asset`, outside `Assets/`, so no reimport is triggered. You will usually want to keep this folder out of version control.

## Limitations

- It depends on an internal Unity method, so it may stop working in other Unity versions.
- Only menus registered with `[MenuItem]` are listed. Native engine items such as `File > New Scene` are not.
- Hidden items may briefly reappear (up to about a second) after a reload or import.
- Items with shortcut keys (e.g. `Tools/Foo %g`) are listed by their path without the shortcut. Not every such item has been verified to disappear.
