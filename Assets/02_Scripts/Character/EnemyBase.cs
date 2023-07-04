using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : Enemy
{
    private NavMeshAgent _navMeshAgent;
    
    IEnumerator Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;
        while (true)
        {
            yield return new WaitForSeconds(2);
            BasicAttack(transform.position, transform.rotation, (float) NetworkManager.Singleton.LocalTime.Time);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        _navMeshAgent.destination = PlayerManager.instance.players[0].transform.position;
        transform.LookAt(PlayerManager.instance.players[0].transform.position);
        Debug.DrawRay(transform.position, transform.forward, Color.red);
    }
}
