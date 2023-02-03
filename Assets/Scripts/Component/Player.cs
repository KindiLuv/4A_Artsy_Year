using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtsyNetcode;
using Unity.Netcode;

public class Player : Character
{
    [ClientRpc] public void PingClientRPC()
    {
        Debug.LogError("pinging server");
        PongServerRpc(GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc] public void PongServerRpc(ulong id)
    {
        Debug.LogError($"client id : {id}");
        PingClientRPC();
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

