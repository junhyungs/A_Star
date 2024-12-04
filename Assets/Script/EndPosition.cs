using UnityEngine;

public class EndPosition : MonoBehaviour
{
    private void Start()
    {
        StartManager.Instance.EndPosition = this;
    }
}
