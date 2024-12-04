using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 _currentTarget;
    private Vector3 _startPosition;
    private List<Vector3> _pathList;

    [Header("Speed")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotationSpeed;
    private int _currentIndex;

    public bool StartMove { get; set; } = false;
    
    private void Awake()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartManager.Instance.PlayerComponent = this;

        _startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void SetPathList(List<Vector3> pathList)
    {
        _pathList = pathList;

        _currentIndex = 0;

        if(_pathList.Count > 0)
        {
            _currentTarget = _pathList[_currentIndex];
        }

        StartMove = true;
    }

    private void Move()
    {
        if (!StartMove)
        {
            return;
        }

        Vector3 moveDirection = (_currentTarget - transform.position).normalized;

        Rotation(moveDirection);

        MoveToTarget(moveDirection);

        float distance = Vector3.Distance(transform.position, _currentTarget);

        if(distance < 0.1f)
        {
            NextTransform();
        }
    }

    private void MoveToTarget(Vector3 moveDirection)
    {
        Vector3 moveVelocity = moveDirection * _moveSpeed;

        _rigidbody.linearVelocity = new Vector3(moveVelocity.x, _rigidbody.linearVelocity.y, moveVelocity.z);
    }

    private void Rotation(Vector3 moveDirection)
    {
        float angle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.fixedDeltaTime);
    }

    private void NextTransform()
    {
        _currentIndex++;

        if(_currentIndex < _pathList.Count)
        {
            _currentTarget = _pathList[_currentIndex];
        }
        else
        {
            StartMove = false;

            _rigidbody.linearVelocity = Vector3.zero;

            transform.position = _startPosition;
        }
    }

    private void OnDrawGizmos()
    {
        if(_pathList != null)
        {
            float gizmoRadius = 0.49f;

            foreach(var path in _pathList)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(path, gizmoRadius);
            }
        }
    }

}
