using System;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
    #region Singleton implementation
    public static Client Instance { set; get; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    //Fields
    public NetworkDriver driver; //Network transport
    private NetworkConnection connection; //a single connection to the server (the client doesn't access any other connection but the server)

    private bool isActive = false; //to check if server is active or not

    public Action connectionDropped; //if one player drops this action will be called

    //Methods
    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port); //ip address will be provided in the input field on the front end; port will be defined in the back end

        connection = driver.Connect(endpoint);

        Debug.Log("Attempting to connect to Server on " + endpoint.Address);
        isActive = true;

        RegisterToEvent(); //register to other scripts if something happens.  Here it will keep track of the "Keep Alive" messages
    }
    public void ShutDown()
    {
        if (isActive)
        {
            UnregisterFromEvent(); //Does the opposite of the RegisterToEvent() method
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection); //resets network connection (shouldn;t be needed here as it is reset elsewhere, but just in case, for now!
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

        driver.ScheduleUpdate().Complete(); //empties queue of messages coming in 
        CheckAlive(); //if the server is down it will not be sending the KeepAlive() message; here we check if the message is sent from the server & that the server is still alive & active
        UpdateMessagePump();
    }

    private void CheckAlive()
    {
        if (!connection.IsCreated && isActive) //if connection is not created and I am supposed to be active
        {
            Debug.Log("Something went wrong, lost connection to the server");
            connectionDropped?.Invoke();
            ShutDown(); //shutdown the client (so we can try to reconnect, or connect to another client)
        }
    }

    private void UpdateMessagePump()//the client version of the UpdateMessagePump method found in the Server script
    {
        DataStreamReader stream; //streamReader to be used for messages if there is one
        
        NetworkEvent.Type cmd; //the messages they can send is either Data (Net message - package with message / specific informaiton in it) or Disconnect
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty) //looking for messages from that specific connection (connection = that specific connection) (driver pulls all connections - server)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                //SendToServer(new NetWelcome());
                Debug.Log("Connected!");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                NetUtility.OnData(stream, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client disconnected from server");
                connection = default(NetworkConnection); //reset the connection
                connectionDropped?.Invoke();// the question mark makes sure that we call an event that is not empty - if we call an event that is empty the game will crash (the question mark checks if it is null - if it is null it does nothing, if it is not null it will call the invoke)
                ShutDown(); //if the other player disconnects, shut down the connection (applicable for 2 player games only)
            }
        }

    }

    public void SendToServer(NetMessage msg) //the client version of the SentToServer method found in the Server script
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);  
        msg.Serialize(ref writer); 
        driver.EndSend(writer); 
    }

    //Event Parsing (events that are bound to every sincle message
    public void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive; //calls OnKeepAlive (with the message received as an arguement) whenever a KeepAlive message is received

    }

    public void UnregisterFromEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage nm) //nm = KeepAlive message received
    {
        SendToServer(nm);//message is sent back to server as is - it isn't specifically parsed.  the server will generate the KeepAlive message every 20 seconds to keep the connection alive
    }
}

