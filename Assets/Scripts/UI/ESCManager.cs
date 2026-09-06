using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ESCManager : MonoBehaviour
{
    [Tooltip("放菜单")]
    public GameObject ESCMenu;//放菜单
    [Tooltip("切出菜单按键")]
    public KeyCode ESCInput = KeyCode.Escape;
    [Tooltip("是否暂停")]
    public bool isPaused { get; set; } = false;
    // Start is called before the first frame update
    void Start()
    {
        ESCMenu.SetActive(false);//确保窗口不可见
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(ESCInput))
        {
            if (isPaused)
            {
                GameResume();
            }
            else
            {
                GamePause();
            }
        }
    }
    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void GamePause()
    {
        isPaused = true;
        ESCMenu.SetActive(true);//菜单可见
        Time.timeScale = 0f;//暂停游戏
        Cursor.lockState = CursorLockMode.Confined;//鼠标可移动
        Cursor.visible = true;//鼠标可见
    }
    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void GameResume()
    {
        isPaused = false;
        ESCMenu.SetActive(false);//菜单不可见
        Time.timeScale = 1f;//恢复游戏
        Cursor.lockState = CursorLockMode.Locked;//鼠标不可移动
        Cursor.visible = false;//鼠标不可见
    }
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void OnBackMainMenuPressed()
    {
        DOTween.KillAll();
        Time.timeScale = 1f;//恢复游戏
        SceneManager.LoadScene(SceneName.MainMenu);
    }
    /// <summary>
    /// 点击退出按钮
    /// </summary>
    public void OnExitPressed()
    {
        Debug.Log("收到退出游戏请求！");
        DOTween.KillAll();
        // 1. 如果是在 Unity 编辑器里运行，点击时停止播放
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        // 2. 如果是打包后的独立程序（Windows/Mac/手机等），执行退出
        Application.Quit();
    }
}
