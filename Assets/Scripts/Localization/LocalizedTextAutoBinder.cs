using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

/// <summary>
/// 文本自动本地化桥接器：挂在带 TextMeshProUGUI 与 LocalizeStringEvent 的物体上。
/// 作用：把 LocalizeStringEvent 的文本更新事件接到 TMP 文本上——
/// 语言切换（SelectedLocale 变化）时 LocalizeStringEvent 会触发更新，
/// 本组件负责把新文本写进 TMP 组件。
///
/// 为什么需要它：LocalizeStringEvent 组件本身的 OnUpdateString 是 UnityEvent，
/// 如果事件里没有持久化绑定 TMP 文本（Inspector 里不显示任何调用），
/// 切换语言时文本就不会刷新。这个组件用代码注册监听，免去手动在
/// Inspector 里接线，也避免不同 TMP 版本的 API 差异。
/// </summary>
[RequireComponent(typeof(LocalizeStringEvent))]
public class LocalizedTextAutoBinder : MonoBehaviour
{
    [Tooltip("要刷新的 TMP 文本；留空自动使用本物体上的 TextMeshProUGUI")]
    public TextMeshProUGUI text;

    [Tooltip("提供本地化文本的事件组件；留空自动使用本物体上的 LocalizeStringEvent")]
    public LocalizeStringEvent localizeEvent;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
        if (localizeEvent == null)
            localizeEvent = GetComponent<LocalizeStringEvent>();

        if (text == null || localizeEvent == null)
        {
            Debug.LogWarning("[LocalizedTextAutoBinder] 缺少 TextMeshProUGUI 或 LocalizeStringEvent，请把两者放在同一物体上。", this);
            return;
        }

        // 语言每次变化/字符串每次刷新时同步到 TMP 文本
        localizeEvent.OnUpdateString.AddListener(ApplyString);

        // 主动刷新一次，确保进入场景/面板激活时文本立刻正确
        localizeEvent.RefreshString();
    }

    private void ApplyString(string localizedValue)
    {
        if (text != null)
            text.text = localizedValue;
    }
}
