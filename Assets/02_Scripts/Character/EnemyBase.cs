using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : Enemy
{
    private NavMeshAgent _navMeshAgent;
    private Vector3 targetPositionImpulse = Vector3.zero;
    private bool isImpulse = false;

    IEnumerator Start()
    {
        /*_navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;*/
        while (true)
        {
            yield return new WaitForSeconds(2);
            BasicAttack(transform.position, transform.rotation, (float) NetworkManager.Singleton.LocalTime.Time);
        }
    }

    public override void KnockBack(Vector3 impulse)
    {
        if (impulse != Vector3.zero)
        {
            StartCoroutine(ImpulseMovementCoroutine(impulse, 0.2f));
        }
    }

    public IEnumerator ImpulseMovementCoroutine(Vector3 impulseForce, float impulseDuration)
    {
        targetPositionImpulse = transform.position + impulseForce;
        if (!isImpulse)
        {
            isImpulse = true;
            float timer = 0f;

            while (timer < impulseDuration)
            {
                float t = timer / impulseDuration;
                transform.position = Vector3.Lerp(transform.position, targetPositionImpulse, t);

                timer += Time.deltaTime;

                yield return null;
            }
            isImpulse = false;
        }
    }

    public override void Update()
    {
        base.Update();
        transform.LookAt(PlayerManager.instance.players[0].transform.position);
        Debug.DrawRay(transform.position, transform.forward, Color.red);
    }
}
