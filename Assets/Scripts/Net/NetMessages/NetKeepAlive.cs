using Unity.Networking.Transport;
public class NetKeepAlive : NetMessage
{
    //this script handles packages concerned with the OpCode KEEP_ALIVE
    //constructors
    public NetKeepAlive() //when the package is created for sending- this class assigns the correct message
    {
        //when a new package is created, assign the correct OpCode for simple parsing
        Code = OpCode.KEEP_ALIVE;
    }

    public NetKeepAlive(DataStreamReader reader) //<-- when a package is received with the keepAlive message
    {
        //parse the data packet
        Code = OpCode.KEEP_ALIVE;
        Deserialize(reader); //unpack the box
    }

    //this is called in the SendToClient method (here it is just the simple byte to keep the network connection alive)
    public override void Serialize(ref DataStreamWriter writer)
    {
        //write the OpCode to the data packet
        writer.WriteByte((byte)Code);
    }

    //on this script the byte is already parsed to keep the network connection alive (first when OnData is called, then in NetKeepAlive above): as there is no other content to this message, we therefore don't need to deserialize any further here 
    public override void Deserialize(DataStreamReader reader)
    {
        
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_KEEP_ALIVE?.Invoke(this, cnn);
    }
}
