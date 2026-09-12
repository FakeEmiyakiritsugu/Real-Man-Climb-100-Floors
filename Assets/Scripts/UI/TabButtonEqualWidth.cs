using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

/// <summary>
/// 设置页顶栏页签按钮的等宽 + 等字号布局组件。
///
/// 背景：
/// TMP 的 Auto Size 是每个文本独立计算的。如果多个按钮宽度相同，
/// 文本越长，TMP 就会把字号算得越小，于是出现有的字大、有的字小。
///
/// 本组件的处理思路：
/// 1. 先让所有页签文本使用同一个固定字号；
/// 2. 计算每个文本在固定字号下需要的按钮宽度；
/// 3. 取出所有按钮里最宽的那个宽度；
/// 4. 把所有按钮的 LayoutElement.preferredWidth 都设成这个最大宽度。
///
/// 最终效果：
/// - 所有页签按钮字号一致；
/// - 所有页签按钮宽度一致；
/// - 切换语言时会自动重新计算，适配当前语言下最长的文本；
/// - 以后新增页签，只要它是 Content 的子物体，就会被自动收集并参与计算。
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(HorizontalLayoutGroup))]
[RequireComponent(typeof(ContentSizeFitter))]
public class TabButtonEqualWidth : MonoBehaviour
{
    [Header("自动收集的引用（留空会自动查找）")]
    [Tooltip("页签按钮所在的父物体（通常就是挂本脚本的 Content）")]
    [SerializeField] private RectTransform content;

    [Tooltip("所有页签按钮里面的 TextMeshProUGUI。留空会自动从 Content 的子物体中查找")]
    [SerializeField] private TextMeshProUGUI[] labels;

    [Header("自动收集")]
    [Tooltip("开启后，每次刷新都会重新从 Content 子物体中收集页签文本。\n以后新增页签会自动加入计算，不需要手动改引用。")]
    public bool autoCollectLabels = true;

    [Header("字号设置")]
    [Tooltip("所有页签统一使用的固定字号。Auto Size 会被强制关闭")]
    public float fixedFontSize = 22f;

    [Header("宽度设置")]
    [Tooltip("按钮最小宽度。即使文字很短，也不会小于这个宽度")]
    public float minButtonWidth = 90f;

    [Tooltip("文字左右两侧留出的额外空间，防止文字贴边")]
    public float horizontalPadding = 24f;

    [Tooltip("按钮最大宽度。0 或负数表示不限制。\n如果限制最大宽度，超长文本建议配合 Ellipsis 或换行")]
    public float maxButtonWidth = 0f;

    [Header("运行时选项")]
    [Tooltip("切换语言后等待几帧再重算。\nLocalizedTextAutoBinder 需要一帧把本地化文字写进 TMP")]
    public int waitFramesAfterLocaleChange = 1;

    private HorizontalLayoutGroup _layoutGroup;
    private ContentSizeFitter _sizeFitter;
    private bool _isRefreshing;

    #region Unity 生命周期

    private void Reset()
    {
        // Reset 会在 Inspector 里第一次挂上组件时调用。
        // 这里自动把引用填好，减少手动拖拽。
        content = GetComponent<RectTransform>();
        CollectLabelsIfNeeded();
        ConfigureLayoutComponents();
    }

    private void Awake()
    {
        // 缓存组件，避免每次 Refresh 都 GetComponent。
        _layoutGroup = GetComponent<HorizontalLayoutGroup>();
        _sizeFitter = GetComponent<ContentSizeFitter>();

        ConfigureLayoutComponents();
        CollectLabelsIfNeeded();
    }

    private void OnEnable()
    {
        // SettingPanel 打开时，本组件才会 OnEnable。
        // 此时文本已经激活，可以正确计算 preferredWidth。
        CollectLabelsIfNeeded();

        // 监听语言切换。语言切换后 LocalizeStringEvent 会异步更新文字，
        // 所以不能立刻计算，需要等一帧。
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        // 面板刚打开时，LocalizedTextAutoBinder 的 Awake/RefreshString 也可能在同一帧稍后执行。
        // 因此这里也延迟一帧再计算，确保读到的是已经本地化后的文字。
        StopAllCoroutines();
        StartCoroutine(RefreshAfterLocaleChange());
    }

