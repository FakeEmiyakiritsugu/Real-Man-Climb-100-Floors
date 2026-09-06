using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumKnockback : MonoBehaviour
{
    [Header("击飞设置")]
    public float knockbackForce = 20f;  // 水平击飞力度
    public float upwardLift = 10f;      // 向上抛的力度

    private Vector3 lastPosition;
    private Vector3 currentSwingDirection; // 摆锤实时的运动方向

    void Start()
    {
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        // 1. 每一帧计算摆锤的真实移动向量
        Vector3 moveDelta = transform.position - lastPosition;

        // 忽略极微小的移动（比如在最高点悬停的那一瞬），防止产生无效向量
        if (moveDelta.sqrMagnitude > 0.0001f)
        {
            currentSwingDirection = moveDelta.normalized;
        }

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody playerRb = other.attachedRigidbody;

        if (playerRb != null && playerRb.CompareTag("Player"))
        {
            // 2. 直接使用摆锤的运动方向作为击飞方向！
            Vector3 pushDirection = currentSwingDirection;

            // 3. 抹平 Y 轴，确保力量全部转化在水平面上，把高度控制权留给 upwardLift
            pushDirection.y = 0;
            pushDirection.Normalize(); // 抹平后必须重新归一化，否则向量长度会缩水

            // 4. 组合最终的破坏力
            Vector3 finalForce = (pushDirection * knockbackForce) + (Vector3.up * upwardLift);

            // 5. 瞬间夺走玩家的速度控制权并施加冲量
            playerRb.velocity = Vector3.zero;
            playerRb.AddForce(finalForce, ForceMode.VelocityChange);
        }
    }
}
