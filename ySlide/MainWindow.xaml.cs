using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace ySlide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        #region Arguments

        private Point mDown, mMove, mMovePre;
        private bool capture;   //Trạng thái vẽ: dragging hoặc vẽ thật
        private ContentControl curControl;
        private Shape curShape;
        private Line curLine;
        private Color fillColor, penColor;
        private double penThickness;
        private PathGeometry pathGeometry;
        private Path path;
        private PathFigure pathFigure;
        private string currentFilePath;
        private ContentControl selectedControl;
        private EditableTextBlock focusedTextbox;
        /// <Text Adjustment>
        private FontFamily fontFamily;
        private double fontSize;
        private FontStyle fontStyle;
        private FontWeight fontWeight;
        private TextDecorationCollection decoration;
        private Color fontColor;
        bool bold = false, italic = false, underlined = false;
        float oldLineCount;
        /// </Text Adjustment>

        #endregion

        #region Methods

        private int GetOutline()
        {
            if (btnSmooth.IsChecked == true)
            {
                return 1;
            }
            else if (btnDash.IsChecked == true)
            {
                return 2;
            }
            return 3;
        }

        private void UpdateCanvasSize()
        {
            if (txtWidth.Text == "")
            {
                txtWidth.Text = Document.Instance.canvas.Width.ToString();
            }
            if (txtHeight.Text == "")
            {
                txtHeight.Text = Document.Instance.canvas.Height.ToString();
            }
            try
            {
                if (txtWidth.Text != "" && txtHeight.Text != "")
                {
                    Document.Instance.canvas.ClipToBounds = true;
                    Document.Instance.canvas.SnapsToDevicePixels = true;
                    Document.Instance.canvas.Width = Convert.ToDouble(txtWidth.Text);
                    Document.Instance.canvas.Height = Convert.ToDouble(txtHeight.Text);
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Invalid size of paint surface: " + "\n" + ex.Message, "Invalid input", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private Object CLoneObject(Object o)
        {
            var xaml = System.Windows.Markup.XamlWriter.Save(o);
            var deepCopy = System.Windows.Markup.XamlReader.Parse(xaml) as Object;
            return deepCopy;
        }

        private void ChangeCurCanvas(Canvas c)
        {
            curCanvas = c;
            curCanvas.MouseDown += canvas_MouseDown;
            curCanvas.MouseUp += canvas_MouseUp;
            curCanvas.MouseMove += canvas_MouseMove;
            curCanvas.MouseLeftButtonUp += canvas_MouseLeftButtonUp;
            curCanvas.PreviewMouseLeftButtonDown += canvas_PreviewMouseLeftButtonDown;
            curCanvas.PreviewMouseLeftButtonUp += canvas_PreviewMouseLeftButtonUp;
        }

        //public System.Drawing.Bitmap CanvasToBitmap(Canvas cv)
        //{
        //    System.Drawing.Bitmap bm;
        //    Rect bounds = VisualTreeHelper.GetDescendantBounds(cv);
        //    double dpi = 96d;
        //    RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, System.Windows.Media.PixelFormats.Default);


        //    DrawingVisual dv = new DrawingVisual();
        //    using (DrawingContext dc = dv.RenderOpen())
        //    {
        //        VisualBrush vb = new VisualBrush(cv);
        //        dc.DrawRectangle(vb, null, new Rect(new System.Windows.Point(), bounds.Size));
        //    }
        //    renderBitmap.Render(dv);

        //    System.IO.MemoryStream stream = new System.IO.MemoryStream();
        //    BitmapEncoder encoder = new BmpBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
        //    encoder.Save(stream);
        //    bm = new System.Drawing.Bitmap(stream);
        //    return bm;
        //}

        //private ImageSource BitmapToImageSource(System.Drawing.Bitmap bm)
        //{
        //    System.Windows.Media.Imaging.BitmapSource b = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bm.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bm.Width, bm.Height));
        //    return b;
        //}

        #endregion

        #region Events

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Document.Instance = new Document(curCanvas);
            listSlides.ItemsSource = Document.Instance.thumbs;
            capture = false;
            penColor = Colors.Black;
            fillColor = Colors.Black;
            penThickness = 1;
            btnSmooth.IsChecked = true;
            fontFamily = new FontFamily();
            fontSize = 12;
            fontColor = Colors.Black;
            fontStyle = FontStyles.Normal;
            fontWeight = FontWeights.Normal;
            decoration = null;
        }

        private void btnPencil_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.pencil;
            curCanvas.Cursor = Cursors.Pen;
        }

        private void btnLine_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.line;
            curCanvas.Cursor = Cursors.Cross;
        }

        private void btnEllipse_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.ellipse;
            curCanvas.Cursor = Cursors.Cross;
        }

        private void btnRectangle_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.rectangle;
            curCanvas.Cursor = Cursors.Cross;
        }

        private void btnText_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.text;
            curCanvas.Cursor = Cursors.IBeam;
        }

        private void rib_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if(rib.SelectedItem as RibbonTab == tabText)
            //{
            //    Document.drawType = DrawType.text;
            //    curCanvas.Cursor = Cursors.IBeam;
            //}
            //else
            //{
            //    Document.drawType = DrawType.nothing;
            //    if(curCanvas != null)
            //        curCanvas.Cursor = Cursors.Arrow;
            //}
        }

        private void btnPointer_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.nothing;
            curCanvas.Cursor = Cursors.Arrow;
        }

        private void btnImage_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.image;
            curCanvas.Cursor = Cursors.Arrow;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                Image newImage = new Image();
                newImage.Source = new BitmapImage(new Uri(dlg.FileName));

                newImage.UpdateLayout();
                ContentControl control = new ContentControl();

                Canvas.SetLeft(control, 10);
                Canvas.SetTop(control, 10);
                control.Content = newImage;
                control.Width = 0;
                control.Height = 0;
                //control.Padding = new Thickness(1);
                control.Background = new SolidColorBrush(Colors.White);
                control.Style = FindResource("DesignerItemStyle") as Style;
                Document.Instance.InsertText(control);
            }
        }

        private void btnErazer_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.erase;
            curCanvas.Cursor = Cursors.Arrow;
        }

        private void btnTriangle_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.triangle;
            curCanvas.Cursor = Cursors.Cross;
        }

        private void btnArrow_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.arrow;
            curCanvas.Cursor = Cursors.Cross;
        }

        private void btnHeart_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.heart;
            curCanvas.Cursor = Cursors.Cross;
        }

        private void btnBold_Click(object sender, RoutedEventArgs e)
        {
            bold = !bold;
            if (bold)
            {
                if (focusedTextbox != null)
                {
                    focusedTextbox.FontWeight = FontWeights.Bold;
                }
            }
            else
            {
                if (focusedTextbox != null)
                {
                    focusedTextbox.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void btnItalic_Click(object sender, RoutedEventArgs e)
        {
            italic = !italic;
            if (italic)
            {
                if (focusedTextbox != null)
                {
                    focusedTextbox.FontStyle = FontStyles.Italic;
                }
            }
            else
            {
                if (focusedTextbox != null)
                {
                    focusedTextbox.FontStyle = FontStyles.Normal;
                }
            }
        }

        private void btnUnderlined_Click(object sender, RoutedEventArgs e)
        {
            underlined = !underlined;
            if (underlined)
            {
                if (focusedTextbox != null)
                {
                    if (focusedTextbox.TextDecorations == null)
                    {
                        focusedTextbox.TextDecorations = new TextDecorationCollection();
                    }
                    focusedTextbox.TextDecorations = TextDecorations.Underline;

                }
            }
            else
            {
                if (focusedTextbox != null)
                {
                    focusedTextbox.TextDecorations = null;
                }
            }
        }

        private void ColorTextButton_Click(object sender, RoutedEventArgs e)
        {
            btnColorText.Background = ((Button)sender).Background;
            if (focusedTextbox != null)
            {
                focusedTextbox.Foreground = btnColorText.Background;
            }
        }

        private void btnOtherColorsText_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.ColorDialog dlg = new System.Windows.Forms.ColorDialog();
            dlg.AllowFullOpen = true;
            dlg.ShowDialog();
            Color color = new Color();
            color.A = dlg.Color.A;
            color.R = dlg.Color.R;
            color.G = dlg.Color.G;
            color.B = dlg.Color.B;
            btnColorText.Background = new SolidColorBrush(color);
            if (focusedTextbox != null)
            {
                focusedTextbox.Foreground = btnColorText.Background;
            }
        }

        private void menuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            currentFilePath = Document.Instance.OpenFile();
            listSlides.SelectedIndex = 0;
            Document.Instance.UpdateThumbs();
            //ScaleTransform scale = new ScaleTransform(5, 5);
            //curCanvas.RenderTransform = scale;
        }

        private void menuFileSave_Click(object sender, RoutedEventArgs e)
        {
            Document.Instance.SaveFile(currentFilePath);
        }

        private void menuFileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            Document.Instance.SaveFile();   //Hien thi hop thoai luu tap tin
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btnColor = (System.Windows.Controls.Button)sender;
            Color color = ((SolidColorBrush)btnColor.Background).Color;
            btnColor1.Background = new SolidColorBrush(color);
            penColor = color;
            fillColor = color;
            if (curCanvas.Children.Count > 1)
            {
                if (Selector.GetIsSelected(curCanvas.Children[1]))
                {
                    selectedControl = curCanvas.Children[1] as ContentControl;
                    ((Shape)selectedControl.Content).Stroke = new SolidColorBrush(penColor);
                }

            }
        }

        private void btnOtherColors_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog dlg = new System.Windows.Forms.ColorDialog();
            dlg.AllowFullOpen = true;
            dlg.ShowDialog();
            Color color = new Color();
            color.A = dlg.Color.A;
            color.R = dlg.Color.R;
            color.G = dlg.Color.G;
            color.B = dlg.Color.B;
            btnColor1.Background = new SolidColorBrush(color);
            penColor = color;
            fillColor = color;
            if (curCanvas.Children.Count > 1)
            {
                if (Selector.GetIsSelected(curCanvas.Children[1]))
                {
                    selectedControl = curCanvas.Children[1] as ContentControl;
                    ((Shape)selectedControl.Content).Stroke = new SolidColorBrush(penColor);
                }

            }
        }

        private void btnAddSlide_Click(object sender, RoutedEventArgs e)
        {
            Canvas newCanvas = new Canvas()
            {
                Background = Brushes.WhiteSmoke,
                Height = 600,
                Width = 800,
                AllowDrop = true,
                Name = "canvas",
            };
            newCanvas.MouseDown += canvas_MouseDown;
            newCanvas.MouseUp += canvas_MouseUp;
            newCanvas.MouseMove += canvas_MouseMove;
            newCanvas.MouseLeftButtonUp += canvas_MouseLeftButtonUp;
            newCanvas.PreviewMouseLeftButtonDown += canvas_PreviewMouseLeftButtonDown;
            newCanvas.PreviewMouseLeftButtonUp += canvas_PreviewMouseLeftButtonUp;

            curCanvas = newCanvas;
            canvasParents.Children.Clear();
            canvasParents.Children.Add(curCanvas);

            Document.Instance.AddSlide(curCanvas);

            listSlides.SelectedIndex = listSlides.Items.Count - 1;
        }

        private void btnVideo_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.video;
            curCanvas.Cursor = Cursors.Arrow;
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Videos"; // Default file name 
            dialog.DefaultExt = ".WMV"; // Default file extension 
            dialog.Filter = "MP4 Files (*.mp4)|*.mp4"; // Filter files by extension  

            // Show open file dialog box 
            Nullable<bool> result = dialog.ShowDialog();

            // Process open file dialog box results  
            if (result == true)
            {
                CustomVideo newVideo = new CustomVideo();
                // Open Document.Instanceument  
                newVideo.Source = dialog.FileName;

                ContentControl control = new ContentControl();
                control.Content = newVideo;
                Canvas.SetLeft(control, 10);
                Canvas.SetTop(control, 10);
                control.Width = 400;
                control.Height = 300;
                control.Background = new SolidColorBrush(Colors.White);
                control.Style = FindResource("DesignerItemStyle") as Style;
                Document.Instance.InsertText(control);
            }
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mDown = e.GetPosition(this.curCanvas);
            capture = true;
            if (Document.DrawType == DrawType.brush || Document.DrawType == DrawType.pencil || Document.DrawType == DrawType.erase)
            {
                pathGeometry = new PathGeometry();
                pathFigure = new PathFigure();
                pathFigure.StartPoint = mDown;
                pathFigure.IsClosed = false;
                pathGeometry.Figures.Add(pathFigure);
                path = new Path();
                path.Stroke = new SolidColorBrush(penColor);
                if (Document.DrawType == DrawType.erase)
                {
                    path.Stroke = new SolidColorBrush(Colors.White);
                }
                if (Document.DrawType == DrawType.brush || Document.DrawType == DrawType.erase)
                {
                    path.StrokeThickness = penThickness;
                }
                else if (Document.DrawType == DrawType.pencil)
                {
                    path.StrokeThickness = 1;
                }
                path.Data = pathGeometry;
                Document.Instance.DrawShape(path, 1);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            capture = false;
            if (curShape != null)
            {
                if (Document.DrawType == DrawType.ellipse || Document.DrawType == DrawType.rectangle || Document.DrawType == DrawType.triangle || Document.DrawType == DrawType.arrow || Document.DrawType == DrawType.heart)
                {
                    Shape temp;
                    if (Document.DrawType == DrawType.ellipse)
                    {
                        temp = new Ellipse();
                    }
                    else if (Document.DrawType == DrawType.rectangle)
                    {
                        temp = new Rectangle();
                    }
                    else if (Document.DrawType == DrawType.triangle)
                    {
                        temp = new Triangle();
                    }
                    else if (Document.DrawType == DrawType.arrow)
                    {
                        temp = new Arrow();
                    }
                    else
                    {
                        temp = new Heart();
                    }
                    temp.Stroke = new SolidColorBrush(penColor);
                    temp.StrokeThickness = penThickness;
                    curControl = new ContentControl();
                    temp.IsHitTestVisible = true;
                    if (Document.DrawType == DrawType.triangle)
                    {
                        ((Triangle)temp).Start = ((Triangle)curShape).Start;
                        temp.Width = curShape.Width;
                        temp.Height = curShape.Height;
                    }
                    if (Document.DrawType == DrawType.arrow)
                    {
                        ((Arrow)temp).Start = ((Arrow)curShape).Start;
                        temp.Width = curShape.Width;
                        temp.Height = curShape.Height;
                    }
                    if (Document.DrawType == DrawType.heart)
                    {
                        ((Heart)temp).Start = ((Heart)curShape).Start;
                        temp.Width = curShape.Width;
                        temp.Height = curShape.Height;
                    }
                    Canvas.SetLeft(curControl, curShape.Margin.Left);
                    Canvas.SetTop(curControl, curShape.Margin.Top);
                    curControl.Width = curShape.Width;
                    curControl.Height = curShape.Height;
                    curControl.Content = temp;
                    curControl.Style = FindResource("DesignerItemStyle") as Style;
                    curControl.Background = new SolidColorBrush(Colors.White);
                    Document.Instance.DrawShape(curControl, GetOutline());

                }

                curShape = null;
            }
            else if (Document.DrawType == DrawType.line && curLine != null)
            {
                Line line = new Line();
                line.Stroke = new SolidColorBrush(penColor);
                line.StrokeThickness = penThickness;
                line.X1 = curLine.X1;
                line.X2 = curLine.X2;
                line.Y1 = curLine.Y1;
                line.Y2 = curLine.Y2;
                Document.Instance.DrawShape(line, GetOutline());
                curLine = null;
            }

            Document.Instance.UpdateThumbs();
        }

        private void canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Document.drawType == DrawType.nothing)
            //{
            if (e.Source != curCanvas && e.Source.GetType() == typeof(ContentControl))
            {
                foreach (UIElement control in curCanvas.Children)
                {
                    Selector.SetIsSelected(control, false);
                }
                Selector.SetIsSelected((DependencyObject)e.Source, true);
            }
            else if(e.Source.GetType() == typeof(Image) || e.Source.GetType() == typeof(CustomVideo) || e.Source.GetType() == typeof(EditableTextBlock))
            {
                foreach (UIElement control in curCanvas.Children)
                {
                    if(control.GetType() == typeof(ContentControl))
                    {
                        if((control as ContentControl).Content == e.Source)
                        {
                            Selector.SetIsSelected((DependencyObject)control, true);
                        }
                    }
                }
                if (e.Source.GetType() == typeof(EditableTextBlock))
                {
                    focusedTextbox = e.Source as EditableTextBlock;
                }
                else focusedTextbox = null;
            }
            else if (Document.DrawType == DrawType.text && e.Source.GetType() != typeof(EditableTextBlock))  //Insert text
            {
                EditableTextBlock txt = new EditableTextBlock();
                txt.Text = "Double click to edit";
                txt.FontFamily = fontFamily;
                txt.FontSize = fontSize;
                txt.FontStyle = fontStyle;
                txt.FontWeight = fontWeight;
                if (decoration != null)
                {
                    txt.TextDecorations = decoration;
                }
                txt.LostKeyboardFocus += txt_LostKeyboardFocus;
                txt.SizeChanged += txt_SizeChanged;
                //txt.TextChanged += txt_TextChanged;
                txt.GotKeyboardFocus += txt_GotKeyboardFocus;
                txt.TextWrapping = TextWrapping.Wrap;
                txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                ContentControl control = new ContentControl();
                Canvas.SetLeft(control, e.GetPosition(curCanvas).X);
                Canvas.SetTop(control, e.GetPosition(curCanvas).Y);
                control.Content = txt;
                control.Width = 100;
                control.Height = 50;
                //control.Padding = new Thickness(1);
                control.Background = new SolidColorBrush(Colors.White);
                control.Style = FindResource("DesignerItemStyle") as Style;
                Document.Instance.InsertText(control);

                curCanvas.Cursor = Cursors.Arrow;
                Document.DrawType = DrawType.nothing;
            }
            //}
        }

        private void canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Line temp = new Line();
            //if (Document.drawType == DrawType.nothing)
            //{
                if (e.Source == curCanvas && e.Source.GetType() != temp.GetType())
                {
                    foreach (UIElement control in curCanvas.Children)
                    {
                        Selector.SetIsSelected(control, false);
                        focusedTextbox = null;
                    }
                }
            //}
            //else if (Document.drawType == DrawType.fill)
            //{
            //    System.Drawing.Color color = new System.Drawing.Color();
            //    color = System.Drawing.Color.FromArgb(fillColor.A, fillColor.R, fillColor.G, fillColor.B);
            //    System.Drawing.Bitmap bm = Document.Instance.CanvasToBitmap(Document.Instance.canvas);
            //    Document.Instance.FloodFill(bm, new System.Drawing.Point((int)e.GetPosition(Document.Instance.canvas).X, (int)e.GetPosition(Document.Instance.canvas).Y), color);
            //}

            Document.Instance.UpdateThumbs();
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            mMovePre = mMove;
            mMove = e.GetPosition(this.curCanvas);
            bool addShape = false;

            if ((Document.DrawType == DrawType.ellipse || Document.DrawType == DrawType.rectangle || Document.DrawType == DrawType.triangle || Document.DrawType == DrawType.arrow || Document.DrawType == DrawType.heart) && capture)
            {

                if (curShape == null)
                {

                    if (Document.DrawType == DrawType.ellipse)
                    {
                        curShape = new Ellipse();
                    }
                    else if (Document.DrawType == DrawType.rectangle)
                    {
                        curShape = new Rectangle();
                    }
                    else if (Document.DrawType == DrawType.triangle)
                    {

                        curShape = new Triangle();
                        ((Triangle)curShape).Start = mDown;
                    }
                    else if (Document.DrawType == DrawType.arrow)
                    {

                        curShape = new Arrow();
                        ((Arrow)curShape).Start = mDown;
                    }
                    else
                    {
                        curShape = new Heart();
                        ((Heart)curShape).Start = mDown;
                    }
                    addShape = true;
                    curShape.StrokeThickness = penThickness;
                    curShape.Stroke = new SolidColorBrush(penColor);
                }

                if (mMove.X <= mDown.X && mMove.Y <= mDown.Y)  //Góc phần tư thứ nhất
                {
                    curShape.Margin = new Thickness(mMove.X, mMove.Y, 0, 0);
                }
                else if (mMove.X >= mDown.X && mMove.Y <= mDown.Y)
                {
                    curShape.Margin = new Thickness(mDown.X, mMove.Y, 0, 0);
                }
                else if (mMove.X >= mDown.X && mMove.Y >= mDown.Y)
                {
                    curShape.Margin = new Thickness(mDown.X, mDown.Y, 0, 0);
                }
                else if (mMove.X <= mDown.X && mMove.Y >= mDown.Y)
                {
                    curShape.Margin = new Thickness(mMove.X, mDown.Y, 0, 0);
                }

                curShape.Width = Math.Abs(mMove.X - mDown.X);
                curShape.Height = Math.Abs(mMove.Y - mDown.Y);


                if (addShape)
                {
                    Document.Instance.DrawCapture(curShape);
                }
            }
            else if (Document.DrawType == DrawType.line && capture)
            {
                if (curLine == null)
                {
                    curLine = new Line();
                    addShape = true;
                }
                curLine.X1 = mDown.X;
                curLine.Y1 = mDown.Y;
                curLine.X2 = mMove.X;
                curLine.Y2 = mMove.Y;
                curLine.StrokeThickness = penThickness;
                curLine.Stroke = new SolidColorBrush(penColor);
                if (addShape)
                {
                    Document.Instance.DrawCapture(curLine);

                }
            }
            else if ((Document.DrawType == DrawType.brush || Document.DrawType == DrawType.pencil || Document.DrawType == DrawType.erase) && capture)
            {
                LineSegment ls = new LineSegment();
                ls.Point = mMove;
                pathFigure.Segments.Add(ls);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnSmooth_Checked(object sender, RoutedEventArgs e)
        {
            btnDash.IsChecked = false;
            btnDot.IsChecked = false;
            if (curCanvas.Children.Count > 1)
            {
                if (Selector.GetIsSelected(curCanvas.Children[1]))
                {
                    selectedControl = curCanvas.Children[1] as ContentControl;
                    ((Shape)selectedControl.Content).StrokeDashArray = null;
                }

            }
        }

        private void btnDash_Checked(object sender, RoutedEventArgs e)
        {
            btnSmooth.IsChecked = false;
            btnDot.IsChecked = false;
            double[] dashes = { 4, 4 };
            if (curCanvas.Children.Count > 1)
            {
                if (Selector.GetIsSelected(curCanvas.Children[1]))
                {
                    selectedControl = curCanvas.Children[1] as ContentControl;
                    ((Shape)selectedControl.Content).StrokeDashArray = new DoubleCollection(dashes);
                }

            }
        }

        private void btnDot_Checked(object sender, RoutedEventArgs e)
        {
            btnSmooth.IsChecked = false;
            btnDash.IsChecked = false;
            double[] dashes = { 4, 1, 4, 1 };
            if (curCanvas.Children.Count > 1)
            {
                if (Selector.GetIsSelected(curCanvas.Children[1]))
                {
                    selectedControl = curCanvas.Children[1] as ContentControl;
                    ((Shape)selectedControl.Content).StrokeDashArray = new DoubleCollection(dashes);
                }

            }
        }

        private void onDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            //Move the Thumb to the mouse position during the drag operation
            //double yadjust = curCanvas.Height + e.VerticalChange;
            //double xadjust = curCanvas.Width + e.HorizontalChange;
            //if ((xadjust >= 0) && (yadjust >= 0))
            //{
            //    curCanvas.Width = xadjust;
            //    curCanvas.Height = yadjust;
            //    Canvas.SetLeft(myThumb, Canvas.GetLeft(myThumb) + e.HorizontalChange);
            //    Canvas.SetTop(myThumb, Canvas.GetTop(myThumb) + e.VerticalChange);
            //    Console.Write("Size: " +
            //            curCanvas.Width.ToString() +
            //             ", " +
            //            curCanvas.Height.ToString());
            //}
        }

        private void onDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            //myThumb.Background = Brushes.Orange;
        }

        private void CanvasSizeChange(object sender, KeyboardFocusChangedEventArgs e)   //Change size of canvas
        {
            UpdateCanvasSize();
        }

        private void ConfirmCanvasSize(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                UpdateCanvasSize();
            }
            else
            {
                System.Windows.Input.Key k = e.Key;
                if (Key.D0 <= k && k <= Key.D9 ||
                    Key.NumPad0 <= k && k <= Key.NumPad9 ||
                    k == Key.OemMinus || k == Key.Subtract ||
                    k == Key.Decimal || k == Key.OemPeriod)
                {
                }
                else
                {
                    e.Handled = true;

                    // just a little sound effect for wrong key pressed
                    System.Media.SystemSound ss = System.Media.SystemSounds.Beep;
                    ss.Play();

                }
            }
        }

        private void cbxThickness_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cbxThickness.SelectedIndex == 0)
            {
                penThickness = 1;
            }
            else if (cbxThickness.SelectedIndex == 1)
            {
                penThickness = 3;
            }
            else if (cbxThickness.SelectedIndex == 2)
            {
                penThickness = 5;
            }
            else if (cbxThickness.SelectedIndex == 3)
            {
                penThickness = 7;
            }
            if (curCanvas.Children.Count > 1)
            {
                if (Selector.GetIsSelected(curCanvas.Children[1]))
                {
                    selectedControl = curCanvas.Children[1] as ContentControl;
                    ((Shape)selectedControl.Content).StrokeThickness = penThickness;
                }

            }
        }

        private void cbxFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            fontFamily = new FontFamily(cbxFontFamily.Items[cbxFontFamily.SelectedIndex].ToString());
            if (focusedTextbox != null)
            {
                focusedTextbox.FontFamily = fontFamily;
            }
        }

        private void cbxFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxFontSize.SelectionBoxItem != null)
            {
                ComboBoxItem ComboItem = (ComboBoxItem)cbxFontSize.SelectedItem;
                string value = ComboItem.Content.ToString();
                fontSize = Convert.ToDouble(value);
                if (focusedTextbox != null)
                {
                    focusedTextbox.FontSize = fontSize;
                }
            }
        }

        private void listSlides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Canvas temp = listSlides.SelectedItem as Canvas;
            //
            //canvas.Children.Clear();
            //foreach (var item in temp.Children)
            //{
            //    canvas.Children.Add(CLoneObject(item) as UIElement);
            //}
            if (listSlides.Items.Count == 0) return;
            ChangeCurCanvas(Document.Instance.slides[listSlides.SelectedIndex]);
            Document.Instance.canvas = curCanvas;
            canvasParents.Children.Clear();
            canvasParents.Children.Add(curCanvas);
        }

        private void onDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //myThumb.Background = Brushes.Blue;
        }

        void txt_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //focusedTextbox = (EditableTextBlock)sender;
        }

        void txt_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //TextBox txt = (TextBox)sender;
            //((ContentControl)txt.Parent).Width = txt.Width;
            //((ContentControl)txt.Parent).Height = txt.Height;
        }

        private void txt_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            oldLineCount = txt.LineCount;

        }

        #endregion
    }
}
