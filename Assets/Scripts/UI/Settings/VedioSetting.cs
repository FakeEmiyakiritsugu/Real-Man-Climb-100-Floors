using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 记得引入 TMP 命名空间

public class VideoSetting : MonoBehaviour
{
    [Header("UI 组件绑定")]
    public Toggle fullscreenToggle;       // 拖入全屏 Toggle
    public TMP_Dropdown resDropdown;       // 拖入分辨率 Dropdown (TMP)

    // 存储经过滤后的分辨率列表
    private List<Resolution> filteredResolutions = new List<Resolution>();

    private void Start()
    {
        // 1. 初始化全屏开关状态
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        // 2. 获取并动态动态填充分辨率下拉菜单
        InitResolutionDropdown();
    }

    // 初始化分辨率下拉列表
    private void InitResolutionDropdown()
    {
        resDropdown.ClearOptions();
        filteredResolutions.Clear();

        // 获取显示器支持的所有原始分辨率（包含不同的刷新率，如60Hz, 144Hz）
        Resolution[] allResolutions = Screen.resolutions;
        List<string> options = new List<string>();

        int currentResIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            // 过滤掉重复的分辨率（我们只需要宽和高，不需要把 60Hz 和 144Hz 分开显示两次）
            bool isDuplicate = false;
            foreach (var res in filteredResolutions)
            {
                if (res.width == allResolutions[i].width && res.height == allResolutions[i].height)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                filteredResolutions.Add(allResolutions[i]);

                // 拼接文字，例如："1920 x 1080"
                string optionText = allResolutions[i].width + " x " + allResolutions[i].height;
                options.Add(optionText);

                // 检查这一项是不是玩家当前电脑正在使用的分辨率
                if (allResolutions[i].width == Screen.width && allResolutions[i].height == Screen.height)
                {
                    currentResIndex = options.Count - 1; // 记录当前索引
                }
            }
        }

        // 把好听的文本塞进下拉菜单
        resDropdown.AddOptions(options);
        // 默认选中当前分辨率
        resDropdown.value = currentResIndex;
        resDropdown.RefreshShownValue();

        // 监听下拉菜单改变事件
        resDropdown.onValueChanged.AddListener(SetResolution);
    }

    // 开关全屏的回调函数
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        // 顺手保存配置
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 切换分辨率的回调函数
    public void SetResolution(int index)
    {
        if (index < 0 || index >= filteredResolutions.Count) return;

        Resolution targetRes = filteredResolutions[index];

        // ★ Unity核心API：传入宽、高、是否全屏
        Screen.SetResolution(targetRes.width, targetRes.height, Screen.fullScreen);

        // 顺手保存配置
        PlayerPrefs.SetInt("ToggleWidth", targetRes.width);
        PlayerPrefs.SetInt("ToggleHeight", targetRes.height);
        PlayerPrefs.Save();
    }
}