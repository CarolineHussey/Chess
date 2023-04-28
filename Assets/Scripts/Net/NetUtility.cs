using System;
using Unity.Networking.Transport;
using UnityEngine;

/// <summary>
/// a helper class to povide certain funcions and events for messaging throughout the app.  the enum messages are stored and utilised here
/// </summary>

//Net Messages
public enum OpCode
{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    REMATCH = 5
}


public static class NetUtility
{
    /// <summary>
    /// OnData is called when a message is received containing data inside of it and parses the data package.  It is shared by both Client and Server
    /// </summary>
    /// <param name="stream">message container</param>
    /// <param name="cnn">the sender details</param>
    /// <param name="server">default arguement is null. when OnData is called in the server script the method is called with 'this' as the arguement here; 
    /// when it is called in the client script the method uses the default value for the server parameter (ie. null) 
    /// this is important to control flow as it is a shared method, and depending on the arguement will call either ReceivedOnServer or ReceivedOnClient</param>
    public static void OnData(DataStreamReader stream, NetworkConnection cnn, Server server = null)
    {
        NetMessage msg = null; // this represents the content of the data package. it is used to create a message when a data package is received (contents of the message will depend on the OpCode)
        var opCode = (OpCode)stream.ReadByte(); //reads the message description posted on the outside of the container to let the recipient know what type of message is inside

        //use a switch statement to control flow depending on message received. the message is created inside of it's own class. 
        switch (opCode)
        {
            case OpCode.KEEP_ALIVE: msg = new NetKeepAlive(stream); break;
            case OpCode.WELCOME: msg = new NetWelcome(stream); break;
            case OpCode.START_GAME: msg = new NetStartGame(stream); break;
            case OpCode.MAKE_MOVE: msg = new NetMakeMove(stream); break;
            case OpCode.REMATCH: msg = new NetRematch(stream); break;
            default:
                Debug.LogError("message received had no OpCode");
                break;
        }

        if (server != null)
            msg.ReceivedOnServer(cnn);
        else
            msg.ReceivedOnClient();

    }
    //Events associated with net messages

    //messages received by client (Action contains message)
    public static Action<NetMessage> C_KEEP_ALIVE;
    public static Action<NetMessage> C_WELCOME;
    public static Action<NetMessage> C_START_GAME;
    public static Action<NetMessage> C_MAKE_MOVE;
    public static Action<NetMessage> C_REMATCH;

    //messages received server side (Action contains message plus sender details)
    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<NetMessage, NetworkConnection> S_WELCOME;
    public static Action<NetMessage, NetworkConnection> S_START_GAME;
    public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
    public static Action<NetMessage, NetworkConnection> S_REMATCH;
}
