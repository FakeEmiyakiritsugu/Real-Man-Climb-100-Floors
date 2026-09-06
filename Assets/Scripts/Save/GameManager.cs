using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager GMInstance { get; private set; }//全局静态单例
    public SaveData GlobalPlayerData { get; set; }//临时存档

    private void Awake()//初始化场景时指向，切换场景时不删除，并防止出现多个
    {
        if(GMInstance == null)
        {
            GMInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
    public void GetSaveData(SaveData Playerdata)
    {
        GlobalPlayerData = Playerdata;
    }
}
