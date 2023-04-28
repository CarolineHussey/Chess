using UnityEngine;
using Unity.Networking.Transport;

public class NetMakeMove : NetMessage
{
    //this script handles packages concerned with the OpCode MAKE_MOVE

    //Fields
    public int originalX;
    public int originalY;
    public int destinationX;
    public int destinationY;
    public int teamID;

    //constructors
    public NetMakeMove() //when the package is created for sending - this class assigns the correct message
    {
        // assign the correct OpCode 
        Code = OpCode.MAKE_MOVE;
    }

    public NetMakeMove(DataStreamReader reader) //<-- when a package is received with the MAKE_MOVE message
    {
        //parse the data packet
        Code = OpCode.MAKE_MOVE;
        Deserialize(reader); //unpack the box
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        //write the OpCode to the data packet
        writer.WriteByte((byte)Code);
        writer.WriteInt(originalX);
        writer.WriteInt(originalY);
        writer.WriteInt(destinationX);
        writer.WriteInt(destinationY);
        writer.WriteInt(teamID);

    }

    //it is important that data is read in the same order that it is written - otherwise it will get corrupted.  
    public override void Deserialize(DataStreamReader reader)
    {
        //the byte datatype is already read in the NetUtility::OnData
        originalX = reader.ReadInt();
        originalY = reader.ReadInt();
        destinationX = reader.ReadInt();
        destinationY = reader.ReadInt();
        teamID = reader.ReadInt();

    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);
    }
}
