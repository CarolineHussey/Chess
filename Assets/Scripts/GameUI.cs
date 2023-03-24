using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { set; get; } //alternatively could use MonoSingleton

    [SerializeField] private Animator menuAnimator;

    private void Awake()
    {
        Instance = this;
    }

    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("NoMenu");
    }

    public void OnOnlineGameButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        //menuAnimator.SetTrigger("NoMenu"); //add additional logic for this later
    }

    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }




}