    private void OnDisable()
    {
        // 取消事件订阅，避免面板关闭后仍被回调。
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

        // 停止可能还在等待的刷新协程。
        StopAllCoroutines();
    }

    private void OnValidate()
    {
        // 只在编辑器里做基础设置校正，不在这里做完整 Refresh。
        // 因为编辑器里对象可能处于 inactive 状态，布局系统可能还没准备好。
        if (!Application.isPlaying)
        {
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            _sizeFitter = GetComponent<ContentSizeFitter>();
            ConfigureLayoutComponents();
        }
    }

    #endregion

    #region 初始化 / 自动收集

    /// <summary>
    /// 如果 Inspector 里没有手动指定 labels，就从 Content 的子物体中自动收集所有 TMP 文本。
    /// 这样以后新增页签也会自动被纳入计算，不需要改脚本。
    /// </summary>
    private void CollectLabelsIfNeeded()
    {
        if (content == null)
            content = GetComponent<RectTransform>();

        // 如果关闭了自动收集，并且 Inspector 里已经手动指定了 labels，就直接使用。
        if (!autoCollectLabels && labels != null && labels.Length > 0)
            return;

        if (content == null)
        {
            Debug.LogWarning("[TabButtonEqualWidth] 找不到 Content RectTransform，无法收集页签文本。", this);
            return;
        }

        // includeInactive = true：设置面板默认是关闭的，页签文本在层级上属于 inactive。
        TextMeshProUGUI[] allTexts = content.GetComponentsInChildren<TextMeshProUGUI>(true);

        // 只收集父级带 Button的 TMP 文本，避免把 Content 下其他说明性文字也算进来。
        // 这样以后新增一个页签按钮时，只要它也是 Content 的子物体，就会自动被发现。
        List<TextMeshProUGUI> result = new List<TextMeshProUGUI>();
        for (int i = 0; i < allTexts.Length; i++)
        {
            TextMeshProUGUI text = allTexts[i];
            if (text == null)
                continue;

            if (FindParentButton(text.transform) != null)
                result.Add(text);
        }

        labels = result.ToArray();
    }

    /// <summary>
    /// 从当前 Transform 开始向上查找 Button 组件。
    /// 注意：不能直接用 GetComponentInParent&lt;Button&gt;()，
    /// 因为 SettingPanel / 页签按钮默认是 inactive 的，
    /// 普通 GetComponentInParent 不会查 inactive 父物体。
    /// 这里手动向上遍历，可以正确找到 inactive 的按钮根物体。
    /// </summary>
    private static Button FindParentButton(Transform t)
    {
        Transform current = t;
        while (current != null)
        {
            Button button = current.GetComponent<Button>();
            if (button != null)
                return button;

            current = current.parent;
        }

        return null;
    }

