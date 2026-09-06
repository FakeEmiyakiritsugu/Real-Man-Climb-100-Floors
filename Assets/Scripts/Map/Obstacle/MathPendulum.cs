using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 强制要求挂载 Rigidbody 组件
[RequireComponent(typeof(Rigidbody))]
public class MathPendulum : MonoBehaviour
{
    [Header("摆动设置")]
    public float swingLimit = 75f;
    public float swingSpeed = 2f;
    public Vector3 swingAxis = new Vector3(0, 0, 1);
    public float phaseOffset = 0f;

    private Quaternion startRotation;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 核心物理设置
        rb.isKinematic = true; // 开启 Kinematic，意味着它不受重力影响，完全由这套代码驱动
        rb.interpolation = RigidbodyInterpolation.Interpolate; // 开启插值，保证视觉上极其丝滑

        startRotation = rb.rotation;
    }

    void FixedUpdate()
    {
        // 数学公式依然完美，算出目标角度
        float angle = swingLimit * Mathf.Sin(Time.time * swingSpeed + phaseOffset);
        Quaternion targetRotation = startRotation * Quaternion.AngleAxis(angle, swingAxis.normalized);

        //终极绝杀：使用物理引擎的专属接口进行旋转！
        // 这会让物理引擎在上一帧和这一帧之间“扫出”一个扇形轨迹，碰到任何东西都会触发！
        rb.MoveRotation(targetRotation);
    }
}
