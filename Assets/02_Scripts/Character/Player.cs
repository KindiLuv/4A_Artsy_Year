using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{

    private Role _playerClass;
    
    //TODO
    //private Inventory _playerInventory;
    
    //TODO
    private Equipment _playerEquipment;

    [SerializeField] private Animator _playerHand;
    private static readonly int Attacking = Animator.StringToHash("Attacking");

    // [SerializeField] private Ani

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

    public void BasicAttack()
    {
        
        switch ((int)_playerEquipment.GetMainWeeapon().weaponType)
        {
            case 0:
                break;
            case 1:
                _playerHand.SetTrigger(Attacking);
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            default:
                break;
        }
    }

    //Load Save data
    public static T LoadScriptableObject<T>() where T : ScriptableObject
    {
        System.Type type = typeof(T);
        return Resources.Load<T>(type.ToString());
    }
    
    
}
