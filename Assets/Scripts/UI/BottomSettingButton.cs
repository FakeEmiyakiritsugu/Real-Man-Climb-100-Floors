using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomSettingButton : MonoBehaviour
{
    public GameObject SettingPanel;

    public void OnSaveSettingButtonPressed()
    {
        PlayerPrefs.Save();
        SettingPanel.SetActive(false);
    }

    public void OnBackSettingButtonPressed()
    {
        SettingPanel.SetActive(false);
    }
}
