using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtsyNetcode;
using Unity.Netcode;

public class PlayerManager : NetEntity
{
    [SerializeField] private GameObject prefabPlayer = null;
    private GameObject[] arraySpawnPoint;

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
            no = GetComponent<NetworkObject>();
            no.SpawnAsPlayerObject(clientId);
            no.ChangeOwnership(clientId);
            i++;
        }
    }
}
