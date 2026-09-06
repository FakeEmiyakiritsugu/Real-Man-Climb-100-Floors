using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashPowerUp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(GameManager.GMInstance.GlobalPlayerData.DashStar == true)
        {
            Destroy(gameObject);
        }
    }

    //// Update is called once per frame
    //void Update()
    //{

    //}
    ////public void AddMaxDashTimes()
    ////{
    ////    MaxDashTimes
    ////}
    private void OnTriggerEnter(Collider other)
    {
        // 利用 attachedRigidbody 确保无论碰到角色的哪个部位都能准确抓取控制器
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
        {
            newThirdPersonController player = other.attachedRigidbody.GetComponent<newThirdPersonController>();

            if (player != null)
            {
                // 1.增加最大冲刺次数
                player.MaxDashTimes += 2;
                //存档增加最大冲刺次数
                GameManager.GMInstance.GlobalPlayerData.MaxDashTimes += 2;
                GameManager.GMInstance.GlobalPlayerData.DashStar = true;
                SaveSystem.Save(GameManager.GMInstance.GlobalPlayerData);
                // 2. (可选) 这里可以播放一个吃到道具的音效或粒子特效
                // AudioSource.PlayClipAtPoint(pickUpSound, transform.position);

                // 3. 杀掉动画并销毁道具本体
                transform.DOKill();
                Destroy(gameObject);
            }
        }
    }
}
