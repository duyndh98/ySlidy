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

namespace ySlidy
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
        private Polyline curLine;
        private Color fillColor;
        private Color penColor;
        private double penThickness;
        private PathGeometry pathGeometry;
        private Path path;
        private PathFigure pathFigure;
        private string currentFilePath;
        private ContentControl selectedControl;
        private TextBox focusedTextbox;
        public TextBox FocusedTextbox
        {
            get { return focusedTextbox; }
            set
            {
                focusedTextbox = value;
                cbxFontSize.SelectedValue = focusedTextbox.FontSize;
                cbxFontFamily.SelectedItem = focusedTextbox.FontFamily;
                btnColorText.Background = focusedTextbox.Foreground;
                switch (focusedTextbox.TextAlignment)
                {
                    case TextAlignment.Left:
                        btnTextAlignLeft.IsSelected = true;
                        break;
                    case TextAlignment.Center:
                        btnTextAlignCenter.IsSelected = true;
                        break;
                    case TextAlignment.Right:
                        btnTextAlignRight.IsSelected = true;
                        break;
                    case TextAlignment.Justify:
                        btnTextAlignJustify.IsSelected = true;
                        break;
                }
                switch (focusedTextbox.VerticalContentAlignment)
                {
                    case VerticalAlignment.Top:
                        btnTextAllignTop.IsSelected = true;
                        break;
                    case VerticalAlignment.Center:
                        btnTextAllignBottom.IsSelected = true;
                        break;
                    case VerticalAlignment.Bottom:
                        btnTextVerticalAlignCenter.IsSelected = true;
                        break;
                }
                btnBold.IsSelected = focusedTextbox.FontWeight == FontWeights.Bold ? true : false;
                btnItalic.IsSelected = focusedTextbox.FontStyle == FontStyles.Italic ? true : false;
                btnUnderlined.IsSelected = focusedTextbox.TextDecorations == TextDecorations.Underline ? true : false;

            }
        }
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
            //if (txtWidth.Text == "")
            //{
            //    txtWidth.Text = Document.Instance.canvas.Width.ToString();
            //}
            //if (txtHeight.Text == "")
            //{
            //    txtHeight.Text = Document.Instance.canvas.Height.ToString();
            //}
            //try
            //{
            //    if (txtWidth.Text != "" && txtHeight.Text != "")
            //    {
            //        Document.Instance.canvas.ClipToBounds = true;
            //        Document.Instance.canvas.SnapsToDevicePixels = true;
            //        Document.Instance.canvas.Width = Convert.ToDouble(txtWidth.Text);
            //        Document.Instance.canvas.Height = Convert.ToDouble(txtHeight.Text);
            //    }
            //}
            //catch (FormatException ex)
            //{
            //    MessageBox.Show("Invalid size of paint surface: " + "\n" + ex.Message, "Invalid input", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

        }

        private Object CLoneObject(Object o)
        {
            var xaml = System.Windows.Markup.XamlWriter.Save(o);
            var deepCopy = System.Windows.Markup.XamlReader.Parse(xaml) as Object;
            return deepCopy;
        }

        private void AddNumberForEachSlide(object sender, RoutedEventArgs e)
        {
            Document.Instance.IsNumberSlide = !Document.Instance.IsNumberSlide;
        }

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
            UpdateCanvasSize();
            cbxFontSize.ItemsSource = new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            cbxFontSize.SelectedItem = Document.Instance.SetUpTextBox.FontSize;
        }

        private void btnPencil_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.pencil;
            curCanvas.Cursor = Cursors.Pen;
            curCanvas.EditingMode = InkCanvasEditingMode.Ink;
            curCanvas.DefaultDrawingAttributes.Color = penColor;
        }

        private void btnLine_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.line;
            curCanvas.Cursor = Cursors.Cross;
            curCanvas.EditingMode = InkCanvasEditingMode.None;
        }

        private void btnEllipse_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.ellipse;
            curCanvas.Cursor = Cursors.Cross;
            curCanvas.EditingMode = InkCanvasEditingMode.None;
        }

        private void btnRectangle_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.rectangle;
            curCanvas.Cursor = Cursors.Cross;
            curCanvas.EditingMode = InkCanvasEditingMode.None;
        }

        private void btnText_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.text;
            curCanvas.Cursor = Cursors.IBeam;
            curCanvas.EditingMode = InkCanvasEditingMode.None;
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            Document.DrawType = DrawType.nothing;
            curCanvas.Cursor = Cursors.Hand;
            curCanvas.EditingMode = InkCanvasEditingMode.Select;
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
            curCanvas.EditingMode = InkCanvasEditingMode.None;
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

                InkCanvas.SetLeft(control, 10);
                InkCanvas.SetTop(control, 10);
                control.Content = newImage;
                control.Width = 100;
                //control.Padding = new Thickness(1);
                control.Background = new SolidColorBrush(Colors.White);
                //control.Style = FindResource("DesignerItemStyle") as Style;
                Document.Instance.Insert(control);
            }
        }

        private void btnErazer_Click(object sender, RoutedEventArgs e)
        {
            //Document.DrawType = DrawType.erase;
            //curCanvas.Cursor = Cursors.Arrow;

            List<UIElement> listPrePareToRemove = new List<UIElement>();
            foreach (UIElement ui in curCanvas.GetSelectedElements())
            {
                listPrePareToRemove.Add(ui);
            }
            foreach (UIElement ui in listPrePareToRemove)
                curCanvas.Children.Remove(ui);
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

        private void btnInsertTable_Click(object sender, RoutedEventArgs e)
        {
            //var container = new ContentControl();
            //var flowdocument = new FlowDocument();
            //container.Content = flowdocument;
            //var table = new Table();
            //table.CellSpacing = 0;
            //flowdocument.Blocks.Add(table);

            //int columns = Convert.ToInt32(tbTableColumn.Text);
            //int rows = Convert.ToInt32(tbTableRow.Text);

            //for (int i = 0; i < columns; i++) {
            //    var tableColumn = new TableColumn();
            //    tableColumn.Width = new GridLength(50);
            //    table.Columns.Add(tableColumn);
            //}


            //for (int i = 0; i < rows; i++)
            //{
            //    var tablerowgroup = new TableRowGroup();
            //    table.RowGroups.Add(tablerowgroup);
            //    var tablerow = new TableRow();
            //    tablerowgroup.Rows.Add(tablerow);
            //    tablerow.FontSize = 14;
            //    if (i == 0)
            //    {
            //        for(int j = 0; j < columns; j++)
            //        {
            //            var tablecell = new TableCell();
            //            if(j == 0)
            //            {
            //                tablecell.BorderThickness = new Thickness(1);
            //            }
            //            else
            //            {
            //                tablecell.BorderThickness = new Thickness(0, 1, 1, 1);
            //            }
            //            tablecell.BorderBrush = Brushes.Black;
            //            var paragraph = new Paragraph();
            //            tablecell.Blocks.Add(paragraph);
            //            var textbox = new TextBox();
            //            textbox.Text = "123";
            //            paragraph.Inlines.Add(textbox);
            //        }
            //    }
            //    else
            //    {
            //        for (int j = 0; j < columns; j++)
            //        {
            //            var tablecell = new TableCell();
            //            if (j == 0)
            //            {
            //                tablecell.BorderThickness = new Thickness(1, 0, 1, 1);
            //            }
            //            else
            //            {
            //                tablecell.BorderThickness = new Thickness(0, 0, 1, 1);
            //            }
            //            tablecell.BorderBrush = Brushes.Black;
            //            var paragraph = new Paragraph();
            //            tablecell.Blocks.Add(paragraph);
            //            var textbox = new TextBox();
            //            textbox.Text = "123";
            //            paragraph.Inlines.Add(textbox);
            //        }
            //    }
            //}
            //container.Width = 50 * columns;
            //Document.Instance.Insert(container);
        }

        #region Edit Text
        private void btnBold_Click(object sender, RoutedEventArgs e)
        {
            if (btnBold.IsSelected)
            {
                if (FocusedTextbox != null)
                {
                    FocusedTextbox.FontWeight = FontWeights.Bold;
                    Document.Instance.SetUpTextBox.FontWeight = FontWeights.Bold;
                }
            }
            else
            {
                if (FocusedTextbox != null)
                {
                    FocusedTextbox.FontWeight = FontWeights.Normal;
                    Document.Instance.SetUpTextBox.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void btnItalic_Click(object sender, RoutedEventArgs e)
        {
            if (btnItalic.IsSelected)
            {
                if (FocusedTextbox != null)
                {
                    FocusedTextbox.FontStyle = FontStyles.Italic;
                    Document.Instance.FontStyle = FontStyles.Italic;
                }
            }
            else
            {
                if (FocusedTextbox != null)
                {
                    FocusedTextbox.FontStyle = FontStyles.Normal;
                    Document.Instance.FontStyle = FontStyles.Normal;
                }
            }
        }

        private void btnUnderlined_Click(object sender, RoutedEventArgs e)
        {
            if (btnUnderlined.IsSelected)
            {
                if (FocusedTextbox != null)
                {
                    if (FocusedTextbox.TextDecorations == null)
                    {
                        FocusedTextbox.TextDecorations = new TextDecorationCollection();
                    }
                    FocusedTextbox.TextDecorations = TextDecorations.Underline;
                    Document.Instance.SetUpTextBox.TextDecorations = TextDecorations.Underline;
                }
            }
            else
            {
                if (FocusedTextbox != null)
                {
                    FocusedTextbox.TextDecorations = null;
                    Document.Instance.SetUpTextBox.TextDecorations = null;
                }
            }
        }

        private void ColorTextButton_Click(object sender, RoutedEventArgs e)
        {
            btnColorText.Background = ((Button)sender).Background;
            if (FocusedTextbox != null)
            {
                FocusedTextbox.Foreground = btnColorText.Background;
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
            if (FocusedTextbox != null)
            {
                FocusedTextbox.Foreground = btnColorText.Background;
            }
        }

        #endregion

        #region Menu Open Save NewFile

        private void menuNewFile_Click(object sender, RoutedEventArgs e)
        {
            Document.Instance.NewFile();
            listSlides.SelectedIndex = 0; //Thay đổi selected slide vê 0 để đổi cả curCanvas
            Document.Instance.UpdateThumbs();
            this.Title = "ySlidy";
        }

        private void menuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            currentFilePath = Document.Instance.OpenFile();
            this.Title = "ySlidy - " + currentFilePath;
            listSlides.SelectedIndex = 0; //Thay đổi selected slide vê 0 để đổi cả curCanvas
            Document.Instance.UpdateThumbs();
        }

        private void menuFileSave_Click(object sender, RoutedEventArgs e)
        {
            Document.Instance.SaveFile(currentFilePath);
            this.Title = "ySlidy - " + currentFilePath;
        }

        private void menuFileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            Document.Instance.SaveFile();   //Hien thi hop thoai luu tap tin
            this.Title = "ySlidy - " + currentFilePath;
        }
        #endregion

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
            curCanvas.DefaultDrawingAttributes.Color = penColor;
        }

        private void btnAddSlide_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas newCanvas = new InkCanvas()
            {
                EditingMode = InkCanvasEditingMode.Select,
                Background = Brushes.WhiteSmoke,
                Height = 600,
                Width = 800,
                AllowDrop = true,
            };
            ChangeCurCanvas(newCanvas);
            canvasParents.Children.Clear();
            canvasParents.Children.Add(curCanvas);

            Document.Instance.AddSlide(curCanvas);

            listSlides.SelectedIndex = listSlides.Items.Count - 1;
        }

        private void btnDelSlide_Click(object sender, RoutedEventArgs e)
        {
            Document.Instance.DelSlide(listSlides.SelectedIndex);
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
                InkCanvas.SetLeft(control, 10);
                InkCanvas.SetTop(control, 10);
                control.Width = 400;
                control.Height = 300;
                control.Background = new SolidColorBrush(Colors.White);
                //control.Style = FindResource("DesignerItemStyle") as Style;
                Document.Instance.Insert(control);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
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
            //    InkCanvas.SetLeft(myThumb, Canvas.GetLeft(myThumb) + e.HorizontalChange);
            //    InkCanvas.SetTop(myThumb, Canvas.GetTop(myThumb) + e.VerticalChange);
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
            Document.Instance.SetUpTextBox.FontFamily = new FontFamily(cbxFontFamily.Items[cbxFontFamily.SelectedIndex].ToString());
            if (FocusedTextbox != null)
            {
                FocusedTextbox.FontFamily = Document.Instance.SetUpTextBox.FontFamily;
            }
        }

        private void cbxFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxFontSize.SelectedItem != null)
            {
                string value = cbxFontSize.SelectedItem.ToString();
                Document.Instance.SetUpTextBox.FontSize = Convert.ToDouble(value);
                if (FocusedTextbox != null)
                {
                    FocusedTextbox.FontSize = Document.Instance.SetUpTextBox.FontSize;
                    Document.Instance.SetLocationInCanvas(FocusedTextbox, curCanvas);
                }
            }
        }

        private void ChangeBackGroundColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog dlg = new System.Windows.Forms.ColorDialog();
            dlg.AllowFullOpen = true;
            dlg.ShowDialog();
            Color color = new Color();
            color.A = dlg.Color.A;
            color.R = dlg.Color.R;
            color.G = dlg.Color.G;
            color.B = dlg.Color.B;
            curCanvas.Background = new SolidColorBrush(color);
        }

        private void ChangeBackGroundImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                ImageBrush newImage = new ImageBrush();
                newImage.ImageSource = new BitmapImage(new Uri(dlg.FileName));

                curCanvas.Background = newImage;
            }

        }

        private void TextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Document.Instance.SetLocationInCanvas(e.Source as TextBox, curCanvas);
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
            //FocusedTextbox = (EditableTextBlock)sender;
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
        }

        private void btnPresent_Click(object sender, RoutedEventArgs e)
        {
            Presentation f = new Presentation(Document.Instance.slides, 0);
            f.ShowDialog();
        }

        private void AngleValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ReadOnlyCollection<UIElement> selectedElements = curCanvas.GetSelectedElements();

            foreach (UIElement element in selectedElements)
            {
                (element.RenderTransform as RotateTransform).Angle = (double)angelObject.Value;
            }
        }

        /// <summary>
        /// Event cho textbox PreviewTextInput chỉ cho phép nhập số
        /// </summary>
        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #endregion

        #region Method for InkCanvas

        private void ChangeCurCanvas(InkCanvas c)
        {
            curCanvas = c;
            curCanvas.MouseDown += canvas_MouseDown;
            curCanvas.MouseMove += canvas_MouseMove;
            curCanvas.MouseLeftButtonUp += canvas_MouseLeftButtonUp;
            curCanvas.PreviewMouseLeftButtonDown += canvas_PreviewMouseLeftButtonDown;
            curCanvas.PreviewMouseLeftButtonUp += canvas_PreviewMouseLeftButtonUp;
            curCanvas.SelectionMoved += canvas_SelectionMoved;
            curCanvas.SelectionChanged += canvas_SelectionChanged;
        }

        public void canvas_MouseDown(object sender, MouseButtonEventArgs e)
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
                    temp.Fill = new SolidColorBrush(Colors.Transparent);
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
                    InkCanvas.SetLeft(curControl, curShape.Margin.Left);
                    InkCanvas.SetTop(curControl, curShape.Margin.Top);
                    curControl.Width = curShape.Width;
                    curControl.Height = curShape.Height;
                    curControl.Content = temp;
                    //curControl.Style = FindResource("DesignerItemStyle") as Style;
                    curControl.Background = new SolidColorBrush(Colors.White);
                    Document.Instance.DrawShape(curControl, GetOutline());

                }

                curShape = null;
            }
            else if (Document.DrawType == DrawType.line && curLine != null)
            {
                Polyline line = new Polyline();
                line.Stroke = new SolidColorBrush(penColor);
                line.StrokeThickness = penThickness;
                //line.X1 = curLine.X1;
                //line.X2 = curLine.X2;
                //line.Y1 = curLine.Y1;
                //line.Y2 = curLine.Y2;
                line.Points = curLine.Points;
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
            else if (e.Source.GetType() == typeof(Image) || e.Source.GetType() == typeof(CustomVideo) || e.Source.GetType() == typeof(TextBox))
            {
                foreach (UIElement control in curCanvas.Children)
                {
                    if (control.GetType() == typeof(ContentControl))
                    {
                        if ((control as ContentControl).Content == e.Source)
                        {
                            Selector.SetIsSelected((DependencyObject)control, true);
                        }
                    }
                }
                if (e.Source.GetType() == typeof(TextBox))
                {
                    FocusedTextbox = e.Source as TextBox;
                    curCanvas.Select(curCanvas.Strokes, new UIElement[] { e.Source as TextBox });
                    Document.Instance.UpdateSetUpTextBox(FocusedTextbox);
                }
            }
            else if (Document.DrawType == DrawType.text && e.Source.GetType() != typeof(TextBox))  //Insert text
            {
                TextBox txt = new TextBox();
                txt.Text = "Add text here.";
                txt.TextWrapping = TextWrapping.Wrap;
                txt.AcceptsReturn = true;
                txt.FontFamily = Document.Instance.SetUpTextBox.FontFamily;
                txt.FontSize = Document.Instance.SetUpTextBox.FontSize;
                txt.FontStyle = Document.Instance.SetUpTextBox.FontStyle;
                txt.FontWeight = Document.Instance.SetUpTextBox.FontWeight;
                txt.TextAlignment = Document.Instance.SetUpTextBox.TextAlignment;
                txt.Foreground = Document.Instance.SetUpTextBox.Foreground;

                txt.Background = Brushes.Transparent;
                txt.BorderBrush = Brushes.Transparent;
                txt.LostKeyboardFocus += txt_LostKeyboardFocus;
                txt.SizeChanged += TextBox_SizeChanged;
                txt.GotKeyboardFocus += txt_GotKeyboardFocus;
                txt.VerticalContentAlignment = VerticalAlignment.Top;

                InkCanvas.SetLeft(txt, e.GetPosition(curCanvas).X);
                InkCanvas.SetTop(txt, e.GetPosition(curCanvas).Y);
                Document.Instance.InsertText(txt);

                curCanvas.Cursor = Cursors.Arrow;
                Document.DrawType = DrawType.nothing;
            }
            //}
        }

        private void btnTextAlignLeft_Selected(object sender, RoutedEventArgs e)
        {
            Document.Instance.SetUpTextBox.TextAlignment = TextAlignment.Left;
            if (FocusedTextbox != null)
                FocusedTextbox.TextAlignment = TextAlignment.Left;
        }

        private void btnTextAlignCenter_Selected(object sender, RoutedEventArgs e)
        {
            Document.Instance.SetUpTextBox.TextAlignment = TextAlignment.Center;
            if (FocusedTextbox != null)
                FocusedTextbox.TextAlignment = TextAlignment.Center;
        }

        private void btnTextAlignRight_Selected(object sender, RoutedEventArgs e)
        {
            Document.Instance.SetUpTextBox.TextAlignment = TextAlignment.Right;
            if (FocusedTextbox != null)
                FocusedTextbox.TextAlignment = TextAlignment.Right;
        }

        private void btnTextAlignJustify_Selected(object sender, RoutedEventArgs e)
        {
            Document.Instance.SetUpTextBox.TextAlignment = TextAlignment.Justify;
            if (FocusedTextbox != null)
                FocusedTextbox.TextAlignment = TextAlignment.Justify;
        }

        private void btnTextAllignTop_Selected(object sender, RoutedEventArgs e)
        {
            Document.Instance.SetUpTextBox.VerticalContentAlignment = VerticalAlignment.Top;
            if (FocusedTextbox != null)
                FocusedTextbox.VerticalContentAlignment = VerticalAlignment.Top;
        }

        private void btnTextVerticalAlignCenter_Selected(object sender, RoutedEventArgs e)
        {
            Document.Instance.SetUpTextBox.VerticalContentAlignment = VerticalAlignment.Center;
            if (FocusedTextbox != null)
                FocusedTextbox.VerticalContentAlignment = VerticalAlignment.Center;
        }

        private void btnTextAllignBottom_Selected(object sender, RoutedEventArgs e)
        {
            Document.Instance.SetUpTextBox.VerticalContentAlignment = VerticalAlignment.Bottom;
            if (FocusedTextbox != null)
                FocusedTextbox.VerticalContentAlignment = VerticalAlignment.Bottom;
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
                    curLine = new Polyline();
                    addShape = true;
                }
                //curLine.X1 = mDown.X;
                //curLine.Y1 = mDown.Y;
                //curLine.X2 = mMove.X;
                //curLine.Y2 = mMove.Y;
                curLine.Points.Clear();
                curLine.Points.Add(new Point(mDown.X, mDown.Y));
                curLine.Points.Add(new Point(mMove.X, mMove.Y));
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

        private void btnChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            var x = Application.Current.Resources.MergedDictionaries;
            var rotate = iconThemeChange.RenderTransform as RotateTransform;
            foreach(var item in x)
            {
                if(item.Source == new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml"))
                {
                    item.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml");
                }
                else
                {
                    item.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");
                }
                return;
            }
        }

        private void canvas_SelectionChanged(object sender, EventArgs e)
        {
            ReadOnlyCollection<UIElement> selectedElements = curCanvas.GetSelectedElements();

            textMenu.Visibility = Visibility.Collapsed;

            foreach (UIElement element in selectedElements)
            {
                if (element.GetType() == typeof(TextBox))
                {
                    FocusedTextbox = element as TextBox;
                    Document.Instance.UpdateSetUpTextBox(FocusedTextbox);
                    textMenu.Visibility = Visibility.Visible;
                }

                //Get angle của các phần tử đưuọc select
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                var x = (element.RenderTransform as RotateTransform);
                if (x == null)
                {
                    element.RenderTransform = new RotateTransform(0);
                    x = (element.RenderTransform as RotateTransform);
                }

                AngleValue.Text = x.Angle.ToString();
            }

        }

        private void canvas_SelectionMoved(object sender, EventArgs e)
        {
            ReadOnlyCollection<UIElement> selectedElements = curCanvas.GetSelectedElements();

            foreach (UIElement element in selectedElements)
            {
                Document.Instance.SetLocationInCanvas(element, curCanvas);
            }
        }

        #endregion
    }
}