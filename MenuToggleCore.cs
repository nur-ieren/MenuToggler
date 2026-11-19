using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace nur_ieren.MenuToggler
{
    public static class MenuToggleCore
    {
        const string SelfRoot = "nur_ieren";

        static readonly MethodInfo s_remove = typeof(Menu).GetMethod(
            "RemoveMenuItem",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null, new[] { typeof(string) }, null);

        // 最後に適用した状態（ドメインリロードでリセット→起動時に再適用されるので設定と一致する）
        static HashSet<string> s_appliedPaths = new HashSet<string>();
        static HashSet<string> s_appliedGroups = new HashSet<string>();

        // 削除対象パスのキャッシュ（TypeCache走査は重いので ApplyHidden 時のみ作り直す）
        static List<string> s_targets = new List<string>();

        public static bool IsRemoveApiAvailable { get { return s_remove != null; } }

        /// <summary>ショートカット表記( %g #g &g _g )を除いたパス。</summary>
        public static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            return Regex.Replace(path, @"\s[%#&_^]+[^\s/]*$", "").Trim();
        }

        public static string TopLevel(string path)
        {
            int i = path.IndexOf('/');
            return i < 0 ? path : path.Substring(0, i);
        }

        static bool IsProtected(string path)
        {
            return TopLevel(path) == SelfRoot;
        }

        /// <summary>[MenuItem]で登録されているメニューを最上位ごとにまとめて返す。</summary>
        public static SortedDictionary<string, List<string>> CollectGrouped()
        {
            var set = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var method in TypeCache.GetMethodsWithAttribute<MenuItem>())
            {
                foreach (var obj in method.GetCustomAttributes(typeof(MenuItem), false))
                {
                    var attr = (MenuItem)obj;
                    if (attr.validate) continue;
                    var raw = attr.menuItem;
                    if (string.IsNullOrEmpty(raw)) continue;
                    if (raw.StartsWith("internal:", StringComparison.Ordinal)) continue;
                    if (raw.StartsWith("CONTEXT/", StringComparison.Ordinal)) continue;
                    var p = Normalize(raw);
                    if (string.IsNullOrEmpty(p) || p.IndexOf('/') < 0) continue;
                    if (IsProtected(p)) continue;
                    set.Add(p);
                }
            }

            var result = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var p in set)
            {
                var top = TopLevel(p);
                List<string> list;
                if (!result.TryGetValue(top, out list))
                {
                    list = new List<string>();
                    result[top] = list;
                }
                list.Add(p);
            }
            return result;
        }

        /// <summary>設定からスナップショットと削除対象キャッシュを作り直し、メニューを取り除く。</summary>
        public static void ApplyHidden()
        {
            var s = MenuToggleSettings.instance;
            s_appliedPaths = new HashSet<string>(s.hiddenPaths);
            s_appliedGroups = new HashSet<string>(s.hiddenGroups);

            var targets = new HashSet<string>(s.hiddenPaths);
            if (s.hiddenGroups.Count > 0)
            {
                foreach (var kv in CollectGrouped())
                {
                    if (!s.hiddenGroups.Contains(kv.Key)) continue;
                    foreach (var p in kv.Value) targets.Add(p);
                }
            }
            s_targets = targets.Where(p => !IsProtected(p)).ToList();

            Reapply();
        }

        /// <summary>キャッシュ済みの対象を取り除くだけ（軽量）。メニューが再構築された後の取りこぼし対策用。</summary>
        public static void Reapply()
        {
            if (s_remove == null) return;
            for (int i = 0; i < s_targets.Count; i++)
            {
                try { s_remove.Invoke(null, new object[] { s_targets[i] }); }
                catch (Exception) { /* 存在しないパスは無視 */ }
            }
        }

        public static bool HasTargets { get { return s_targets.Count > 0; } }

        /// <summary>「表示に戻した」項目があれば、メニュー再構築のためスクリプトリロードが必要。</summary>
        public static bool NeedsReload()
        {
            var s = MenuToggleSettings.instance;
            if (s_appliedPaths.Any(p => !s.hiddenPaths.Contains(p))) return true;
            if (s_appliedGroups.Any(g => !s.hiddenGroups.Contains(g))) return true;
            return false;
        }
    }

    [InitializeOnLoad]
    static class MenuToggleBootstrap
    {
        const double BurstSeconds = 10.0;  // きっかけ後に再適用を続ける時間
        const double BurstInterval = 1.0;  // 再適用の間隔

        static double s_burstEnd;
        static double s_nextTick;

        static MenuToggleBootstrap()
        {
            // 起動・リロード直後：フル適用
            EditorApplication.delayCall += () =>
            {
                MenuToggleCore.ApplyHidden();
                StartBurst();
            };

            // リロード完了時：メニューが作り直されるのでキャッシュ分を再適用
            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                EditorApplication.delayCall += () =>
                {
                    MenuToggleCore.Reapply();
                    StartBurst();
                };
            };

            // インポート等でプロジェクトが変わった時
            EditorApplication.projectChanged += StartBurst;

            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                    EditorApplication.delayCall += () => { MenuToggleCore.Reapply(); StartBurst(); };
            };

            EditorApplication.update += Tick;
        }

        static void StartBurst()
        {
            if (!MenuToggleCore.HasTargets) return;
            s_burstEnd = EditorApplication.timeSinceStartup + BurstSeconds;
            s_nextTick = 0;
        }

        static void Tick()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now > s_burstEnd || now < s_nextTick) return;
            s_nextTick = now + BurstInterval;
            MenuToggleCore.Reapply();
        }
    }
}
