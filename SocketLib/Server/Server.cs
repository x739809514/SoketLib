// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static void Main(String[] args)
    {
        CreateBasicServer();
    }

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
                if (lenRcv==0)
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
}