using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;


namespace RXNet;

public class RXTool
{
    // pack information for head length
    public static byte[] PackLenInfo(byte[] data){
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
                Console.WriteLine("Deserialize failed");
                return null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
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
            Console.WriteLine(e.ToString());
            return null;
        }

    }

}