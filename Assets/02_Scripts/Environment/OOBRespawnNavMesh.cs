using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
public class OOBRespawnNavMesh : OOBRespawn
{
    protected override void OnTriggerStay(Collider other)
    {
        Character character = other.GetComponent<Character>();
        NavMeshHit myNavHit;
        if (NetworkManager.Singleton.IsServer && character != null)
        {
            if (NavMesh.SamplePosition(character.LastGroundPosition, out myNavHit, 20, -1))
            {
                character.Teleportation(myNavHit.position);
            }
        }
    }
}
