using System.Text;
using Newtonsoft.Json;
using RXNet;

public class TEServer
{
    static void Main(string[] args)
    {
        RXSocket server = new RXSocket();
        server.StartAsServer("127.0.0.1", 12703, 10);

        while (true)
        {
            string str = Console.ReadLine();
            if(str.Equals("quit")){
                server.CloseServer();
                break;
            }
            HelloMsg msg = new HelloMsg() { info = str };
            var sessions = server.ReturnSession();
            for (int i = 0; i < sessions.Count; i++)
            {
                var jsonData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
                var data = RXTool.PackLenInfo(jsonData);
                sessions[i].SendMsg(data);
            }
        }
    }
}