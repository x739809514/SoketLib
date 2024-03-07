using RXNet;
using RXProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEServer
{
    public class ServerSession : RXSession<NetMsg>
    {
        protected override void OnHandleMessage(RXMsg msg)
        {
            NetMsg netMsg = (NetMsg)msg;
            if (netMsg == null) return;

            /*Protocols*/
            ReqInfo info = netMsg.ReqInfo;
            if (info != null)
            {
                RXTool.Log("server ID is: " + info.serverID + "\nname is: " + info.name + "\nport is: " + info.port, LogEnum.None);
            }

        }
    }
}
