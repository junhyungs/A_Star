using UnityEngine;
using UnityEngine.UI;

public class ToggleBox : MonoBehaviour
{
    [Header("AllowDigonal")]
    [SerializeField] private bool _allowDigonal;

    [Header("DontCrossCorner")]
    [SerializeField] private bool _dontCrossCorner;

    private Toggle _toggle;
    
    void Start()
    {
        _toggle = GetComponent<Toggle>();

        _toggle.isOn = false;

        ToggleAddListener();
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveAllListeners();
    }

    private void ToggleAddListener()
    {
        if (_allowDigonal)
        {
            _toggle.onValueChanged.AddListener(AllowDigonal);
        }
        else if(_dontCrossCorner)
        {
            _toggle.onValueChanged.AddListener(DontCrossCorner);
        }
    }
    
    private void AllowDigonal(bool isOn)
    {
        if (isOn)
        {
            StartManager.Instance.AllowDigonal = true;
        }
        else
        {
            StartManager.Instance.AllowDigonal = false;
        }
    }

    private void DontCrossCorner(bool isOn)
    {
        if (isOn)
        {
            StartManager.Instance.DontCrossCorner = true;
        }
        else
        {
            StartManager.Instance.DontCrossCorner = false;
        }
    }

}
