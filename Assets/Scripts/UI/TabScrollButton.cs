using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabScrollButton : MonoBehaviour
{
    public ScrollRect SR;
    public float ButtonNumber = 5;



    // 定义一个结构体，把按钮和面板成对绑定
    [System.Serializable]
    public struct TabItem
    {
        public string tabName;       // 标签名字（纯编组好看用，不影响逻辑）
        public Button tabButton;     // 顶部的按钮
        public GameObject contentPanel; // 中间对应的 Content 面板
    }

    [Header("标签页配置列表")]
    public TabItem[] settingsTabs;

    [Header("默认打开第几个标签页 (从0开始算)")]
    public int defaultTabIndex = 0;

    private void Start()
    {
        // 自动遍历所有配置好的标签页，动态绑定点击事件
        for (int i = 0; i < settingsTabs.Length; i++)
        {
            // ★ 非常关键：在循环里写 Lambda 表达式时，必须用一个局部变量把当前的 i 存下来
            // 否则所有按钮被点击时，读取到的 i 都会变成循环结束后的最终值（导致永远只触发最后一个）
            int index = i;

            if (settingsTabs[i].tabButton != null)
            {
                // 绑定点击事件：点击时调用切换函数
                settingsTabs[i].tabButton.onClick.AddListener(() => SwitchTab(index));
            }
        }

        // 游戏启动时，默认打开指定的第一个页面
        SwitchTab(defaultTabIndex);
    }

    /// <summary>
    /// 核心切换逻辑
    /// </summary>
    /// <param name="targetIndex">想要打开的面板索引</param>
    public void SwitchTab(int targetIndex)
    {
        // 安全检查：防止数组越界
        if (targetIndex < 0 || targetIndex >= settingsTabs.Length) return;

        for (int i = 0; i < settingsTabs.Length; i++)
        {
            // 1. 控制面板的显示和隐藏
            if (settingsTabs[i].contentPanel != null)
            {
                // 如果是当前点击的索引，就 SetActive(true)；否则 SetActive(false)
                settingsTabs[i].contentPanel.SetActive(i == targetIndex);
            }

            // 2. 【可选小细节】让选中的按钮变成“不可点击”状态，未选中的可以点击
            // 这样玩家就能一眼看出当前在哪个页面，UI 体验极佳
            if (settingsTabs[i].tabButton != null)
            {
                settingsTabs[i].tabButton.interactable = (i != targetIndex);
            }
        }
    }


    public void OnLeftArrowPressed()
    {
        float ChangeNumer = 1 / (ButtonNumber-4);
        if(SR.horizontalNormalizedPosition-ChangeNumer >=0)
        {
            SR.horizontalNormalizedPosition -= ChangeNumer;
        }
        else
        {
            SR.horizontalNormalizedPosition = 0;
        }
        Debug.Log($"左滚动至{SR.horizontalNormalizedPosition},ChangeNumer{ChangeNumer}");
    }

    public void OnRightArrowPressed()
    {
        float ChangeNumer = 1 / (ButtonNumber - 4);
        if (SR.horizontalNormalizedPosition + ChangeNumer <= 1)
        {
            SR.horizontalNormalizedPosition += ChangeNumer;
        }
        else
        {
            SR.horizontalNormalizedPosition = 1;
        }
        Debug.Log($"右滚动至{SR.horizontalNormalizedPosition},ChangeNumer{ChangeNumer}");
    }
}
