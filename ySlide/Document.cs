using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using System.Windows.Input;

namespace ySlidy
{
    /// <summary>
    /// Các kiểu vẽ/chức năng của paint
    /// </summary>
    public enum DrawType { nothing, pencil, brush, line, ellipse, rectangle, triangle, arrow, heart, fill, erase, text, image, video };

    /// <summary>
    /// Quản lý danh sách slide, slide hiện tại, hình thu nhỏ, style của textbox, đánh số slide
    /// </summary>
    public class Document : Window
    {
        private static Document instance;
        public ObservableCollection<InkCanvas> slides;
        public ObservableCollection<System.Windows.Controls.Image> thumbs;
        public InkCanvas canvas;  //Danh sách các hình trên trang
        private static DrawType drawType; //Kiểu vẽ hiện tại.
        private SetUpTextBox setUpTextBox = new SetUpTextBox(); //Style của textbox
        private bool isNumberSlide; //Biến để kiểm tra tính năng đánh số slide có đang bật hay không
        public bool IsNumberSlide
        {
            get { return isNumberSlide; }
            set
            {
                isNumberSlide = value;
                NumberSlide(value);
            }
        }

        private string themeURI;

        public static Document Instance { get { if (instance == null) instance = new Document(); return instance; } set => instance = value; }

        internal static DrawType DrawType { get => drawType; set => drawType = value; }

        public SetUpTextBox SetUpTextBox { get => setUpTextBox; set => setUpTextBox = value; }

        /// <summary>
        /// Khởi tạo mặc định
        /// </summary>
        public Document()
        {
            InkCanvas newCanvas = new InkCanvas();
            slides = new ObservableCollection<InkCanvas>();
            thumbs = new ObservableCollection<System.Windows.Controls.Image>();
            slides.Add(newCanvas);
            thumbs.Add(CanvasToImage(newCanvas));
            DrawType = DrawType.nothing;
        }

        /// <summary>
        /// Khởi tạo tài liệu với 1 slide
        /// </summary>
        public Document(InkCanvas c)
        {
            slides = new ObservableCollection<InkCanvas>();
            thumbs = new ObservableCollection<System.Windows.Controls.Image>();
            DrawType = DrawType.nothing;
            canvas = c;
            slides.Add(c);
            AddThumb(CanvasToImage(c));
        }

        /// <summary>
        /// Khởi tạo tài liệu với 1 danh sách slide
        /// </summary>
        /// <param name="l">Danh sách slide</param>
        public Document(ObservableCollection<InkCanvas> l)
        {
            slides = l;
            thumbs = new ObservableCollection<System.Windows.Controls.Image>();
            canvas = slides.First<InkCanvas>();
            DrawType = DrawType.nothing;
            foreach (InkCanvas item in l)
            {
                AddThumb(CanvasToImage(item));
            }
        }

        /// <summary>
        /// Xóa, làm sạch tài liệu
        /// </summary>
        private void Clear()
        {
            slides.Clear();
            canvas = null;
            thumbs.Clear();
            DrawType = DrawType.nothing;
        }

        /// <summary>
        /// Thêm slide mới từ 1 inkcanvas
        /// </summary>
        public void AddSlide(InkCanvas c)
        {
            slides.Add(c);
            canvas = c;
            AddThumb(CanvasToImage(c));
        }

        /// <summary>
        /// Xóa slide
        /// </summary>
        /// <param name="index">Vị trí slide cần xóa</param>
        public void DelSlide(int index)
        {
            if (slides.Count <= 1) return;
            if (index < 0 || index >= slides.Count) return;
            slides.RemoveAt(index);
            if (index != slides.Count)
                canvas = slides[index];
            else canvas = slides[index - 1];
            UpdateThumbs();
        }

        /// <summary>
        /// Cập nhật lại hình thu nhỏ
        /// </summary>
        public void UpdateThumbs()
        {
            thumbs.Clear();
            foreach (InkCanvas item in slides)
            {
                AddThumb(CanvasToImage(item));
            }
        }

        /// <summary>
        /// Thêm hình thu nhỏ vào list
        /// </summary>
        /// <param name="thumb">Hình thu nhỏ</param>
        public void AddThumb(System.Windows.Controls.Image thumb)
        {
            if (thumb.Source == null) //Nếu như hình thu nhỏ chưa vẽ được thì tạo 1 hình default
            {
                thumb.Width = 100;
                thumb.Height = 75;
                thumb.Source = new BitmapImage(new Uri("Images/DefaultImage.png", UriKind.Relative));
            }
            thumbs.Add(thumb);
        }

