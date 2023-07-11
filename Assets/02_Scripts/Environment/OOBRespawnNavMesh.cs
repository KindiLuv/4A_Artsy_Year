using Unity.Netcode;
using UnityEngine;

public class OOBRespawnNavMesh : OOBRespawn
{
    protected override void OnTriggerStay(Collider other)
    {
        Character character = other.GetComponent<Character>();        
        if (NetworkManager.Singleton.IsServer && character != null)
        {
            
                character.TeleportationLGPClientRpc();
                character.TakeDamage(10);
            
        }
    }
}
