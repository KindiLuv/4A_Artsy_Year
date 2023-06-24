using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
public class OOBRespawnNavMesh : OOBRespawn
{
    protected virtual void OnTriggerStay(Collider other)
    {
        NavMeshSurface surface = ProceduralMapManager.Instance.NavMesh;
        Character character = other.GetComponent<Character>();
        NavMeshHit myNavHit;
        if (IsServer && character != null)
        {
            if (NavMesh.SamplePosition(character.transform.position, out myNavHit, 10, -1))
            {
                character.Teleportation(myNavHit.position);
                TeleportClientRpc(myNavHit.position, character.GetComponent<NetworkObject>().NetworkObjectId);
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
