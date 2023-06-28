using ArtsyNetcode;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetEntity
{

    private Role _playerClass;
    
    //TODO: private Inventory _playerInventory;

    private Equipment _playerEquipment;

    private CharacterSO characterData = null;

    [SerializeField] private Animator _playerHand;
    private static readonly int Attacking = Animator.StringToHash("Attacking");

    #region Getter Setter

    public CharacterSO CharacterData { get { return characterData; } set { characterData = value; } }

    #endregion

    [SerializeField] private GameObject playerModelSpawn = null;

    private void Start()
    {
        if (GetComponent<Equipment>())
        {
            _playerEquipment = GetComponent<Equipment>();
        }
        else
        {
            _playerEquipment = gameObject.AddComponent<Equipment>();
        }
        if(SaveManager.Instance.CurrentPlayerChracterChoise >= 0)
        {
            characterData = GameRessourceManager.Instance.Chracters[SaveManager.Instance.CurrentPlayerChracterChoise];
        }
        LoadCharacterData();
        UpdatePlayerEquipment();        
    }    

    public void LoadCharacterData()
    {
        foreach(Transform t in playerModelSpawn.transform)
        {
            Destroy(t.gameObject);
        }
        if (characterData != null)
        {
            Instantiate(characterData.Prefab, playerModelSpawn.transform.position, playerModelSpawn.transform.rotation, playerModelSpawn.transform);
        }
    }


    public void UpdatePlayerEquipment()
    {
        if (HasWeapon())
        {
            Instantiate(_playerEquipment.GetMainWeeapon().weaponModel, _playerHand.transform);
        }
    }

    public bool HasWeapon()
    {
        return _playerEquipment.GetMainWeeapon() != null;
    }

    public void BasicAttack()
    {        
        if(!HasWeapon())
        {
            return;
        }
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

    public static T LoadScriptableObject<T>() where T : ScriptableObject
    {
        System.Type type = typeof(T);
        return Resources.Load<T>(type.ToString());
    }       
}
