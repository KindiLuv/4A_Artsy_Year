using System;
using UnityEngine;

public class OOBRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    
    private void OnTriggerStay(Collider other)
    {
        other.transform.position = respawnPoint.transform.position;
    }
}
