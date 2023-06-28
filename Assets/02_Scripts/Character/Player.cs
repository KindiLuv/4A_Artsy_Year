using ArtsyNetcode;
using Assets.Scripts.NetCode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetEntity
{
    private Role _playerClass;
    private int characterID = -1;
    private int weaponID = -1;
    private CharacterSO _character;
    private WeaponSO _weapon;

    [SerializeField] private Animator _playerHand;
    [SerializeField] private GameObject playerModelSpawn = null;

    private static readonly int Attacking = Animator.StringToHash("Attacking");

    #region Getter Setter

    public int CharacterID { get { return characterID; } set { characterID = value; } }
    public int WeaponID { get { return weaponID; } set { weaponID = value; } }

    #endregion


    private void Start()
    {
        if (GameNetworkManager.IsOffline)
        {
            LoadData(characterID,weaponID);            
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer)
        {
            return;
        }
        characterID = SaveManager.Instance.CurrentPlayerCharacterChoise;
        weaponID = SaveManager.Instance.CurrentPlayerWeaponChoise;
        LoadDataServerRpc(characterID,weaponID);
    }

    [ServerRpc]
    public void LoadDataServerRpc(int ci,int wi)
    {
        LoadDataClientRpc(ci, wi);
    }

    [ClientRpc]
    public void LoadDataClientRpc(int ci,int wi)
    {
        LoadData(ci, wi);
    }

    public void LoadData(int ci, int wi)
    {
        foreach(Transform t in playerModelSpawn.transform)
        {
            Destroy(t.gameObject);
        }
        if (ci >= 0)
        {
            _character = GameRessourceManager.Instance.Characters[ci% GameRessourceManager.Instance.Characters.Count];
            if (_character != null)
            {
                Instantiate(_character.Prefab, playerModelSpawn.transform.position, playerModelSpawn.transform.rotation, playerModelSpawn.transform);
            }
        }
        foreach (Transform t in _playerHand.transform)
        {
            Destroy(t.gameObject);
        }
        if (wi >= 0)
        {
            _weapon = GameRessourceManager.Instance.Weapons[wi % GameRessourceManager.Instance.Weapons.Count];
            if(_weapon != null)
            {
                Instantiate(_weapon.weaponModel, _playerHand.transform);
            }
        }
        characterID = ci;
        weaponID = wi;
    }


    [ServerRpc]
    public void SpawnProjectileServerRpc()
    {
        
    }

    [ClientRpc]
    public void SpawnProjectileClientRpc()
    {

    }

    public bool HasWeapon()
    {
        return _weapon != null;
    }

    public void BasicAttack()
    {        
        if(!HasWeapon())
        {
            return;
        }
        switch ((int)_weapon.weaponType)
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
