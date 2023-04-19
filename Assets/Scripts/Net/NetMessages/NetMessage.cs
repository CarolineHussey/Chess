using UnityEngine;
using Unity.Networking.Transport;
/// <summary>
/// the smallest message size of a Netmessage is a byte, and we can send 0 - 255 bytes. if we send more than 255 messages, each message will then be 2 bytes (a more costly operation)
/// 
/// </summary>

public class NetMessage //NetMessage is the message container that is sent between client and server (the contents will be based on a base message that all other messages will inherit from through virtual methods)
{
    public OpCode Code { set; get; }

    //server writes a message to the message container
    public virtual void Serialize(ref DataStreamWriter writer ) 
    {
        writer.WriteByte((byte)Code);
    }

    //client receives the message and reads it.
    public virtual void Deserialize(DataStreamReader reader) 
    {

    }

    public virtual void ReceivedOnClient() //when message is received from client, we know who is sending the message, so don;t need the NetworkConnection parameter 
    {

    }

    public virtual void ReceivedOnServer(NetworkConnection cnn)//server needs to know which client the message is from so that info is passed as an arguement (cnn)
    {

    }
}
