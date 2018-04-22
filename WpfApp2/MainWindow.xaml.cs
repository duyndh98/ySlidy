using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        Brush _previousFill;
        private enum MyShape
        {
            Pencil, Line, Ellipse, Rectangle, Text
        }
        
        private MyShape currShape = MyShape.Pencil;

        public MainWindow()
        {
            InitializeComponent();
        }

        private TabItem GetTargetTabItem(object originalSource)
        {
            var current = originalSource as DependencyObject;

            while (current != null)
            {
                var tabItem = current as TabItem;
                if (tabItem != null)
                {
                    return tabItem;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            var tabItemTarget = e.Source as TabItem;

            var tabItemSource = e.Data.GetData(typeof(TabItem)) as TabItem;

            if (!tabItemTarget.Equals(tabItemSource))
            {
                var tabControl = tabItemTarget.Parent as TabControl;
                int sourceIndex = tabControl.Items.IndexOf(tabItemSource);
                int targetIndex = tabControl.Items.IndexOf(tabItemTarget);

                tabControl.Items.Remove(tabItemSource);
                tabControl.Items.Insert(targetIndex, tabItemSource);

                tabControl.Items.Remove(tabItemTarget);
                tabControl.Items.Insert(sourceIndex, tabItemTarget);
            }
        }

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var tabItem = e.Source as TabItem;

            if (tabItem == null)
                return;

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
            }
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            if (ellipse != null && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(ellipse, ellipse.Fill.ToString(), DragDropEffects.Copy);
            }
        }

        private void Ellipse_DragEnter(object sender, DragEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            if (ellipse != null)
            {
                // Save the current Fill brush so that you can revert back to this value in DragLeave.
                _previousFill = ellipse.Fill;

                // If the DataObject contains string data, extract it.
                if (e.Data.GetDataPresent(DataFormats.StringFormat))
                {
                    string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

                    // If the string can be converted into a Brush, convert it.
                    BrushConverter converter = new BrushConverter();
                    if (converter.IsValid(dataString))
                    {
                        Brush newFill = (Brush)converter.ConvertFromString(dataString);
                        ellipse.Fill = newFill;
                    }
                }
            }
        }

        private void Ellipse_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

                // If the string can be converted into a Brush, allow copying.
                BrushConverter converter = new BrushConverter();
                if (converter.IsValid(dataString))
                {
                    e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                }
            }
        }

        private void Ellipse_DragLeave(object sender, DragEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            if (ellipse != null)
            {
                ellipse.Fill = _previousFill;
            }
        }

        private void Ellipse_Drop(object sender, DragEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            if (ellipse != null)
            {
                // If the DataObject contains string data, extract it.
                if (e.Data.GetDataPresent(DataFormats.StringFormat))
                {
                    string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

                    // If the string can be converted into a Brush, 
                    // convert it and apply it to the ellipse.
                    BrushConverter converter = new BrushConverter();
                    if (converter.IsValid(dataString))
                    {
                        Brush newFill = (Brush)converter.ConvertFromString(dataString);
                        ellipse.Fill = newFill;
                    }
                }
            }
        }

        private void PencilButton_Click(object sender, RoutedEventArgs e)
        {
            currShape = MyShape.Pencil;
        }

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            currShape = MyShape.Line;
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            currShape = MyShape.Ellipse;
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            currShape = MyShape.Rectangle;
        }

        Point start, end;
        Brush customStroke = Brushes.Black, customFill = Brushes.White;
        double customStrokeThickNess = 1;


        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(canvas);
            end = start;
            switch (currShape)
            {
                case MyShape.Pencil:
                    Polyline newPolyline = new Polyline()
                    {
                        Stroke = customStroke,
                        StrokeThickness = customStrokeThickNess,
                    };
                    newPolyline.Points.Add(start);
                    canvas.Children.Add(newPolyline);
                    break;
                case MyShape.Line:
                    Line newLine = new Line()
                    {
                        Stroke = customStroke,
                        X1 = start.X,
                        Y1 = start.Y,
                        X2 = end.X,
                        Y2 = end.Y,
                    };
                    canvas.Children.Add(newLine);
                    break;
                case MyShape.Ellipse:
                    Ellipse newEllipse = new Ellipse()
                    {
                        Stroke = customStroke,
                        Fill = customFill,
                        StrokeThickness = customStrokeThickNess,
                        Height = 10,
                        Width = 10,
                    };
                    canvas.Children.Add(newEllipse);
                    break;
                case MyShape.Rectangle:
                    Rectangle newRectangle = new Rectangle()
                    {
                        Stroke = customStroke,
                        Fill = customFill,
                        StrokeThickness = customStrokeThickNess,
                        Height = 10,
                        Width = 10,
                    };
                    canvas.Children.Add(newRectangle);
                    break;
                case MyShape.Text:
                    TextBlock newTextBlock = new TextBlock()
                    {

                    };
                    break;
                default:
                    return;
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                end = e.GetPosition(canvas);
                switch (currShape)
                {
                    case MyShape.Pencil:
                        (canvas.Children[canvas.Children.Count - 1] as Polyline).Points.Add(end);
                        break;
                    case MyShape.Line:
                        DrawLine(canvas.Children[canvas.Children.Count - 1] as Line);
                        break;
                    case MyShape.Ellipse:
                        DrawEllipse(canvas.Children[canvas.Children.Count - 1] as Ellipse);
                        break;
                    case MyShape.Rectangle:
                        DrawRectangle(canvas.Children[canvas.Children.Count - 1] as Rectangle);
                        break;
                    default:
                        return;
                }
            }
        }

        private void DrawRectangle(Rectangle newRectangle)
        {
            if (end.X >= start.X)
            {
                newRectangle.SetValue(Canvas.LeftProperty, start.X);
                newRectangle.Width = end.X - start.X;
            }
            else
            {
                newRectangle.SetValue(Canvas.LeftProperty, end.X);
                newRectangle.Width = start.X - end.X;
            }

            if (end.Y >= start.Y)
            {
                newRectangle.SetValue(Canvas.TopProperty, start.Y);
                newRectangle.Height = end.Y - start.Y;
            }
            else
            {
                newRectangle.SetValue(Canvas.TopProperty, end.Y);
                newRectangle.Height = start.Y - end.Y;
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tempTextBlock = sender as TextBlock;
            tempTextBlock.Text = "123123123";
        }

        private void DrawEllipse(Ellipse newEllipse)
        {
            if (end.X >= start.X)
            {
                newEllipse.SetValue(Canvas.LeftProperty, start.X);
                newEllipse.Width = end.X - start.X;
            }
            else
            {
                newEllipse.SetValue(Canvas.LeftProperty, end.X);
                newEllipse.Width = start.X - end.X;
            }

            if (end.Y >= start.Y)
            {
                newEllipse.SetValue(Canvas.TopProperty, start.Y);
                newEllipse.Height = end.Y - start.Y;
            }
            else
            {
                newEllipse.SetValue(Canvas.TopProperty, end.Y);
                newEllipse.Height = start.Y - end.Y;
            }

        }

        private void DrawLine(Line newLine)
        {
            newLine.X1 = start.X;
            newLine.Y1 = start.Y;
            newLine.X2 = end.X;
            newLine.Y2 = end.Y;
        }
    }
}
