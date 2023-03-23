namespace Anemos.Contracts.Models;

public interface ISensorModel
{
    public string Id
    {
        get;
    }

    public string Name
    {
        get;
    }

    public string LongName
    {
        get;
    }

    public decimal? Value
    {
        get;
    }

    void Update();
}