        /// <summary>
        /// Vẽ đường nét đứt mẫu hình khi đang kéo hình
        /// </summary>
        /// <param name="shape">Hình</param>
        public void DrawCapture(Shape shape)
        {
            double[] dashes = { 2, 2 };
            shape.StrokeDashArray = new System.Windows.Media.DoubleCollection(dashes);

            canvas.Children.Add(shape);
        }

        /// <summary>
        /// Vẽ hình tùy theo outline
        /// </summary>
        /// <param name="control"></param>
        /// <param name="outline"></param>
        public void DrawShape(ContentControl control, int outline)
        {
            canvas.Children.RemoveAt(canvas.Children.Count - 1);// Xóa capture
            RefreshCanvas();
            if (outline == 1)
            {
                ((Shape)control.Content).StrokeDashArray = null;
            }
            else if (outline == 2)
            {
                double[] dashes = { 4, 4 };
                ((Shape)control.Content).StrokeDashArray = new System.Windows.Media.DoubleCollection(dashes);
            }
            else
            {
                double[] dashes = { 4, 1, 4, 1 };
                //shape.SnapsToDevicePixels = true;
                ((Shape)control.Content).StrokeDashArray = new System.Windows.Media.DoubleCollection(dashes);
            }
            //
            canvas.Children.Add(control);

        }

        public void DrawShape(Shape shape, int outline)
        {
            canvas.Children.RemoveAt(canvas.Children.Count - 1);
            RefreshCanvas();
            if (outline == 1)
            {
                shape.StrokeDashArray = null;
            }
            else if (outline == 2)
            {
                double[] dashes = { 4, 4 };
                shape.StrokeDashArray = new System.Windows.Media.DoubleCollection(dashes);
            }
            else
            {
                double[] dashes = { 4, 1, 4, 1 };
                //shape.SnapsToDevicePixels = true;
                shape.StrokeDashArray = new System.Windows.Media.DoubleCollection(dashes);
            }

            canvas.Children.Add(shape);

        }

        public void RemoveShape(ContentControl shape)
        {
            canvas.Children.Remove(shape);
        }

        public void NewFile()
        {
            Instance.Clear();

            InkCanvas inkCanvas = new InkCanvas();
            inkCanvas.Width = 800;
            inkCanvas.Height = 600;
            inkCanvas.EditingMode = InkCanvasEditingMode.Select;
            inkCanvas.AllowDrop = true;
            inkCanvas.Background = System.Windows.Media.Brushes.WhiteSmoke;

            AddSlide(inkCanvas);
            UpdateThumbs();
        }

        /// <summary>
        /// Mở file
        /// </summary>
        /// <returns>Trả về đường dẫn file mở để lưu lại</returns>
        public string OpenFile()
        {
            Instance.Clear();
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Choose an YSlidy file";
            dlg.Filter = "YSlidy File (*.ys*)|*.ys*";
            try
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    List<string> stringlist = File.ReadAllLines(dlg.FileName).OfType<string>().ToList();
                    foreach (String item in stringlist)
                    {
                        InkCanvas deepCopy = System.Windows.Markup.XamlReader.Parse(item) as InkCanvas;
                        int n = deepCopy.Children.Count;
                        for(int i = 0;i<n;i++)
                        {
                            if(deepCopy.Children[i].GetType() == typeof(ContentControl))
                            {
                                if((deepCopy.Children[i] as ContentControl).Content.GetType() == typeof(CustomVideo))
                                {
                                    var oldvideo = (deepCopy.Children[i] as ContentControl).Content as CustomVideo;
                                    var c = new CustomVideo();
                                    c.Source = oldvideo.Source;
                                    (deepCopy.Children[i] as ContentControl).Content = c;
                                }
                            }
                        }
                        AddSlide(deepCopy);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error: Could not read file from disk.\nOriginal error: " + ex.Message);
            }

            UpdateThumbs();
            return dlg.FileName;
        }


        public void SaveFile(string path = null)
        {
            if (path == null)
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.Title = "Save as";
                dlg.Filter = "YSlidy files (*.ys)|*.ys|All files (*.*)|*.*";
                try
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        path = dlg.FileName;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Error: Could not read file from disk.\nOriginal error: " + ex.Message);
                }
            }

            List<string> stringlist = new List<string>();

