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
    private List<GameObject> bulletList = new List<GameObject>();
    private List<float> bulletTime = new List<float>();
    
    [ServerRpc]
    private void InstantiateBulletAndMovesItServerRPC()
    {
        GameObject instantObj = Instantiate(bullet,transform.position,Quaternion.identity);
        bulletTime.Add(0.0f);
        bulletList.Add(instantObj);
        NetworkObject netObj = instantObj.GetComponent<NetworkObject>();
        netObj.Spawn();
        TestClientRPC(netObj.NetworkObjectId);
    }

    [ClientRpc]
    private void TestClientRPC(ulong networkId)
    {
        if(IsHost)
        {
            return;
        }
        NetworkObject obj = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out obj))
        {
            bulletList.Add(obj.gameObject);
            bulletTime.Add(0.0f);
        }
    }
    
    private void Update()
    {
            for (int i = 0; i < bulletList.Count;i++)
            {
                bulletList[i].transform.position += Vector3.forward * Time.deltaTime * 5.0f + (Mathf.Sin(bulletTime[i])*Vector3.left*Time.deltaTime*3.0f);
                bulletTime[i] += Time.deltaTime * 10.0f;
            }
        

            if (IsClient && IsLocalPlayer)
            {            
                if (Input.GetKey(KeyCode.E))
                {
                    InstantiateBulletAndMovesItServerRPC();
                }
            }
    }
}	 						  				  	 	 