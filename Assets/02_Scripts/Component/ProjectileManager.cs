using ArtsyNetcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public struct ProjectileData
{
    public Vector3 direction;
    public GameObject obj;
}

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private GameObject projectilePool = null;
    private Dictionary<ProjectileSO,List<ProjectileData>> projectiles = new Dictionary<ProjectileSO,List<ProjectileData>>();
    private Dictionary<ProjectileSO, List<ProjectileData>> usedProjectiles = new Dictionary<ProjectileSO, List<ProjectileData>>();
    private System.Random pseudoRandom = new System.Random(0);
    private static ProjectileManager instance = null;

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

    public void SpawnProjectile(WeaponSO weapon,Vector3 position,Quaternion rotation,float timeStamp = 0)
    {
        for (int i = 0; i < weapon.projectile.Count; i++)
        {
            if (!projectiles.ContainsKey(weapon.projectile[i]))
            {
                projectiles[weapon.projectile[i]] = new List<ProjectileData>();
            }
        }
        int sp = weapon.spawnProjectileCount + pseudoRandom.Next(weapon.spawnProjectileAddRandom);
        for (int i = 0; i < sp; i++)
        {
            ProjectileSO p = weapon.projectile[pseudoRandom.Next(weapon.projectile.Count)];
            Quaternion rot = rotation * Quaternion.Euler(0f, weapon.offsetAngle + weapon.Angle, 0f);
            ProjectileData pd;
            pd.direction = rot * Vector3.forward;
            pd.obj = Instantiate(p.prefab[pseudoRandom.Next(p.prefab.Count)], position, rot, projectilePool.transform);
            UpdateProjectile(p, pd, timeStamp);
            projectiles[p].Add(pd);
        }
    }

    public void Update()
    {
        foreach(KeyValuePair<ProjectileSO, List<ProjectileData>> pair in projectiles)
        {
            for (int i = 0; i < pair.Value.Count; i++)
            {
                UpdateProjectile(pair.Key, pair.Value[i],Time.deltaTime);
            }
        }
    }

    public void UpdateProjectile(ProjectileSO p, ProjectileData pd,float time)
    {
        switch(p.moveProjectileType)
        {
            case MoveProjectileType.Linear:
                pd.obj.transform.position += pd.direction * p.speed * time;
                break;
            case MoveProjectileType.Sin:
                break;
            default:
                break;
        }
    }
}
