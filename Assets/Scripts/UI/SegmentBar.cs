using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SegmentBar : MonoBehaviour
{
    public static SegmentBar Instance;


    [Header("玩家引用（把玩家物体拖到这里）")]
    public newThirdPersonController playerController;

    [Header("UI素材（非常重要，必须赋值！）")]
    public Sprite defaultSprite; // 解决无法渐变的核心：必须有一个白色图片素材

    [Header("最大耐力格数（改这里自动变格子）")]
    public int maxStaminaSegments = 1;

    //[Header("当前剩余耐力")]
    //public int currentStamina;

    [Header("单个格子尺寸")]
    public float segmentWidth = 14;
    public float segmentHeight = 25;

    [Header("格子颜色")]
    public Color fullColor = Color.yellow;      // 已恢复完成的颜色
    public Color recoveringColor = Color.green;// 正在恢复中的颜色
    public Color emptyColor = Color.gray;      // 耗尽的颜色

    [Header("4. 恢复设置")]
    public float recoverDelay = 0.5f;     // 消耗后延迟多久开始恢复
    public float recoverSpeed = 0.25f;  // 每秒恢复的耐力值


    // 核心数值（浮点型，支持渐变）
    private float _currentStamina;//"当前剩余耐力
    private float _maxStamina;
    private float _recoverTimer = 0f;       // 恢复延迟计时器

    // 存储所有耐力格子
    private List<Image> _staminaImages = new List<Image>();
    private HorizontalLayoutGroup _layoutGroup;

    //private void Start()
    //{
    //    maxStaminaSegments = dd.maxstamina
    //}

    /// <summary>
    /// 单例模式
    /// </summary>
    private void Awake()
    {
        if (Instance == null)  // 还没有实例
        {
            Instance = this;
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            maxStaminaSegments = playerController.maxstamina;
            _maxStamina = maxStaminaSegments;
            Debug.Log(playerController.maxstamina);
            // 初始化生成耐力格子
            GenerateStaminaSegments();
        }
        else
        {
            Destroy(gameObject); // 已经有了，我自杀，保证永远只有一个
        }
    }

    private void Update()
    {
        // 自动恢复逻辑
        StaminaRecover();
    }


    /// <summary>
    /// 根据最大耐力数 动态生成居中的耐力格子
    /// </summary>
    void GenerateStaminaSegments()
    {
        // 清空旧格子（注意：改为销毁所有子物体，避免层级残留）
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        _staminaImages.Clear();

        // 生成新格子
        for (int i = 0; i < maxStaminaSegments; i++)
        {
            // 1. 创建背景底槽 (负责显示 emptyColor)
            GameObject bgObj = new GameObject("StaminaBG_" + (i + 1));
            bgObj.transform.SetParent(transform, false);

            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = defaultSprite; // 必须赋值图片！
            bgImg.color = emptyColor;     // 永远保持灰色底色
            // 设置格子大小
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.sizeDelta = new Vector2(segmentWidth, segmentHeight);

            // 2. 创建前景填充层 (负责颜色变换和渐变填充)
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(bgObj.transform, false); // 作为背景的子物体
            // 设置为填充模式（实现渐变效果）
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.sprite = defaultSprite; // 必须赋值图片！
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;// 从左向右填充

            // 让填充层自动铺满父物体（背景槽）
            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            fillRt.anchoredPosition = Vector2.zero;

            // 只把填充层存入列表，因为我们只控制它的渐变
            _staminaImages.Add(fillImg);

        }

        // 初始化当前耐力
        _currentStamina = _maxStamina;
        UpdateStaminaVisual();
    }

    /// <summary>
    /// 刷新渐变显示 + 颜色区分,修改的是渐变层的颜色
    /// </summary>
    void UpdateStaminaVisual()
    {
        for (int i = 0; i < _staminaImages.Count; i++)
        {
            Image img = _staminaImages[i];
            float segmentValue = Mathf.Clamp(_currentStamina - i, 0, 1);//计算填充比例

            // 颜色逻辑（核心）
            if (segmentValue >= 1)
            {
                img.color = fullColor;       // 已恢复完成
            }
            else if (segmentValue > 0)
            {
                img.color = recoveringColor; // 正在恢复（渐变中）
            }
            

            img.fillAmount = segmentValue; // 渐变填充，填充比例
        }
    }

    /// <summary>
    /// 耐力自动恢复
    /// </summary>
    void StaminaRecover()
    {
        // 延迟倒计时
        if (_recoverTimer > 0)
        {
            _recoverTimer -= Time.deltaTime;
            return;
        }

        // 渐变恢复耐力
        if (_currentStamina < _maxStamina)
        {
            _currentStamina += recoverSpeed * Time.deltaTime;
            _currentStamina = Mathf.Min(_currentStamina, _maxStamina);
            UpdateStaminaVisual();
        }
    }

    /// <summary>
    /// 消耗number耐力
    /// </summary>
    public bool ConsumeStamina(float number)
    {
        if (_currentStamina < number) return false;

        _currentStamina -= number;
        _recoverTimer = recoverDelay; // 触发恢复延迟
        UpdateStaminaVisual();
        return true;
    }

    /// <summary>
    /// 恢复number耐力
    /// </summary>
    public void RecoverStamina(float number)
    {
        if (_currentStamina >= _maxStamina) return;
        _currentStamina += number;
        UpdateStaminaVisual();
    }

    /// <summary>
    /// 判断是否还有足够耐力
    /// </summary>
    public bool HasStamina(float number)
    {
        bool answer=false;
        if (_currentStamina >= number)
        {
            answer = true;
        }
        return answer;
    }

    // 当你在Inspector改最大耐力时，自动刷新格子
    private void OnValidate()
    {
        if (Application.isPlaying && _layoutGroup != null)
        {
            _maxStamina = maxStaminaSegments;
            GenerateStaminaSegments();
        }
    }
}