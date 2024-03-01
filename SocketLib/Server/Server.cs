﻿// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

class SocketObj
{
    public Socket skt;
    public string str;
}

class ClientSocketObj
{
    public Socket skt;
    public byte[] data;
}

[Serializable]
class LoginMsg
{
    public int serverID;
    public string mail;
    public string password;
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
            socket.BeginAccept(new AsyncCallback(ASyncConnection), new SocketObj() { skt = socket, str = "default" });
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
                clientSocket.BeginReceive(dataRcv, 0, dataRcv.Length, SocketFlags.None, new AsyncCallback(ASyncHeadRcv), new ClientSocketObj() { skt = clientSocket, data = dataRcv });

                // since a child thread is closed, so we need to start a new one
                args.skt.BeginAccept(new AsyncCallback(ASyncConnection), new SocketObj() { skt = args.skt, str = "default" });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void ASyncHeadRcv(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is ClientSocketObj args)
            {
                // receive data buffer
                byte[] dataRcv = args.data;
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
                    if (lenRcv < 4)
                    {
                        // Todo: Continue to study
                    }
                    else
                    {
                        // here server receives the head of msg, then parse the head msg
                        // in this 4 byte msg, there is the length of the body message
                        int bodyLen = BitConverter.ToInt32(args.data, 0);
                        args.data = new byte[bodyLen];
                        // recevive data
                        args.skt.BeginReceive(
                            args.data,
                            0,
                            bodyLen,
                            SocketFlags.None,
                            new AsyncCallback(ASyncBodyRcv),
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
                if (lenRcv < args.data.Length)
                {
                    // Todo: continue to receive
                }
                else
                {
                    LoginMsg loginMsg = DeSerializeData(args.data);
                    Console.WriteLine("Server_SeverID: " + loginMsg.serverID);
                    Console.WriteLine("Server_Mail: " + loginMsg.mail);
                    Console.WriteLine("Server_Password: " + loginMsg.password);
                }

                args.skt.BeginReceive(args.data, 0, 4, SocketFlags.None, new AsyncCallback(ASyncHeadRcv), new ClientSocketObj() { skt = args.skt, data = new byte[4]});
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void ASyncDataRcv(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is ClientSocketObj args)
            {
                // receive data buffer
                byte[] dataRcv = args.data;
                int lenRcv = args.skt.EndReceive(result);
                if (lenRcv == 0)
                {
                    Console.WriteLine("Client is offline");
                    args.skt.Shutdown(SocketShutdown.Both);
                    args.skt.Close();
                    return;
                }
                Console.WriteLine("The Child thread ID: " + Thread.CurrentThread.ManagedThreadId.ToString());

                //string msgRcv = Encoding.UTF8.GetString(dataRcv, 0, lenRcv);
                //Console.WriteLine("Rcv Client Msg: " + msgRcv);
                LoginMsg loginMsg = DeSerializeData(dataRcv);
                Console.WriteLine("Server_SeverID: " + loginMsg.serverID);
                Console.WriteLine("Server_Mail: " + loginMsg.mail);
                Console.WriteLine("Server_Password: " + loginMsg.password);

                string sendingdata = JsonConvert.SerializeObject(loginMsg);
                // send what its receive
                byte[] rcv = Encoding.UTF8.GetBytes(sendingdata);

                //args.skt.Send(rcv);
                NetworkStream ns = null;
                try
                {
                    ns = new NetworkStream(args.skt);
                    if (ns.CanWrite)
                    {
                        ns.BeginWrite(
                            rcv,
                            0,
                            rcv.Length,
                            new AsyncCallback(SendHandle),
                            ns
                        );
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                args.skt.BeginReceive(dataRcv, 0, 1024, SocketFlags.None, new AsyncCallback(ASyncDataRcv), new ClientSocketObj() { skt = args.skt, data = dataRcv });
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