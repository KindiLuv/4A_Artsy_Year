using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    //TODO
    //private Role _playerClass;
    
    //TODO
    //private Inventory _playerInventory;
    
    //TODO
    private Equipment _playerEquipment;

    [SerializeField] private GameObject _playerHand;

    private void Start()
    {
        if (GetComponent<Equipment>())
        {
            _playerEquipment = GetComponent<Equipment>();
        }
        else
        {
            _playerEquipment = gameObject.AddComponent<Equipment>();
            //TODO
            //Load player save data
        }
        
        UpdatePlayerEquipment();
        
    }

    public void UpdatePlayerEquipment()
    {
        Instantiate(_playerEquipment.GetMainWeeapon().weaponModel, _playerHand.transform);
    }

    //Load Save data
    public static T LoadScriptableObject<T>() where T : ScriptableObject
    {
        System.Type type = typeof(T);
        return Resources.Load<T>(type.ToString());
    }
    
    
}
