using System.Diagnostics;

namespace Parser;

public class Table
{
    public Dictionary<string, Cell> Cells { get; }

    public Table()
    {
        Cells = new Dictionary<string, Cell>();
    }
    public static bool IsCyclicDependency(string name, string dependencyName)
    {
        if (name == dependencyName)
        {
            return true;
        }

        return Calculator.CellTable.Cells[dependencyName].Depends_on.Any(childName => IsCyclicDependency(name, childName));
    }
   

    public bool EditCell(string cellAddress, string newExpression)
    {
        var cell = Calculator.CellTable.Cells[cellAddress];
        var oldDependence = cell.Depends_on;
        Calculator.EvaluatingCellName = cellAddress;
        cell.Expression = newExpression;
        try
        {
            cell.Depends_on = new List<string>();

            RefreshRecursively(cellAddress);

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            cell.Depends_on = oldDependence;
            return false;
        }
        return true;
    }

    

   
    public List<string> GetCellAddresses()
    {
        return Cells.Keys.ToList();
    }
    public void Clear()
    {
        Cells.Clear();
    }
    public void SetCell(string cellAddress, Cell cell)
    {
        Cells[cellAddress] = cell;
    }


    private void Refresh(string cellName)
    {

        var cell = Cells[cellName];
        cell.Depends_on.Clear();
        Calculator.EvaluatingCellName = cellName;
        cell.Value = Calculator.Evaluate(cell.Expression);
        //if (!AffectedCells.Contains(cellName))
       // {
       //     AffectedCells.Add(cellName);
       // }
    }

    public void RefreshRecursively(string cellName)
    {
        var cell = Cells[cellName];
       
        Refresh(cellName);

        for (int i = 0; i < cell.ObservedBy.Count; i++)
        {
            var observer = Cells[cell.ObservedBy[i]];
            if (observer.Depends_on.Contains(cellName))
            {
                RefreshRecursively(cell.ObservedBy[i]);
            }
            else
            {
                cell.ObservedBy.RemoveAt(i--);
            }
        }
    }
}

