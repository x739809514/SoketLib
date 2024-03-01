// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

class SocketObj
{
    public Socket skt;
    public string str;
}

class ClientSocketObj
{
    public Socket skt;
    public NetPackage pack;
}

[Serializable]
class LoginMsg
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
class SendMsg
{
    public string info;

    public override string ToString()
    {
        return "info: " + info;
    }
}

class Server
{
    static void Main(String[] args)
    {
        Console.WriteLine("The main thread ID: " + Thread.CurrentThread.ManagedThreadId.ToString());
        CreateASyncConnection();
        //CreateBasicServer();
        Console.Read();
    }

    static LoginMsg DeSerializeData(byte[] dat)
    {
        string data = Encoding.UTF8.GetString(dat);
        LoginMsg msg = JsonConvert.DeserializeObject<LoginMsg>(data);
        if (msg != null)
        {
            return msg;
        }
        else
        {
            Console.WriteLine("Deserialize failed");
            return null;
        }
    }

    #region ASynclear
    static void CreateASyncConnection()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12701);
        try
        {
            socket.Bind(pt);
            socket.Listen(100);
            Console.WriteLine("Server Start...");
            socket.BeginAccept(ASyncConnection, new SocketObj() { skt = socket, str = "default" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void ASyncConnection(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is SocketObj args)
            {
                Socket clientSocket = args.skt.EndAccept(result);
                Console.WriteLine("The Receive thread ID: " + Thread.CurrentThread.ManagedThreadId.ToString());

                string msg = "Connect Successful";
                byte[] bytes = Encoding.UTF8.GetBytes(msg);
                clientSocket.Send(bytes);

                byte[] dataRcv = new byte[4];
                NetPackage pkg = new NetPackage();
                clientSocket.BeginReceive(pkg.headBuff, 0, pkg.headLen, SocketFlags.None, ASyncHeadRcv, new ClientSocketObj() { skt = clientSocket, pack = pkg });

                // since a child thread is closed, so we need to start a new one
                args.skt.BeginAccept(ASyncConnection, new SocketObj() { skt = args.skt, str = "default" });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    // Send Info Message to Client
    static byte[] SendInfoMsg(Socket socket, SendMsg msg)
    {
        #pragma warning restore format
        string json = JsonConvert.SerializeObject(msg);
        byte[] data = Encoding.UTF8.GetBytes(json);

        int len = data.Length;
        byte[] pkg = new byte[len + 4];
        byte[] head = BitConverter.GetBytes(len);
        head.CopyTo(pkg, 0);
        data.CopyTo(pkg, 4);

        return data;
    }

    static void ASyncHeadRcv(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is ClientSocketObj args)
            {
                // receive data buffer
                byte[] dataRcv = args.pack.headBuff;
                int lenRcv = args.skt.EndReceive(result);
                if (lenRcv == 0)
                {
                    Console.WriteLine("Client is offline");
                    args.skt.Shutdown(SocketShutdown.Both);
                    args.skt.Close();
                    return;
                }
                else
                {
                    args.pack.headIndex += lenRcv;
                    if (args.pack.headIndex < 4)
                    {
                        Console.WriteLine(lenRcv + "_head need to continue receive");
                        args.skt.BeginReceive(
                            args.pack.headBuff,
                            args.pack.headIndex,
                            args.pack.headLen - args.pack.headIndex,
                            SocketFlags.None,
                            ASyncHeadRcv,
                            args
                        );
                    }
                    else
                    {
                        args.pack.InitBodyBuff();
                        // recevive data
                        args.skt.BeginReceive(
                            args.pack.bodyBuff,
                            0,
                            args.pack.bodyLen,
                            SocketFlags.None,
                            ASyncBodyRcv,
                            args
                        );
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void ASyncBodyRcv(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is ClientSocketObj args)
            {
                int lenRcv = args.skt.EndReceive(result);
                args.pack.bodyIndex += lenRcv;
                if (args.pack.bodyIndex < args.pack.bodyLen)
                {
                    Console.WriteLine("body need to continue receive");
                    args.skt.BeginReceive(
                        args.pack.bodyBuff,
                        args.pack.bodyIndex,
                        args.pack.bodyLen - args.pack.bodyIndex,
                        SocketFlags.None,
                        ASyncBodyRcv,
                        args
                    );
                }
                else
                {
                    LoginMsg loginMsg = DeSerializeData(args.pack.bodyBuff);
                    Console.WriteLine(loginMsg.ToString());
                    // Console.WriteLine("Server_SeverID: " + loginMsg.serverID);
                    // Console.WriteLine("Server_Mail: " + loginMsg.mail);
                    // Console.WriteLine("Server_Password: " + loginMsg.password);
                }

                args.skt.BeginReceive(args.pack.bodyBuff, 0, 4, SocketFlags.None, ASyncHeadRcv, new ClientSocketObj() { skt = args.skt, pack = args.pack });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void SendHandle(IAsyncResult result)
    {
        try
        {

            if (result.AsyncState is NetworkStream ns)
            {
                ns.EndWrite(result);
                ns.Flush();
                ns.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void HandleInfoMsg(SendMsg msg)
    {
        Console.WriteLine("Client Rcv Data: " + msg.ToString());
    }
    #endregion

    #region SyncReceive
    static void CreateBasicServer()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12700);
        socket.Bind(pt);
        socket.Listen(100);
        Console.WriteLine("Server Start...");

        while (true)
        {
            Socket skt = socket.Accept();

            Thread thread = new Thread(ReveiveHandle);
            thread.Start(skt);
        }
    }

    private static void ReveiveHandle(object obj)
    {
        Socket skt = (Socket)obj;
        string msg = "Connect Successful";
        byte[] bytes = Encoding.UTF8.GetBytes(msg);
        skt.Send(bytes);

        while (true)
        {
            try
            {
                // receive data buffer
                byte[] dataRcv = new byte[1024];
                int lenRcv = skt.Receive(dataRcv);
                if (lenRcv == 0)
                {
                    Console.WriteLine("Client is quit");
                    break;
                }
                string msgRcv = Encoding.UTF8.GetString(dataRcv, 0, lenRcv);
                Console.WriteLine("Rcv Client Msg: " + msgRcv);
                // send what its receive
                byte[] rcv = Encoding.UTF8.GetBytes(msgRcv);
                skt.Send(rcv);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                break;
            }
        }
    }
    #endregion
}