using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 存档保存读取系统
/// </summary>
public static class SaveSystem
{
    // 使用静态属性获取路径
    public static string SavePath{ get; set; }
    // 自定义 16 字节（128位）的密钥
    public static readonly string EncryptionKey = "KeepJumping12345";
    /// <summary>
    /// 初始化一些属性
    /// </summary>
    //告诉 Unity 在游戏启动、第一个场景加载之前，自动跑这个方法
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SavePath = Path.Combine(Application.persistentDataPath, "gamesave.kjp");
    }
    /// <summary>
    /// 保存存档
    /// </summary>
    /// <param name="data"></param>
    public static void Save(SaveData data)
    {
        string datajson = JsonConvert.SerializeObject(data);//存档转json

        byte[] purebyte = Encoding.UTF8.GetBytes(datajson);//纯净字节
        byte[] aeskeybyte = Encoding.UTF8.GetBytes(EncryptionKey);//密码字节
        try
        {
            using (Aes aes = Aes.Create())//创建加密流水线
            {
                aes.Key = aeskeybyte;
                using (MemoryStream ms = new MemoryStream())//创建内存流
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);//记录随机的IV
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))//创建加密流
                    {
                        cs.Write(purebyte, 0, purebyte.Length);
                        cs.FlushFinalBlock();
                    }
                    File.WriteAllBytes(SavePath, ms.ToArray());//写入文件
                    Debug.Log("保存存档成功");
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"存档失败，未知错误：{e.Message}");
        }
    }

    /// <summary>
    /// 读取存档返回存档文件
    /// </summary>
    /// <returns></returns>
    public static SaveData Load()
    {
        if(!File.Exists(SavePath))//无存档情况
        {
            Debug.LogWarning("当前不存在存档");
            return new SaveData(0);
        }

        try
        {
            byte[] Encryptionbyte = File.ReadAllBytes(SavePath);//存档字节流
            byte[] Keybyte = Encoding.UTF8.GetBytes(EncryptionKey);


            using (Aes aes = Aes.Create())//解密流水线
            {
                aes.Key = Keybyte;
                byte[] IVbyte = new byte[16];
                Array.Copy(Encryptionbyte, 0, IVbyte, 0, 16);//获取IV
                aes.IV = IVbyte;
                using (MemoryStream ms = new MemoryStream(Encryptionbyte, 16, Encryptionbyte.Length - 16))//内存流
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))//解密流
                    {
                        using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))//读取流，转为json
                        {
                            string SavedataJson = sr.ReadToEnd();
                            return JsonConvert.DeserializeObject<SaveData>(SavedataJson);//转为SaveData
                        }
                    }
                }
            }
        }
       catch(CryptographicException)
        {
            Debug.LogError("读档出错，解密时出问题，可能是读取的存档被错误修改过");
            return new SaveData(2);
        }
        catch(Exception e)
        {
            Debug.LogError($"读档出错，未知错误：{e.Message}");
            return new SaveData(2);
        }
    }



    

}
