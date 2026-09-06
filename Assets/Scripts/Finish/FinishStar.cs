using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishStar : MonoBehaviour
{
    [Header("关卡结算设置")]
    [Tooltip("碰到终点星后要跳转的场景名称")]
    public string finishSceneName = "GameOverScene";

    private void OnTriggerEnter(Collider other)
    {
        // 利用 attachedRigidbody 确保无论碰到角色的哪个部位都能准确抓取控制器
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
        {
            newThirdPersonController player = other.attachedRigidbody.GetComponent<newThirdPersonController>();

            if (player != null)
            {
                // 打印一条日志方便在编辑器里测试
                Debug.Log($"恭喜通关！即将跳转到结算场景: {finishSceneName}");
                DOTween.KillAll();
                // 执行场景跳转
                SceneManager.LoadScene(finishSceneName);
            }
        }
    }
}
