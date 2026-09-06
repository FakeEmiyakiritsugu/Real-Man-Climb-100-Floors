using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 必须引入 DOTween 命名空间

public class MovingPlatform : MonoBehaviour
{
    [Header("移动设置")]
    public Vector3 moveOffset = new Vector3(10f, 0f, 0f);
    public float duration = 2f;
    public Ease easeType = Ease.Linear;

    private Vector3 lastPosition;

    // 1. 修改存储类型为 Rigidbody，直接操作刚体
    private HashSet<Rigidbody> playerRigidbodiesOnPlatform = new HashSet<Rigidbody>();

    void Start()
    {
        lastPosition = transform.position;

        // 让 DOTween 正常移动平台
        transform.DOMove(transform.position + moveOffset, duration)
          .SetEase(easeType)
          .SetLoops(-1, LoopType.Yoyo)
          .SetUpdate(UpdateType.Fixed) // 建议保持 Fixed，这样物理位移更规律
          .OnUpdate(SyncPlayerPosition);
    }
    // 由 DOTween 在物理帧自动调用的同步方法
    void SyncPlayerPosition()
    {
        // 算出最纯粹的物理位移
        Vector3 deltaPosition = transform.position - lastPosition;

        foreach (Rigidbody playerRb in playerRigidbodiesOnPlatform)
        {
            if (playerRb != null)
            {
                // 3. 使用物理引擎专属方法，彻底消除幽灵滑行和闪现！
                playerRb.MovePosition(playerRb.position + deltaPosition);
            }
        }

        // 更新坐标留给下一步
        lastPosition = transform.position;
    }
    //// 2. 依然放在 LateUpdate 执行，确保在所有动画和 Invector 基础逻辑之后
    //void LateUpdate()
    //{
    //    // 算出平台这一帧的位移差
    //    Vector3 deltaPosition = transform.position - lastPosition;

    //    // 3. 遍历刚体名单，直接修改 Rigidbody.position
    //    foreach (Rigidbody rb in playerRigidbodiesOnPlatform)
    //    {
    //        if (rb != null)
    //        {
    //            // 直接对刚体位置进行操作，不通过 transform 间接修改
    //            // 这能让物理引擎更平滑地接受位置变更
    //            rb.position += deltaPosition;
    //        }
    //    }

    //    // 记录当前位置留给下一帧
    //    lastPosition = transform.position;
    //}

    private void OnTriggerEnter(Collider other)
    {
        // 获取碰到平台的物体的刚体组件
        Rigidbody playerRb = other.attachedRigidbody;

        // 如果是玩家（带有 Player 标签的刚体），将其存入刚体名单
        if (playerRb != null && playerRb.CompareTag("Player"))
        {
            playerRigidbodiesOnPlatform.Add(playerRb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody playerRb = other.attachedRigidbody;

        if (playerRb != null && playerRb.CompareTag("Player"))
        {
            playerRigidbodiesOnPlatform.Remove(playerRb);
        }
    }
}
