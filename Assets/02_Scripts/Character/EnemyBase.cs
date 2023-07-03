using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.NetCode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : Enemy
{
    private NavMeshAgent _navMeshAgent;

    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        _navMeshAgent.destination = PlayerManager.instance.players[0].transform.position;
        Debug.Log("I have  : "+ _health);
    }
}
