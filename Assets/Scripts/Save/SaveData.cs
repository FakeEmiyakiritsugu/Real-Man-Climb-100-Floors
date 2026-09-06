using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

/// <summary>
/// 存档数据
/// </summary>
[System.Serializable]//序列化
public class SaveData
{
    public int SDStatus { get; set; } = 0;//存档状态，0新存档，1老存档，2读档错误
    public string test{ get; set; } = "test";
    //public Vector3 PlayerPosition = new Vector3(497,0.6f,512);//人物位置
    public float xPlayerPosition = 497f;//人物位置x
    public float yPlayerPosition = 0.6f;//人物位置y
    public float zPlayerPosition = 512f;//人物位置z
    public int MaxDashTimes = 0;//最大冲刺数
    public bool DashStar = false;//冲刺星星是否已经吃了
    public SaveData()//无参数默认存档
    {
        //this.SDStatus = 0;
    }
    public SaveData(int SDStatus)//读存档特供
    {
        this.SDStatus = SDStatus;
    }
}

