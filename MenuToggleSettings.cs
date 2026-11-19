using System.Collections.Generic;
using UnityEditor;

namespace nur_ieren.MenuToggler
{
    /// <summary>
    /// 非表示設定の保存先。UserSettings/nur_ieren/MenuToggler.asset に保存されます
    /// （Assets外なので再インポートが発生しません）。
    /// </summary>
    [FilePath("UserSettings/nur_ieren/MenuToggler.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class MenuToggleSettings : ScriptableSingleton<MenuToggleSettings>
    {
        public List<string> hiddenPaths = new List<string>();   // 個別に隠すメニュー
        public List<string> hiddenGroups = new List<string>();  // 最上位メニューごと隠す

        public void SaveSettings()
        {
            Save(true);
        }
    }
}
