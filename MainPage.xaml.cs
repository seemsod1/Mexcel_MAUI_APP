using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Parser;
using Cell = Parser.Cell;
using Grid = Microsoft.Maui.Controls.Grid;
using System.Xml.Linq;

namespace MyExcelMAUIApp
{
    public partial class MainPage : ContentPage
    {


        private IDictionary<string, IView> _widgets;


        const int CountColumn = 6;
        const int CountRow = 6;

        IFileSaver fileSaver;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public MainPage(IFileSaver fileSaver)
        {
            _widgets = new Dictionary<string, IView>();
            InitializeComponent();
            this.fileSaver = fileSaver;
            CreateGrid();
        }
        //створення таблиці
        private void CreateGrid()
        {

            AddColumnsAndColumnLabels();
            AddRowsAndCellEntries();

        }
        private void AddColumnsAndColumnLabels()
        {
            for (var col = 0; col < CountColumn; col++)
            {
                AddColumn();
            }
        }

        private void AddColumn()
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var colCount = grid.ColumnDefinitions.Count - 1;
            var rowCount = grid.RowDefinitions.Count;

            var label = new Label
            {
                Text = GetColumnName(colCount),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, colCount);
            grid.Children.Add(label);
            _widgets[$"{GetColumnName(colCount)}0"] = label;

            for (var row = 1; row < rowCount; ++row)
            {
                var entry = CreateEntry();
                Grid.SetRow(entry, row);
                Grid.SetColumn(entry, colCount);
                grid.Children.Add(entry);
                _widgets[GetCellName(entry)] = entry;

                Calculator.CellTable.Cells[GetCellName(entry)] = new Parser.Cell();
            }
        }
        private void AddRowsAndCellEntries()
        {
            for (var row = 0; row < CountRow; row++)
            {
                AddRow();
            }
        }
        private void AddRow()
        {
            grid.RowDefinitions.Add(new RowDefinition());

            var colCount = grid.ColumnDefinitions.Count;
            var rowCount = grid.RowDefinitions.Count - 1;

            var label = new Label
            {
                Text = rowCount.ToString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, rowCount);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);
            _widgets[$"0{rowCount}"] = label;


