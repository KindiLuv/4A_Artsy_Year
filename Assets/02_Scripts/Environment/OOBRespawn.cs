using System;
using Unity.Netcode;
using UnityEngine;

public class OOBRespawn : NetworkBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerStay(Collider other)
    {
        Character character = other.GetComponent<Character>();
        if (IsServer && character != null)
        {
            character.Teleportation(respawnPoint.position);
            TeleportClientRpc(respawnPoint.position,character.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    [ClientRpc]
    private void TeleportClientRpc(Vector3 teleportTarget,ulong netId)
    {
        NetworkObject obj = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out obj))
        {
            obj.GetComponent<Character>().Teleportation(teleportTarget);
        }
    }
}
