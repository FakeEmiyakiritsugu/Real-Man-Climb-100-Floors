using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SelectMovingPlatform : MonoBehaviour
{
    [Header("移动设置")]
    //[Tooltip("自动移动")]
    //public Vector3 AutoMove = new Vector3(0f, 0f, 0f);
    //[Tooltip("自动移动时间")]
    //public float AutoDuration = 2f;
    //[Tooltip("自动移动的移动方式")]
    //public Ease AutoeaseType = Ease.Linear;
    [Tooltip("踩了才动")]
    public Vector3 moveOffset = new Vector3(0f, 5f, 0f); // 支持任意方向的 PingPong
    [Tooltip("非自动的移动时间")]
    public float duration = 2f;
    [Tooltip("非自动的移动方式")]
    public Ease easeType = Ease.Linear;

    [Header("感应设置")]
    public bool moveOnlyWhenOccupied = true;
    public bool returnToStart = true;
    public float returnDuration = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 lastPosition;

    private Tween currentTween;

    // 用于记录当前平台是不是朝着 Target 走
    private bool isHeadingToTarget = true;

    // 记录乘客（使用 Rigidbody 保证物理同步）
    private HashSet<Rigidbody> playersOnPlatform = new HashSet<Rigidbody>();

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;
        lastPosition = transform.position;

        //// 自动移动
        //transform.DOMove(transform.position + AutoMove, AutoDuration)
        //    .SetEase(AutoeaseType)
        //    .SetLoops(-1, LoopType.Yoyo)
        //    .SetUpdate(UpdateType.Fixed);


        // 如果不是感应模式，一开始就让它自己动起来
        if (!moveOnlyWhenOccupied)
        {
            StartPingPong();
        }
    }

    /// <summary>
    /// 核心逻辑 1：手写 PingPong 循环。完美代替 SetLoops(-1)，因为这样可以随时无缝打断！
    /// </summary>
    void StartPingPong()
    {
        // 先杀掉之前的动画，防止多重动画打架发生闪现
        if (currentTween != null) currentTween.Kill();

        // 确定这一趟的目的地
        Vector3 destination = isHeadingToTarget ? targetPos : startPos;

        // 【关键防御】按当前剩余距离比例计算时间，保证中途踩上去速度不突变
        float distanceRatio = Vector3.Distance(transform.position, destination) / moveOffset.magnitude;
        if (distanceRatio < 0.001f) distanceRatio = 1f; // 防止除以 0

        currentTween = transform.DOMove(destination, duration * distanceRatio)
            .SetEase(easeType)
            .SetUpdate(UpdateType.Fixed) // 必须是 Fixed！这是修复 Fall 动画的第一步
            .OnUpdate(SyncPlayers)       // 修复 Fall 动画的第二步：平台动的一瞬间，立刻拉着玩家动
            .OnComplete(() =>
            {
                // 到达终点后，反转方向，自己调用自己，形成完美循环！
                isHeadingToTarget = !isHeadingToTarget;
                StartPingPong();
            });
    }

    /// <summary>
    /// 核心逻辑 2：平滑返回起点的逻辑
    /// </summary>
    void ReturnToStart()
    {
        if (currentTween != null) currentTween.Kill();

        float distanceRatio = Vector3.Distance(transform.position, startPos) / moveOffset.magnitude;
        if (distanceRatio < 0.001f) return; // 已经到家了就不动了

        // 强制重置方向，确保下次玩家踩上来时，是朝着目标前进的
        isHeadingToTarget = true;

        currentTween = transform.DOMove(startPos, returnDuration * distanceRatio)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(UpdateType.Fixed)
            .OnUpdate(SyncPlayers);
    }

    /// <summary>
    /// 核心逻辑 3：绝对同步的方法（由 DOTween 每一帧自动触发）
    /// </summary>
    void SyncPlayers()
    {
        Vector3 deltaPosition = transform.position - lastPosition;

        foreach (Rigidbody playerRb in playersOnPlatform)
        {
            if (playerRb != null)
            {
                // 【修复问题二】使用 MovePosition！
                // 因为这个函数是在 UpdateType.Fixed 里被 DOTween 触发的，
                // 玩家的刚体和平台在同一个物理帧被同步推走，Invector 的射线绝对查不出破绽，彻底告别无限 Fall！
                playerRb.MovePosition(playerRb.position + deltaPosition);
            }
        }

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody playerRb = other.attachedRigidbody;
        if (playerRb != null && playerRb.CompareTag("Player"))
        {
            // 这是第一个踩上来的人
            if (playersOnPlatform.Count == 0 && moveOnlyWhenOccupied)
            {
                StartPingPong();
            }
            playersOnPlatform.Add(playerRb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody playerRb = other.attachedRigidbody;
        if (playerRb != null && playerRb.CompareTag("Player"))
        {
            playersOnPlatform.Remove(playerRb);

            // 所有人都下车了
            if (playersOnPlatform.Count == 0 && moveOnlyWhenOccupied)
            {
                if (returnToStart)
                {
                    ReturnToStart();
                }
                else
                {
                    // 不返回起点就原地刹车
                    if (currentTween != null) currentTween.Kill();
                }
            }
        }
    }
}