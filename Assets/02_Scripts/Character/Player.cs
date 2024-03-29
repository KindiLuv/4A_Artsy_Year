using ArtsyNetcode;
using Assets.Scripts.NetCode;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEngine.SceneManagement;

public class Player : NetEntity
{
    private int characterID = -1;
    protected int _currentWeapon = -1;
    //private int weaponID = -1;
    private CharacterSO _character;
    protected List<WeaponSO> _weapons = new List<WeaponSO>();

    private PlayerController _playerController;
    [SerializeField] private Animator _playerHand;
    [SerializeField] private GameObject playerModelSpawn = null;

    private static readonly int Attacking = Animator.StringToHash("Attacking");

    #region Getter Setter

    public int CharacterID { get { return characterID; } set { characterID = value; } }
    
    public int CurrentWeapon { get { return _currentWeapon; } }
    //public int WeaponID { get { return weaponID; } set { weaponID = value; } }
    public List<WeaponSO> Weapons { get { return _weapons; } }
    #endregion

    protected void Start()
    {
        _playerController = GetComponent<PlayerController>();
        if (GameNetworkManager.IsOffline)
        {
            LoadData(characterID);            
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer)
        {
            return;
        }
        characterID = SaveManager.Instance.CurrentPlayerCharacterChoise;
        //weaponID = SaveManager.Instance.CurrentPlayerWeaponChoise;
        LoadDataServerRpc(characterID);
    }

    [ServerRpc]
    public void LoadDataServerRpc(int ci)
    {
        LoadDataClientRpc(ci);
    }

    [ClientRpc]
    public void LoadDataClientRpc(int ci)
    {
        LoadData(ci);
    }

    public void TakeLoot(LootableSO ls, int nb)
    {
        int id = GameRessourceManager.Instance.GetIdByLoot(ls);
        if(id == 0 && IsLocalPlayer)
        {
            UICoin.Instance.CoinNumber += nb;
        }
        else if(id == 1 && IsLocalPlayer)
        {  
            TakeHealServerRpc(nb);
        }
    }

    [ServerRpc]
    public void TakeHealServerRpc(int nb)
    {
        GetComponent<PlayerController>().HealDamage(nb * 4);
    }

    public void LoadData(int ci)
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
                if (!GameNetworkManager.IsOffline)
                {
                    _playerController = GetComponent<PlayerController>();
                    _playerController.SetupCSO(_character);
                    _weapons = new List<WeaponSO>(_character.Weapons);
                }
            }
        }
        foreach (Transform t in _playerHand.transform)
        {
            Destroy(t.gameObject);
        }
        if (_weapons.Count > 0)
        {
            if(_weapons[0] != null)
            {
                if (_weapons[0].weaponModel != null)
                {
                    Instantiate(_weapons[0].weaponModel, _playerHand.transform);
                }
                _currentWeapon = 0;
            }
            int enumSize = Enum.GetValues(typeof(WeaponType)).Length;
            for (int i = 0; i < enumSize;i++)
            {
                _playerHand.SetLayerWeight(i+1, 0.0f);
            }
            _playerHand.SetLayerWeight((int)_weapons[0].weaponType+1, 1.0f); 
        }
        characterID = ci;
    }

    public void LoadWeapon(int wi)
    {
        if (_weapons.Count > wi)
        {
            foreach (Transform t in _playerHand.transform)
            {
                Destroy(t.gameObject);
            }
            if(_weapons[wi] != null)
            {
                if (_weapons[wi].weaponModel != null)
                {
                    Instantiate(_weapons[wi].weaponModel, _playerHand.transform);
                }
                _currentWeapon = wi;
            }
            int enumSize = Enum.GetValues(typeof(WeaponType)).Length;
            for (int i = 0; i < enumSize;i++)
            {
                _playerHand.SetLayerWeight(i+1, 0.0f);
            }
            _playerHand.SetLayerWeight((int)_weapons[wi].weaponType+1, 1.0f); 
        }
    }

    public bool HasWeapon()
    {
        return _weapons.Count > 0;
    }
    
    [ServerRpc]
    public void AddWeaponServerRpc()
    {
        int wi = Random.Range(0, GameRessourceManager.Instance.Weapons.Count);
        AddWeaponClientRpc(wi);
    }

    [ClientRpc]
    public void AddWeaponClientRpc(int wi)
    {
        AddWeapon(wi);
    }

    public void AddRandomWeapon()
    {
        if (GameNetworkManager.IsOffline || IsLocalPlayer)
        {
            AddWeaponServerRpc();
        }
    }

    public void AddWeapon(int wi)
    {
        if (_weapons.Count == GameRessourceManager.Instance.Weapons.Count+1)
        {
            return;
        }
        
        if (_weapons.Contains(GameRessourceManager.Instance.Weapons[wi]))
        {
            AddWeapon((wi+1)%GameRessourceManager.Instance.Weapons.Count);
            return;
        }
        _weapons.Add(GameRessourceManager.Instance.Weapons[wi]);
    }

    [ServerRpc]
    public void BasicAttackServerRpc(Vector3 pos, Quaternion rot,ulong localClientId,double timeStamp)
    {        
        BasicAttack(pos,rot,(float)timeStamp,true);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(x => x != localClientId).ToList() }
        };
        BasicAttackClientRpc(pos, rot, timeStamp,clientRpcParams);
    }

    [ClientRpc]
    public void BasicAttackClientRpc(Vector3 pos, Quaternion rot,double timeStamp, ClientRpcParams clientRpcParams = default)
    {
        BasicAttack(pos, rot, (float)timeStamp);       
    }

    public void BasicAttack(Vector3 pos, Quaternion rot,float time,bool serveur = false)
    {        
        if(!HasWeapon())
        {
            return;
        }        
        _playerHand.SetTrigger(Attacking);
        if (IsLocalPlayer && !serveur)
        {
            BasicAttackServerRpc(pos,rot,NetworkManager.Singleton.LocalClientId, NetworkManager.Singleton.LocalTime.Time);
        }
        if (!serveur && IsClient || serveur && !IsClient)
        {
            ProjectileManager.Instance.SpawnProjectile(_weapons[_currentWeapon], pos, rot, _playerController.Team,((float)NetworkManager.Singleton.LocalTime.Time) - time);
        }
    }

    public static T LoadScriptableObject<T>() where T : ScriptableObject
    {
        System.Type type = typeof(T);
        return Resources.Load<T>(type.ToString());
    }       
}
