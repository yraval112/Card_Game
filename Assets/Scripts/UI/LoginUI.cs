using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField usernameInput;

    public void OnSetBtnClick()
    {
        string playerName = usernameInput.text;
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Username cannot be empty");
            return;
        }

        GameSocketManager.Instance.Connect(playerName);
        gameObject.SetActive(false);
    }
}
