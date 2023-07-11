using ArtsyNetcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class BiomeRoom : NetEntity//Instantiate enemy/vague when player enter in room
{
    [SerializeField] private List<OpeningDoor> doors;
    private bool isVisited = false;
    public void SetBiomeRoom(List<OpeningDoor> d)
    {
        doors = d;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.gameObject.tag == "Player" && !isVisited)
        {
            isVisited = true;
            foreach (OpeningDoor od in doors)
            {
                od.Locked = true;
            }
            //NavMeshHit hit;
            //if (NavMesh.SamplePosition(other.transform.position, out hit, Mathf.Infinity, -1))
            {
                foreach (Character c in PlayerManager.instance.players)
                {
                    if (c.gameObject != other.gameObject)
                    {
                        TeleportClientRpc(other.transform.position, c.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                }
            }
        }
    }


    [ClientRpc]
    protected void TeleportClientRpc(Vector3 teleportTarget, ulong netId)
    {
        NetworkObject obj = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out obj))
        {
            obj.GetComponent<Character>().Teleportation(teleportTarget);
        }
    }
}
