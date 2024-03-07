namespace RXNet;

[Serializable]
public class RXMsg
{
    public int cmd;
}

[Serializable]
public class HelloMsg : RXMsg
{
    public string info;

    public override string ToString()
    {
        return "Info is: " + info;
    }
}

