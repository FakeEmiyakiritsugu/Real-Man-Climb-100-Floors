using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Components;

/// <summary>
/// 一键把场景里已写好中文文案的 TMP 文本绑定到 MainMenuTable 的翻译 Key：
/// 1. 给文本所在物体添加 LocalizeStringEvent（没有的话），并把 StringReference
///    指向 MainMenuTable 的对应 Key（按中文文案反查 Key）；
/// 2. 添加 LocalizedTextAutoBinder 桥接组件——由它在运行时把本地化文本写进
///    TMP（解决之前"切换语言但文本不刷新"的问题：LocalizeStringEvent 的
///    OnUpdateString 事件里没有绑定任何文本）。
///
/// 用法：菜单 Tools > Localization > Bind MainMenu Texts，对当前场景执行。
/// 每次执行都会以 Key 为准重建引用，可放心重复运行。
/// </summary>
public static class BindLocalizedTextsTool
{
    private const string TableName = "MainMenuTable";

    /// <summary>MainMenuTable(zh-Hans) 里的中文文案 -> Key 映射（文案与 MainMenu 场景实际 UI 文本一致）</summary>
    private static readonly Dictionary<string, string> ZhTextToKey = new Dictionary<string, string>
    {
        { "开始游戏", "Start_New_Game" },
        { "继续游戏", "Load_Game" },
        { "游戏说明", "Game_Guide" },
        { "结束游戏", "Exit_Game" },
        { "游戏设置", "Game_Settings" },
        { "选择语言", "Choose_Language" },
    };

    [MenuItem("Tools/Localization/Bind MainMenu Texts")]
    public static void BindTexts()
    {
        int bound = 0;

        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        for (int i = 0; i < allTexts.Length; i++)
        {
            TextMeshProUGUI tmp = allTexts[i];
            if (tmp == null)
                continue;

            // 只处理当前场景里的物体（跳过资源包里的 Prefab 资产）
            GameObject go = tmp.gameObject;
            if (go == null || !go.scene.IsValid() || !go.scene.isLoaded)
                continue;

            if (string.IsNullOrEmpty(tmp.text))
                continue;

            if (!ZhTextToKey.TryGetValue(tmp.text.Trim(), out string key))
                continue;

            // 1. LocalizeStringEvent：没有就添加，并指向 MainMenuTable 对应 Key
            LocalizeStringEvent lse = go.GetComponent<LocalizeStringEvent>();
            if (lse == null)
                lse = go.AddComponent<LocalizeStringEvent>();
            lse.StringReference.SetReference(TableName, key);

            // 2. 文本桥接组件：运行时把本地化结果写进 TMP
            LocalizedTextAutoBinder binder = go.GetComponent<LocalizedTextAutoBinder>();
            if (binder == null)
                binder = go.AddComponent<LocalizedTextAutoBinder>();
            binder.text = tmp;
            binder.localizeEvent = lse;

            EditorUtility.SetDirty(lse);
            EditorUtility.SetDirty(binder);
            EditorUtility.SetDirty(tmp);
            bound++;
            Debug.Log($"[BindTexts] ✓ '{GetPath(go.transform)}' 文本 \"{tmp.text.Trim()}\" -> Key [{key}]");
        }

        Debug.Log($"[BindTexts] 完成：本次绑定/刷新 {bound} 个文本。请保存场景。");
    }

    /// <summary>
    /// 给"已经手动挂过 LocalizeStringEvent、但文本内容不在映射表里"的物体补上桥接组件
    /// （例如 StartText/“Keep Jumping” 这类静态文本已被用户挂过事件组件的物体）。
    /// </summary>
    [MenuItem("Tools/Localization/Add Binder To Existing Localize Events (Debug)")]
    public static void AddBinderToExistingLocalizeEvents()
    {
        int added = 0;
        LocalizeStringEvent[] allEvents = Resources.FindObjectsOfTypeAll<LocalizeStringEvent>();
        for (int i = 0; i < allEvents.Length; i++)
        {
            LocalizeStringEvent lse = allEvents[i];
            if (lse == null) continue;
            GameObject go = lse.gameObject;
            if (go == null || !go.scene.IsValid() || !go.scene.isLoaded) continue;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                Debug.Log($"[AddBinder] 跳过 '{GetPath(go.transform)}'（同一物体上没有 TextMeshProUGUI）");
                continue;
            }

            LocalizedTextAutoBinder binder = go.GetComponent<LocalizedTextAutoBinder>();
            if (binder == null)
                binder = go.AddComponent<LocalizedTextAutoBinder>();
            binder.text = tmp;
            binder.localizeEvent = lse;

            EditorUtility.SetDirty(binder);
            added++;
            Debug.Log($"[AddBinder] ✓ '{GetPath(go.transform)}'（文本 \"{tmp.text}\"）已补桥接组件");
        }
        Debug.Log($"[AddBinder] 完成：补桥接 {added} 个。请保存场景。");
    }

    [MenuItem("Tools/Localization/Dump Scene Texts (Debug)")]
    public static void DumpSceneTexts()
    {
        int count = 0;
        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        for (int i = 0; i < allTexts.Length; i++)
        {
            TextMeshProUGUI tmp = allTexts[i];
            if (tmp == null) continue;
            GameObject go = tmp.gameObject;
            if (go == null || !go.scene.IsValid() || !go.scene.isLoaded) continue;

            count++;
            Debug.Log($"[Dump] ({go.activeInHierarchy}) {GetPath(go.transform)} :: text=\"{tmp.text}\"");
        }
        Debug.Log($"[Dump] 共 {count} 个场景 TMP 文本");
    }

    private static string GetPath(Transform t)
    {
        if (t == null) return string.Empty;
        return t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;
    }
}
