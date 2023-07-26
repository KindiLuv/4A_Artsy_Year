using UnityEngine;
public class SquareGrid
{
    public Square[,] squares;

    public SquareGrid(int[,] map, float squareSize)
    {
        int nodeCountX = map.GetLength(0);
        int nodeCountY = map.GetLength(1);
        float mapWidth = nodeCountX * squareSize;
        float mapHeight = nodeCountY * squareSize;

        ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

        for (int x = 0; x < nodeCountX; x++)
        {
            for (int y = 0; y < nodeCountY; y++)
            {
                Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                bool state = false;
                state = map[x, y] == 5 || map[x, y] == 1 || (map[x, y] == 2 && (x == nodeCountX-1 || y == nodeCountY-1 || x == 0 ||y == 0));            
                controlNodes[x, y] = new ControlNode(pos, state, squareSize);
            }
        }

        squares = new Square[nodeCountX-1, nodeCountY-1];
        for (int x = 0; x < nodeCountX-1; x++)
        {
            for (int y = 0; y < nodeCountY-1; y++)
            {
                squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
            }
        }

    }
}