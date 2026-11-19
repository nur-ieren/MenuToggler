using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace nur_ieren.MenuToggler
{
    public class MenuToggleWindow : EditorWindow
    {
        SortedDictionary<string, List<string>> _groups;
        readonly Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();
        string _search = "";
        Vector2 _scroll;

        [MenuItem("nur_ieren/Menu Toggler")]
        static void Open()
        {
            var w = GetWindow<MenuToggleWindow>("Menu Toggler");
            w.minSize = new Vector2(380, 420);
            w.Show();
        }

        void OnEnable()
        {
            Refresh();
        }

        void Refresh()
        {
            _groups = MenuToggleCore.CollectGrouped();
        }

        void OnGUI()
        {
            var s = MenuToggleSettings.instance;

            if (!MenuToggleCore.IsRemoveApiAvailable)
            {
                EditorGUILayout.HelpBox(
                    "このUnityバージョンでは Menu.RemoveMenuItem が見つからず、メニューを隠せません。",
                    MessageType.Error);
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField);
                if (GUILayout.Button("再読込", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    Refresh();
                if (GUILayout.Button("全て表示", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    s.hiddenPaths.Clear();
                    s.hiddenGroups.Clear();
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var kv in _groups)
            {
                var items = kv.Value
                    .Where(p => string.IsNullOrEmpty(_search) ||
                                p.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                if (items.Count == 0) continue;

                bool groupHidden = s.hiddenGroups.Contains(kv.Key);
                bool open;
                if (!_foldouts.TryGetValue(kv.Key, out open)) open = false;

                using (new EditorGUILayout.HorizontalScope())
                {
                    open = EditorGUILayout.Foldout(open || !string.IsNullOrEmpty(_search),
                        string.Format("{0}  ({1})", kv.Key, items.Count), true);
                    bool visible = EditorGUILayout.ToggleLeft("表示", !groupHidden, GUILayout.Width(60));
                    if (visible == groupHidden)
                    {
                        if (visible) s.hiddenGroups.Remove(kv.Key);
                        else s.hiddenGroups.Add(kv.Key);
                    }
                }
                _foldouts[kv.Key] = open;

                if (!open && string.IsNullOrEmpty(_search)) continue;

                using (new EditorGUI.DisabledScope(groupHidden))
                {
                    foreach (var path in items)
                    {
                        bool hidden = s.hiddenPaths.Contains(path);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(24);
                            bool visible = EditorGUILayout.ToggleLeft(
                                path.Substring(kv.Key.Length + 1), !hidden);
                            if (visible == hidden)
                            {
                                if (visible) s.hiddenPaths.Remove(path);
                                else s.hiddenPaths.Add(path);
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.HelpBox(
                "チェックを外したメニューが非表示になります。\n" +
                "表示に戻す場合のみ、メニュー再構築のためスクリプトが再読込されます。",
                MessageType.Info);

            if (GUILayout.Button("適用", GUILayout.Height(28)))
            {
                s.SaveSettings();
                bool reload = MenuToggleCore.NeedsReload();
                MenuToggleCore.ApplyHidden();
                if (reload) EditorUtility.RequestScriptReload();
            }
        }
    }
}
