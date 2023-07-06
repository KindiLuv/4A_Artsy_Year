using System.Collections.Generic;
using UnityEngine;
using ArtsyNetcode;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerManager : NetEntity
{
    [SerializeField] private GameObject prefabPlayer = null;
    private GameObject[] arraySpawnPoint;
    public static PlayerManager instance;
    public List<Character> players;

    protected override void Awake()
    {
        base.Awake();
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        if(!IsServer)
        {
            return;
        } 
        arraySpawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint");        
        int i = 0;
        NetworkObject no = null;
        GameObject player;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            player = Instantiate(prefabPlayer,arraySpawnPoint[i%arraySpawnPoint.Length].transform.position,Quaternion.identity);
            players.Add(player.GetComponent<Character>());
            no = player.GetComponent<NetworkObject>();            
            no.SpawnAsPlayerObject(clientId);
            no.ChangeOwnership(clientId);
            i++;
        }
    }
}
