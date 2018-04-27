using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ySlide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        Brush _previousFill;
        private enum MouseFunctions
        {
            None, Move, Pencil, Line, Ellipse, Rectangle, Text, Image
        }

        private MouseFunctions curMouseFunction = MouseFunctions.None;

        PersonCollectionViewModel viewModel = new PersonCollectionViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = rotate;
        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
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

        private void PencilButton_Click(object sender, RoutedEventArgs e)
        {
            PencilButton.IsChecked = true;
            curMouseFunction = MouseFunctions.Pencil;
            canvas.Cursor = Cursors.Pen;
        }

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            LineButton.IsChecked = true;
            curMouseFunction = MouseFunctions.Line;
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            EllipseButton.IsChecked = true;
            curMouseFunction = MouseFunctions.Ellipse;
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            RectangleButton.IsChecked = true;
            curMouseFunction = MouseFunctions.Rectangle;
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            TextButton.IsChecked = true;
            curMouseFunction = MouseFunctions.Text;
            canvas.Cursor = Cursors.IBeam;
        }

        private void NoneButton_Click(object sender, RoutedEventArgs e)
        {
            NoneButton.IsChecked = true;
            curMouseFunction = MouseFunctions.None;
            canvas.Cursor = Cursors.Arrow;
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            MoveButton.IsChecked = true;
            curMouseFunction = MouseFunctions.Move;
            canvas.Cursor = Cursors.Hand;
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            curMouseFunction = MouseFunctions.Image;
        }

        Point start, end, endPre;
        Brush customStroke = Brushes.Black, customFill = Brushes.Transparent;
        double customStrokeThickNess = 1;

        UIElement objectToMove = null, objectIsChosen = null;


        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Canvas");
            start = e.GetPosition(canvas);
            end = start;
            switch (curMouseFunction)
            {
                case MouseFunctions.Pencil:
                    Polyline newPolyline = new Polyline()
                    {
                        Stroke = customStroke,
                        StrokeThickness = customStrokeThickNess,
                    };
                    newPolyline.Points.Add(start);
                    newPolyline.MouseDown += canvasChildren_MouseDown;
                    canvas.Children.Add(newPolyline);
                    break;
                case MouseFunctions.Line:
                    Line newLine = new Line()
                    {
                        Stroke = customStroke,
                        X1 = start.X,
                        Y1 = start.Y,
                        X2 = end.X,
                        Y2 = end.Y,
                    };
                    newLine.MouseDown += canvasChildren_MouseDown;
                    canvas.Children.Add(newLine);
                    break;
                case MouseFunctions.Ellipse:
                    Ellipse newEllipse = new Ellipse()
                    {
                        Stroke = customStroke,
                        Fill = customFill,
                        StrokeThickness = customStrokeThickNess,
                        Height = 10,
                        Width = 10,
                    };
                    newEllipse.MouseDown += canvasChildren_MouseDown;
                    canvas.Children.Add(newEllipse);
                    break;
                case MouseFunctions.Rectangle:
                    Rectangle newRectangle = new Rectangle()
                    {
                        Stroke = customStroke,
                        Fill = customFill,
                        StrokeThickness = customStrokeThickNess,
                        Height = 10,
                        Width = 10,
                    };
                    newRectangle.MouseDown += canvasChildren_MouseDown;
                    canvas.Children.Add(newRectangle);
                    break;
                case MouseFunctions.Text:
                    EditableTextBlock newEditTextBox = new EditableTextBlock()
                    {
                        Text = "Double click to edit",
                        Cursor = Cursors.IBeam,
                        //IsInEditMode = true,
                    };
                    newEditTextBox.SetValue(Canvas.TopProperty, start.Y);
                    newEditTextBox.SetValue(Canvas.LeftProperty, start.X);
                    newEditTextBox.MouseDown += canvasChildren_MouseDown;
                    canvas.Children.Add(newEditTextBox);
                    NoneButton_Click(sender, e);
                    break;
                case MouseFunctions.Image:
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

                    Nullable<bool> result = dlg.ShowDialog();

                    if (result == true)
                    { 
                        Image newImage = new Image() {

                        };
                        newImage.Source = new BitmapImage(new Uri(dlg.FileName));
                        newImage.SetValue(Canvas.LeftProperty, start.X);
                        newImage.SetValue(Canvas.TopProperty, start.Y);
                        newImage.MouseDown += canvasChildren_MouseDown;
                        canvas.Children.Add(newImage);
                    }
                    break;
                default:
                    return;
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            objectToMove = null;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                endPre = end;
                end = e.GetPosition(canvas);
                switch (curMouseFunction)
                {
                    case MouseFunctions.Pencil:
                        (canvas.Children[canvas.Children.Count - 1] as Polyline).Points.Add(end);
                        break;
                    case MouseFunctions.Line:
                        DrawLine(canvas.Children[canvas.Children.Count - 1] as Line);
                        break;
                    case MouseFunctions.Ellipse:
                        DrawEllipse(canvas.Children[canvas.Children.Count - 1] as Ellipse);
                        break;
                    case MouseFunctions.Rectangle:
                        DrawRectangle(canvas.Children[canvas.Children.Count - 1] as Rectangle);
                        break;
                    case MouseFunctions.Move:
                        if(objectToMove != null)
                        {
                            var top = (double)objectToMove.GetValue(Canvas.TopProperty);
                            var left = (double)objectToMove.GetValue(Canvas.LeftProperty);

                            objectToMove.SetValue(Canvas.TopProperty, top + (end.Y - endPre.Y));
                            objectToMove.SetValue(Canvas.LeftProperty, left + (end.X - endPre.X));
                        }
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

        private void canvasChildren_MouseDown(object sender, MouseButtonEventArgs e)
        {
            objectToMove = sender as UIElement;
            DataContext = sender;
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
