using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtsyNetcode;
using Unity.Netcode;

public class Player : Character
{
    [ClientRpc] public void PingClientRPC()
    {
        Debug.Log("pinging server");
        if (IsOwner)
        {
            PongServerRpc(GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    [ServerRpc] public void PongServerRpc(ulong id)
    {
        Debug.Log($"client id : {id}");
        //PingClientRPC();
    }
    
    void Start()
    {
        if (!IsServer)
        {
            return;
        }
        
        PingClientRPC();
    }

    void Update()
    {
        
    }
}

