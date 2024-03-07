using System.Text;
using Newtonsoft.Json;
using RXNet;
using RXProtocol;

namespace TEServer
{
    public class TEServer
    {
        static void Main(string[] args)
        {
            RXSocket<ServerSession,NetMsg> server = new RXSocket<ServerSession,NetMsg>();
            server.StartAsServer("127.0.0.1", 12703, 10);

            while (true)
            {
                string str = Console.ReadLine();
                if (str.Equals("quit"))
                {
                    server.CloseServer();
                    break;
                }

                var sessions = server.ReturnSession();
                for (int i = 0; i < sessions.Count; i++)
                {
                    var jsonData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new NetMsg()
                    {
                        cmd = (int)CMD.RpsInfo,
                        RpsInfo = new RpsInfo()
                        {
                            isSuccess = true,
                        }
                    }));
                    var data = RXTool.PackLenInfo(jsonData);
                    sessions[i].SendMsg(data);
                }
            }
        }
    }
}