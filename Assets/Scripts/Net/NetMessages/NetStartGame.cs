using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetStartGame : NetMessage
{
    //this script handles packages concerned with the OpCode START_GAME

    //constructors
    public NetStartGame() //when the package is created for sending - this class assigns the correct message
    {
        // assign the correct OpCode 
        Code = OpCode.START_GAME;
    }

    public NetStartGame(DataStreamReader reader) //<-- when a package is received with the START_GAME message
    {
        //parse the data packet
        Code = OpCode.START_GAME;
        Deserialize(reader); //unpack the box
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        //write the OpCode to the data packet
        writer.WriteByte((byte)Code);
    }

    //it is important that data is read in the same order that it is written - otherwise it will get corrupted.  
    public override void Deserialize(DataStreamReader reader)
    {
        //the byte datatype is already read in the NetUtility::OnData
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_START_GAME?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_START_GAME?.Invoke(this, cnn);
    }
}
