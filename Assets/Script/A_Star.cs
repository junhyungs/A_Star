using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool IsWall { get; set; } //������ �Ǵ�(��ֹ�)
    public Node ParentNode { get; set; } //�θ� ���
    public int PositionX { get; set; } //positionX
    public int PositionZ { get; set; } //positionZ
    public int Cost_G { get; set; } //���� �������� ���� �������� ���
    public int Cost_H {  get; set; } //���� �������� ���� �������� ���
    public int Cost_F //�� ���
    {
        get { return Cost_G + Cost_H; }
    }
}

public class A_Star
{
    private Transform _destination; //������(��������)
    private Transform _selfTransform; //����
    private bool _allowDigonal; //�밢�� �̵��� ����Ұ���
    private bool _dontCrossCorner; //�ڳʸ� �������� ������

    private Vector2Int _bottomLeft; //�׸��� ���� �ϴ� ��ǥ
    private Vector2Int _topRight; //�׸��� ���� ��� ��ǥ
    private Vector2Int _startPosition; //�������� ��ǥ
    private Vector2Int _endPosition; //�������� ��ǥ

    private List<Node> _finalNodeList; //���� ��θ� �����ϴ� ����Ʈ (���⼭�� ������)
    private List<Node> _openList; // Ž���� ��带 �����ϰ� ����. ���� ����� ���� ��θ� Ž���ϱ� ���� ����Ʈ.
    private List<Node> _closeList; //Ž���� �Ϸ�� ��带 �����ϴ� ����Ʈ
    private Node[,] _nodeArray; //�׸����� ��ü ������ �����ϴ� 2���� �迭

    private Node _startNode; //���� ���
    private Node _endNode; //���� ���
    private Node _currentNode; //���� ���

    private int _gridSizeX; //�׸��� ũ�� X
    private int _gridSizeZ; //�׸��� ũ�� Z

    private const int _costStraight = 10; //���� �̵����
    private const int _costDiagonal = 14; //�밢�� �̵���� : �밢�� �̵������ ��Ÿ����� ������ ���� �� ���� ���̰� 1�� �� ������ ���̴� ��Ʈ2 �̹Ƿ� 1.4 ���ñⰡ ��. ���⿡ 10�� ���ؼ� 14�� ���.

    private int _expandGridRange = 2;
    private float _sphereRadius = 0.49f;

   

    public List<Vector3> PathFinding(Transform selfTransform, Transform destination, bool allowDigonal, bool dontCrossCorner)
    {
        _selfTransform = selfTransform;
        _destination = destination;
        _allowDigonal = allowDigonal;
        _dontCrossCorner = dontCrossCorner;

        CreateGrid();

        _gridSizeX = _topRight.x - _bottomLeft.x + 1;
        _gridSizeZ = _topRight.y - _bottomLeft.y + 1;

        _nodeArray = new Node[_gridSizeX, _gridSizeZ];

        for(int i = 0; i < _nodeArray.GetLength(0); i++)
        {
            for(int k = 0; k < _nodeArray.GetLength(1); k++)
            {
                bool isObstacle = false;

                Vector3 spherePosition = new Vector3(i + _bottomLeft.x, _selfTransform.position.y, k + _bottomLeft.y);

                Collider[] colliders = Physics.OverlapSphere(spherePosition, _sphereRadius, LayerMask.GetMask("Wall"));

                if(colliders.Length > 0)
                {
                    isObstacle = true;
                }

                Node node = new Node();

                node.IsWall = isObstacle;
                node.PositionX = i + _bottomLeft.x;
                node.PositionZ = k + _bottomLeft.y;

                _nodeArray[i, k] = node;
            }
        }

        _startNode = _nodeArray[_startPosition.x - _bottomLeft.x, _startPosition.y - _bottomLeft.y]; //���� ��ǥ��(_startPosition, _endPosition)�� Grid �󿡼��� ��ǥ�� ��ȯ.
        _endNode = _nodeArray[_endPosition.x - _bottomLeft.x, _endPosition.y - _bottomLeft.y]; //Grid�� ���� (0,0)�� ���� �ϴܿ� �����Ƿ� ���� �ϴ� ��ǥ�� �������� ���.  

        _openList = new List<Node>();
        _closeList = new List<Node>();
        _finalNodeList = new List<Node>();

        _openList.Add(_startNode);

        while(_openList.Count > 0)
        {
            _currentNode = _openList[0];

             for(int i = 1; i < _openList.Count; i++)
            {
                if (_openList[i].Cost_F <= _currentNode.Cost_F && _openList[i].Cost_H < _currentNode.Cost_H)
                {
                    _currentNode = _openList[i];
                }
            }

            _openList.Remove(_currentNode);
            _closeList.Add(_currentNode);

            if(_currentNode == _endNode)
            {
                Node targetNode = _endNode;

                List<Vector3> worldPositionList = new List<Vector3>();

                while(targetNode != _startNode)
                {
                    Vector3 position = new Vector3(targetNode.PositionX, _selfTransform.position.y, targetNode.PositionZ);

                    worldPositionList.Add(position);
                    
                    targetNode = targetNode.ParentNode;
                }

                Vector3 startPosition = new Vector3(_startNode.PositionX, _selfTransform.position.y, _startNode.PositionZ);

                worldPositionList.Add(startPosition);

                worldPositionList.Reverse();

                return worldPositionList;
            }

            if (_allowDigonal)
            {
                OpenListAdd(_currentNode.PositionX + 1, _currentNode.PositionZ - 1);// ��
                OpenListAdd(_currentNode.PositionX - 1, _currentNode.PositionZ + 1);// ��
                OpenListAdd(_currentNode.PositionX + 1, _currentNode.PositionZ + 1);// ��
                OpenListAdd(_currentNode.PositionX - 1, _currentNode.PositionZ - 1);// ��
            }

            OpenListAdd(_currentNode.PositionX + 1, _currentNode.PositionZ); // ->
            OpenListAdd(_currentNode.PositionX - 1, _currentNode.PositionZ); // <-
            OpenListAdd(_currentNode.PositionX, _currentNode.PositionZ + 1); // ��
            OpenListAdd(_currentNode.PositionX, _currentNode.PositionZ - 1); // ��
        }

        return null;
    }