            foreach (InkCanvas item in Instance.slides)
            {
                var xaml = System.Windows.Markup.XamlWriter.Save(item);
                stringlist.Add(xaml.ToString());
            }
            if(path != null)
                File.WriteAllLines(path, stringlist);
        }

        private string CreateTempFile()
        {
            string fileName = string.Empty;

            try
            {
                fileName = System.IO.Path.GetTempFileName();

                // Create a FileInfo object to set the file's attributes
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);

                // Set the Attribute property of this file to Temporary. 
                fileInfo.Attributes = System.IO.FileAttributes.Temporary;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to create tempfile\nDetail: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return fileName;
        }

        public UIElement GetHitElement(System.Drawing.Point hittedPoint)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            Shape temp;
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                temp = (Shape)canvas.Children[i];
                System.Drawing.Size childSize = new System.Drawing.Size((int)temp.Width, (int)temp.Height);
                System.Drawing.Point childPos = new System.Drawing.Point();
                childPos.X = (int)canvas.Children[i].TranslatePoint(new System.Windows.Point(0, 0), canvas).X;
                childPos.Y = (int)canvas.Children[i].TranslatePoint(new System.Windows.Point(0, 0), canvas).Y;
                path.AddRectangle(new System.Drawing.Rectangle(childPos, childSize));
                if (path.IsVisible(hittedPoint))
                {
                    return canvas.Children[i];
                }
            }
            return null;
        }

        public void Fill(UIElement shape, System.Windows.Media.Color fillColor)
        {
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i] == shape)
                {
                    Shape temp = (Shape)canvas.Children[i];
                    temp.Fill = new SolidColorBrush(fillColor);
                }
            }
        }

        public void Insert(ContentControl control)
        {
            canvas.Children.Add(control);
        }

        public void InsertText(TextBox textBox)
        {
            canvas.Children.Add(textBox);
        }

        public void SetLocationInCanvas(UIElement element, InkCanvas curCanvas)
        {
            double x = InkCanvas.GetLeft(element);
            if (x > curCanvas.Width - element.RenderSize.Width)
                InkCanvas.SetLeft(element, curCanvas.Width - element.RenderSize.Width);
            else if (x < 0)
                InkCanvas.SetLeft(element, 0);

            x = InkCanvas.GetTop(element);
            if (x > curCanvas.Height - element.RenderSize.Height)
                InkCanvas.SetTop(element, curCanvas.Height - element.RenderSize.Height);
            else if (x < 0)
                InkCanvas.SetTop(element, 0);
        }

        public void UpdateSetUpTextBox(TextBox t)
        {
            setUpTextBox.FontFamily = t.FontFamily;
            setUpTextBox.FontSize = t.FontSize;
            setUpTextBox.FontStyle = t.FontStyle;
            setUpTextBox.FontWeight = t.FontWeight;
            setUpTextBox.Foreground = t.Foreground;
        }

        public void FloodFill(System.Drawing.Bitmap bm, System.Drawing.Point p, System.Drawing.Color Color)
        {

            Stack<System.Drawing.Point> S = new Stack<System.Drawing.Point>();
            System.Drawing.Color OriColor = bm.GetPixel(p.X, p.Y);
            bm.SetPixel(p.X, p.Y, Color);
            S.Push(p);
            while (S.Count != 0)
            {
                p = S.Pop();
                if ((p.X - 1 >= 0) && SameColor(OriColor, bm.GetPixel(p.X - 1, p.Y)))
                {
                    bm.SetPixel(p.X - 1, p.Y, Color);
                    S.Push(new System.Drawing.Point(p.X - 1, p.Y));
                }
                if ((p.X + 1 < bm.Width) && SameColor(OriColor, bm.GetPixel(p.X + 1, p.Y)))
                {
                    bm.SetPixel(p.X + 1, p.Y, Color);
                    S.Push(new System.Drawing.Point(p.X + 1, p.Y));
                }
                if ((p.Y - 1 >= 0) && SameColor(OriColor, bm.GetPixel(p.X, p.Y - 1)))
                {
                    bm.SetPixel(p.X, p.Y - 1, Color);
                    S.Push(new System.Drawing.Point(p.X, p.Y - 1));
                }
                if ((p.Y + 1 < bm.Height) && SameColor(OriColor, bm.GetPixel(p.X, p.Y + 1)))
                {
                    bm.SetPixel(p.X, p.Y + 1, Color);
                    S.Push(new System.Drawing.Point(p.X, p.Y + 1));
                }
            }
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Width = canvas.ActualWidth;
            img.Height = canvas.ActualHeight;
            img.Source = BitmapToImageSource(bm);
            canvas.Children.Clear();
            canvas.Children.Add(img);
        }

        private ImageSource BitmapToImageSource(Bitmap bm)
        {
            if (bm == null) return null;
            System.Windows.Media.Imaging.BitmapSource b = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bm.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bm.Width, bm.Height));
            return b;
        }

        private bool SameColor(System.Drawing.Color c1, System.Drawing.Color c2)
        {
            return ((c1.A == c2.A) && (c1.B == c2.B) && (c1.G == c2.G) && (c1.R == c2.R));
        }

        public Bitmap CanvasToBitmap(InkCanvas cv)
        {
            if (cv.ActualHeight == 0) return null;
            Bitmap bm;
            Rect bounds = VisualTreeHelper.GetDescendantBounds(cv);
            double dpi = 96d;
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, System.Windows.Media.PixelFormats.Default);


            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(cv);
                dc.DrawRectangle(vb, null, new Rect(new System.Windows.Point(), bounds.Size));
            }
            renderBitmap.Render(dv);

            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            encoder.Save(stream);
            bm = new System.Drawing.Bitmap(stream);
            return bm;
        }

        public void RefreshCanvas()
        {
            //System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            //img.Width = canvas.ActualWidth;
            //img.Height = canvas.ActualHeight;
            //img.Source = BitmapToImageSource(CanvasToBitmap(canvas));
            //canvas.Children.Clear();
            //canvas.Children.Add(img);
        }

        private System.Windows.Controls.Image CanvasToImage(InkCanvas c)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Width = c.ActualWidth;
            img.Height = c.ActualHeight;
            img.Source = BitmapToImageSource(CanvasToBitmap(c));
            return img;
        }

        /// <summary>
        /// Parameter equal true to add, false to remove
        /// </summary>
        private void NumberSlide(bool addOrRemove)
        {
            if (addOrRemove)
            {
                int i = 1;
                foreach (InkCanvas canvas in slides)
                {
                    bool itHave = false;
                    foreach (UIElement ui in canvas.Children)
                    {
                        if (ui.GetType() == typeof(TextBox) && (ui as TextBox).Name == "NumberOfSlide")
                        {
                            itHave = true;
                            break;
                        }
                    }
                    if (!itHave)
                    {
                        TextBox txt = new TextBox();
                        txt.Name = "NumberOfSlide";
                        txt.Text = i.ToString();
                        txt.TextWrapping = TextWrapping.NoWrap;
                        txt.Height = 20;
                        txt.Width = canvas.Width;
                        txt.IsReadOnly = true;
                        txt.FontFamily = new System.Windows.Media.FontFamily("Arial");
                        txt.FontSize = 12;
                        txt.TextAlignment = TextAlignment.Center;
                        txt.Foreground = System.Windows.Media.Brushes.Gray;
                        txt.Background = System.Windows.Media.Brushes.Transparent;
                        txt.BorderBrush = System.Windows.Media.Brushes.Transparent;
                        txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                        InkCanvas.SetLeft(txt, 0);
                        InkCanvas.SetTop(txt, 0);
                        canvas.Children.Add(txt);
                    }
                    i++;
                }
            }
            else
            {
                foreach (InkCanvas canvas in slides)
                {
                    foreach (UIElement ui in canvas.Children)
                    {
                        if (ui.GetType() == typeof(TextBox) && (ui as TextBox).Name == "NumberOfSlide")
                        {
                            canvas.Children.Remove(ui);
                            break;
                        }
                    }
                }
            }
        }
    }

    public class SetUpTextBox
    {
        private System.Windows.Media.FontFamily fontFamily;
        private double fontSize;
        private System.Windows.FontStyle fontStyle;
        private FontWeight fontWeight;
        private System.Windows.TextAlignment textAlignment;
        private System.Windows.Media.Brush foreground;
        private TextDecorationCollection textDecorations;
        private VerticalAlignment verticalContentAlignment;

        public System.Windows.Media.FontFamily FontFamily { get => fontFamily; set => fontFamily = value; }
        public double FontSize { get => fontSize; set => fontSize = value; }
        public System.Windows.FontStyle FontStyle { get => fontStyle; set => fontStyle = value; }
        public FontWeight FontWeight { get => fontWeight; set => fontWeight = value; }
        public TextAlignment TextAlignment { get => textAlignment; set => textAlignment = value; }
        public System.Windows.Media.Brush Foreground { get => foreground; set => foreground = value; }
        public TextDecorationCollection TextDecorations { get { if (textDecorations == null) textDecorations = new TextDecorationCollection(); return textDecorations; } set => textDecorations = value; }
        public VerticalAlignment VerticalContentAlignment { get => verticalContentAlignment; set => verticalContentAlignment = value; }

        public SetUpTextBox()
        {
            fontFamily = new System.Windows.Media.FontFamily("Arial");
            fontSize = 14;
            fontStyle = FontStyles.Normal;
            fontWeight = FontWeights.Normal;
            textAlignment = TextAlignment.Left;
            foreground = System.Windows.Media.Brushes.Black;
            textDecorations = System.Windows.TextDecorations.Underline;
            verticalContentAlignment = VerticalAlignment.Top;
        }
    }
}