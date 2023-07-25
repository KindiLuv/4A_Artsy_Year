using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UICoin : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText = null;
    private int coin;
    private static UICoin instance = null;
    private void Awake()
    {
        instance = this;
    }

    #region Getter Setter
    public static UICoin Instance { get { return instance; } }
    public int CoinNumber { get { return coin; } 
        set 
        {
            coin = value;
            coinText.text = coin.ToString();
        } 
    }
    #endregion
}
