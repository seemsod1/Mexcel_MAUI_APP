

namespace Backend;

public class Cell
{
    public double Value { get; set; }
    public string Expression { get; set; }
    public List<string> Depends_on { get; set; }
    public List<string> ObservedBy { get; set;  }

public Cell()
{ 
    Value = 0;
    Expression = "";
    Depends_on = new List<string>();
    ObservedBy = new List<string>();
}
}
