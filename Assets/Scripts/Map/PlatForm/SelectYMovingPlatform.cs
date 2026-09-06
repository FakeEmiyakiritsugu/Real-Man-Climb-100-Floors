using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
//【修复 Bug 1】：强制最后执行，彻底消灭折返时的 1 帧闪现！
[DefaultExecutionOrder(100)]
public class SelectYMovingPlatform : MonoBehaviour
{
    [Header("水平移动 (始终运行)")]
    public Vector3 horizontalOffset = new Vector3(10f, 0f, 0f);
    public float horizontalDuration = 3f;
    public Ease horizontalEase = Ease.Linear;

    [Header("垂直移动 (感应运行)")]
    public float verticalHeight = 5f;
    public float verticalDuration = 2f;
    public float returnDuration = 1.5f;
    public Ease verticalEase = Ease.InOutQuad;

    private Vector3 initialPos;
    private Vector3 lastPosition;

    private Tween verticalTween;
    private HashSet<Rigidbody> playersOnPlatform = new HashSet<Rigidbody>();

    void Start()
    {
        initialPos = transform.position;
        lastPosition = transform.position;

        if (horizontalOffset.x != 0)
        {
            transform.DOMoveX(initialPos.x + horizontalOffset.x, horizontalDuration)
                .SetEase(horizontalEase).SetLoops(-1, LoopType.Yoyo).SetUpdate(UpdateType.Fixed);
        }

        if (horizontalOffset.z != 0)
        {
            transform.DOMoveZ(initialPos.z + horizontalOffset.z, horizontalDuration)
                .SetEase(horizontalEase).SetLoops(-1, LoopType.Yoyo).SetUpdate(UpdateType.Fixed);
        }
    }

    void FixedUpdate()
    {
        // 因为有了 [DefaultExecutionOrder(100)]，现在算出来的 delta 绝对精确，没有任何滞后
        Vector3 deltaPosition = transform.position - lastPosition;

        if (deltaPosition != Vector3.zero)
        {
            foreach (Rigidbody playerRb in playersOnPlatform)
            {
                if (playerRb != null)
                {
                    // 依然是你最喜欢、最稳定的这句同步代码，绝不起飞
                    playerRb.MovePosition(playerRb.position + deltaPosition);
                }
            }
        }

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody playerRb = other.attachedRigidbody;
        if (playerRb != null && playerRb.CompareTag("Player"))
        {
            if (playersOnPlatform.Count == 0)
            {
                if (verticalTween != null) verticalTween.Kill();

                //【修复 Bug 2】：加上了 .SetLoops(-1, LoopType.Yoyo)，现在上下也会无限反复了！
                verticalTween = transform.DOMoveY(initialPos.y + verticalHeight, verticalDuration)
                    .SetEase(verticalEase)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(UpdateType.Fixed);
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

            if (playersOnPlatform.Count == 0)
            {
                if (verticalTween != null) verticalTween.Kill();

                float currentY = transform.position.y;
                float distanceRatio = Mathf.Abs(currentY - initialPos.y) / verticalHeight;
                if (distanceRatio < 0.001f) distanceRatio = 1f;

                verticalTween = transform.DOMoveY(initialPos.y, returnDuration * distanceRatio)
                    .SetEase(Ease.InOutQuad)
                    .SetUpdate(UpdateType.Fixed);
            }
        }
    }
}