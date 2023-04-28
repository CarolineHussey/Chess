using UnityEngine;
using Unity.Networking.Transport;

public class NetRematch : NetMessage
{
    public int teamId;
    public byte wantRematch;
    public NetRematch() //when the package is created for sending - this class assigns the correct message
    {
        // assign the correct OpCode 
        Code = OpCode.REMATCH;
    }

    public NetRematch(DataStreamReader reader) //<-- when a package is received with the REMATCH message
    {
        //parse the data packet
        Code = OpCode.REMATCH;
        Deserialize(reader); //unpack the box
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        //write the OpCode to the data packet
        writer.WriteByte((byte)Code);
        writer.WriteInt(teamId);
        writer.WriteByte(wantRematch);

    }

    //it is important that data is read in the same order that it is written - otherwise it will get corrupted.  
    public override void Deserialize(DataStreamReader reader)
    {
        //the byte datatype is already read in the NetUtility::OnData
        teamId = reader.ReadInt();
        wantRematch = reader.ReadByte();

    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_REMATCH?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_REMATCH?.Invoke(this, cnn);
    }
}
