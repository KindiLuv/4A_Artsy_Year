using ArtsyNetcode;
using Assets.Scripts.NetCode;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using System;

public class Player : NetEntity
{
    private int characterID = -1;
    private int weaponID = -1;
    private CharacterSO _character;
    private WeaponSO _weapon;

    private PlayerController _playerController;
    [SerializeField] private Animator _playerHand;
    [SerializeField] private GameObject playerModelSpawn = null;

    private static readonly int Attacking = Animator.StringToHash("Attacking");

    #region Getter Setter

    public int CharacterID { get { return characterID; } set { characterID = value; } }
    public int WeaponID { get { return weaponID; } set { weaponID = value; } }
    public WeaponSO Weapon { get { return _weapon; } }
    #endregion


    protected void Start()
    {
        _playerController = GetComponent<PlayerController>();
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
                if (!GameNetworkManager.IsOffline)
                {
                    _playerController = GetComponent<PlayerController>();
                    _playerController.SetupCSO(_character);
                }
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
            int enumSize = Enum.GetValues(typeof(WeaponType)).Length;
            for (int i = 0; i < enumSize;i++)
            {
                _playerHand.SetLayerWeight(i+1, 0.0f);
            }
            _playerHand.SetLayerWeight((int)_weapon.weaponType+1, 1.0f); 
        }
        characterID = ci;
        weaponID = wi;
    }

    public bool HasWeapon()
    {
        return _weapon != null;
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
            ProjectileManager.Instance.SpawnProjectile(_weapon, pos, rot, _playerController.Team,((float)NetworkManager.Singleton.LocalTime.Time) - time);
        }
    }

    public static T LoadScriptableObject<T>() where T : ScriptableObject
    {
        System.Type type = typeof(T);
        return Resources.Load<T>(type.ToString());
    }       
}
