using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour
{
    private SquareGrid squareGrid;
    private MeshFilter walls;
    private MeshFilter cave;
    private MeshFilter ground;
    private MeshFilter border;
    private MeshFilter holeBorder;
    private float wallHeight = 4.0f;

    List<Vector3> vertices;
    List<int> triangles;
    List<Vector3> holeBorders = new List<Vector3>();

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();    
    public void InitMesh(MeshFilter w, MeshFilter c, MeshFilter g, MeshFilter b,MeshFilter hb, float h)
    {
        wallHeight = h;
        walls = w;
        border = b;
        cave = c;
        ground = g;
        holeBorder = hb;
    }

    public void GenerateMesh(int[,] map, float squareSize,Vector2Int pos)
    {
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        List<Bounds> door = new List<Bounds>();       
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == 2)
                {
                    if (x == 0 || x == map.GetLength(0) - 1 || y == 0 || y == map.GetLength(1) - 1)
                    {
                        Bounds b = new Bounds();
                        b.center = new Vector3(x - (map.GetLength(0) / 2), wallHeight, y - (map.GetLength(1) / 2));
                        b.extents = new Vector3(1.5f,wallHeight,1.5f);
                        door.Add(b);
                    }                
                }
            }
        }

        Vector3 change;
        for(int i = 0; i < vertices.Count;i++)
        {
            for(int j = 0; j < door.Count;j++)
            {
                if(door[j].Contains(vertices[i]))
                {
                    change = vertices[i];
                    change.y = -wallHeight*2; 
                    vertices[i] = change;
                    break;
                }
            }            
        }  

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;
      

        CreateWallMesh(door);
        CreateBorderWall();
        CreateGroundMesh(map, map.GetLength(0));
        CreateBorderHole();        
    }

    void CreateBorderWall()
    {
        List<Vector3> borderVertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normal = new List<Vector3>();
        List<int> wallTriangles = new List<int>();

        Mesh borderMesh = new Mesh();

        Vector3[] faceNormals =
        {
                    Vector3.down,
                    Vector3.up,
                    Vector3.back,
                    Vector3.forward, 
                    Vector3.left, 
                    Vector3.right
        };

        float a = 0.5f;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                Vector3 currentVertex = vertices[outline[i]];

                int startIndex = borderVertices.Count;
                
                borderVertices.Add(currentVertex + new Vector3(-a, -a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, -a, -a));//down
                borderVertices.Add(currentVertex + new Vector3(-a, -a, a));
                borderVertices.Add(currentVertex + new Vector3(a, -a, a));

                borderVertices.Add(currentVertex + new Vector3(-a, a, a));
                borderVertices.Add(currentVertex + new Vector3(a, a, a));//up
                borderVertices.Add(currentVertex + new Vector3(-a, a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, a, -a));

                borderVertices.Add(currentVertex + new Vector3(-a, a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, a, -a));//front
                borderVertices.Add(currentVertex + new Vector3(-a, -a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, -a, -a));

                borderVertices.Add(currentVertex + new Vector3(a, a, a));
                borderVertices.Add(currentVertex + new Vector3(-a, a, a));//back
                borderVertices.Add(currentVertex + new Vector3(a, -a, a));
                borderVertices.Add(currentVertex + new Vector3(-a, -a, a));

                borderVertices.Add(currentVertex + new Vector3(-a, -a, -a));//left
                borderVertices.Add(currentVertex + new Vector3(-a, -a, a));
                borderVertices.Add(currentVertex + new Vector3(-a, a, -a));
                borderVertices.Add(currentVertex + new Vector3(-a, a, a));

                borderVertices.Add(currentVertex + new Vector3(a, -a, a));//right
                borderVertices.Add(currentVertex + new Vector3(a, -a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, a, a));
                borderVertices.Add(currentVertex + new Vector3(a, a, -a));
                
                for (int k = 0; k < 6; k++)
                {
                    wallTriangles.Add(startIndex + (k*4));
                    wallTriangles.Add(startIndex + 1 + (k * 4));
                    wallTriangles.Add(startIndex + 2 + (k * 4));
                    wallTriangles.Add(startIndex + 1 + (k * 4));
                    wallTriangles.Add(startIndex + 3 + (k * 4));
                    wallTriangles.Add(startIndex + 2 + (k * 4));

                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(1, 0));
                    uvs.Add(new Vector2(0, 1));
                    uvs.Add(new Vector2(1, 1));
                }

                for (int k = 0; k < 6; k++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        normal.Add(faceNormals[k]);
                    }
                }
            }
        }

        borderMesh.vertices = borderVertices.ToArray();
        borderMesh.triangles = wallTriangles.ToArray();
        borderMesh.normals = normal.ToArray();
        borderMesh.uv = uvs.ToArray();

        borderMesh.RecalculateNormals();

        border.mesh = borderMesh;
        border.sharedMesh = borderMesh; 
    }

    void CreateBorderHole()
    {
        List<Vector3> borderVertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normal = new List<Vector3>();
        List<int> wallTriangles = new List<int>();

        Mesh borderMesh = new Mesh();

        Vector3[] faceNormals =
        {
                    Vector3.down,
                    Vector3.up,
                    Vector3.back,
                    Vector3.forward, 
                    Vector3.left, 
                    Vector3.right
        };

        float a = 0.25f;
        List<Vector3> neighbor = new List<Vector3>();
        Vector3 pos = Vector3.zero;
        Vector3[] offset = new Vector3[] 
        {
            new Vector3(1,0,0),
            new Vector3(-1,0,0),
            new Vector3(0,0,1),
            new Vector3(0,0,-1)
        };

        for(int i = 0; i < holeBorders.Count;i++)
        {
            for (int j = 0; j < offset.Length; j++)
            {
                pos = holeBorders[i] + offset[j];
                if (!neighbor.Contains((holeBorders[i] + pos) / 2.0f) && holeBorders.Contains(pos))
                {
                    neighbor.Add((holeBorders[i] + pos) / 2.0f);
                }
            }
        }

        holeBorders.AddRange(neighbor);

            for (int i = 0; i < holeBorders.Count; i++)
            {
                Vector3 currentVertex = holeBorders[i];
                currentVertex -= new Vector3(0.0f, a-0.01f, 0.0f);
                int startIndex = borderVertices.Count;
                
                borderVertices.Add(currentVertex + new Vector3(-a, -a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, -a, -a));//down
                borderVertices.Add(currentVertex + new Vector3(-a, -a, a));
                borderVertices.Add(currentVertex + new Vector3(a, -a, a));

                borderVertices.Add(currentVertex + new Vector3(-a, a, a));
                borderVertices.Add(currentVertex + new Vector3(a, a, a));//up
                borderVertices.Add(currentVertex + new Vector3(-a, a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, a, -a));

                borderVertices.Add(currentVertex + new Vector3(-a, a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, a, -a));//front
                borderVertices.Add(currentVertex + new Vector3(-a, -a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, -a, -a));

                borderVertices.Add(currentVertex + new Vector3(a, a, a));
                borderVertices.Add(currentVertex + new Vector3(-a, a, a));//back
                borderVertices.Add(currentVertex + new Vector3(a, -a, a));
                borderVertices.Add(currentVertex + new Vector3(-a, -a, a));

                borderVertices.Add(currentVertex + new Vector3(-a, -a, -a));//left
                borderVertices.Add(currentVertex + new Vector3(-a, -a, a));
                borderVertices.Add(currentVertex + new Vector3(-a, a, -a));
                borderVertices.Add(currentVertex + new Vector3(-a, a, a));

                borderVertices.Add(currentVertex + new Vector3(a, -a, a));//right
                borderVertices.Add(currentVertex + new Vector3(a, -a, -a));
                borderVertices.Add(currentVertex + new Vector3(a, a, a));
                borderVertices.Add(currentVertex + new Vector3(a, a, -a));
                
                for (int k = 0; k < 6; k++)
                {
                    wallTriangles.Add(startIndex + (k*4));
                    wallTriangles.Add(startIndex + 1 + (k * 4));
                    wallTriangles.Add(startIndex + 2 + (k * 4));
                    wallTriangles.Add(startIndex + 1 + (k * 4));
                    wallTriangles.Add(startIndex + 3 + (k * 4));
                    wallTriangles.Add(startIndex + 2 + (k * 4));

                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0.5f, 0));
                    uvs.Add(new Vector2(0, 0.5f));
                    uvs.Add(new Vector2(0.5f, 0.5f));
                }

                for (int k = 0; k < 6; k++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        normal.Add(faceNormals[k]);
                    }
                }
            }


        borderMesh.vertices = borderVertices.ToArray();
        borderMesh.triangles = wallTriangles.ToArray();
        borderMesh.normals = normal.ToArray();
        borderMesh.uv = uvs.ToArray();

        borderMesh.RecalculateNormals();

        holeBorder.mesh = borderMesh;
        holeBorder.sharedMesh = borderMesh; 
    }

    void CreateWallMesh(List<Bounds> door)
    {
        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        List<int> wallTriangles = new List<int>();

        Mesh wallMesh = new Mesh();        
        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left
                wallVertices.Add(vertices[outline[i + 1]]); // right;
                float height = wallHeight;

                for(int j = 0; j < door.Count;j++)
                {
                    if(door[j].Contains(vertices[outline[i]]))
                    {                    
                        height = 0.0f;
                    }
                }

                wallVertices.Add(vertices[outline[i]] - Vector3.up * height); // bottom left
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * height); // bottom right
                uvs.Add(new Vector2(i, height));
                uvs.Add(new Vector2(i + 1, height));
                uvs.Add(new Vector2(i, 0));
                uvs.Add(new Vector2(i+1,0));              

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);                
            }
        }        

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.uv = uvs.ToArray();
        walls.mesh = wallMesh;        
        walls.mesh.RecalculateNormals();

        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }
    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }

    }

    void CreateGroundMesh(int[,] map,int gridSize, float gridSpacing = 1.0f)
    { 
        holeBorders.Clear();        
        int indexHole = 0;
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(gridSize+1) * (gridSize+1)];
        Vector2[] uv =  new Vector2[(gridSize + 1) * (gridSize + 1)];

        List<int> triangles = new List<int>();
        for (int z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                if (x != gridSize && z != gridSize && map[x, z] == 3)
                {
                    bool bord = false;
                    for (int k = -1; k <= 1 && !bord; k++)
                    {
                        for (int j = -1; j <= 1 && !bord; j++)
                        {
                            if (x + k >= 0 && x + k < gridSize && z + j >= 0 && z + j < gridSize && (k != 0 || j != 0) && map[x + k, z + j] != 3)
                            {
                                bord = true;
                            }
                        }
                    }
                    if (bord)
                    {
                        vertices[x+ (gridSize + 1) * z] = new Vector3(x * gridSpacing, 0.0f, z * gridSpacing);
                        holeBorders.Add(new Vector3(x * gridSpacing, 0.0f, z * gridSpacing));
                        uv[x + (gridSize + 1) * z] = new Vector2((float)x / (gridSize / 8), (float)z / (gridSize / 8));
                    }
                }
                else
                {
                    vertices[x + (gridSize + 1) * z] = new Vector3(x * gridSpacing, 0.0f, z * gridSpacing);
                    uv[x + (gridSize + 1) * z] = new Vector2((float)x / (gridSize / 8), (float)z / (gridSize / 8));
                }
            }
        }

        List<Vector3> globalVertices = new List<Vector3>();
        List<Vector2> globalUv = new List<Vector2>();

        int vert = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            globalVertices.Add(vertices[i]);
            globalUv.Add(uv[i]);
            bool isFill = true;
            for (int x= 0; x <= 1 && isFill; x++)
            {
                for (int y = 0; y <= 1 && isFill; y++)
                {
                    if (i + x + y * (gridSize + 1) < vertices.Length)
                    {
                        if (i != 0 && vertices[i + x + y * (gridSize + 1)] == Vector3.zero)
                        {
                            isFill = false;
                        }
                    }
                    else
                    {
                        isFill = false;
                    }
                }
            }
            if (isFill)
            {         
                triangles.Add(vert + 0);
                triangles.Add(vert + (gridSize + 1));
                triangles.Add(vert + 1);
                triangles.Add(vert + 1);
                triangles.Add(vert + (gridSize + 1));
                triangles.Add(vert + (gridSize + 2));                
            }
            vert++;
        }

        mesh.vertices = globalVertices.ToArray();
        mesh.uv = globalUv.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        ground.mesh = mesh;
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }
}