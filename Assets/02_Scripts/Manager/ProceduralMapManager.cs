using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.FilePathAttribute;

public class ProceduralMapManager : MonoBehaviour
{
    [SerializeField] private int chunckSize = 50;
    [SerializeField] private float wallSize = 4.0f;
    [SerializeField] private string seed;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] [Range(0, 100)] private int randomFillPercent = 50;
    [SerializeField] [Range(0, 100)] private int randomAddPercent = 0;
    [SerializeField] private int countZone = 10;
    [SerializeField] private int idBiome = 0;
    [SerializeField] private Material matVoid = null;
    private SpawnableObject[] spawnData;
    private Biome[] biomeData;
    private Dictionary<Vector2Int, Chunck> maps = new Dictionary<Vector2Int, Chunck>();
    private NavMeshSurface ns;
    private static ProceduralMapManager instance;

    #region GetterSetter
    public static ProceduralMapManager Instance { get { return instance; } }
    public NavMeshSurface NavMesh { get { return ns; } }
    #endregion
    struct Chunck
    {
        public int[,] map;
        public List<Spawnable> spawnables;
    }
    struct Spawnable
    {
        public int id;
        public Vector3 position;
        public Quaternion rotation;
    }
    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        spawnData = Resources.LoadAll<SpawnableObject>("SpawnableObject");
        biomeData = Resources.LoadAll<Biome>("Biome");
        Create();
    }

    float[] RightAngle = new float[]
    {
        0.0f,180.0f,90.0f,270.0f
    };

    public float CalculateAngle(RotateMode rm,System.Random pseudoRandom,float AngleMin, float Angle, out bool Rotate)
    {
        switch(rm)
        {
            case RotateMode.Zero:
                Rotate = true;
                return 0.0f;
            case RotateMode.No:
                Rotate = false;
                return 0.0f;
            case RotateMode.TwoRightAngleRandom:
                Rotate = true;
                return RightAngle[pseudoRandom.Next(2)];
            case RotateMode.FoorRightAngleRandom:
                Rotate = true;
                return RightAngle[pseudoRandom.Next(4)];
            case RotateMode.RandomBetweenTwoAngle:
                Rotate = true;
                return (float)(pseudoRandom.NextDouble()* Angle) + AngleMin;
            default:
                break;
        }
        Rotate = true;
        return 0.0f;
    }

    public List<Vector2Int> CheckIsSpawnable(int[,] map, Vector2Int size, int nb, System.Random pseudoRandom,bool onWall = false)
    {
        List<Vector2Int> spawnableLocations = new List<Vector2Int>();
        int targetIDSpawn = 0;
        int replaceID = 4;
        if(onWall)
        {
            targetIDSpawn = 1;
            replaceID = 5;
        }
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] == targetIDSpawn)
                {
                    bool isSpawnable = true;
                    for (int dx = -size.x; dx < size.x; dx++)
                    {
                        for (int dy = -size.y; dy < size.y; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= mapWidth || ny >= mapHeight || nx < 0 || ny < 0)
                            {
                                isSpawnable = false;
                                break;
                            }

                            if (map[nx, ny] != targetIDSpawn)
                            {
                                isSpawnable = false;
                                break;
                            }
                        }

                        if (!isSpawnable)
                        {
                            break;
                        }
                    }

                    if (isSpawnable)
                    {
                        spawnableLocations.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

          Shuffle(spawnableLocations, pseudoRandom);

        List<Vector2Int> finalSpawnableLocations = new List<Vector2Int>();
        while(finalSpawnableLocations.Count < nb && spawnableLocations.Count > 0)
        {
            Vector2Int location = spawnableLocations[spawnableLocations.Count-1];
            spawnableLocations.RemoveAt(spawnableLocations.Count - 1);
            bool isSpawnable = true;
            for (int dx = -size.x; dx < size.x; dx++)
            {
                for (int dy = -size.y; dy < size.y; dy++)
                {
                    int nx = location.x + dx;
                    int ny = location.y + dy;
                    if(map[nx, ny] == replaceID)
                    {
                        isSpawnable = false;
                        break;
                    }                    
                }
                if(!isSpawnable)
                { 
                    break;
                }
            }
            if(isSpawnable)
            {
                finalSpawnableLocations.Add(location);
                for (int dx = -size.x; dx < size.x; dx++)
                {
                    for (int dy = -size.y; dy < size.y; dy++)
                    {
                        int nx = location.x + dx;
                        int ny = location.y + dy;
                        map[nx, ny] = replaceID;
                    }
                }
            }
        }
        return finalSpawnableLocations;
    }

    private void Shuffle<T>(List<T> list, System.Random random)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n--);
            T temp = list[n];
            list[n] = list[k];
            list[k] = temp;
        }
    }

    public void Create()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        Biome b = GetBiomeID(idBiome);
        Dictionary<Vector2Int, KeyValuePair<List<Vector2Int>, List<Vector2Int>>> dungeon = CreateDungeon(Vector2Int.zero);
        int countBoss = 0;
        foreach (KeyValuePair<Vector2Int, KeyValuePair<List<Vector2Int>, List<Vector2Int>>> pair in dungeon)
        {
            CreateChunck(pair.Key, countBoss == dungeon.Count-1);
            countBoss++;
        }
        foreach (KeyValuePair<Vector2Int, KeyValuePair<List<Vector2Int>, List<Vector2Int>>> pair in dungeon)
        {
            for (int i = 0; i < pair.Value.Key.Count; i++)
            {
                ConnectChunks(pair.Key, pair.Value.Key[i]);
            }
        }
        foreach (KeyValuePair<Vector2Int, Chunck> pair in maps)
        {
            for (int x = 0; x < chunckSize; x++)
            {
                if (pair.Value.map[x, 0] == 0) { pair.Value.map[x, 0] = 1; }
                if (pair.Value.map[x, chunckSize - 1] == 0) { pair.Value.map[x, chunckSize - 1] = 1; }
            }
            for (int y = 0; y < chunckSize; y++)
            {
                if (pair.Value.map[0, y] == 0) { pair.Value.map[0, y] = 1; }
                if (pair.Value.map[chunckSize - 1, y] == 0) { pair.Value.map[chunckSize - 1, y] = 1; }
            }
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        foreach (KeyValuePair<Vector2Int, Chunck> pair in maps)
        {
            if (pseudoRandom.NextDouble() * 100.0 <= b.HoldeRateChunck)
            {
                int[,] nmap = InitChunkHole(b.HoleFill, pseudoRandom);
                for(int i = 0; i < nmap.GetLength(0); i++)
                {
                    for(int j = 0; j < nmap.GetLength(1); j++)
                    {
                        bool sideofWall = false;
                        for(int x = -1; x <= 1 && !sideofWall; x++)
                        {
                            for(int y= -1; y <= 1 && !sideofWall; y++)
                            {
                                if (x + i < chunckSize && y + j < chunckSize && x+i >= 0 && y + j >= 0)
                                {
                                    if(pair.Value.map[i+x, j+y] == 1 || pair.Value.map[i + x, j + y] == 2)
                                    {
                                        sideofWall = true;
                                    }
                                }
                            }
                        }
                        if(pair.Value.map[i,j] == 0)
                        {
                            if (nmap[i, j] != 1 && !sideofWall)
                            {
                                pair.Value.map[i, j] = 3;
                            }                            
                        }
                    }
                }
            }
        }

        foreach (KeyValuePair<Vector2Int, Chunck> pair in maps)
        {
            for (int i = 0; i < b.SpawnObjects.Count; i++)
            {
                if (pseudoRandom.NextDouble() * 100.0 <= b.SpawnObjects[i].SpawnRateChunck)
                {
                    int spawnNb = 0;
                    if(b.SpawnObjects[i].HasSpawnFillPercent)
                    {
                        spawnNb = (int)(pseudoRandom.NextDouble() * b.SpawnObjects[i].SpawnFillPercent * chunckSize * chunckSize);
                    }
                    else
                    {
                        spawnNb = b.SpawnObjects[i].SpawnNumber;
                    }
                    List<Vector2Int> posList = CheckIsSpawnable(pair.Value.map, b.SpawnObjects[i].Size, spawnNb, pseudoRandom, b.SpawnObjects[i].WallObject);
                    for (int j = 0; j < posList.Count; j++)
                    {
                        Spawnable sp;
                        sp.id = b.SpawnObjects[i].SpawnID;
                        sp.position = new Vector3(posList[j].x + pair.Key.x * chunckSize, -wallSize, posList[j].y + pair.Key.y * chunckSize) - new Vector3(chunckSize / 2.0f, b.SpawnObjects[i].WallObject ? -wallSize : 0.0f, chunckSize / 2.0f);
                        sp.rotation = Quaternion.identity;
                        pair.Value.spawnables.Add(sp);
                    }
                }
            }
        }


        List<Vector2Int> borderMaps = new List<Vector2Int>();
        Vector2Int bo;
        Mesh mflat = new Mesh();
        List<Vector3> pos = new List<Vector3>();
        List<int> indice = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        float csDivid = chunckSize / 2.0f;
        pos.Add(new Vector3(csDivid, 0, csDivid));
        pos.Add(new Vector3(-csDivid, 0, csDivid));
        pos.Add(new Vector3(csDivid, 0, -csDivid));
        pos.Add(new Vector3(-csDivid, 0, -csDivid));
        indice.Add(1);
        indice.Add(0);        
        indice.Add(2);
        indice.Add(2);
        indice.Add(3);        
        indice.Add(1);
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 10.0f));
        uvs.Add(new Vector2(10.0f, 0.0f));
        uvs.Add(new Vector2(10.0f, 10.0f));


        mflat.vertices = pos.ToArray();
        mflat.triangles = indice.ToArray();
        mflat.uv = uvs.ToArray();
        mflat.RecalculateNormals();        

        foreach (KeyValuePair<Vector2Int, Chunck> pair in maps)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    bo = pair.Key + new Vector2Int(x, y);
                    if(!(x == 0 && y == 0) && !maps.ContainsKey(bo) && !borderMaps.Contains(bo))
                    {
                        GameObject obj = new GameObject("Chunck_" + bo.ToString());
                        obj.transform.position = new Vector3(bo.x * (chunckSize), 0, bo.y * (chunckSize));
                        obj.transform.parent = transform;
                        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
                        MeshFilter mf = obj.AddComponent<MeshFilter>();
                        mf.mesh = mflat;
                        borderMaps.Add(bo);
                        mr.material = b.Ceil;
                    }
                }
            }
        }
        GameObject navmeshSurface = new GameObject("NavMeshSurface");
        navmeshSurface.transform.parent = transform;
        ns = navmeshSurface.AddComponent<NavMeshSurface>();
        ns.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        foreach (KeyValuePair<Vector2Int, Chunck> pair in maps)
        {
            GameObject obj = new GameObject("Chunck_" + pair.Key.ToString());
            obj.transform.position = new Vector3(pair.Key.x * (chunckSize), 0, pair.Key.y * (chunckSize));
            obj.transform.parent = transform;            
            MeshGenerator mg = obj.AddComponent<MeshGenerator>();            
            GameObject wall = new GameObject("ChunckWall_" + pair.Key.ToString());
            wall.transform.parent = obj.transform;
            wall.transform.localPosition = Vector3.zero;
            wall.transform.localScale = Vector3.one*1.016f;
            MeshRenderer mr1 = wall.AddComponent<MeshRenderer>();
            GameObject cave = new GameObject("ChunckCave_" + pair.Key.ToString());
            cave.transform.parent = obj.transform;
            cave.transform.localPosition = Vector3.zero;          
            cave.transform.localScale = Vector3.one*1.016f;
            MeshRenderer mr2 = cave.AddComponent<MeshRenderer>();            
            GameObject ground = new GameObject("ChunckGround_" + pair.Key.ToString());
            ground.transform.parent = obj.transform;
            ground.transform.localPosition = new Vector3(-(chunckSize)/2,-wallSize, -(chunckSize) / 2);
            MeshRenderer mr3 = ground.AddComponent<MeshRenderer>();
            GameObject border = new GameObject("ChunckBorder_" + pair.Key.ToString());
            border.transform.parent = obj.transform;
            border.transform.localPosition = Vector3.zero;
            MeshRenderer mr4 = border.AddComponent<MeshRenderer>();
            GameObject holeBorder = new GameObject("ChunckHoleBorder_" + pair.Key.ToString());
            holeBorder.transform.parent = obj.transform;
            holeBorder.transform.localPosition = new Vector3(-(chunckSize) / 2, -wallSize, -(chunckSize) / 2);
            MeshRenderer mr7 = holeBorder.AddComponent<MeshRenderer>();
            mg.InitMesh(wall.AddComponent<MeshFilter>(), cave.AddComponent<MeshFilter>(), ground.AddComponent<MeshFilter>(), border.AddComponent<MeshFilter>(), holeBorder.AddComponent<MeshFilter>(), wallSize);
            mg.GenerateMesh(pair.Value.map, 1,pair.Key);
            Mesh current = ground.GetComponent<MeshFilter>().mesh;   
            MeshCollider mc = ground.AddComponent<MeshCollider>();
            mc.sharedMesh = current;

            GameObject borderB = new GameObject("ChunckBorderBase_" + pair.Key.ToString());
            borderB.transform.parent = obj.transform;
            borderB.transform.localPosition = new Vector3(0, -wallSize, 0);
            borderB.transform.localScale = Vector3.one * 1.016f;
            MeshRenderer mr5 = borderB.AddComponent<MeshRenderer>();
            MeshFilter mf = borderB.AddComponent<MeshFilter>();
            GameObject voidB = new GameObject("ChunckVoid_" + pair.Key.ToString());
            voidB.transform.parent = obj.transform;
            voidB.transform.localPosition = new Vector3(0, -wallSize*2, 0);
            voidB.transform.localScale = Vector3.one * 1.016f;
            voidB.AddComponent<OOBRespawnNavMesh>();
            BoxCollider bc = voidB.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = mflat.bounds.extents*2.0f + new Vector3(0, wallSize, 0);            
            MeshRenderer mr6 = voidB.AddComponent<MeshRenderer>();
            MeshFilter mf6 = voidB.AddComponent<MeshFilter>();
            mf6.mesh = mflat;
            mr6.material = matVoid;            
            mf.mesh = border.GetComponent<MeshFilter>().mesh;

            
            mr1.material = b.Wall;
            mr2.material = b.Ceil;
            mr3.material = b.Ground;
            mr4.material = b.Border;
            mr5.material = b.Border;
            mr7.material = b.BorderHole;
            foreach (Spawnable spawn in pair.Value.spawnables)
            {
                SpawnableObject so = GetID(spawn.id);
                if (so != null)
                {
                    GameObject go = Instantiate(so.Prefab[pseudoRandom.Next(so.Prefab.Count)],spawn.position,spawn.rotation);
                    bool rotate = false;
                    float angle = CalculateAngle(so.RotateMode, pseudoRandom, so.AngleMin, so.Angle, out rotate);
                    if (rotate)
                    {
                        go.transform.eulerAngles = new Vector3(0, angle, 0);
                    }
                    if (!so.IsNetCodeObject)
                    {
                        go.transform.parent = obj.transform;
                    }
                }
            }
        }
        ns.BuildNavMesh();
    }

    public Biome GetBiomeID(int id)
    {
        Biome obj = null;
        for (int i = 0; i < biomeData.Length; i++)
        {
            if (biomeData[i].BiomeID == id)
            {
                return biomeData[i];
            }
        }
        return obj;
    }

    public SpawnableObject GetID(int id)
    {
        SpawnableObject obj = null;
        for(int i = 0; i < spawnData.Length;i++)
        {
            if (spawnData[i].SpawnID == id)
            {
                return spawnData[i];
            }
        }
        return obj;
    }

    public Dictionary<Vector2Int, KeyValuePair<List<Vector2Int>, List<Vector2Int>>> CreateDungeon(Vector2Int startPosition)
    {
        Dictionary<Vector2Int, KeyValuePair<List<Vector2Int>, List<Vector2Int>>> placedZone = new Dictionary<Vector2Int, KeyValuePair<List<Vector2Int>, List<Vector2Int>>>();
        List<Vector2Int> nextZone = AddZone(startPosition, startPosition, placedZone);
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        for (int i = 1; i < countZone;i++)
        {
            int rand = pseudoRandom.Next(nextZone.Count);
            int rdsub = pseudoRandom.Next(placedZone[nextZone[rand]].Value.Count);
            nextZone = AddZone(nextZone[rand], placedZone[nextZone[rand]].Value[rdsub], placedZone);
        }
        return placedZone;
    }

    public List<Vector2Int> AddZone(Vector2Int startPos, Vector2Int endPos, Dictionary<Vector2Int, KeyValuePair<List<Vector2Int>, List<Vector2Int>>> placedZone)
    {
        placedZone[endPos] = new KeyValuePair<List<Vector2Int>, List<Vector2Int>>(new List<Vector2Int>(), new List<Vector2Int>());
        foreach (KeyValuePair<Vector2Int, KeyValuePair<List<Vector2Int>, List<Vector2Int>>> pair in placedZone)
        {
            pair.Value.Value.Remove(endPos);
        }
        if (placedZone.ContainsKey(startPos) && startPos != endPos)
        {
            placedZone[startPos].Key.Add(endPos);
        }
        Vector2Int[] around = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };
        for(int i = 0; i < around.Length;i++)
        {
            if (!placedZone.ContainsKey(around[i] + endPos))
            {
                placedZone[endPos].Value.Add(around[i] + endPos);
            }
        }
        List<Vector2Int> nextZone = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, KeyValuePair<List<Vector2Int>, List<Vector2Int>>> pair in placedZone)
        {
            if (pair.Value.Value.Count > 0)
            {
                nextZone.Add(pair.Key);
            }
        }
        return nextZone;
    }

    private void ConnectChunks(Vector2Int chunk1, Vector2Int chunk2)
    {
        int[,] map1 = maps[chunk1].map;
        int[,] map2 = maps[chunk2].map;

        float minDist = float.MaxValue;
        Vector2Int fp1 = Vector2Int.zero;
        Vector2Int fp2 = Vector2Int.zero;

        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                if (map1[x, y] == 0)
                {
                    for (int i = 0; i < chunckSize; i++)
                    {
                        for (int j = 0; j < chunckSize; j++)
                        {
                            if (map2[i, j] == 0)
                            {
                                float dist = Vector2Int.Distance(new Vector2Int((chunk1.x * chunckSize) + x, (chunk1.y * chunckSize) + y), new Vector2Int((chunk2.x * chunckSize) + i, (chunk2.y * chunckSize) + j));
                                if(dist < minDist)
                                {
                                    minDist = dist;
                                    fp1 = new Vector2Int(x,y);
                                    fp2 = new Vector2Int(i, j);
                                }
                            }
                        }
                    }
                }
            }
        }

        Vector2 gfp1 = new Vector2((chunk1.x * chunckSize) + fp1.x, (chunk1.y * chunckSize) + fp1.y);
        Vector2 gfp2 = new Vector2((chunk2.x * chunckSize) + fp2.x, (chunk2.y * chunckSize) + fp2.y);
        int d = Mathf.RoundToInt(Vector2.Distance(gfp1, gfp2));
        Vector2 direction = (gfp2 - gfp1).normalized;
        List<Vector2> doorPos;
        for (int k = -1; k < 2; k++)
        {
            for (int j = -1; j < 2; j++)
            {
                for (int i = 0; i < d; i++)
                {
                    int x = Mathf.RoundToInt(k+fp1.x + direction.x * i);
                    int y = Mathf.RoundToInt(j+fp1.y + direction.y * i);
                    if (x >= 0 && x < chunckSize && y >= 0 && y < chunckSize)
                    {
                        map1[x, y] = 2;
                    }
                    x = Mathf.RoundToInt(k+fp2.x + -direction.x * i);
                    y = Mathf.RoundToInt(j+fp2.y + -direction.y * i);
                    if (x >= 0 && x < chunckSize && y >= 0 && y < chunckSize)
                    {
                        map2[x, y] = 2;
                    }
                }
            }
        }        
        Vector3 offset = -new Vector3((chunckSize)/2,0, (chunckSize) / 2);
        Spawnable spawn;
        spawn.id = 0;
        spawn.position = ((new Vector3(gfp1.x, -wallSize*2, gfp1.y) + new Vector3(gfp2.x, 0, gfp2.y)) / 2.0f) + offset;
        spawn.rotation = Quaternion.LookRotation((new Vector3(gfp1.x, 0, gfp1.y) - new Vector3(gfp2.x, 0, gfp2.y)).normalized, Vector3.up);
        maps[chunk1].spawnables.Add(spawn);
    }

    private int[,] InitChunkHole(int fill, System.Random pseudoRandom)
    {
        int[,] chunckMap = new int[chunckSize, chunckSize];        
        int fillpercent = Mathf.Min(fill, 99);
        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                if (x == 0 || x == chunckSize - 1 || y == 0 || y == chunckSize - 1)
                {
                    chunckMap[x, y] = 1;
                }
                else
                {
                    chunckMap[x, y] = (pseudoRandom.Next(0, 100) < fillpercent) ? 1 : 0;
                }
            }
        }
        for (int i = 0; i < 5; i++)
        {
            SmoothMap(chunckMap);
        }
        return chunckMap;
    }

    private void CreateChunck(Vector2Int pos,bool special = false)
    {
        if (maps.ContainsKey(pos))
        {
            return;
        }
        int[,] chunckMap = new int[chunckSize, chunckSize];
        System.Random pseudoRandom = new System.Random(seed.GetHashCode() + pos.GetHashCode());
        int fillpercent;
        if (special)
        {
            fillpercent = 10;
        }
        else
        {
            fillpercent = Mathf.Min(randomFillPercent + pseudoRandom.Next(randomAddPercent), 90);
        }
        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                if (x == 0 || x == chunckSize - 1 || y == 0 || y == chunckSize - 1)
                {
                    chunckMap[x, y] = 1;
                }
                else
                {
                    chunckMap[x, y] = (pseudoRandom.Next(0, 100) < fillpercent) ? 1 : 0;
                }
            }
        }
        for (int i = 0; i < 5; i++)
        {
            SmoothMap(chunckMap);
        }
        ProcessMap(chunckMap);
        Chunck c;
        c.map = chunckMap;
        c.spawnables = new List<Spawnable>();
        maps[pos] = c;
    }

    void SmoothMap(int[,] chunckMap)
    {
        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y, chunckMap);

                if (neighbourWallTiles > 4)
                {
                    chunckMap[x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    chunckMap[x, y] = 0;
                }

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY, int[,] map)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < chunckSize && y >= 0 && y < chunckSize;
    }

    void ProcessMap(int[,] map)
    {
        List<List<Coord>> wallRegions = GetRegions(1,map);
        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0,map);
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        if (survivingRooms.Count > 0)
        {
            survivingRooms.Sort();
            survivingRooms[0].isMainRoom = true;
            survivingRooms[0].isAccessibleFromMainRoom = true;

            ConnectClosestRooms(survivingRooms, map);
        }
    }

    List<List<Coord>> GetRegions(int tileType, int[,] map)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[chunckSize, chunckSize];

        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y,map);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY, int[,] map)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[chunckSize, chunckSize];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    void ConnectClosestRooms(List<Room> allRooms, int[,] map, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB, map);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB, map);
            ConnectClosestRooms(allRooms, map, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, map, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB, int[,] map)
    {
        Room.ConnectRooms(roomA, roomB);
        List<Coord> line = GetLine(tileA, tileB, map);
        foreach (Coord c in line)
        {
            DrawCircle(c, 5, map);
        }
    }

    List<Coord> GetLine(Coord from, Coord to, int[,] map)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    void DrawCircle(Coord c, int r, int[,] map)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }
}
