using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehavior : IDamageable
{
    [SerializeField] private EnemySO enemyAttributes;
    [SerializeField] private Transform target;
    private NavMeshAgent _navMeshAgent;
    
    private void Start()
    {
        //Setting up enemy data
        AddLives(enemyAttributes.lives);
        
        GetComponent<MeshRenderer>().material.color = enemyAttributes.color;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = enemyAttributes.speed;
        
        //Debug
        Debug.Log($"Name : {enemyAttributes.firstName}");
        Debug.Log($"Lives : {GetLives()}");
        Debug.Log($"Is alive : {GetState()}");
        Debug.Log($"Speed : {enemyAttributes.speed}");
        Debug.Log($"Weapon (wip) : {enemyAttributes.weapon}");
    }
    
    private void Update()
    {
        _navMeshAgent.destination = target.position;
    }
}
