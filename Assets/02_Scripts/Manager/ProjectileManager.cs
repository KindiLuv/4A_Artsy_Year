using ArtsyNetcode;
using Assets.Scripts.NetCode;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.TextCore.Text;

public class ProjectileData
{
    public Vector3 direction;
    public GameObject obj;
    public float timeLeft;
    public int wallBoundsLeft;
    public float damage;
    public Team teamProjectile;
    public float pioffset;
}

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private GameObject projectilePool = null;
    [SerializeField] private LayerMask collisionMask = default;
    private Dictionary<ProjectileSO,List<ProjectileData>> projectiles = new Dictionary<ProjectileSO,List<ProjectileData>>();
    private Dictionary<ProjectileSO, Queue<ProjectileData>> usedProjectiles = new Dictionary<ProjectileSO, Queue<ProjectileData>>();
    private System.Random pseudoRandom = new System.Random(0);
    private static ProjectileManager instance = null;
    private List<IDamageable> targetIDamageable = new List<IDamageable>();
    private bool hitWall = false;
    private Vector3 targetImpact = Vector3.zero;
    private int indexUpdate = 0;
    private Collider[] colliders = new Collider[256];
    #region Getter Setter

    public static ProjectileManager Instance { get { return instance; } }
    #endregion


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public int RandomCheck()
    {
        return pseudoRandom.Next();
    }

    public void RandomReset(int s)
    {
        pseudoRandom = new System.Random(s);
        ResetUsedProjectiles();
    }

    public void ResetUsedProjectiles()
    {
        foreach (KeyValuePair<ProjectileSO, Queue<ProjectileData>> pair in usedProjectiles)
        {
            foreach (ProjectileData element in pair.Value)
            {
                Destroy(element.obj);
            }
        }
        usedProjectiles.Clear();
    }

    public void SpawnProjectile(WeaponSO weapon,Vector3 position,Quaternion rotation,Team t,float timeStamp = 0)
    {
        for (int i = 0; i < weapon.projectile.Count; i++)
        {
            if (!projectiles.ContainsKey(weapon.projectile[i]))
            {
                projectiles[weapon.projectile[i]] = new List<ProjectileData>();
                usedProjectiles[weapon.projectile[i]] = new Queue<ProjectileData>();
            }
        }
        int sp = weapon.spawnProjectileCount + pseudoRandom.Next(weapon.spawnProjectileAddRandom);
        for (int i = 0; i < sp; i++)
        {
            ProjectileSO p = weapon.projectile[pseudoRandom.Next(weapon.projectile.Count)];
            Quaternion rot = rotation * Quaternion.Euler(0f, weapon.offsetAngle + (float)pseudoRandom.NextDouble()* weapon.Angle, 0f);
            ProjectileData pd = new ProjectileData();
            if (usedProjectiles[p].Count == 0)
            {                
                pd.direction = rot * Vector3.forward;
                pd.obj = Instantiate(p.prefab[pseudoRandom.Next(p.prefab.Count)], position+ (rot * weapon.spawnProjectileLocalPosition), rot, projectilePool.transform);
            }
            else
            {
                pd = usedProjectiles[p].Dequeue();
                pd.direction = rot * Vector3.forward;
                pd.obj.SetActive(true);
                pseudoRandom.Next(p.prefab.Count);//juste pour etre sur au niveaux de la synchronisation
                pd.obj.transform.position = position+(rot * weapon.spawnProjectileLocalPosition);
                pd.obj.transform.rotation = rot;
            }
            if(weapon.muzzelFlash != null)
            {
                Destroy(Instantiate(weapon.muzzelFlash, pd.obj.transform.position, pd.obj.transform.rotation, projectilePool.transform) ,weapon.muzzelDestroyTime);
            }
            pd.timeLeft = p.lifeTime;
            pd.wallBoundsLeft = p.wallBounds;
            pd.damage = weapon.dmgPerHit + pseudoRandom.Next(weapon.dmgPerHitAddRandom);
            pd.teamProjectile = t;   
            if(p.moveProjectileType == MoveProjectileType.Sin)
            {
                pd.pioffset = ((Mathf.PI * 2.0f) - (p.lifeTime * p.sinFrequence) % (Mathf.PI * 2.0f)) + 1.3f;//<== tg c'est magique
            }
            projectiles[p].Add(pd);
            UpdateProjectile(p, pd, timeStamp);
        }
    }

    public void FixedUpdate()
    {        
        foreach(KeyValuePair<ProjectileSO, List<ProjectileData>> pair in projectiles)
        {            
            for (indexUpdate = 0; indexUpdate < pair.Value.Count; indexUpdate++)
            {
                UpdateProjectile(pair.Key, pair.Value[indexUpdate],Time.fixedDeltaTime);
            }
        }
    }

    public void DespawnProjectile(ProjectileSO p, ProjectileData pd)
    {
        pd.obj.SetActive(false);        
        projectiles[p].Remove(pd);
        usedProjectiles[p].Enqueue(pd);
        indexUpdate--;
    }

    public void ExplodeProjectile(ProjectileSO p, ProjectileData pd,List<IDamageable> cs,bool hw)
    {
        List<IDamageable> hitIDamageable = new List<IDamageable>();
        for (int i = 0; i < cs.Count; i++)
        {
            if(
                pd.teamProjectile == Team.Player && cs[i].GetTeam() == Team.Enemy
            ||  pd.teamProjectile == Team.Enemy && cs[i].GetTeam() == Team.Player
            || pd.teamProjectile == Team.PlayerFF && cs[i].GetTeam() == Team.PlayerFF
            || cs[i].GetTeam() == Team.Object)
            {
                hitIDamageable.Add(cs[i]);
            }
        }        
        if(hitIDamageable.Count > 0 && GameNetworkManager.IsOffline || NetworkManager.Singleton.IsServer)
        {
            for (int i = 0; i < hitIDamageable.Count; i++)
            {
                hitIDamageable[i].TakeDamage(pd.damage);
            }
        }
        if (hw || hitIDamageable.Count > 0)
        {
            if (p.explodeEffect != null)
            {
                Destroy(Instantiate(p.explodeEffect, pd.obj.transform.position, pd.obj.transform.rotation, projectilePool.transform), p.timeExplode);
            }
            DespawnProjectile(p, pd);
        }
    }

    public bool isCollide(ProjectileSO p, ProjectileData pd)
    {
        Vector3 rotateOffsetPosition = pd.obj.transform.rotation*p.collideOffset;
        targetIDamageable.Clear();
        hitWall = false;
        int numColliders = 0;
        if (p.shapeProjectileType == ShapeProjectileType.Sphere)
        {
            numColliders = Physics.OverlapSphereNonAlloc(pd.obj.transform.position+ rotateOffsetPosition, p.radius,colliders, collisionMask);
        }
        else if (p.shapeProjectileType == ShapeProjectileType.Box)
        {
            numColliders = Physics.OverlapBoxNonAlloc(pd.obj.transform.position+ rotateOffsetPosition, p.extends, colliders,Quaternion.identity, collisionMask);
        }
        else if (p.shapeProjectileType == ShapeProjectileType.Raycast)
        {
            RaycastHit[] hits = null;
            Ray r = new Ray(pd.obj.transform.position+ rotateOffsetPosition, pd.direction);
            hits = Physics.RaycastAll(r, p.rayDistance);
            if(hits != null)
            {
                for(int i = 0; i < hits.Length; i++)
                {
                    IDamageable c = hits[i].collider.GetComponent<IDamageable>();
                    if (c != null)
                    {
                        targetIDamageable.Add(c);                                                
                    }
                    else
                    {
                        hitWall = true;
                        targetImpact = hits[i].normal;
                        return true;
                    }                    
                }
                if(targetIDamageable.Count > 0)
                {
                    targetImpact = Vector3.zero;
                    hitWall = false;
                    return true;
                }
            }
        }
        else if(p.shapeProjectileType == ShapeProjectileType.ArcSphere)
        {
            numColliders = Physics.OverlapSphereNonAlloc(pd.obj.transform.position + rotateOffsetPosition, p.radius,colliders, collisionMask);
            Plane plane = new Plane(pd.obj.transform.rotation*Vector3.forward, pd.obj.transform.position + rotateOffsetPosition);

            int newSize = 0;
            for (int i = 0; i < numColliders; i++)
            {
                if (plane.GetDistanceToPoint(colliders[i].transform.position) > 0)
                {
                    colliders[newSize++] = colliders[i];
                }
            }
            numColliders = newSize;
        }

        if (numColliders > 0)
        {
            int IDamageableCount = 0;
            for (int i = 0; i < numColliders; i++)
            {
                IDamageable c = colliders[i].GetComponent<IDamageable>();
                if (c != null)
                {
                    targetIDamageable.Add(c);
                    IDamageableCount++;
                }
            }            
           
            targetImpact = Vector3.zero;
            hitWall = false;
            if (IDamageableCount != numColliders)
            {
                RaycastHit hit;
                if (Physics.Raycast(pd.obj.transform.position + rotateOffsetPosition, pd.direction, out hit, Mathf.Infinity, collisionMask))
                {
                    targetImpact = hit.normal;
                    hitWall = true;
                }
            }   
            
            return true;
        }
        targetImpact = Vector3.zero;
        return false;
    }

    public void UpdateProjectile(ProjectileSO p, ProjectileData pd,float time)
    {
        switch(p.moveProjectileType)
        {
            case MoveProjectileType.Static:
                break;
            case MoveProjectileType.Linear:
                pd.obj.transform.position += pd.direction * p.speed * time;
                break;
            case MoveProjectileType.Sin:
                pd.obj.transform.position += pd.direction * p.speed * time + (Mathf.Sin(pd.timeLeft* p.sinFrequence + pd.pioffset) * Vector3.Cross(Vector3.up,pd.direction).normalized * p.sinForce);
                break;
            case MoveProjectileType.Follow:
                int numColliders = 0;
                numColliders = Physics.OverlapSphereNonAlloc(pd.obj.transform.position,p.radiusSearch,colliders,collisionMask);
                int IDnearIDamagable = -1;
                float d = Mathf.Infinity;
                for(int i = 0; i < numColliders; i++)
                {
                    IDamageable idam = colliders[i].GetComponent<IDamageable>();
                    if (idam != null && idam.GetTeam() != pd.teamProjectile)
                    {
                        float nd = Vector3.Distance(pd.obj.transform.position, colliders[i].transform.position);
                        if (nd < d)
                        {
                            d = nd;
                            IDnearIDamagable = i;
                        }
                    }
                }
                if(IDnearIDamagable >= 0)
                {
                    Vector3 dir = (colliders[IDnearIDamagable].transform.position - pd.obj.transform.position).normalized;
                    dir.y = 0;
                    pd.direction = Vector3.Lerp(pd.direction, dir, p.colapseSearchSpeed * Time.deltaTime);
                }
                pd.obj.transform.position += pd.direction * p.speed * time;
                break;
            default:
                break;
        }
        pd.timeLeft -= time;
        if(pd.timeLeft <= 0.0f)
        {
            DespawnProjectile(p, pd);
            return;
        }

        if(isCollide(p,pd))
        {
            if(pd.wallBoundsLeft > 0 && targetIDamageable.Count == 0 && hitWall)
            {
                pd.wallBoundsLeft--;
                Vector3 reflectionDirection = Vector3.Reflect(pd.direction, targetImpact);
                reflectionDirection.y = pd.direction.y;
                pd.direction = reflectionDirection;
            }
            else
            {
                ExplodeProjectile(p, pd, targetIDamageable, hitWall);
            }
        }
    }
}
