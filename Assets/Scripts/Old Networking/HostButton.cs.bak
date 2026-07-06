using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HostButton : MonoBehaviour
{
    public Button networkHostButton;
    public TMP_InputField lobbyName;
    [SerializeField]
    InputAction escape;
    bool isTyping = false;
    public void ToggleFields()
    {
        gameObject.GetComponent<Image>().enabled = isTyping;
        gameObject.GetComponentInChildren<TMP_Text>().enabled = isTyping;
        isTyping = !isTyping;
        lobbyName.gameObject.SetActive(isTyping);
        networkHostButton.gameObject.SetActive(isTyping);
    }
    void Start()
    {
        escape.Enable();
    }
    void Update()
    {
        if (escape.WasPressedThisFrame() && isTyping)
        {
            ToggleFields();
        }
    }
}
