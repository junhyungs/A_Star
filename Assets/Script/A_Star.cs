using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool IsWall { get; set; } //벽인지 판단(장애물)
    public Node ParentNode { get; set; } //부모 노드
    public int PositionX { get; set; } //positionX
    public int PositionZ { get; set; } //positionZ
    public int Cost_G { get; set; } //시작 지점에서 현재 지점까지 비용
    public int Cost_H {  get; set; } //임의 지점에서 도착 지점까지 비용
    public int Cost_F //총 비용
    {
        get { return Cost_G + Cost_H; }
    }
}

public class A_Star
{
    private Transform _destination; //목적지(도착지점)
    private Transform _selfTransform; //본인
    private bool _allowDigonal; //대각선 이동을 허용할건지
    private bool _dontCrossCorner; //코너를 가로질러 갈건지

    private Vector2Int _bottomLeft; //그리드 좌측 하단 좌표
    private Vector2Int _topRight; //그리드 우측 상단 좌표
    private Vector2Int _startPosition; //시작지점 좌표
    private Vector2Int _endPosition; //도착지점 좌표

    private List<Node> _finalNodeList; //최종 경로를 저장하는 리스트 (여기서는 사용안함)
    private List<Node> _openList; // 탐색할 노드를 저장하고 관리. 가장 비용이 적은 경로를 탐색하기 위한 리스트.
    private List<Node> _closeList; //탐색이 완료된 노드를 저장하는 리스트
    private Node[,] _nodeArray; //그리드의 전체 정보를 저장하는 2차원 배열

    private Node _startNode; //시작 노드
    private Node _endNode; //종료 노드
    private Node _currentNode; //현재 노드

    private int _gridSizeX; //그리드 크기 X
    private int _gridSizeZ; //그리드 크기 Z

    private const int _costStraight = 10; //직선 이동비용
    private const int _costDiagonal = 14; //대각선 이동비용 : 대각선 이동비용은 피타고라스의 정리에 따라 두 변의 길이가 1일 때 빗변의 길이는 루트2 이므로 1.4 뭐시기가 됨. 여기에 10을 곱해서 14로 사용.

    private int _expandGridRange = 2; //추가 범위. 추가 범위를 안넣어주면 현재 지점부터 목표 지점까지 딱붙어서 그리드가 정해짐. 조금 여유있게 찾기위함.
    private float _sphereRadius = 0.49f; //경로상에 장애물을 판단하는 오버랩 스피어의 반지름. 지름이 1이되어 경계면이 타이트 해지면 잘못된 판정을 할 수 있음. 조금 여유를 주기 위해 0.98f

   

    public List<Vector3> PathFinding(Transform selfTransform, Transform destination, bool allowDigonal, bool dontCrossCorner)
    {
        _selfTransform = selfTransform; //대충 받어...
        _destination = destination;
        _allowDigonal = allowDigonal;
        _dontCrossCorner = dontCrossCorner;

        CreateGrid(); //그리드를 생성하는 메서드. 

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

        _startNode = _nodeArray[_startPosition.x - _bottomLeft.x, _startPosition.y - _bottomLeft.y]; //월드 좌표계(_startPosition, _endPosition)를 Grid 상에서의 좌표로 변환.
        _endNode = _nodeArray[_endPosition.x - _bottomLeft.x, _endPosition.y - _bottomLeft.y]; //Grid의 원점 (0,0)은 왼쪽 하단에 있으므로 왼쪽 하단 좌표를 기준으로 계산.  

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
                OpenListAdd(_currentNode.PositionX + 1, _currentNode.PositionZ - 1);// ↘
                OpenListAdd(_currentNode.PositionX - 1, _currentNode.PositionZ + 1);// ↖
                OpenListAdd(_currentNode.PositionX + 1, _currentNode.PositionZ + 1);// ↗
                OpenListAdd(_currentNode.PositionX - 1, _currentNode.PositionZ - 1);// ↙
            }

