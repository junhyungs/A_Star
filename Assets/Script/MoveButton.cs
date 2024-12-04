using UnityEngine;
using UnityEngine.UI;
public class MoveButton : MonoBehaviour
{
    private Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();

        _button.onClick.AddListener(StartManager.Instance.SetDestination);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(StartManager.Instance.SetDestination);
    }
}
