using System.Net;
using System.Net.Sockets;

namespace RXNet
{
    public class RXSession<K> where K : RXMsg
    {
        private Socket skt;
        public Action closeCB = null;

        public void StartRcvData(Socket socket, Action callback)
        {
            skt = socket;
            closeCB = callback;
            OnConnectSuccess();
            RXPkg pkg = new RXPkg();
            socket.BeginReceive(pkg.headBuff, 0, pkg.headLen, SocketFlags.None, ASyncHeadRcv, pkg);
        }

        public void ASyncHeadRcv(IAsyncResult result)
        {
            try
            {
                RXPkg pkg = (RXPkg)result.AsyncState;
                // receive data buffer
                byte[] dataRcv = pkg.headBuff;
                int lenRcv = skt.EndReceive(result);
                if (lenRcv == 0)
                {
                    Console.WriteLine("Server is offline");
                    CloseSession();
                    return;
                }
                else
                {
                    pkg.headIndex += lenRcv;
                    if (pkg.headIndex < 4)
                    {
                        skt.BeginReceive(
                            pkg.headBuff,
                            pkg.headIndex,
                            pkg.headLen - pkg.headIndex,
                            SocketFlags.None,
                            ASyncHeadRcv,
                            result
                        );
                    }
                    else
                    {
                        pkg.InitBodyBuff();
                        // recevive data
                        skt.BeginReceive(
                            pkg.bodyBuff,
                            0,
                            pkg.bodyLen,
                            SocketFlags.None,
                            ASyncBodyRcv,
                            pkg
                        );
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                CloseSession();
            }
        }

        private void ASyncBodyRcv(IAsyncResult result)
        {
            try
            {
                RXPkg pkg = (RXPkg)result.AsyncState;
                int lenRcv = skt.EndReceive(result);
                if (lenRcv == 0)
                {
                    if (skt != null)
                    {
                        skt.Shutdown(SocketShutdown.Both);
                        skt.Close();
                    }
                    return;
                }
                pkg.bodyIndex += lenRcv;
                if (pkg.bodyIndex < pkg.bodyLen)
                {
                    skt.BeginReceive(
                        pkg.bodyBuff,
                        pkg.bodyIndex,
                        pkg.bodyLen - pkg.bodyIndex,
                        SocketFlags.None,
                        ASyncBodyRcv,
                        pkg
                    );
                }
                else
                {
                    RXMsg sendMsg = RXTool.DeSerializeData<K>(pkg.bodyBuff);
                    OnHandleMessage(sendMsg);
                    pkg.ResetData();
                    skt.BeginReceive(pkg.bodyBuff, 0, 4, SocketFlags.None, ASyncHeadRcv, pkg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                CloseSession();
            }
        }

        public void SendMsg<K>(K msg) where K : RXMsg
        {
            byte[] data = RXTool.SerializeData<K>(msg);
            byte[] pkg = RXTool.PackLenInfo(data);

            NetworkStream ns = null;
            try
            {
                ns = new NetworkStream(skt);
                if (ns.CanWrite)
                {
                    ns.BeginWrite(
                        pkg,
                        0,
                        pkg.Length,
                        SendCB,
                        ns
                    );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public void SendMsg(byte[] msg)
        {
            NetworkStream ns = null;
            try
            {
                ns = new NetworkStream(skt);
                if (ns.CanWrite)
                {
                    ns.BeginWrite(
                        msg,
                        0,
                        msg.Length,
                        SendCB,
                        ns
                    );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendCB(IAsyncResult result)
        {
            try
            {
                if (result.AsyncState is NetworkStream ns)
                {
                    ns.EndWrite(result);
                    ns.Flush();
                    ns.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void CloseSession()
        {
            closeCB?.Invoke();

            if (skt != null)
            {
                skt.Shutdown(SocketShutdown.Both);
                skt.Close();
            }
            OnConnectClose();
        }

        #region

        protected virtual void OnConnectSuccess()
        {

        }

        protected virtual void OnConnectClose()
        {

        }

        protected virtual void OnHandleMessage(RXMsg msg)
        {
            
        }
        #endregion
    }
}

