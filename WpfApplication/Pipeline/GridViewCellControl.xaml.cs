using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static IronPython.SQLite.PythonSQLite;

namespace OpenSense.WPF.Pipeline {
    public partial class GridViewCellControl : UserControl {
        public GridViewCellControl() {
            InitializeComponent();
        }

        private void ButtonMove_Click(object sender, RoutedEventArgs e) {
            if (sender == ButtonMoveUp) {
                var oldRow = Grid.GetRow(this);
                var newRow = Math.Max(0, oldRow - 1);
                if (oldRow == newRow) {
                    return;
                }
                Grid.SetRow(this, newRow);
            } else if (sender == ButtonMoveDown) {
                var oldRow = Grid.GetRow(this);
                var newRow = oldRow + 1;
                Grid.SetRow(this, newRow);
            } else if (sender == ButtonMoveLeft) {
                var oldCol = Grid.GetColumn(this);
                var newCol = Math.Max(0, oldCol - 1);
                if (oldCol == newCol) {
                    return;
                }
                Grid.SetColumn(this, newCol);
            } else if (sender == ButtonMoveRight) {
                var oldCol = Grid.GetColumn(this);
                var newCol = oldCol + 1;
                Grid.SetColumn(this, newCol);
            } else {
                throw new InvalidOperationException();
            }

            AdjustGridDefinitions();
        }

        private void AdjustGridDefinitions() {
            var grid = FindParent<Grid>(this);
            var maxRow = -1;
            var maxCol = -1;
            foreach (var child in grid.Children) {
                if (child is GridViewCellControl cell) {
                    maxRow = Math.Max(maxRow, Grid.GetRow(cell));
                    maxCol = Math.Max(maxCol, Grid.GetColumn(cell));
                }
            }
            var rowCount = maxRow + 1;
            var colCount = maxCol + 1;
            while (grid.RowDefinitions.Count > rowCount) {
                grid.RowDefinitions.RemoveAt(grid.RowDefinitions.Count - 1);
            }
            while (grid.ColumnDefinitions.Count > colCount) {
                grid.ColumnDefinitions.RemoveAt(grid.ColumnDefinitions.Count - 1);
            }
            var rowStyle = (Style)grid.Resources["styleSplitterRow"];
            var colStyle = (Style)grid.Resources["styleSplitterCol"];
            Debug.Assert(rowStyle is not null);
            Debug.Assert(colStyle is not null);
            var splitters = grid.Children.OfType<GridSplitter>();
            var rowSplitters = splitters.Where(s => s.Style == rowStyle);
            var colSplitters = splitters.Where(s => s.Style == colStyle);
            var unwantedSplitters = rowSplitters.Where(s => (int)s.Tag >= maxRow).Concat(colSplitters.Where(s => (int)s.Tag >= maxCol)).ToArray();
            foreach (var splitter in unwantedSplitters) {
                grid.Children.Remove(splitter);//Only remove unwanted splitters. Grid size infomation is preserved by splitters.
            }

            for (var i = grid.RowDefinitions.Count; i < rowCount; i++) {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (var i = grid.ColumnDefinitions.Count; i < colCount; i++) {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            foreach (var splitter in rowSplitters) {
                Grid.SetColumnSpan(splitter, colCount);
            }
            foreach (var splitter in colSplitters) {
                Grid.SetRowSpan(splitter, rowCount);
            }
            for (var i = rowSplitters.Count(); i < rowCount; i++) {
                var splitter = new GridSplitter {
                    Style = rowStyle,
                    Tag = i,
                };
                Grid.SetRow(splitter, i);
                Grid.SetColumnSpan(splitter, colCount);
                grid.Children.Add(splitter);
            }
            for (var i = colSplitters.Count(); i < colCount; i++) {
                var splitter = new GridSplitter {
                    Style = colStyle,
                    Tag = i,
                };
                Grid.SetColumn(splitter, i);
                Grid.SetRowSpan(splitter, rowCount);
                grid.Children.Add(splitter);
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject is null) {
                return null;
            }
            var parent = parentObject as T;
            if (parent != null) {
                return parent;
            } else {
                var result = FindParent<T>(parentObject);
                return result;
            }
        }
    }
}
