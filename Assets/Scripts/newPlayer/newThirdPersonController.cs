using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;

public class newThirdPersonController : vThirdPersonController
{



    #region
    [Header("new Controller")]
    [Tooltip("最大耐力")]
    public int maxstamina = 2;
    [Tooltip("max air jump times")]
    public int maxairjumptimes = 0;//最大空中跳跃次数
    //[Tooltip("是否允许在空中冲刺（适配你的二段跳）")]
    //public bool allowAirSprint = false;

    //冲刺部分
    [Tooltip("单次冲刺的持续时间（秒），建议0.15-0.3s")]
    public float DashDuration = 1f;
    [Tooltip("冲刺的移动速度（米/秒），建议15-25，过高易穿墙")]
    public float dashSpeed = 3f;
    [Tooltip("冲刺冷却时间（秒），避免连续冲刺")]
    public float dashCooldown = 0.5f;
    
    [Tooltip("无方向输入时，是否朝角色正前方冲刺")]
    public bool dashForwardWhenNoInput = false;
    [Tooltip("空中冲刺时是否禁用重力（避免冲刺时下落）")]
    public bool disableGravityWhenDash = true;

    [Tooltip("最大冲刺数")]
    public int MaxDashTimes = 0;


    [Tooltip("音效播放器")]
    public AudioSource audiosource;

    [Tooltip("落地音效")]
    public AudioClip LandSoftSound;


    #endregion
    #region
    public bool isDashing = false;//是否正在冲刺
    #endregion
    #region
    //脚本内部状态变量
    private int currentstamina = 1;//当前耐力条
    //跳跃相关
    private int currentairjumptimes = 0;//当前跳跃次数

    //冲刺相关
    public float lastDashTime = -100f; // 上次冲刺时间
    public int CurrentDashTimes = 0;//当前连续冲刺次数
    #endregion

    // 核心重写：为了在冲刺时拦截 Invector 的默认移动逻辑
    public override void UpdateMotor()
    {
        CheckGround();     // 始终检测地面和重力（保证自然下落）
        CheckSlopeLimit(); // 始终检测斜坡

        if (!isDashing)
        {
            // 如果不在冲刺中，正常执行跳跃和空中/地面控制
            ControlJumpBehaviour();
            AirControl();
        }
        else
        {
            // 如果在冲刺中，拦截 ControlJumpBehaviour 和 AirControl，防止它们覆盖冲刺速度
            // 但因为 CheckGround 依然在运行，额外的重力会被正确施加
        }
    }
    public int GetMaxDashTimes()
    {
        return MaxDashTimes;
    }
    public int GetCurrentDashTimes()
    {
        return CurrentDashTimes;
    }
    public int get_currentairjumptimes()
    {
        return currentairjumptimes;
    }
    public int add_currentairjumptimes(int number)
    {
        currentairjumptimes += number;
        return currentairjumptimes;
    }

    protected override void CheckGround()
    {
        CheckGroundDistance();
        ControlMaterialPhysics();

        if (groundDistance <= groundMinDistance)
        {
            if(isGrounded==false)//第一次改变落地状态触发音效
            {
                PlayLandingSound();//播放落地音效
            }

            isGrounded = true;

            

            //在空中与地表发生变化
            IsGroundedChanged();

            if (!isJumping && groundDistance > 0.05f)
                _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);

            heightReached = transform.position.y;
        }
        else
        {
            if (groundDistance >= groundMaxDistance)
            {
                // set IsGrounded to false 
                isGrounded = false;
                // check vertical velocity
                verticalVelocity = _rigidbody.velocity.y;
                // apply extra gravity when falling
                if (!isJumping)
                {
                    _rigidbody.AddForce(transform.up * extraGravity * Time.deltaTime, ForceMode.VelocityChange);
                }
            }
            else if (!isJumping)
            {
                _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
            }
        }
    }
    protected virtual void IsGroundedChanged()//空中与地面的状态发生变化
    {
        if (isGrounded)//从空中到地面你
        {
            currentairjumptimes = 0;
        }
    }
    /// <summary>
    /// 发起冲刺 (由外部输入脚本调用)
    /// </summary>
    public virtual void Dash()
    {
        // 检查冷却和状态
        if (isDashing || Time.time < lastDashTime + dashCooldown)
            return;

        // 这里可以加入耐力检测逻辑
         if (currentstamina < 1)
        {
            Debug.LogWarning("耐力未检测便尝试启动冲刺");
            return;
        }


        StartCoroutine(DashRoutine());
    }
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // 1. 确定冲刺方向
        // 默认向玩家当前输入的方向冲刺；如果没有输入，且允许无输入向前冲刺，则向角色正前方冲刺
        Vector3 dashDir = input.magnitude > 0.1f ? moveDirection.normalized :
                          (dashForwardWhenNoInput ? transform.forward : Vector3.zero);

        // 如果既没按键又不允许原地冲刺，直接结束
        if (dashDir == Vector3.zero)
        {
            isDashing = false;
            yield break;
        }
        SegmentBar.Instance.ConsumeStamina(1);
        //触发冲刺动画
        string dashAnimName = "DashForward"; // 默认设定一个基础动画名

        if (input.magnitude > 0.1f)
        {
            // 比较 Z轴(前后) 和 X轴(左右) 输入的绝对值，找出“主导方向”
            if (Mathf.Abs(input.z) >= Mathf.Abs(input.x))
            {
                // 前后输入大于等于左右输入，说明主要是前后冲
                dashAnimName = input.z > 0 ? "DashForward" : "DashBackward";
            }
            else
            {
                // 左右输入大于前后输入，说明主要是左右冲
                dashAnimName = input.x > 0 ? "DashRight" : "DashLeft";
            }
        }
        else
        {
            // 如果没有输入方向，但代码走到了这里，说明 dashForwardWhenNoInput 是 true
            dashAnimName = "DashForward";
        }

        // 播放计算出的动画，0.2f 是过渡时间(Blend Time)，让动作切换不生硬
        animator.CrossFadeInFixedTime(dashAnimName, 0.3f);


        // 2. 锁定 Invector 的底层输入移动
        lockMovement = true;
        lockRotation = true; // 冲刺期间不准扭头

        float timer = 0f;

        while (timer < DashDuration)
        {
            // 计算目标速度：水平方向强制为 dashDir * dashSpeed，垂直方向保留当前的物理下落速度
            Vector3 targetVelocity = dashDir * dashSpeed;
            targetVelocity.y = _rigidbody.velocity.y; // 灵魂代码：保留Y轴动量以实现抛物线下落

            _rigidbody.velocity = targetVelocity;

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        //// 3. 冲刺结束收尾
        //// 保留少量惯性，手感更好（不会瞬间停住）
        //Vector3 endVel = _rigidbody.velocity;
        //endVel.x *= 0.3f;
        //endVel.z *= 0.3f;
        //_rigidbody.velocity = endVel;

        lockMovement = false;
        lockRotation = false;
        isDashing = false;
    }
    /// <summary>
    /// 重新修改去掉没输入就强行阻止人物移动的设定
    /// </summary>
    public override void ControlAnimatorRootMotion()
    {
        //Debug.Log("ControlAnimatorRootMotion");
        if (useRootMotion)
            MoveCharacter(moveDirection);
    }
    /// <summary>
    /// 播放落地音效
    /// </summary>
    public void PlayLandingSound()
    {
        float volume = 3*Mathf.InverseLerp(0f, 10f, -verticalVelocity);
        audiosource.PlayOneShot(LandSoftSound, volume);
        Debug.Log($"播放落地音效,音量大小为{volume},落地速度为{-verticalVelocity}");
    }
}

