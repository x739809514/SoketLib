using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;


namespace RXNet;

public class RXTool
{
    public static Action<String, LogEnum> logActoin;
    public static bool isLog = true;
    // pack information for head length
    public static byte[] PackLenInfo(byte[] data)
    {
        int len = data.Length;
        byte[] pkg = new byte[len + 4];
        byte[] head = BitConverter.GetBytes(len);
        head.CopyTo(pkg, 0);
        data.CopyTo(pkg, 4);

        return pkg;
    }


    public static K DeSerializeData<K>(byte[] dat) where K : RXMsg
    {
        try
        {
            string data = Encoding.UTF8.GetString(dat);
            K msg = JsonConvert.DeserializeObject<K>(data);
            if (msg != null)
            {
                return msg;
            }
            else
            {
                RXTool.Log("Deserialize failed", LogEnum.Error);
                return null;
            }
        }
        catch (Exception e)
        {
            RXTool.Log(e.ToString(), LogEnum.Error);
            return null;
        }
    }

    public static byte[] SerializeData<K>(K msg) where K : RXMsg
    {
        try
        {
            string json = JsonConvert.SerializeObject(msg);
            return Encoding.UTF8.GetBytes(json);
        }
        catch (Exception e)
        {
            RXTool.Log(e.ToString(), LogEnum.Error);
            return null;
        }

    }

    public static void Log(string msg, LogEnum logEnum)
    {
        if (isLog == false) return;
        msg = DateTime.Now.ToLongTimeString() + ">> " + msg;

        logActoin?.Invoke(msg, logEnum);

        switch (logEnum)
        {
            case LogEnum.None:
                Console.WriteLine(msg);
                break;
            case LogEnum.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case LogEnum.Warn:
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            default:
                Console.WriteLine(msg);
                break;
        }
    }
}

public enum LogEnum
{
    None = 0,
    Error = 1,
    Warn = 2,
}