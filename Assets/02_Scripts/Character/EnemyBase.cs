using System.Collections;
using Assets.Scripts.NetCode;
using Unity.Netcode;
using Unity.VisualScripting;
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
    private Vector3 randomDirection;
    private Vector3 targetRandomDirection;
    private float timeChangeRandomDirection;
    private float timeIdelChangePosition;
    private LayerMask lm;
    private Vector3 lastPosSpeed = Vector3.zero;

    protected override void Start()
    {
        base.Start();
        lm = 1 << 8 | 1 << 9;
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

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if(IsServer && targetPlayer == null)
        {
            float distance = Mathf.Infinity;
            int idt = -1;
            for (int i = 0; i < PlayerManager.instance.players.Count; i++)
            {
                float d = Vector3.Distance(PlayerManager.instance.players[i].transform.position, transform.position);
                if (d < distance)
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
        if (_enemy.effectDie != null)
        {
            Destroy(Instantiate(_enemy.effectDie, transform.position, transform.rotation), 2.0f);
        }
        Destroy(gameObject);
    }

    protected override void Update()
    {
        base.Update();

        if (_animator != null)
        {
            if (_navMeshAgent == null)
            {
                _animator.SetFloat(ASpeed, ((lastPosSpeed-transform.position).magnitude * 10.0f) / _speed);
                lastPosSpeed = transform.position;
            }
            else
            {
                _animator.SetFloat(ASpeed, _navMeshAgent.velocity.magnitude / _navMeshAgent.speed);
            }
        }
        if (GameNetworkManager.IsOffline || IsServer)
        {
            if (!_ded)
            {
                timeChangePlayer -= Time.deltaTime;
                if (timeChangePlayer <= 0.0f)
                {
                    timeChangePlayer = 0.5f;
                    float distance = _enemy.maxFollowDistance;
                    int idt = -1;
                    RaycastHit hit;
                    Vector3 offsetUp = transform.position + Vector3.up;
                    for (int i = 0; i < PlayerManager.instance.players.Count; i++)
                    {
                        float d = Vector3.Distance(PlayerManager.instance.players[i].transform.position, offsetUp);
                        if (d < distance && PlayerManager.instance.players[i].isAlive())
                        {                            
                            if (Physics.Raycast(offsetUp, (PlayerManager.instance.players[i].transform.position- offsetUp).normalized, out hit,Mathf.Infinity, lm))
                            {
                                if (hit.transform == PlayerManager.instance.players[i].transform)
                                {
                                    distance = d;
                                    idt = i;
                                }
                            }
                            //Debug.DrawRay(offsetUp, PlayerManager.instance.players[i].transform.position - offsetUp);
                        }
                    }
                    if (idt >= 0)
                    {
                        targetPlayer = PlayerManager.instance.players[idt];
                    }
                    else
                    {
                        targetPlayer = null;
                    }
                }
                if (targetPlayer != null)
                {
                    timeToShoot -= Time.deltaTime;
                    direction = targetPlayer.transform.position - transform.position;
                    direction.y = 0;
                    transform.rotation = Quaternion.LookRotation(direction);
                    RaycastHit hit;
                    Vector3 offsetUp = transform.position + Vector3.up;
                    if (Physics.Raycast(offsetUp, (targetPlayer.transform.position - offsetUp).normalized, out hit, Mathf.Infinity, lm))
                    {
                        if (hit.transform == targetPlayer.transform)
                        {
                            timeChangePlayer = _enemy.timeLostPlayer;
                            _navMeshAgent.destination = ((transform.position - targetPlayer.transform.position).normalized * _enemy.minDistanceTarget) + targetPlayer.transform.position + transform.TransformDirection(randomDirection);
                            if (timeToShoot <= 0 && _currentWeapon != -1)
                            {
                                BasicAttackClientRpc(transform.position, transform.rotation, (float)NetworkManager.Singleton.LocalTime.Time);
                                timeToShoot = _weapons[_currentWeapon].spawnProjectileRate;
                            }
                            timeChangeRandomDirection -= Time.deltaTime;
                            if(timeChangeRandomDirection <= 0.0f)
                            {
                                timeChangeRandomDirection = _enemy.timeSmoothRandom;
                                targetRandomDirection = new Vector3(Random.Range(_enemy.randomMin.x, _enemy.randomMax.x), Random.Range(_enemy.randomMin.y, _enemy.randomMax.y), Random.Range(_enemy.randomMin.z, _enemy.randomMax.z));
                            }
                            randomDirection = Vector3.Lerp(randomDirection, targetRandomDirection, Time.deltaTime*5.0f);
                        }
                        else
                        {
                            _navMeshAgent.destination = targetPlayer.transform.position;
                        }
                    }
                    else
                    {
                        _navMeshAgent.destination = targetPlayer.transform.position;
                    }                                  
                }
                else if (timeIdelChangePosition <= 0.0f && _enemy.idelRandomSphereInsideUnits != 0.0f)
                {
                    timeIdelChangePosition = Random.Range(_enemy.minTimeRandomSIU, _enemy.maxTimeRandomSIU);
                    Vector2 iuc = Random.insideUnitCircle * _enemy.idelRandomSphereInsideUnits;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(transform.position + new Vector3(iuc.x, 0.0f, iuc.y), out hit, Mathf.Infinity, -1))
                    {
                        _navMeshAgent.destination = hit.position;
                    }
                }
                else
                {
                    timeIdelChangePosition -= Time.deltaTime;                    
                }
            }
            else
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }

    }

}
