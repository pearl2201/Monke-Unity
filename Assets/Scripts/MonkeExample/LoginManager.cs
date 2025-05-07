
using LiteNetLib.Utils;
using MonkeNet.Client;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonkeExample
{
    public class LoginManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameObject loginWindow;
        [SerializeField]
        private TMP_InputField nameInput;
        [SerializeField]
        private Button submitLoginButton;

        void Start()
        {
            Debug.Log("Login Manager start");
            ClientManager.Instance.NetworkManager.ClientConnected += StartLoginProcess;
            submitLoginButton.onClick.AddListener(OnSubmitLogin);
            ClientManager.Instance.CommandReceived += OnMessage;
            loginWindow.SetActive(value: true);
            //loginWindow.SetActive(false);
        }

        private void OnMessage(Area area, int areaId, INetSerializable command)
        {
            if (command is LoginInfoData loginInfoData)
            {
                OnLoginAccept(loginInfoData);
            }
            else if (command is LoginRequestDenied loginRequestDenied)
            {
                OnLoginDecline();
            }

        }

        private void StartLoginProcess(MonkeNetPeer id)
        {
            Debug.Log("StartLoginProcess");
            loginWindow.SetActive(true);
        }

        void OnDestroy()
        {
            ClientManager.Instance.NetworkManager.ClientConnected -= StartLoginProcess;
            ClientManager.Instance.CommandReceived -= OnMessage;
        }

        public void OnSubmitLogin()
        {
            if (!String.IsNullOrEmpty(nameInput.text))
            {
                loginWindow.SetActive(false);
                ClientManager.Instance.SendCommandToServer(new LoginAnonymouseRequestData(nameInput.text));
            }
        }

        private void OnLoginDecline()
        {
            loginWindow.SetActive(true);
        }

        private void OnLoginAccept(LoginInfoData data)
        {
            CacheRuntime.Instance.PlayerId = data.Id;
            CacheRuntime.Instance.LobbyInfoData = data.Data;
            SceneManager.LoadScene("Lobby");
            
        }
    }
}
