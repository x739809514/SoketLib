using System.Text;
using Newtonsoft.Json;


namespace RXNet;

class RXTool
{

    public static RXMsg DeSerializeData(byte[] dat)
    {
        string data = Encoding.UTF8.GetString(dat);
        RXMsg msg = JsonConvert.DeserializeObject<RXMsg>(data);
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

    public static byte[] SerializeData(RXMsg msg)
    {
        string json = JsonConvert.SerializeObject(msg);
        return Encoding.UTF8.GetBytes(json);
    }

}