            for (var col = 1; col < colCount; ++col)
            {
                var entry = CreateEntry();
                Grid.SetRow(entry, rowCount);
                Grid.SetColumn(entry, col);
                grid.Children.Add(entry);
                _widgets[GetCellName(entry)] = entry;

                Calculator.CellTable.Cells[GetCellName(entry)] = new Parser.Cell();
            }
        }
        private string GetCellName(IView view)
        {
            return GetNameByPosition(grid.GetRow(view), grid.GetColumn(view));
        }

        private string GetNameByPosition(int row, int col)
        {
            return GetColumnName(col) + row.ToString();
        }

        private Entry CreateEntry()
        {
            var entry = new Entry
            {
                Text = "",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill
            };
            entry.Unfocused += Entry_Unfocused;
            entry.Focused += Entry_Focused;

            return entry;
        }

        private string GetColumnName(int colIndex)
        {
            int dividend = colIndex;
            string columnName = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;

            }
            return columnName;
        }
        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            var name = GetCellName(entry);
            entry.Text = Calculator.CellTable.Cells[name].Expression;
        }
        private async void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            var cellAddress = GetCellName(entry);
            var newExpression = entry.Text;
            var oldExpression = Calculator.CellTable.Cells[cellAddress].Expression;
            
            if (!Calculator.CellTable.EditCell(cellAddress, newExpression))
            {
                Calculator.CellTable.Cells[cellAddress].Expression = oldExpression;
                await DisplayAlert("Помилка", "Введено недопустимий вираз", "ОК");
            }
            UpdateUIDependencies(cellAddress);
            UpdateUI(cellAddress);
        }



        private void UpdateUIDependencies(string name)
        {
            foreach (var observerName in Calculator.CellTable.Cells[name].ObservedBy)
            {
                UpdateUI(observerName);
                UpdateUIDependencies(observerName);
            }
        }

        private void UpdateUI(string cellToUPD)
        {
            var cell = Calculator.CellTable.Cells[cellToUPD];
            var entry = (Entry)_widgets[cellToUPD];
            entry.Text = cell.Expression == "" ? "" : cell.Value.ToString();
        }






        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Create a dictionary to store the cell data
                var cellData = new Dictionary<string, string>();

                // Iterate through all cells in the Table
                foreach (var cellAddress in Calculator.CellTable.GetCellAddresses())
                {
                    var cell = Calculator.CellTable.Cells[cellAddress];

                    if (cell != null)
                    {
                        // Serialize the cell data (you might want to adjust this based on your data structure)
                        string cellJson = JsonSerializer.Serialize(new
                        {
                            Value = cell.Value,
                            Expression = cell.Expression,
                            DependenciesOn = cell.Depends_on,
                            DependenciesBy = cell.ObservedBy
                        });

                        cellData[cellAddress] = cellJson;
                    }
                }

                // Serialize the cell data dictionary to JSON
                string json = JsonSerializer.Serialize(cellData);

                // Create a memory stream to store the JSON data
                using var stream = new MemoryStream(Encoding.Default.GetBytes(json));

                // Save the JSON data to a file using the file saver
                var path = await fileSaver.SaveAsync("table.json", stream, cancellationTokenSource.Token);

                // Display a message to the user
                await DisplayAlert("Success", $"The table has been saved to {path}", "OK");
            }
            catch (Exception ex)
            {
                // Handle the exception
                await DisplayAlert("Error", $"An error occurred while saving the table: {ex.Message}", "OK");
            }
        }



        private async void ReadButton_Clicked(object sender, EventArgs e)
        {

        }


        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти?",
            "Так", "Ні");
            if (answer)
            {
                System.Environment.Exit(0);
            }
        }
        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка", "Лабораторна робота 1. Студента Ріпи Вадима",
            "OK");
        }
        private void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            if (grid.RowDefinitions.Count > 1)
            {
                var lastRowIndex = grid.RowDefinitions.Count - 1;
                var actualColumnsCount = grid.ColumnDefinitions.Count - 1;
                var name = $"0{lastRowIndex}";

                grid.RowDefinitions.RemoveAt(lastRowIndex);
                grid.Children.Remove(_widgets[name]); // Remove label
                _widgets.Remove(name);
                for (var col = 1; col < actualColumnsCount + 1; ++col)
                {
                    name = GetNameByPosition(lastRowIndex, col);
                    grid.Children.Remove(_widgets[name]); // Remove entry
                    _widgets.Remove(name);
                    Calculator.CellTable.Cells.Remove(name);
                }
            }
        }
        private void DeleteColumnButton_Clicked(object sender, EventArgs e)
        {
            if (grid.ColumnDefinitions.Count > 1)
            {
                var lastColumnIndex = grid.ColumnDefinitions.Count - 1;
                var actualRowsCount = grid.RowDefinitions.Count - 1;
                var name = $"{GetColumnName(lastColumnIndex)}0";

                grid.ColumnDefinitions.RemoveAt(lastColumnIndex);
                grid.Children.Remove(_widgets[name]); // Remove label
                _widgets.Remove(name);
                for (var row = 1; row < actualRowsCount + 1; ++row)
                {
                    name = GetNameByPosition(row, lastColumnIndex);
                    grid.Children.Remove(_widgets[name]); // Remove entry
                    _widgets.Remove(name);
                    Calculator.CellTable.Cells.Remove(name);
                }
            }
        }
        private void AddRowButton_Clicked(object sender, EventArgs e)
        {
            grid.RowDefinitions.Add(new RowDefinition());

            var colCount = grid.ColumnDefinitions.Count;
            var rowCount = grid.RowDefinitions.Count - 1;

            var label = new Label
            {
                Text = rowCount.ToString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, rowCount);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);
            _widgets[$"0{rowCount}"] = label;


            for (var col = 1; col < colCount; ++col)
            {
                var entry = CreateEntry();
                Grid.SetRow(entry, rowCount);
                Grid.SetColumn(entry, col);
                grid.Children.Add(entry);
                _widgets[GetCellName(entry)] = entry;

                Calculator.CellTable.Cells[GetCellName(entry)] = new Parser.Cell();
            }
        }
        private void AddColumnButton_Clicked(object sender, EventArgs e)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var colCount = grid.ColumnDefinitions.Count - 1;
            var rowCount = grid.RowDefinitions.Count;

            var label = new Label
            {
                Text = GetColumnName(colCount),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, colCount);
            grid.Children.Add(label);
            _widgets[$"{GetColumnName(colCount)}0"] = label;

            for (var row = 1; row < rowCount; ++row)
            {
                var entry = CreateEntry();
                Grid.SetRow(entry, row);
                Grid.SetColumn(entry, colCount);
                grid.Children.Add(entry);
                _widgets[GetCellName(entry)] = entry;

                Calculator.CellTable.Cells[GetCellName(entry)] = new Parser.Cell();
            }
        }


    }
}