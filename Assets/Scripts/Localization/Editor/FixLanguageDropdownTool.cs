using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 一键修复场景里的语言下拉框（LanguageDropdown）从"分辨率下拉框"复制带过来的残留设置：
/// 1. 把下拉框标题文字和弹出列表选项文字的字体换成项目中文字体（SourceHanSerifSC-VF SDF），
///    否则语言名"中文（简体）"等会显示成方块；
/// 2. 清空残留的占位选项（Option A / Option B / Option C）——运行时会由 LanguageDropdown.cs
///    根据 Available Locales 自动填充真实语言；
/// 3. 清空标题文字残留。
///
/// 用法：菜单 Tools > Localization > Fix Language Dropdown，对当前场景执行。
/// 该工具也可以复用（以后新增语言下拉框复制粘贴后，再跑一次即可）。
/// </summary>
public static class FixLanguageDropdownTool
{
    private const string ChineseFontPath = "Assets/Font/SourceHanSerifSC-VF SDF.asset";

    [MenuItem("Tools/Localization/Fix Language Dropdown")]
    public static void FixLanguageDropdown()
    {
        TMP_FontAsset chineseFont =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ChineseFontPath);
        if (chineseFont == null)
        {
            Debug.LogError($"[FixLanguageDropdown] 找不到中文字体资产: {ChineseFontPath}");
            return;
        }

        int fixedCount = 0;
        TMP_Dropdown[] allDropdowns = Resources.FindObjectsOfTypeAll<TMP_Dropdown>();
        for (int i = 0; i < allDropdowns.Length; i++)
        {
            TMP_Dropdown dd = allDropdowns[i];
            if (dd == null || !dd.name.Contains("LanguageDropdown"))
                continue;

            // 1. 字体换成中文字体（属性赋值会由 TMP 自动处理材质/图集联动）
            if (dd.captionText != null)
            {
                dd.captionText.font = chineseFont;
                dd.captionText.text = string.Empty; // 3. 清空标题残留，避免显示 Option A
            }

            if (dd.itemText != null)
            {
                dd.itemText.font = chineseFont;
            }

            // 2. 清空残留的占位选项
            dd.ClearOptions();
            dd.value = 0;

            EditorUtility.SetDirty(dd);
            if (dd.captionText != null)
                EditorUtility.SetDirty(dd.captionText);
            if (dd.itemText != null)
                EditorUtility.SetDirty(dd.itemText);

            fixedCount++;
            Debug.Log($"[FixLanguageDropdown] 已修复: {dd.name}（标题/选项字体 -> {chineseFont.name}，选项已清空）");
        }

        if (fixedCount == 0)
        {
            Debug.LogWarning("[FixLanguageDropdown] 当前场景中没有找到名字包含 LanguageDropdown 的 TMP_Dropdown。");
            return;
        }

        // 提示保存场景，把修复落盘
        Debug.Log($"[FixLanguageDropdown] 修复完成（共 {fixedCount} 个）。请记得保存场景（Ctrl+S）。");
    }
}
