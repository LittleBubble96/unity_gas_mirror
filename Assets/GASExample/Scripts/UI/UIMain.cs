using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSEngine.GAS;

public class UIMain : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI mrText;
    [SerializeField] private TextMeshProUGUI attackText;

    [Title("cd")]
    [SerializeField] private Image qCdImage;
    [SerializeField] private TextMeshProUGUI qCdText;
    private XYPlayer player => GASExampleManager.Instance.LocalPlayer;

    public void PlayerInfoUpdate()
    {
        if (!player)
        {
            return;
        }
        levelText.text = $"Level: {player.Level}";
        hpText.text = $"HP: {player.Hp}";
        mpText.text = $"MP: {player.Mp}";
        armorText.text = $"Armor: {player.Armor}";
        mrText.text = $"MR: {player.Mr}";
        attackText.text = $"Attack: {player.Atk}";
    }
    
    public void RefreshHp()
    {
        if (!player)
        {
            return;
        }
        hpText.text = $"HP: {player.Hp}";
    }
    
    public void RefreshMp()
    {
        if (!player)
        {
            return;
        }
        mpText.text = $"MP: {player.Mp}";
    }

    public void RefreshFireCd()
    {
        if (!player)
        {
            return;
        }
        float cd = player.GetFireCoolDown();
        float totalCd = player.GetFireTotalCoolDown();
        if (cd > 0 && totalCd > 0)
        {
            qCdImage.fillAmount = cd / totalCd;
            qCdText.text = $"{Mathf.CeilToInt(cd)}s";
        }
        else
        {
            qCdImage.fillAmount = 0;
            qCdText.text = "";
        }
    }

    private float _internal = 0.1f;
    private float _timerCount;

    private void Update()
    {
        if (_timerCount > _internal)
        {
            _timerCount = 0f;
            RefreshFireCd();
        }
        _timerCount += Time.deltaTime;
    }
}
