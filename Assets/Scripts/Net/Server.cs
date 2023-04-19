using System;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Server : MonoBehaviour
{
    #region Singleton Implementation
    //create an instance of the object as a "singleton" (Monosingleton would offer better protection against new singleton)
    public static Server Instance { set; get; } //alternatively could use MonoSingleton

    //use awake call to assign instance
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    
    //Fields
    public NetworkDriver driver; //Network transport
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

        endpoint.Port = port; //sets the port field to the port send in when the method is called with an arguement

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

    public void Update()
    {
        if (!isActive)
            return; //if it is not active, do nothing
        KeepAlive(); //sends a message every 20 seconds to make sre there is constant communication back & forth between the server & client. Here the server will send a message (the client will listen out for any messages from the server, and if it finds one, it will ping back
        driver.ScheduleUpdate().Complete(); //empties queue of messages coming in 

        CleanupConections(); //is there anyone no longer connected that we still have the reference
        AcceptNewConnections(); //is there anyone trying to connect to our server?
        UpdateMessagePump(); //are they sending us a message? do we have to reply?
    }

    private void KeepAlive()
    {
        if (Time.time - lastKeepAlive > keepAliveTickRate)
        {
            lastKeepAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }

    private void CleanupConections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated) //if the connecton is not created
            {
                connections.RemoveAtSwapBack(i); //remove the connection at swapback
                --i;
            }
        }
    }
    private void AcceptNewConnections()
    {
        NetworkConnection c;

        while ((c = driver.Accept()) != default(NetworkConnection)) //if someone is trying to make a connection and it is not the default network connection
        {
            connections.Add(c); //add to list of connections
        }
    }

    private void UpdateMessagePump()
    {
        DataStreamReader stream; //streamReader to be used for messages if there is one
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd; //the messages they can send is either Data (Net message - package with message / specific informaiton in it) or Disconnect
            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(stream, connections[i], this);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    connections[i] = default(NetworkConnection); //reset the connection
                    connectionDropped?.Invoke(); 
                    ShutDown(); //if the other player disconnects, shut down the connection (applicable for 2 player games only)
                }
            }
        }

    }
    
    //Server Specific methods
    /// <summary>
    /// Use when you want to send a specific message to a specifc person (only one client)
    /// </summary>
    /// <param name="connection">Start a connection with a very specific person</param>
    /// <param name="msg"></param>
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer); //BeginSend starts by grabbing the packet, writes down the name/address of the destination. It puts the writer into the pipeline so that the clients can send messages between each other; 
        msg.Serialize(ref writer); //The packet is received from BeginSend and puts it through our own messageWriter - where we can fill in the package received with a message (information).
        driver.EndSend(writer); //The packet is sent back to the address provided in BeginSend - this time with the message / information we wrote to it.
    }

    /// <summary>
    /// Use when you want to send a single message to every single client there is
    /// connections is a list of clients connected to the server
    /// </summary>
    /// <param name="msg"></param>
    public void Broadcast(NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++) //go through every client in the list of connections
            if(connections[i].IsCreated)
            {
             //   Debug.Log($"Sending {msg.Code} to : {connections[i].InternalId}");
                SendToClient(connections[i], msg);//create a package for every client listed in connections (each client will have their own package for sending / receiving messages)
            }
    }


}
