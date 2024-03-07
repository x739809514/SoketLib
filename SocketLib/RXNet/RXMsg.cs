namespace RXNet;

[Serializable]
public class RXMsg
{

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

