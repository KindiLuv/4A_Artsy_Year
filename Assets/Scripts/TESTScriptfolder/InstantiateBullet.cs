using System;
using System.Collections;
using System.Collections.Generic;
using ArtsyNetcode;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class InstantiateBullet : NetEntity
{
    [SerializeField] private GameObject bullet;
    private List<GameObject> bulletList;
    
    [ServerRpc]
    private void InstantiateBulletAndMovesItServerRPC()
    {
        bulletList.Add(Instantiate(bullet));
        NetworkObject obj = GetComponent<NetworkObject>();
        TestClientRPC(obj.NetworkObjectId);
    }

    [ClientRpc]
    private void TestClientRPC(ulong id)
    {
        
    }
    
    private void Update()
    {
        foreach (var o in bulletList)
        {
            o.transform.position += Vector3.forward;
        }

        if (!IsServer)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                InstantiateBulletAndMovesItServerRPC();
            }
        }
    }
}
