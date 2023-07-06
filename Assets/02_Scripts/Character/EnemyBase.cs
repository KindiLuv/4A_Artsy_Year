using System.Collections;
using Assets.Scripts.NetCode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : Enemy
{
    private NavMeshAgent _navMeshAgent;
    private Vector3 targetPositionImpulse = Vector3.zero;
    private bool isImpulse = false;
    private float timeChangePlayer = 0.5f;
    private float timeToShoot;
    private Character targetPlayer = null;
    private Vector3 direction;
    protected override void Start()
    {
        base.Start();
        if (IsServer)
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.speed = _speed;      
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

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if(_enemy.effectDie != null)
        {
            Destroy(Instantiate(_enemy.effectDie,transform.position,transform.rotation), 2.0f);
        }
        Destroy(gameObject);
    }

    protected override void Update()
    {
        base.Update();
        if(_animator != null)
        {
            _animator.SetFloat(ASpeed, _navMeshAgent.velocity.magnitude/ _navMeshAgent.speed);
        }
        if (GameNetworkManager.IsOffline || IsServer)
        {
            if (!_ded)
            {
                timeChangePlayer-=Time.deltaTime;
                if (timeChangePlayer <= 0.0f)
                {
                    timeChangePlayer = 0.5f;
                    float distance = _enemy.maxFollowDistance;
                    int idt = -1;
                    for (int i = 0; i < PlayerManager.instance.players.Count;i++)
                    {
                        float d = Vector3.Distance(PlayerManager.instance.players[i].transform.position, transform.position);
                        if(d < distance)
                        {
                            distance = d;
                            idt = i;
                        }
                    }
                    if (idt >= 0)
                    {
                        targetPlayer = PlayerManager.instance.players[idt];
                    }
                }
                if (targetPlayer != null)
                {
                    timeToShoot -= Time.deltaTime;
                    _navMeshAgent.destination = targetPlayer.transform.position;
                    direction = targetPlayer.transform.position - transform.position;
                    direction.y = 0;
                    transform.rotation = Quaternion.LookRotation(direction);
                    if(timeToShoot <= 0 && _currentWeapon != -1)
                    {
                        BasicAttackClientRpc(transform.position, transform.rotation, (float)NetworkManager.Singleton.LocalTime.Time);
                        timeToShoot = _weapons[_currentWeapon].spawnProjectileRate;
                    }
                }
                
            }
            else
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
        
    }
    
}
