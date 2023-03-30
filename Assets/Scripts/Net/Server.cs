using System;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Server : MonoBehaviour
{
    #region Singleton Implementation
    //create an instance of the object as a "singleton" (Monosngleton would offer better protection against new singleton)
    public static Server Instance { set; get; } //alternatively could use MonoSingleton

    //use awake call to assign instance
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    
    //Fields
    public NetworkDriver driver;
    private NativeList<NetworkConnection> connections; //list of network connections - everyone who is connected to this server

    private bool isActive = false; //to check if server is active or not

    private const float keepAliveTickRate = 20.0f; //send a message every 20 seconds to prevent connection from droppping (if connection drops for ~30 seconds the client will automatically disconnect)
    private float lastKeepAlive; //tmestamp for keepAliveTickRate

    public Action connectionDropped; //if one player drops this action will be called

    //Methods
    public void Init(ushort port) //we have to send in a port that is not in any other use by the host machine
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4; //allow any IPv4 address to connect

        endpoint.Port = port;

        //listener
        if (driver.Bind(endpoint) != 0) //if not connected (0 = successful)
        {
            Debug.Log("Unable to bind on port" + endpoint.Port); //if Binding is not a success we were unable to bind to the port (potentially server is already occupied, or disconnected)
            return;
        }   
        else
        {
            driver.Listen(); //we are listening!
            Debug.Log("Listening on port " + endpoint.Port);
        }

        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent); //parameters: max connections (two in this case) 
        isActive = true;

    }
    public void ShutDown()
    {
        if (isActive)
        {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
        }
    }
    public void OnDestroy()
    {
        ShutDown();
    }
}
