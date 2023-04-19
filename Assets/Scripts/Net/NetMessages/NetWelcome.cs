using System.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetWelcome : NetMessage
{
    //this script handles packages concerned with the OpCode WELCOME
    
    //this property is the data contained inside of the message packet
    public int AssignedTeam { set; get; } //the server will assign the team, the client does not get to choose.  

    //constructors
    public NetWelcome() //when the package is created for sending - this class assigns the correct message
    {
        // assign the correct OpCode 
        Code = OpCode.WELCOME;
    }

    public NetWelcome(DataStreamReader reader) //<-- when a package is received with the Welcome message
    {
        //parse the data packet
        Code = OpCode.WELCOME;
        Deserialize(reader); //unpack the box
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        //write the OpCode and team to the data packet
        writer.WriteByte((byte)Code);
        writer.WriteInt(AssignedTeam); //Assignedteam is sent as an empty - the server will assign that 
    }

    //it is important that data is read in the same order that it is written - otherwise it will get corrupted.  
    public override void Deserialize(DataStreamReader reader)
    {
        //the byte datatype is already read in the NetUtility::OnData - so we only need to read the second datatype here which is an int
        AssignedTeam = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_WELCOME?.Invoke(this, cnn);
    }

}