            OpenListAdd(_currentNode.PositionX + 1, _currentNode.PositionZ); // ->
            OpenListAdd(_currentNode.PositionX - 1, _currentNode.PositionZ); // <-
            OpenListAdd(_currentNode.PositionX, _currentNode.PositionZ + 1); // ↑
            OpenListAdd(_currentNode.PositionX, _currentNode.PositionZ - 1); // ↓
        }

        return null;
    }

    private void OpenListAdd(int checkPositionX, int checkPositionZ)
    {
        bool isWithinBounds = checkPositionX >= _bottomLeft.x && checkPositionX < _topRight.x + 1 //3가지 bool 조건은 이웃 노드 좌표에 대한 처리
            && checkPositionZ >= _bottomLeft.y && checkPositionZ < _topRight.y + 1;

        bool isNotWall = !_nodeArray[checkPositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y].IsWall;

        bool isNotCloseList = !_closeList.Contains(_nodeArray[checkPositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y]);
        
        if(isWithinBounds && isNotWall && isNotCloseList)
        {
            if (_allowDigonal) //대각선 이동이 가능한가? 둘 다 갈 수 있는 조건이여야 하므로 AND 조건
            {
                bool isWall = _nodeArray[_currentNode.PositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y].IsWall //왼쪽 위 좌표 확인, 오른쪽 아래 좌표 확인.
                    && _nodeArray[checkPositionX - _bottomLeft.x, _currentNode.PositionZ - _bottomLeft.y].IsWall;

                if (isWall)
                    return;
            }

            if (_dontCrossCorner) //모서리를 지나갈 수 있는가? 여기서는 왼쪽 위나 오른쪽 아래중 1곳이 갈 수 있으면 모서리를 지나 갈 수 있으므로 OR 조건. 
            {
                bool isWall = _nodeArray[_currentNode.PositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y].IsWall //왼쪽 위 좌표 확인, 오른쪽 아래 좌표 확인.
                    || _nodeArray[checkPositionX - _bottomLeft.x, _currentNode.PositionZ - _bottomLeft.y].IsWall;

                if (isWall)
                    return;
            }

            Node neighborNode = _nodeArray[checkPositionX - _bottomLeft.x, checkPositionZ - _bottomLeft.y]; //이웃노드

            int cost = _currentNode.Cost_G + (_currentNode.PositionX - checkPositionX == 0
                || _currentNode.PositionZ - checkPositionZ == 0 ? _costStraight : _costDiagonal); //비용 계산. x축이 이동하지 않았거나, z축이 이동하지 않았으면 10의 직선비용.
                                                                                                  //둘 다 이동했으면 14의 대각선 이동비용을 가진다.
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

        minX = _selfTransform.position.x; //좌측 하단 좌표(출발지점)
        maxX = _destination.position.x; //우측 상단 좌표(도착지점)
        minZ = _selfTransform.position.z; //좌측 하단 좌표(출발지점)
        maxZ = _destination.position.z; //우측 상단 좌표(도착지점)

        if (_selfTransform.position.x > _destination.position.x) //큰 수, 작은 수 맞춰주기.
        {
            temp = minX;
            minX = maxX;
            maxX = temp;
        }

        if (_selfTransform.position.z > _destination.position.z)// ''
        {
            temp = minZ;
            minZ = maxZ;
            maxZ = temp;
        }

        minX -= _expandGridRange; //확장 작은 수는 - 해서 더 작게 큰 수는 + 해서 더 크게. 
        minZ -= _expandGridRange;
        maxX += _expandGridRange;
        maxZ += _expandGridRange;

        _bottomLeft = new Vector2Int((int)minX, (int)minZ); //좌측 하단
        _topRight = new Vector2Int((int)maxX, (int)maxZ); //우측 상단
        _startPosition = new Vector2Int((int)_selfTransform.position.x, (int)_selfTransform.position.z); //시작지점
        _endPosition = new Vector2Int((int)_destination.position.x, (int)_destination.position.z); //도착지점
    }
}
