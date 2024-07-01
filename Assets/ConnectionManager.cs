using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    public void CreateRoom()
    {
        NetworkManager.Instance.CreateSession(inputField.text);
    }

    public void JoinRoom()
    {
        NetworkManager.Instance.JoinSession(inputField.text);

    }
}
