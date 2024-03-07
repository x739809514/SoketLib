using RXNet;

namespace RXProtocol
{
    public class NetMsg : RXMsg
    {
        public ReqInfo ReqInfo;
        public RpsInfo RpsInfo;
    }

    public class ReqInfo
    {
        public int serverID;
        public string name;
        public int port;
    }

    public class RpsInfo
    {
        public bool isSuccess;
    }

    public enum CMD
    {
        Null = 0,
        ReqInfo = 1,
        RpsInfo = 2,
    }
}