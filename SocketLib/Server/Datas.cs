using System.Net.Sockets;

public class SocketObj
{
    public Socket skt;
    public string str;
}

public class ClientSocketObj
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
public class SendMsg
{
    public string info;

    public override string ToString()
    {
        return "info: " + info;
    }
}
