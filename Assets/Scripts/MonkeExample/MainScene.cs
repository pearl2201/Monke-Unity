using MonkeNet.Client;
using MonkeNet.Shared;
using System;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    public bool isClient;
    [SerializeField] GameObject clientGroup;
    [SerializeField] GameObject loginManager;
    [SerializeField] GameObject serverGroup;
    [SerializeField] GameObject natigationBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SendRequestCreateClient()
    {
        //ClientManager.Instance.MakeEntityRequest((byte)GameEntityManager.EntityType.Player);
    }

    public async void CreateHost()
    {
        CreateServer();
        //await Task.Delay(TimeSpan.FromSeconds(1f));
        OnConnectedToServer();

    }

    public void OnConnectedToServer()
    {
        isClient = true;
        clientGroup.SetActive(true);
        natigationBar.SetActive(false);
        loginManager.gameObject.SetActive(true);
        MonkeNetManager.Instance.CreateClient("localhost", 9999);

    }

    public void CreateServer()
    {
        isClient = false;
        serverGroup.SetActive(true);
        MonkeNetManager.Instance.CreateServer(9999);
        natigationBar.SetActive(false);
    }    
}
