using System.Net;
using System.Net.Sockets;

public class NetPackage
{
    public int headLen = 4;
    public byte[] headBuff;
    public int headIndex = 0;

    public int bodyLen = 0;
    public byte[] bodyBuff;
    public int bodyIndex = 0;

    public NetPackage()
    {
        headBuff = new byte[4];
    }

    public void InitBodyBuff()
    {
        // here server receives the head of msg, then parse the head msg
        // in this 4 byte msg, there is the length of the body message
        bodyLen = BitConverter.ToInt32(headBuff, 0);
        bodyBuff = new byte[bodyLen];
    }
}

public class SendObj
{
    public Socket skt;
    public EndPoint pt;
}

public class ReceiveData
{
    public Socket skt;
    public NetPackage netPack;
}

[Serializable]
public class LoginMsg
{
    public int serverID;
    public string mail;
    public string password;
    public override string ToString()
    {
        return "ServerID is: " + serverID + " mail is: " + mail + " password is:" + password;
    }
}

[Serializable]
public class SendMsg{
    public string info;

    public override string ToString()
    {
        return "info: " + info;
    }
}