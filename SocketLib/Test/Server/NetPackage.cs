public class NetPackage
{
    public int headLen = 4;
    public byte[] headBuff;
    public int headIndex = 0;

    public int bodyLen = 0;
    public byte[] bodyBuff;
    public int bodyIndex = 0;

    public NetPackage()
    {
        headBuff = new byte[4];
    }

    public void InitBodyBuff()
    {
        // here server receives the head of msg, then parse the head msg
        // in this 4 byte msg, there is the length of the body message
        bodyLen = BitConverter.ToInt32(headBuff, 0);
        bodyBuff = new byte[bodyLen];
    }

}