    /// <summary>
    /// 强制校正 Content 上的布局组件设置。
    /// 本脚本依赖这两个设置：
    /// 1. HorizontalLayoutGroup 的 Child Control Width = true，才会使用子物体的 preferredWidth；
    /// 2. ContentSizeFitter 的 Horizontal Fit = PreferredSize，Content 才会随按钮总宽度自动变宽。
    /// </summary>
    private void ConfigureLayoutComponents()
    {
        if (_layoutGroup == null)
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();

        if (_layoutGroup != null)
        {
            _layoutGroup.childControlWidth = true;       // 允许布局系统读取按钮的 preferredWidth
            _layoutGroup.childForceExpandWidth = false;  // 不要让按钮被强行拉伸，否则宽度不再由文本决定
            _layoutGroup.childControlHeight = true;      // 高度也交给布局系统控制（按钮已有 LayoutElement.preferredHeight = 40）
            _layoutGroup.childForceExpandHeight = false;
        }

        if (_sizeFitter == null)
            _sizeFitter = GetComponent<ContentSizeFitter>();

        if (_sizeFitter != null)
        {
            _sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
    }

    #endregion

    #region 语言切换 / 刷新

    /// <summary>
    /// 语言切换回调。
    /// 这里不直接 Refresh，而是启动协程等待若干帧。
    /// 原因：LocalizedTextAutoBinder 是通过 LocalizeStringEvent 的事件把文字写进 TMP 的，
    /// 事件触发可能在语言切换的同一帧稍后发生，直接计算会读到旧文字。
    /// </summary>
    private void OnLocaleChanged(Locale locale)
    {
        StopAllCoroutines();
        StartCoroutine(RefreshAfterLocaleChange());
    }

    /// <summary>
    /// 等待若干帧后再刷新布局。
    /// </summary>
    private IEnumerator RefreshAfterLocaleChange()
    {
        int frames = Mathf.Max(0, waitFramesAfterLocaleChange);
        for (int i = 0; i < frames; i++)
            yield return null;

        RefreshEqualWidth();
    }

    /// <summary>
    /// Inspector 右键菜单里的立即刷新按钮等宽布局。
    /// 方便你在编辑器里手动测试，不用每次进 Play Mode。
    /// </summary>
    [ContextMenu("立即刷新按钮等宽布局")]
    public void RefreshEqualWidth()
    {
        if (_isRefreshing)
            return;

        _isRefreshing = true;

        try
        {
            CollectLabelsIfNeeded();

            if (labels == null || labels.Length == 0)
            {
                Debug.LogWarning("[TabButtonEqualWidth] 没有找到任何页签 TextMeshProUGUI。", this);
                return;
            }

            // 第一遍：先统一字号，然后计算每个按钮需要的宽度。
            float targetWidth = minButtonWidth;

            for (int i = 0; i < labels.Length; i++)
            {
                TextMeshProUGUI label = labels[i];
                if (label == null)
                    continue;

                ApplyFixedFontSettings(label);

                // ForceMeshUpdate(true)：即使对象处于 inactive 状态，也强制重新计算文本网格。
                // 只有重新计算后，preferredWidth 才是当前文字/字号下的准确宽度。
                label.ForceMeshUpdate(true);

                float needWidth = Mathf.Max(
                    minButtonWidth,
                    label.preferredWidth + horizontalPadding
                );

                if (needWidth > targetWidth)
                    targetWidth = needWidth;
            }

            // 可选：限制最大宽度，防止某个超长翻译把整个顶栏撑得太大。
            if (maxButtonWidth > 0f)
                targetWidth = Mathf.Min(targetWidth, maxButtonWidth);

            // 第二遍：把所有按钮的宽度统一成 targetWidth。
            for (int i = 0; i < labels.Length; i++)
            {
                TextMeshProUGUI label = labels[i];
                if (label == null)
                    continue;

                ApplyButtonWidth(label, targetWidth);
            }

            // 布局重建。
            // 先重建 Content 自身，再强制刷新 Canvas，确保 ScrollRect 也拿到新的内容宽度。
            if (content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            Canvas.ForceUpdateCanvases();
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    /// <summary>
    /// 把单个 TMP 文本设置为统一固定字号，并关闭可能造成字号不一致的 Auto Size。
    /// </summary>
    private void ApplyFixedFontSettings(TextMeshProUGUI label)
    {
        label.enableAutoSizing = false;
        label.fontSize = fixedFontSize;
        label.enableWordWrapping = false;

        // 宽度由脚本统一控制，所以文本不需要省略号；如果以后限制了 maxButtonWidth，
        // 可以把这里改成 TextOverflowModes.Ellipsis。
        label.overflowMode = TextOverflowModes.Overflow;
    }

    /// <summary>
    /// 把某个页签文本所在的按钮根物体宽度统一设置为 targetWidth。
    /// 按钮根物体必须有 LayoutElement；如果没有，脚本会自动添加一个。
    /// </summary>
    private void ApplyButtonWidth(TextMeshProUGUI label, float targetWidth)
    {
        // Text (TMP) 的父物体就是页签按钮根物体。
        RectTransform buttonRect = label.transform.parent as RectTransform;
        if (buttonRect == null)
        {
            Debug.LogWarning($"[TabButtonEqualWidth] '{label.name}' 的父物体不是 RectTransform，已跳过。", this);
            return;
        }

        LayoutElement layoutElement = buttonRect.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = buttonRect.gameObject.AddComponent<LayoutElement>();

        // minWidth 和 preferredWidth 都设为同一个值，确保最终宽度完全一致。
        layoutElement.minWidth = targetWidth;
        layoutElement.preferredWidth = targetWidth;
        layoutElement.flexibleWidth = -1f;
    }

    #endregion
}



