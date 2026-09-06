using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;

public class newThirdpersoninput : vThirdPersonInput
{
    #region
    [Header("new Controller Input")]
    [Tooltip("冲锋按键")]
    public KeyCode dashinput = KeyCode.LeftShift;//冲锋按键



    [HideInInspector] public newThirdPersonController dd;
    #endregion


    // 静态指针，全游戏唯一的主角输入实例
    public static newThirdpersoninput Instance;


    private void Awake()
    {
        // 1. 必须把自己登记到静态变量里！否则 UI 脚本通过 Instance 找不到你
        Instance = this;

        // 2. 正确的初始化冲刺键：
        // 先从 PlayerPrefs 读取保存的按键字符串，如果读不到（第一次进游戏），就用默认的 "LeftShift"
        string savedDashStr = PlayerPrefs.GetString("DashKey", KeyCode.LeftShift.ToString());
        // 再把读出来的 "LeftShift" 转换成真正的 KeyCode
        this.dashinput = (KeyCode)System.Enum.Parse(typeof(KeyCode), savedDashStr);

        // 3. 正确的初始化跳跃键：
        // 先读取，读不到就默认 "Space"（空格）
        string savedJumpStr = PlayerPrefs.GetString("JumpKey", KeyCode.Space.ToString());
        // 转换成真正的 KeyCode
        this.jumpInput = (KeyCode)System.Enum.Parse(typeof(KeyCode), savedJumpStr);

        Debug.Log($"主角初始化成功！当前冲刺键：{this.dashinput}，当前跳跃键：{this.jumpInput}");
    }

    //private Vector3 lastPos;

    //private void LateUpdate()
    //{
    //    if (dd != null)
    //    {
    //        if (Vector3.Distance(lastPos, dd.transform.position) > 0.01f)
    //        {
    //            Debug.Log(
    //                "位置被修改："
    //                + dd.transform.position
    //                + " 时间：" + Time.time
    //            );
    //        }

    //        lastPos = dd.transform.position;
    //    }
    //}

    //重写cc的初始化逻辑
    protected override void InitilizeController()
    {
        dd = GetComponent<newThirdPersonController>();
        cc = dd;

        if (dd != null)
        {
            dd.Init();
            //人物位置初始化
            Vector3 PlayerPst = new Vector3(GameManager.GMInstance.GlobalPlayerData.xPlayerPosition, GameManager.GMInstance.GlobalPlayerData.yPlayerPosition, GameManager.GMInstance.GlobalPlayerData.zPlayerPosition);


            // 然后开启协程设置位置
            StartCoroutine(LoadPlayerPosition());
            //dd._rigidbody.position = PlayerPst;
            //dd._rigidbody.velocity = Vector3.zero; // 清空可能存在的出生速度
            ////dd.transform.position = PlayerPst;
            //// 告诉全场景的碰撞体：“我已经在这里了！”
            //Physics.SyncTransforms();
            Debug.Log($"人物位置读取为{dd._rigidbody.position}");

            //给予最大冲刺次数
            dd.MaxDashTimes = GameManager.GMInstance.GlobalPlayerData.MaxDashTimes;
            Cursor.lockState = CursorLockMode.Locked;//限制鼠标位置
            Cursor.visible = false;//限制鼠标不可见
        }

    }

    private IEnumerator LoadPlayerPosition()
    {
        yield return null;

        //this.enabled = false;

        Vector3 playerPos = new Vector3(
            GameManager.GMInstance.GlobalPlayerData.xPlayerPosition,
            GameManager.GMInstance.GlobalPlayerData.yPlayerPosition,
            GameManager.GMInstance.GlobalPlayerData.zPlayerPosition);


        dd._rigidbody.position = playerPos;


        // 清空物理状态
        dd._rigidbody.velocity = Vector3.zero;
        dd._rigidbody.angularVelocity = Vector3.zero;


        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();

        //恢复控制
        //this.enabled = true;

        Debug.Log("读档位置：" + dd._rigidbody.position);
    }

    protected override void InputHandle()
    {
        MoveInput();
        CameraInput();
        SprintInput();
        StrafeInput();
        DashInput();
        JumpInput();
    }

    //跳跃条件
    protected override bool JumpConditions()
    {
        bool jumpflag = false;
        if (cc.isGrounded && dd.GroundAngle() < dd.slopeLimit && !dd.stopMove && !dd.isJumping)//一段跳
        {
            jumpflag = true;
        }
        else if (!dd.isGrounded && dd.get_currentairjumptimes() < dd.maxairjumptimes && SegmentBar.Instance.ConsumeStamina(1))//允许空中跳跃,消耗一格耐力
        {
            jumpflag = true;
            dd.add_currentairjumptimes(1);
        }
        else
        {
            jumpflag = false;
        }
        return jumpflag;
    }

    /// <summary>
    /// 冲刺条件
    /// </summary>
    /// <returns></returns>
    protected bool DashConditions()
    {
        bool dashflag = false;
        if(!dd.isDashing&&dd.GetCurrentDashTimes()<dd.GetMaxDashTimes()&&SegmentBar.Instance.HasStamina(1))//有耐力，不在冲刺
        {
            dashflag = true;
        }
        return dashflag;
    }


    //冲刺输入检测,暂时没有限制连续冲刺次数
    protected virtual void DashInput()
    {
        if (Input.GetKeyDown(dashinput) && dd.input.magnitude > 0.1f && DashConditions())//冲刺同时按下方向键
        {
            dd.Dash();
        }
    }


}
