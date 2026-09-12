using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 一次性设置工具：把 TabButtonEqualWidth 正确挂到 MainMenu 场景的
/// Canvas/SettingPanel/TopBar/Scroll View/Viewport/Content 上。
///
/// 使用方式：
/// 菜单 Tools > UI > Setup Tab Button Equal Width
///
/// 这个脚本只负责编辑器配置，不参与游戏运行时逻辑。
/// 它的作用：
/// 1. 从之前误挂的 Text (TMP) 物体上移除 TabButtonEqualWidth，
///    以及 AddComponent 时被 [RequireComponent] 自动带上去的
///    HorizontalLayoutGroup / ContentSizeFitter；
/// 2. 在正确的 Content 物体上挂 TabButtonEqualWidth；
/// 3. 自动填写 content 和 labels 引用；
/// 4. 校正 Content 的 HorizontalLayoutGroup / ContentSizeFitter 设置；
/// 5. 调用一次 RefreshEqualWidth()，方便立刻在编辑器里看到效果。
/// </summary>
public static class TabButtonEqualWidthSetup
{
    [MenuItem("Tools/UI/Setup Tab Button Equal Width")]
    public static void Setup()
    {
        // =========================
        // 1. 找到正确的 Content 物体
        // =========================
        Transform content = FindContentTransform();

        if (content == null)
        {
            Debug.LogError("[TabButtonEqualWidthSetup] 找不到 Canvas/SettingPanel/TopBar/Scroll View/Viewport/Content。");
            return;
        }

        Debug.Log($"[TabButtonEqualWidthSetup] 找到 Content: {GetPath(content)}");

        // =========================
        // 2. 清理之前误挂到其他物体上的组件
        // =========================
        TabButtonEqualWidth[] all = Resources.FindObjectsOfTypeAll<TabButtonEqualWidth>();
        for (int i = 0; i < all.Length; i++)
        {
            TabButtonEqualWidth component = all[i];
            if (component == null)
                continue;

            GameObject go = component.gameObject;

            // 正确的目标物体不清理。
            if (go == content.gameObject)
                continue;

            // 只处理场景里的物体，不处理 Prefab 资源。
            if (!go.scene.IsValid())
                continue;

            // 只清理误挂在 Text (TMP) 子物体上的组件。
            // 如果以后你把 TabButtonEqualWidth 正常挂到别的面板上，这个工具不会误删。
            if (!go.name.StartsWith("Text (TMP)") || go.GetComponent<Button>() != null)
                continue;

            Debug.Log($"[TabButtonEqualWidthSetup] 清理误挂组件: {GetPath(go.transform)}");

            // 记录一下误挂物体上的布局组件，
            // 因为 [RequireComponent] 会把它们自动加到误挂物体上。
            HorizontalLayoutGroup wrongLayout = go.GetComponent<HorizontalLayoutGroup>();
            ContentSizeFitter wrongFitter = go.GetComponent<ContentSizeFitter>();

            // 删除本脚本组件。
            UnityEngine.Object.DestroyImmediate(component, true);

            // 删除因为 RequireComponent 被自动加上去的布局组件。
            // Text (TMP) 本来不应该有这两个组件。
            if (wrongLayout != null)
                UnityEngine.Object.DestroyImmediate(wrongLayout, true);

            if (wrongFitter != null)
                UnityEngine.Object.DestroyImmediate(wrongFitter, true);
        }

        // =========================
        // 3. 在 Content 上确保存在 TabButtonEqualWidth
        // =========================
        TabButtonEqualWidth target = content.GetComponent<TabButtonEqualWidth>();
        if (target == null)
            target = content.gameObject.AddComponent<TabButtonEqualWidth>();

        // =========================
        // 4. 用 SerializedObject 自动填写私有序列化字段
        //    content: Content 自身
        //    labels : Content 下所有 TMP 文本（包含 inactive）
        // =========================
        TextMeshProUGUI[] labels = content.GetComponentsInChildren<TextMeshProUGUI>(true);

        SerializedObject serializedObject = new SerializedObject(target);

        SerializedProperty contentProperty = serializedObject.FindProperty("content");
        if (contentProperty != null)
            contentProperty.objectReferenceValue = content;

        SerializedProperty labelsProperty = serializedObject.FindProperty("labels");
        if (labelsProperty != null)
        {
            labelsProperty.arraySize = labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                labelsProperty.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
            }
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        // =========================
        // 5. 校正布局组件设置
        // =========================
        HorizontalLayoutGroup layoutGroup = content.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandHeight = false;
            EditorUtility.SetDirty(layoutGroup);
        }

        ContentSizeFitter sizeFitter = content.GetComponent<ContentSizeFitter>();
        if (sizeFitter != null)
        {
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            EditorUtility.SetDirty(sizeFitter);
        }

        // =========================
        // 6. 立刻刷新一次等宽布局
        // =========================
        target.RefreshEqualWidth();

        // 输出每个页签的字号和按钮宽度，方便在 Console 里核验结果。
        for (int i = 0; i < labels.Length; i++)
        {
            TextMeshProUGUI label = labels[i];
            if (label == null)
                continue;

            RectTransform buttonRect = label.transform.parent as RectTransform;
            LayoutElement layoutElement = buttonRect != null ? buttonRect.GetComponent<LayoutElement>() : null;
            float width = layoutElement != null ? layoutElement.preferredWidth : -1f;

            Debug.Log($"[TabButtonEqualWidthSetup] 核验: text='{label.text}' fontSize={label.fontSize} autoSize={label.enableAutoSizing} buttonWidth={width}");
        }

        // 标记 Content 和场景为 Dirty，提示用户保存场景。
        EditorUtility.SetDirty(target);
        EditorUtility.SetDirty(content.gameObject);

        // 在 Hierarchy 里选中 Content，方便用户检查。
        Selection.activeGameObject = content.gameObject;

        Debug.Log("[TabButtonEqualWidthSetup] 设置完成。请检查 Content 上的 TabButtonEqualWidth 和 LayoutElement，然后保存场景。");
    }

    /// <summary>
    /// 在场景中查找名称层级为 Content 的物体。
    /// 不能直接用 GameObject.Find("Content")，因为 SettingPanel 默认是 inactive，
    /// GameObject.Find 默认找不到 inactive 物体。
    /// 这里使用 Resources.FindObjectsOfTypeAll，并额外检查父子层级。
    /// </summary>
    private static Transform FindContentTransform()
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform t = allTransforms[i];
            if (t == null || t.name != "Content")
                continue;

            // 只处理场景里的物体，跳过 Prefab 资源。
            if (!t.gameObject.scene.IsValid())
                continue;

            // 层级检查：Content 的父物体应该是 Viewport，
            // Viewport 的父物体是 Scroll View，再上面是 TopBar，最上面是 Canvas。
            if (!HasParentChain(t, "Content", "Viewport", "Scroll View", "TopBar", "SettingPanel", "Canvas"))
                continue;

            return t;
        }

        return null;
    }

    /// <summary>
    /// 检查 Transform 的祖先名称是否符合预期。
    /// names[0] 是当前物体名，names[1] 是父物体名，依次往上。
    /// </summary>
    private static bool HasParentChain(Transform t, params string[] names)
    {
        Transform current = t;
        for (int i = 0; i < names.Length; i++)
        {
            if (current == null || current.name != names[i])
                return false;

            current = current.parent;
        }

        return current == null;
    }

    /// <summary>
    /// 生成 "Canvas/Parent/Child" 这样的路径，用于日志输出。
    /// </summary>
    private static string GetPath(Transform t)
    {
        if (t == null)
            return string.Empty;

        return t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;
    }
}




