using ArtsyNetcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class BiomeRoom : NetEntity//Instantiate enemy/vague when player enter in room
{
    [SerializeField] private List<OpeningDoor> doors;
    [SerializeField] private bool isVisited = false;
    [SerializeField] private int vagues = 0;
    [SerializeField] private Biome biome = null;
    [SerializeField] private List<Enemy> enemyInRoom = new List<Enemy>();
    [SerializeField] private List<Vector2Int> posibleSpawnPosition = new List<Vector2Int>();
    private List<int> idEnemy;
    private int[,] maps;
    private float wallHeight;
    public void SetBiomeRoom(List<OpeningDoor> d,int v,Biome b,int[,] m,List<int> ide,float wh)
    {
        this.doors = d;
        this.vagues = v;
        this.biome = b;
        this.idEnemy = ide;
        this.maps = m;
        this.wallHeight = wh;
        bool pp = false;
        for (int i = 0; i < m.GetLength(0); i++)
        {
            for (int j = 0; j < m.GetLength(1); j++)
            {                
                if (m[i, j] == 0)
                {
                    pp = true;
                    for (int x = -1; x <= 1 && pp; x ++)
                    {
                        for (int y = -1; y <= 1 && pp; y++)
                        {
                            if (i+x >= 0 && j+y >= 0 && i+x < m.GetLength(0) && j+y < m.GetLength(1))
                            {
                                if (m[i + x, j + y] == 1)
                                {
                                    pp = false;
                                }
                            }
                        }
                    }
                    if (pp)
                    {
                        posibleSpawnPosition.Add(new Vector2Int(i, j));
                    }
                }
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.gameObject.tag == "Player" && !isVisited)
        {
            isVisited = true;
            Debug.Log("isVisited");
            foreach (OpeningDoor od in doors)
            {
                od.Locked = true;
            }

            foreach (Character c in PlayerManager.instance.players)
            {
                if (c.gameObject != other.gameObject)
                {
                    TeleportClientRpc(other.transform.position, c.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
        }
    }

    private void Update()
    {
        if(!IsServer)
        {
            return;
        }
        for(int i = 0; i < enemyInRoom.Count; i++)
        {
            if(enemyInRoom[i] == null)
            {
                enemyInRoom.RemoveAt(i);
                i--;
            }
        }
        if(isVisited && this.vagues > 0 && enemyInRoom.Count == 0)
        {            
            this.vagues--;
            int enemyInV = biome.EnemyPerRoom + Random.Range(0,biome.AddRandomEnemyPerRoom);
            
            for(int i = 0; i < enemyInV && idEnemy.Count > 0; i++)
            {
                LoadEnemy(idEnemy[Random.Range(0, idEnemy.Count)],GetRadomPosition());
            }
            /*for(int i =0; i< posibleSpawnPosition.Count; i++)
            {
                Vector3 pos = new Vector3(transform.position.x + posibleSpawnPosition[i].x - (maps.GetLength(0) / 2.0f), -wallHeight, transform.position.z + posibleSpawnPosition[i].y - (maps.GetLength(1) / 2.0f));
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = pos;
            }*/
        }
        if(isVisited && this.vagues == 0 && enemyInRoom.Count == 0)
        {
            GetComponent<NetworkObject>().Despawn();            
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (isVisited)
        {
            foreach (OpeningDoor od in doors)
            {
                od.Locked = false;
            }
        }
        Destroy(gameObject);
    }

    public Vector3 GetRadomPosition()
    {
        Vector2Int posint = Vector2Int.zero;
        //NavMeshHit hit;
        if (posibleSpawnPosition.Count > 0)
        {
            posint = posibleSpawnPosition[Random.Range(0, posibleSpawnPosition.Count)];
        }
        Vector3 pos = new Vector3(transform.position.x + posint.x - (maps.GetLength(0)/2.0f), -wallHeight, transform.position.z + posint.y- (maps.GetLength(1) / 2.0f));
        /*if(NavMesh.SamplePosition(pos,out hit,Mathf.Infinity,-1))
        {
            pos = hit.position;
        }*/
        return pos;
    }

    public void LoadEnemy(int enemyId, Vector3 pos)
    {
        enemyInRoom.Add(Instantiate(GameRessourceManager.Instance.EnemyTypePrebab[(int)GameRessourceManager.Instance.Enemies[enemyId].EnemyType], pos, Quaternion.identity).GetComponent<Enemy>());
        Enemy p = enemyInRoom[enemyInRoom.Count - 1];
        p._enemy = GameRessourceManager.Instance.Enemies[enemyId];
        p.SetupEnemy();
        p.GetComponent<NetworkObject>().Spawn();
        p.LoadDataClientRpc(enemyId);
    }

    [ClientRpc]
    protected void TeleportClientRpc(Vector3 teleportTarget, ulong netId)
    {
        NetworkObject obj = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out obj))
        {
            obj.GetComponent<Character>().Teleportation(teleportTarget);
        }
    }
}