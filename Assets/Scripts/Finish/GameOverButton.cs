using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverButton : MonoBehaviour
{
    [Header("跳转的场景设置")]
    private string MainMenuSceneName = SceneName.MainMenu;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    // 绑定到“返回主菜单”按钮
    public void OnBMMButtonPressed()
    {
        // 重新加载你的 3D 核心游戏场景
        SceneManager.LoadScene(MainMenuSceneName);
    }
}
