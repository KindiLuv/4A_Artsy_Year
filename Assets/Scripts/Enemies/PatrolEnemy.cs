using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PatrolEnemy : IDamageable
{
    [SerializeField] private StaticEnemySO staticEnemySo;
    private Vector3 target;

    private void Start()
    {
        target = staticEnemySo.goal1;
        GetComponent<MeshRenderer>().material.color = staticEnemySo.color;
    }

    private void Update()
    {
        var step =  staticEnemySo.speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);
        transform.LookAt(target);
        
        if (Vector3.Distance(transform.position, target) < 0.001f)
        {
            target = target == staticEnemySo.goal1 ? staticEnemySo.goal2 : staticEnemySo.goal1;
        }
    }
}
