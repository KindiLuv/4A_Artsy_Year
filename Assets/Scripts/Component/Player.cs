using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

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

    private void Start()
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

