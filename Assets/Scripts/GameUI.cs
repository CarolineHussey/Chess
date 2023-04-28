using UnityEngine;
using TMPro;
using System;

public enum CameraAngle
{
    menu = 0,
    whiteTeam = 1,
    blackTeam = 2
}

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { set; get; } //alternatively could use MonoSingleton

    public Server server;
    public Client client;

    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject[] cameraAngles;

    public Action<bool> SetLocalGame; 
    private void Awake()
    {
        Instance = this;
        RegisterEvents();
    }

    //Cameras

    /// <summary>
    /// iterates through all camera angles in the list of cameraAngles and sets them to inactive.  it then activates the right camera for the respective player
    /// this is called in 'OnStartGameClient' in ChessB - tied to START_GAME messaging
    /// </summary>
    /// <param name="index">takes the team identifier as an arguement and uses that to set the right camera</param>
    public void ChangeCamera(CameraAngle index)
    {
        for (int i = 0; i < cameraAngles.Length; i++)
            cameraAngles[i].SetActive(false);

        cameraAngles[(int)index].SetActive(true);
    }

    //Buttons
    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("NoMenu");
        SetLocalGame?.Invoke(true);
        server.Init(8005);
        client.Init("127.0.0.1", 8005);
    }

    public void OnOnlineGameButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        server.Init(8005);
        client.Init("127.0.0.1", 8005);
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
        client.Init(addressInput.text, 8005);
        menuAnimator.SetTrigger("NoMenu"); 
    }

    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        server.ShutDown();
        client.ShutDown();
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnExitGameMenu()
    {
        ChangeCamera(CameraAngle.menu);
        menuAnimator.SetTrigger("StartMenu");
    }

    //use messaging (copied from ChessB) to control menu switching when game starts
    #region
    private void RegisterEvents()
    {

        NetUtility.C_START_GAME += OnSartGameClient; //client is listening for the START_GAME message

    }

    private void UnRegisterEvents()
    {
        NetUtility.C_START_GAME -= OnSartGameClient;
    }
    private void OnSartGameClient(NetMessage obj)
    {
        menuAnimator.SetTrigger("NoMenu");
    }

    #endregion


}
