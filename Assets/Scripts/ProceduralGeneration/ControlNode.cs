using UnityEngine;

public class ControlNode : Node
{
    public bool active;
    public Node above, right;

    public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
    {
        active = _active;
        above = new Node(position + Vector3.forward * squareSize / 2f);
        right = new Node(position + Vector3.right * squareSize / 2f);
    }
}