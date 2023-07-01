using ArtsyNetcode;
using Assets.Scripts.NetCode;
using System.Collections;
using System.Collections.Generic;
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
}

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private GameObject projectilePool = null;
    [SerializeField] private LayerMask collisionMask = default;
    private Dictionary<ProjectileSO,List<ProjectileData>> projectiles = new Dictionary<ProjectileSO,List<ProjectileData>>();
    private Dictionary<ProjectileSO, Queue<ProjectileData>> usedProjectiles = new Dictionary<ProjectileSO, Queue<ProjectileData>>();
    private System.Random pseudoRandom = new System.Random(0);
    private static ProjectileManager instance = null;
    private List<Character> targetCharacter = new List<Character>();
    private bool hitWall = false;
    private Vector3 targetImpact = Vector3.zero;
    private int indexUpdate = 0;
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

    public void ExplodeProjectile(ProjectileSO p, ProjectileData pd,List<Character> cs,bool hw)
    {
        List<Character> hitChracter = new List<Character>();
        for (int i = 0; i < cs.Count; i++)
        {
            if(
                pd.teamProjectile == Team.Player && cs[i].Team == Team.Enemy
            ||  pd.teamProjectile == Team.Enemy && cs[i].Team == Team.Player
            || pd.teamProjectile == Team.PlayerFF && cs[i].Team == Team.PlayerFF)
            {
                hitChracter.Add(cs[i]);
            }
        }        
        if(hitChracter.Count > 0 && GameNetworkManager.IsOffline || NetworkManager.Singleton.IsServer)
        {
            for (int i = 0; i < hitChracter.Count; i++)
            {
                hitChracter[i].TakeDamage(pd.damage);
            }
        }
        if (hw || hitChracter.Count > 0)
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
        Collider[] hitCollider = null;
        Vector3 rotateOffsetPosition = pd.obj.transform.rotation*p.collideOffset;
        targetCharacter.Clear();
        hitWall = false;
        if (p.shapeProjectileType == ShapeProjectileType.Sphere)
        {
            hitCollider = Physics.OverlapSphere(pd.obj.transform.position+ rotateOffsetPosition, p.radius, collisionMask);
        }
        else if (p.shapeProjectileType == ShapeProjectileType.Box)
        {
            hitCollider = Physics.OverlapBox(pd.obj.transform.position+ rotateOffsetPosition, p.extends,Quaternion.identity, collisionMask);
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
                    Character c = hits[i].collider.GetComponent<Character>();
                    if (c != null)
                    {
                        targetCharacter.Add(c);                                                
                    }
                    else
                    {
                        hitWall = true;
                        targetImpact = hits[i].normal;
                        return true;
                    }                    
                }
                if(targetCharacter.Count > 0)
                {
                    targetImpact = Vector3.zero;
                    hitWall = false;
                    return true;
                }
            }
        }
        else if(p.shapeProjectileType == ShapeProjectileType.ArcSphere)
        {
            hitCollider = Physics.OverlapSphere(pd.obj.transform.position + rotateOffsetPosition, p.radius, collisionMask);
            Plane plane = new Plane(pd.obj.transform.rotation*Vector3.forward, pd.obj.transform.position + rotateOffsetPosition);
            List<Collider> filteredColliders = new List<Collider>();

            foreach (Collider collider in hitCollider)
            {
                if (plane.GetDistanceToPoint(collider.transform.position) > 0)
                {
                    filteredColliders.Add(collider);
                }
            }

            hitCollider = filteredColliders.ToArray();
        }

        if (hitCollider != null)
        {
            for (int i = 0; i < hitCollider.Length; i++)
            {
                Character c = hitCollider[i].GetComponent<Character>();
                if (c != null)
                {
                    targetCharacter.Add(c);
                    targetImpact = Vector3.zero;
                    hitWall = false;
                    return true;
                }
            }            
            if(hitCollider.Length > 0)
            {
                targetImpact = Vector3.zero;
                hitWall = false;
                RaycastHit hit;
                if(Physics.Raycast(pd.obj.transform.position+ rotateOffsetPosition, pd.direction, out hit, Mathf.Infinity, collisionMask))
                {
                    targetImpact = hit.normal;
                    hitWall = true;
                }
                return true;
            }            
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
                pd.obj.transform.position += pd.direction * p.speed * time + (Mathf.Sin(pd.timeLeft) * Vector3.Cross(Vector3.up,pd.direction).normalized * p.sinForce);
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
            if(pd.wallBoundsLeft > 0 && targetCharacter.Count == 0 && hitWall)
            {
                pd.wallBoundsLeft--;
                Vector3 reflectionDirection = Vector3.Reflect(pd.direction, targetImpact);
                reflectionDirection.y = pd.direction.y;
                pd.direction = reflectionDirection;
            }
            else
            {
                ExplodeProjectile(p, pd, targetCharacter, hitWall);
            }
        }
    }
}
