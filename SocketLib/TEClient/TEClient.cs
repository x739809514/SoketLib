using System.Text;
using Newtonsoft.Json;
using RXNet;
using RXProtocol;
using TEClient;

public class RXClient
{
    static void Main(string[] args)
    {
        RXSocket<ClientSession, NetMsg> client = new RXSocket<ClientSession, NetMsg>();
        client.StartASClient("127.0.0.1", 12703);
        while (true)
        {
            string str = Console.ReadLine();
            if (str.Equals("quit"))
            {
                client.CloseClient();
                break;
            }
            else
            {
                ReqInfo msg = new ReqInfo() { serverID = 101, name = "Russell", port = 12703 };
                var jsonData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new NetMsg()
                {
                    cmd = (int)CMD.ReqInfo,
                    ReqInfo = msg,
                }));
                var data = RXTool.PackLenInfo(jsonData);
                client.session.SendMsg(data);
            }
            
        }
    }
}