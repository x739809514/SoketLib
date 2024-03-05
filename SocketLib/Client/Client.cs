// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Newtonsoft.Json;



class Client
{
    static bool isCanceled = false;
    static int[] segLenArr = new int[] { 2, 1, 3, 1000 };
    static int sendIndex = -1;

    static void Main(String[] args)
    {
        ASyncConnection();
        Console.Read();
    }

    static byte[] SerializableData()
    {
        LoginMsg msg = new LoginMsg()
        {
            serverID = 101,
            mail = "1612650023@qq.com",
            password = "xxoo"
        };

        string json = JsonConvert.SerializeObject(msg);
        byte[] data = Encoding.UTF8.GetBytes(json);

        int len = data.Length;
        byte[] pkg = new byte[len + 4];
        byte[] head = BitConverter.GetBytes(len);
        head.CopyTo(pkg, 0);
        data.CopyTo(pkg, 4);

        // send by segment
        List<byte[]> dataList = new List<byte[]>();
        int takeCount = 0;
        for (int i = 0; i < segLenArr.Length; i++)
        {
            byte[] segBytes = pkg.Skip(takeCount).Take(segLenArr[i]).ToArray();
            takeCount += segLenArr[i];
            dataList.Add(segBytes);
        }
        sendIndex++;

        return dataList[sendIndex];
    }

    static SendMsg DeSerializeData(byte[] dat)
    {
        string data = Encoding.UTF8.GetString(dat);
        SendMsg msg = JsonConvert.DeserializeObject<SendMsg>(data);
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

    #region ASync
    static void ASyncConnection()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12701);
        Console.WriteLine("Client Start...");
        try
        {
            // connection is only once
            socket.BeginConnect(pt, new AsyncCallback(ConnectionHandle), new SendObj() { skt = socket, pt = pt });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        // send data
        while (true)
        {
            string msgSend = Console.ReadLine();
            if (msgSend.Length > 0)
            {
                if (msgSend.Equals("quit"))
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    isCanceled = true;
                    break;
                }
                else
                {
                    //ASync Sending data
                    //socket.Send(Encoding.UTF8.GetBytes(msgSend));
                    byte[] data = Encoding.UTF8.GetBytes(msgSend);
                    data = SerializableData();
                    NetworkStream ns = null;
                    try
                    {
                        ns = new NetworkStream(socket);
                        if (ns.CanWrite)
                        {
                            ns.BeginWrite(
                                data,
                                0,
                                data.Length,
                                new AsyncCallback(SendHandle),
                                ns
                            );
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
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

    static void ConnectionHandle(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is SendObj args)
            {
                args.skt.EndConnect(result);

                byte[] dataRcv = new byte[4];
                NetPackage pkg = new NetPackage();
                args.skt.BeginReceive(pkg.headBuff, 0, pkg.headLen, SocketFlags.None, ASyncHeadRcv, new ReceiveData() { skt = args.skt, netPack = pkg });

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
            if (result.AsyncState is ReceiveData args)
            {
                // receive data buffer
                byte[] dataRcv = args.netPack.headBuff;
                int lenRcv = args.skt.EndReceive(result);
                if (lenRcv == 0)
                {
                    Console.WriteLine("Server is offline");
                    args.skt.Shutdown(SocketShutdown.Both);
                    args.skt.Close();
                    return;
                }
                else
                {
                    args.netPack.headIndex += lenRcv;
                    if (args.netPack.headIndex < 4)
                    {
                        args.skt.BeginReceive(
                            args.netPack.headBuff,
                            args.netPack.headIndex,
                            args.netPack.headLen - args.netPack.headIndex,
                            SocketFlags.None,
                            ASyncHeadRcv,
                            args
                        );
                    }
                    else
                    {
                        args.netPack.InitBodyBuff();
                        // recevive data
                        args.skt.BeginReceive(
                            args.netPack.bodyBuff,
                            0,
                            args.netPack.bodyLen,
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
            if (result.AsyncState is ReceiveData args)
            {
                int lenRcv = args.skt.EndReceive(result);
                args.netPack.bodyIndex += lenRcv;
                if (args.netPack.bodyIndex < args.netPack.bodyLen)
                {
                    args.skt.BeginReceive(
                        args.netPack.bodyBuff,
                        args.netPack.bodyIndex,
                        args.netPack.bodyLen - args.netPack.bodyIndex,
                        SocketFlags.None,
                        ASyncBodyRcv,
                        args
                    );
                }
                else
                {
                    SendMsg sendMsg = DeSerializeData(args.netPack.bodyBuff);
                    Console.WriteLine(sendMsg.ToString());
                }

                args.skt.BeginReceive(args.netPack.bodyBuff, 0, 4, SocketFlags.None, ASyncHeadRcv, new ReceiveData() { skt = args.skt, netPack = args.netPack });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    #endregion

    #region Sync
    static void SyncSendData()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12700);

        socket.Connect(pt);
        Console.WriteLine("Client Start...");

        while (true)
        {
            byte[] dataRcv = new byte[1024];
            int len = socket.Receive(dataRcv);

            string rcvMsg = Encoding.UTF8.GetString(dataRcv, 0, len);
            Console.WriteLine("Rcv Server Data: " + rcvMsg);

            while (true)
            {
                string msgSend = Console.ReadLine();
                if (msgSend.Length > 0)
                {
                    if (msgSend.Equals("quit"))
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    else
                    {
                        socket.Send(Encoding.UTF8.GetBytes(msgSend));
                        break;
                    }
                }
            }
        }
    }
    #endregion
}