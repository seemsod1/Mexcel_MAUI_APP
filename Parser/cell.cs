

namespace Parser
{
    public class Cell
    {
        public double Value { get; set; }
        public string Expression { get; set; }
        public IList<string> Depends_on { get; set; }
        public IList<string> ObservedBy { get; }
    
    public Cell()
    { 
        Value = 0;
        Expression = "";
        Depends_on = new List<string>();
        ObservedBy = new List<string>();
    }
    }
}
