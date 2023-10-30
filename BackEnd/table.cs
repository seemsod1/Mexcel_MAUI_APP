using System.Diagnostics;
using System.Linq.Expressions;

namespace Backend;

public class Table
{
    public Dictionary<string, Cell> Cells { get; set; }
    public List<string> AffectedCells { get; set; }

    public Table()
    {
        Cells = new Dictionary<string, Cell>();
        AffectedCells = new List<string>();
    }
    public static bool IsCyclicDependency(string name, string dependencyName)
    {
        if (name == dependencyName)
        {
            return true;
        }

        return Calculator.CellTable.Cells[dependencyName].Depends_on.Any(childName => IsCyclicDependency(name, childName));
    }

    public bool SetCell(string cellAddress, string newExpression)
    {
        var cell = Calculator.CellTable.Cells[cellAddress];
        var oldDependence = cell.Depends_on;
        Calculator.EvaluatingCellName = cellAddress;
        cell.Expression = newExpression;
        try
        {
            cell.Depends_on = new List<string>();

            RefreshCellRecursively(cellAddress);

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
    public void AffectAll()
    {
        foreach (var cell in Cells)
        {   
            AffectedCells.Add(cell.Key);
        }
    }
    public void Clear()
    {
        foreach (var cell in Cells) 
        {
            cell.Value.Value = 0;
            cell.Value.Expression = "";
            cell.Value.Depends_on = new List<string>();
            cell.Value.ObservedBy = new List<string>();

            AffectedCells.Add(cell.Key);

        }
    }
    


    private void RefreshCell(string cellName)
    {

        var cell = Cells[cellName];
        cell.Depends_on.Clear();
        Calculator.EvaluatingCellName = cellName;
        cell.Value = Calculator.Evaluate(cell.Expression);
        if (!AffectedCells.Contains(cellName))
        {
            AffectedCells.Add(cellName);
        }
    }

    public void RefreshCellRecursively(string cellName)
    {
        var cell = Cells[cellName];
       
        RefreshCell(cellName);

        for (int i = 0; i < cell.ObservedBy.Count; i++)
        {
            var observer = Cells[cell.ObservedBy[i]];
            if (observer.Depends_on.Contains(cellName))
            {
                RefreshCellRecursively(cell.ObservedBy[i]);
            }
            else
            {
                cell.ObservedBy.RemoveAt(i--);
            }
        }
    }
}

