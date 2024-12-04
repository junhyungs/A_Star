using UnityEngine;

public class StartManager : MonoBehaviour
{
    public static StartManager Instance;

    public Player PlayerComponent { get; set; }
    public EndPosition EndPosition { get; set; }
    public bool AllowDigonal { get; set; } = false;
    public bool DontCrossCorner { get; set; } = false;

    private A_Star _aStar;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _aStar = new A_Star();
    }

    public void SetDestination()
    {
        var endTransform = EndPosition.transform;

        var playerTransform = PlayerComponent.transform;

        var pathList = _aStar.PathFinding(playerTransform, endTransform, AllowDigonal, DontCrossCorner);

        PlayerComponent.SetPathList(pathList);
    }
}
