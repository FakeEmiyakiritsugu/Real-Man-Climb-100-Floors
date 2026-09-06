using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// 语言下拉选择器（挂在 TMP_Dropdown 所在物体上，或在 Inspector 里手动拖入引用）
///
/// 功能：
/// 1. 自动读取 Localization 中配置的 Available Locales，把每个语言填充成下拉选项，
///    选项显示各语言自己的名称（例如：中文（简体） / English），玩家最容易看懂；
/// 2. 玩家选中后语言【立即生效】——所有挂了 Localize String Event 的 TMP 文本
///    会自动刷新，不需要重载场景；
/// 3. 选择会保存到 PlayerPrefs，下次启动游戏自动恢复玩家上次选的语言；
/// 4. 内置各种保护：等待 Localization 初始化完成、空语言列表提示、
///    保存的语言已不存在时自动兜底，不会报错。
///
/// 使用步骤：
/// 1. 场景中创建下拉框：GameObject > UI > Dropdown - TextMeshPro；
/// 2. 把本脚本挂到该 Dropdown 物体上（或挂到别的物体后把 Dropdown 拖进
///    "UI 组件绑定 / dropdown" 字段）；
/// 3. 其余 UI 文本都通过 Localize String Event 组件绑定 String Table 的 Key；
/// 4. 运行游戏即可点开下拉选择语言。
/// </summary>
public class LanguageDropdown : MonoBehaviour
{
    [Header("UI 组件绑定")]
    [Tooltip("语言下拉框。留空则自动使用本物体身上的 TMP_Dropdown")]
    public TMP_Dropdown dropdown;

    [Header("保存设置")]
    [Tooltip("PlayerPrefs 键名，用来记住玩家上次选择的语言")]
    public string playerPrefsKey = "SelectedLanguage";

    private void Awake()
    {
        // 没手动拖引用时，依次尝试：
        // 1. 本物体身上的 TMP_Dropdown；
        // 2. 场景中名字叫 LanguageDropdown 的下拉框（方便把本脚本挂在 SettingManager 之类的管理物体上）；
        // 3. 场景中任意一个 TMP_Dropdown。
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();

        if (dropdown == null)
            dropdown = FindDropdownInScene();

        if (dropdown == null)
        {
            Debug.LogError("[LanguageDropdown] 找不到 TMP_Dropdown！请把脚本挂在下拉框物体上，或在 Inspector 里手动把下拉框拖入 dropdown 字段。", this);
            return;
        }

        // Localization 就绪前先禁用下拉框，防止玩家点开空列表
        dropdown.interactable = false;
        dropdown.ClearOptions();
    }

    /// <summary>在场景中查找语言下拉框：优先找名字含 LanguageDropdown 的，其次任意 TMP_Dropdown</summary>
    private static TMP_Dropdown FindDropdownInScene()
    {
        TMP_Dropdown[] all = FindObjectsOfType<TMP_Dropdown>(true);
        if (all == null || all.Length == 0)
            return null;

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name.Contains("LanguageDropdown"))
                return all[i];
        }

        // 没有命名匹配的就用第一个（请确保场景中语言下拉是唯一/首个 TMP_Dropdown，
        // 否则请手动拖引用，避免抓错对象）
        return all[0];
    }

    private IEnumerator Start()
    {
        if (dropdown == null)
            yield break;

        // 1. 等待 Localization 系统初始化完成（首次进入场景时是异步的）
        yield return LocalizationSettings.InitializationOperation;

        List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
        if (locales == null || locales.Count == 0)
        {
            Debug.LogWarning("[LanguageDropdown] Available Locales 为空，请先到 Project Settings > Localization 中添加语言。", this);
            yield break;
        }

        // 2. 决定当前语言：优先恢复玩家上次保存的；没有记录则用当前语言；再不行就取第一个
        Locale current = FindSavedLocale(locales);
        if (current == null)
            current = LocalizationSettings.SelectedLocale;
        if (current == null || !locales.Contains(current))
            current = locales[0];

        // 3. 立刻应用该语言，保证一进场景 UI 文本就是玩家上次选的语言
        if (LocalizationSettings.SelectedLocale != current)
            LocalizationSettings.SelectedLocale = current;

        // 4. 把全部可用语言填进下拉框，并默认选中当前语言
        int selectedIndex = 0;
        var optionData = new List<TMP_Dropdown.OptionData>(locales.Count);
        for (int i = 0; i < locales.Count; i++)
        {
            optionData.Add(new TMP_Dropdown.OptionData(GetDisplayName(locales[i])));
            if (locales[i] == current)
                selectedIndex = i;
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(optionData);

        // SetValueWithoutNotify：设置默认选中项但不会触发切换事件
        dropdown.SetValueWithoutNotify(selectedIndex);
        dropdown.RefreshShownValue();
        dropdown.interactable = true;

        // 5. 监听玩家在下拉框中的选择
        dropdown.onValueChanged.AddListener(OnLanguageSelected);
    }

    /// <summary>玩家在下拉框里选了一项 → 即时切换</summary>
    private void OnLanguageSelected(int index)
    {
        List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
        if (locales == null || index < 0 || index >= locales.Count)
            return;

        ApplyLocale(locales[index]);
    }

    /// <summary>从 PlayerPrefs 里查找上次保存的语言；没有或已被删除则返回 null</summary>
    private Locale FindSavedLocale(List<Locale> locales)
    {
        string savedCode = PlayerPrefs.GetString(playerPrefsKey, string.Empty);
        if (string.IsNullOrEmpty(savedCode))
            return null;

        for (int i = 0; i < locales.Count; i++)
        {
            if (locales[i].Identifier.Code == savedCode)
                return locales[i];
        }
        return null;
    }

    /// <summary>切换语言：即时生效并保存到本地</summary>
    private void ApplyLocale(Locale locale)
    {
        if (locale == null)
            return;

        // 设置 SelectedLocale 后，所有 Localize String Event 会自动刷新文本
        LocalizationSettings.SelectedLocale = locale;

        // 记住玩家选择，下次启动恢复
        PlayerPrefs.SetString(playerPrefsKey, locale.Identifier.Code);
        PlayerPrefs.Save();

        Debug.Log("[LanguageDropdown] 已切换语言: " + locale.Identifier.Code);
    }

    /// <summary>
    /// 用各语言自己的名称作为显示文本（如 "中文（简体）" / "English"），
    /// 而不是代码（zh-Hans / en），玩家更容易看懂。
    /// </summary>
    private static string GetDisplayName(Locale locale)
    {
        try
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(locale.Identifier.Code);
            if (culture != null && !string.IsNullOrEmpty(culture.NativeName))
                return culture.NativeName;
        }
        catch
        {
            // 个别自定义 Locale 查不到 CultureInfo，走下面的兜底
        }

        // 兜底：显示 Locale 资产上配置的名称，再不行就直接显示语言代码
        string configuredName = locale.LocaleName;
        return string.IsNullOrEmpty(configuredName) ? locale.Identifier.Code : configuredName;
    }
}
