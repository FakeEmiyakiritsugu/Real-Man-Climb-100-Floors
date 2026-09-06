using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePosition : MonoBehaviour
{
    public Vector3 initposition { get; set; }//初始位置
    // Start is called before the first frame update
    /// <summary>
    /// 获取初始位置
    /// </summary>
    void Start()
    {
        Collider PlatformCollider = GetComponent<Collider>();
        Vector3 t = PlatformCollider.bounds.center;
        t.y = PlatformCollider.bounds.max.y+0f;
        initposition = t;
    }
    /// <summary>
    /// 记录存档位置
    /// </summary>
    /// <param name="other"></param>
    public void OnTriggerEnter(Collider other)
    {
        Rigidbody PlayerRB = other.attachedRigidbody;
        
        if (PlayerRB!=null&&PlayerRB.CompareTag("Player"))
        {
            Debug.Log(initposition);
            Debug.Log(gameObject.name);
            GameManager.GMInstance.GlobalPlayerData.xPlayerPosition = initposition.x;
            GameManager.GMInstance.GlobalPlayerData.yPlayerPosition = initposition.y;
            GameManager.GMInstance.GlobalPlayerData.zPlayerPosition = initposition.z;
            Debug.Log($"存档位置为{GameManager.GMInstance.GlobalPlayerData.xPlayerPosition},{GameManager.GMInstance.GlobalPlayerData.yPlayerPosition},{GameManager.GMInstance.GlobalPlayerData.zPlayerPosition}");
            SaveSystem.Save(GameManager.GMInstance.GlobalPlayerData);
        }
    }

}
