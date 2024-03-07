using System.Text;
using Newtonsoft.Json;
using RXNet;

public class RXClient
{
    private

    static void Main(string[] args)
    {
        RXSocket client = new RXSocket();
        client.StartASClient("127.0.0.1", 12703);
        while (true)
        {
            string str = Console.ReadLine();
            if(str.Equals("quit")){
                client.CloseClient();
                break;
            }
            HelloMsg msg = new HelloMsg() { info = str };
            var jsonData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
            var data = RXTool.PackLenInfo(jsonData);
            client.session.SendMsg(data);
        }
    }
}