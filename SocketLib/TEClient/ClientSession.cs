using RXNet;
using RXProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEClient
{
    public class ClientSession : RXSession<NetMsg>
    {
        protected override void OnHandleMessage(RXMsg msg)
        {
            NetMsg netMsg = (NetMsg)msg;
            if (netMsg == null) return;

            /*Protocols*/
            RpsInfo info = netMsg.RpsInfo;
            if (info != null)
            {
                RXTool.Log("isSuccess is: " + info.isSuccess,LogEnum.None);
            }

        }
    }
}
