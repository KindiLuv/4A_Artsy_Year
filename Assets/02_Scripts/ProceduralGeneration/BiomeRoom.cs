using ArtsyNetcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeRoom : NetEntity//Instantiate enemy/vague when player enter in room
{
    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.gameObject.tag == "Player")
        {
            Debug.Log("Test");
        }
    }
}
