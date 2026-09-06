using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlSetting : MonoBehaviour
{
    [Header("UI 组件绑定")]
    //[SerializeField] private Text actionLabel;      // 显示“向前移动”的文本
    //public Text MoveForwardButtonText;    //前进按钮文本
    //public Button MoveForwardButton;     // 前进按钮
    //public Text MoveBackwardButtonText;    //后退按钮文本
    //public Button MoveBackwardButton;     // 后退按钮
    //public Text MoveLeftButtonText;    //向左按钮文本
    //public Button MoveLeftButton;     // 向左按钮
    //public Text MoveRightButtonText;    //向右按钮文本
    //public Button MoveRightButton;     // 向右按钮
    public TextMeshProUGUI JumpButtonText;    //跳跃按钮文本
    public Button JumpButton;     // 跳跃按钮
    public TextMeshProUGUI DashButtonText;    //冲刺按钮文本
    public Button DashButton;     // 冲刺按钮

    [Header("配置项")]
    //public string MoveForwardKey = "ForwardKey"; // 前进Key的键名
    //public KeyCode MoveForwardBoard = KeyCode.W; // 默认前进按键
    //public string MoveBackwardKey = "ForwardKey"; // 后退Key的键名
    //public KeyCode MoveBackwardBoard = KeyCode.W; // 默认后退按键
    //public string MoveLeftKey = "ForwardKey"; // 向左Key的键名
    //public KeyCode MoveLeftBoard = KeyCode.W; // 默认向左按键
    //public string MoveRightKey = "ForwardKey"; // 向右Key的键名
    //public KeyCode MoveRightBoard = KeyCode.W; // 默认向右按键
    public string JumpKey = "JumpKey"; // 跳跃Key的键名
    public KeyCode JumpBoard = KeyCode.Space; // 默认跳跃按键
    public string DashKey = "DashKey"; // 冲刺Key的键名
    public KeyCode DashBoard = KeyCode.LeftShift; // 默认冲刺按键


    public KeyCode CurrentKey { get; private set; }//当前按键

    private bool IsListening = false;//是否正在改键


    private void Start()
    {
        // 1. 初始化跳跃键：读取并显示
        string savedJump = PlayerPrefs.GetString(JumpKey, JumpBoard.ToString());
        JumpButtonText.text = savedJump;

        // 2. 初始化冲刺键：读取并显示（之前漏掉这步啦）
        string savedDash = PlayerPrefs.GetString(DashKey, DashBoard.ToString());
        DashButtonText.text = savedDash;

        // 3. 动态绑定按钮事件 
        JumpButton.onClick.AddListener(() => StartRebinding(JumpButtonText, JumpKey, 2));
        DashButton.onClick.AddListener(() => StartRebinding(DashButtonText, DashKey, 1));
    }


    // 点击按钮时触发
    public void StartRebinding(TextMeshProUGUI KeyButtonText, string saveKey, int keytype)
    {
        if (IsListening) return; // 如果已经在等待按键了，直接返回

        StartCoroutine(WaitForKeyPress(KeyButtonText,saveKey,keytype));
    }

    // 核心协程：等待玩家按下任意键
    private IEnumerator WaitForKeyPress(TextMeshProUGUI KeyButtonText,string saveKey,int keytype)
    {
        IsListening = true;
        KeyButtonText.text = "...请按下任意键...";
        KeyButtonText.color = Color.red; // 变成黄色提示玩家

        // 等待一帧，防止把“点击按钮”的这次鼠标点击误判为改键输入
        yield return null;

        while (IsListening)
        {
            // 遍历所有可能的键盘按键
            if (Input.anyKeyDown)
            {
                // 排除鼠标点击（如果你允许用鼠标改键，可以删掉这段判断）
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                {
                    yield return null;
                    continue;
                }

                // 寻找玩家究竟按下了哪个键
                foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kcode))
                    {
                        // 如果玩家按了 Escape 键，代表取消改键，不进行保存
                        if (kcode == KeyCode.Escape)
                        {
                            break;
                        }

                        // 成功检测到新按键！
                        CurrentKey = kcode;

                        if (keytype == 1)
                        {
                            if (newThirdpersoninput.Instance != null)
                            {
                                newThirdpersoninput.Instance.dashinput = CurrentKey;
                            }

                        }
                        else if (keytype == 2)
                        {
                            if (newThirdpersoninput.Instance != null)
                            {
                                newThirdpersoninput.Instance.jumpInput = CurrentKey;
                            }

                        }
                        else
                        {

                        }

                        // 保存到 PlayerPrefs
                        PlayerPrefs.SetString(saveKey, CurrentKey.ToString());
                        PlayerPrefs.Save();
                        break;
                    }
                }

                // 结束监听
                IsListening = false;
            }
            yield return null;
        }

        // 恢复 UI 状态
        KeyButtonText.text = CurrentKey.ToString();
        KeyButtonText.color = Color.black; // 恢复原本颜色（根据你的UI调整）
    }
}
