using System.Net;
using System.Net.Sockets;

namespace RXNet
{
    public class RXSession
    {
        private Socket skt;

        public void StartRcvData(Socket socket)
        {
            skt = socket;
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
                    if (skt != null)
                    {
                        skt.Shutdown(SocketShutdown.Both);
                        skt.Close();
                    }
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
            }
        }

        public void ASyncBodyRcv(IAsyncResult result)
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
                    RXMsg sendMsg = RXTool.DeSerializeData(pkg.bodyBuff);
                    Console.WriteLine(sendMsg.ToString());
                }

                skt.BeginReceive(pkg.bodyBuff, 0, 4, SocketFlags.None, ASyncHeadRcv, pkg);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


    }
}

