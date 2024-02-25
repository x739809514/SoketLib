// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main(String[] args)
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12700);

        socket.Connect(pt);
        Console.WriteLine("Client Start...");

        byte[] dataRcv = new byte[1024];
        int len = socket.Receive(dataRcv);

        string rcvMsg = Encoding.UTF8.GetString(dataRcv, 0, len);
        Console.WriteLine("Rcv Server Data: " + rcvMsg);
    }
}