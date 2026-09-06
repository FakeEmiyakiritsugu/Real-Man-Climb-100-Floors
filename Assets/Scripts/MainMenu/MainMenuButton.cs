using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [Header("跳转的场景设置")]
    [Tooltip("开始游戏")]
    private string startSceneName = SceneName.SampleScene;
    [Tooltip("结束游戏")]
    private string exitSceneName = "GameOverScene";

    public GameObject SettingPanel;
    public GameObject CoursePanel;

    // 绑定到“开始”按钮
    public void OnStartButtonPressed()
    {
        // 重新加载你的 3D 核心游戏场景
        SceneManager.LoadScene(startSceneName);
        //新建存档
        GameManager.GMInstance.GlobalPlayerData = new SaveData();
    }

    //继续游戏按钮
    public void OnContinueButtonPressed()
    {
        // 重新加载你的 3D 核心游戏场景
        SceneManager.LoadScene(startSceneName);
        //加载存档
        GameManager.GMInstance.GlobalPlayerData = SaveSystem.Load();
    }

    //游戏说明按钮
    public void OnCourseButtonPressed()
    {
        CoursePanel.SetActive(true);
    }

    //游戏说明返回
    public void OnCourseBackPressed()
    {
        CoursePanel.SetActive(false);
    }


    //临时设置按钮，测试用
    //public void OnSettingButtonPressed()
    //{
    //    SaveData testdata;
    //    testdata = SaveSystem.Load();
    //    Debug.Log($"SDStatus = {testdata.SDStatus} test = {testdata.test} 数据读取成功");
    //}
    public void OnSettingButtonPressed()
    {
        SettingPanel.SetActive(true);
    }

    // 绑定到“退出游戏”按钮
    public void OnExitButtonPressed()
    {
        Debug.Log("收到退出游戏请求！");

        // 1. 如果是在 Unity 编辑器里运行，点击时停止播放
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif

        // 2. 如果是打包后的独立程序（Windows/Mac/手机等），执行退出
        Application.Quit();
    }
}