    private void OpenListAdd(int checkPositionX, int checkPositionZ)
    {
        bool isWithinBounds = checkPositionX >= _bottomLeft.x && checkPositionX < _topRight.x + 1 //3���� bool ������ �̿� ��� ��ǥ�� ���� ó��
            && checkPositionZ >= _bottomLeft.y && checkPositionZ < _topRight.y + 1;

        bool isNotWall = !_nodeArray[checkPositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y].IsWall;

        bool isNotCloseList = !_closeList.Contains(_nodeArray[checkPositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y]);
        
        if(isWithinBounds && isNotWall && isNotCloseList)
        {
            if (_allowDigonal) //�밢�� �̵��� �����Ѱ�? �� �� �� �� �ִ� �����̿��� �ϹǷ� AND ����
            {
                bool isWall = _nodeArray[_currentNode.PositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y].IsWall //���� �� ��ǥ Ȯ��, ������ �Ʒ� ��ǥ Ȯ��.
                    && _nodeArray[checkPositionX - _bottomLeft.x, _currentNode.PositionZ - _bottomLeft.y].IsWall;

                if (isWall)
                    return;
            }

            if (_dontCrossCorner) //�𼭸��� ������ �� �ִ°�? ���⼭�� ���� ���� ������ �Ʒ��� 1���� �� �� ������ �𼭸��� ���� �� �� �����Ƿ� OR ����. 
            {
                bool isWall = _nodeArray[_currentNode.PositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y].IsWall //���� �� ��ǥ Ȯ��, ������ �Ʒ� ��ǥ Ȯ��.
                    || _nodeArray[checkPositionX - _bottomLeft.x, _currentNode.PositionZ - _bottomLeft.y].IsWall;

                if (isWall)
                    return;
            }

            Node neighborNode = _nodeArray[checkPositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y]; //�̿����

            int cost = _currentNode.Cost_G + (_currentNode.PositionX - checkPositionX == 0
                || _currentNode.PositionZ - checkPositionZ == 0 ? _costStraight : _costDiagonal); //��� ���. x���� �̵����� �ʾҰų�, z���� �̵����� �ʾ����� 10�� �������.
                                                                                                  //�� �� �̵������� 14�� �밢�� �̵������ ������.
            if (cost < neighborNode.Cost_G || !_openList.Contains(neighborNode))
            {
                neighborNode.Cost_G = cost;
                neighborNode.Cost_H = (Mathf.Abs(neighborNode.PositionX - _endNode.PositionX)
                    + Mathf.Abs(neighborNode.PositionZ - _endNode.PositionZ)) * _costStraight;

                neighborNode.ParentNode = _currentNode;

                _openList.Add(neighborNode);
            }
        }
    }

    private void CreateGrid()
    {
        float minX, maxX, minZ, maxZ, temp;

        minX = _selfTransform.position.x;
        maxX = _destination.position.x;
        minZ = _selfTransform.position.z;
        maxZ = _destination.position.z;

        if (_selfTransform.position.x > _destination.position.x)
        {
            temp = minX;
            minX = maxX;
            maxX = temp;
        }

        if (_selfTransform.position.z > _destination.position.z)
        {
            temp = minZ;
            minZ = maxZ;
            maxZ = temp;
        }

        minX -= _expandGridRange;
        minZ -= _expandGridRange;
        maxX += _expandGridRange;
        maxZ += _expandGridRange;

        _bottomLeft = new Vector2Int((int)minX, (int)minZ);
        _topRight = new Vector2Int((int)maxX, (int)maxZ);
        _startPosition = new Vector2Int((int)_selfTransform.position.x, (int)_selfTransform.position.z);
        _endPosition = new Vector2Int((int)_destination.position.x, (int)_destination.position.z);
    }
}
