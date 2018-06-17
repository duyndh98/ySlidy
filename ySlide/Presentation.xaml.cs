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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ySlidy
{
    /// <summary>
    /// Interaction logic for Presentation.xaml
    /// </summary>
    public partial class Presentation : Window
    {
        List<InkCanvas> slides;
        InkCanvas curCanvas;
        int curIndex;
        int numberOfVideos = 0;
        int numberOfVideosPlayed = 0;

        public Presentation()
        {
            InitializeComponent();

            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

        /// <summary>
        /// Khởi tạo và truyền vào list slides, cùng với slide bắt đầu trình chiếu
        /// </summary>
        public Presentation(ObservableCollection<InkCanvas> listslide, int startSlide) : this()
        {
            slides = new List<InkCanvas>(listslide);
            curIndex = startSlide;
            //curCanvas = System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(slides[startSlide])) as InkCanvas;
            //MainGrid.Children.Add(curCanvas);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeCanvas(slides[curIndex]);

            var scaleTime = MainGrid.ActualHeight / 600;
            ScaleTransform scale = new ScaleTransform(scaleTime, scaleTime);
            displayCanVas.RenderTransform = scale;
        }

        void ChangeCanvas(InkCanvas a)
        {
            var xaml = System.Windows.Markup.XamlWriter.Save(a);
            curCanvas = System.Windows.Markup.XamlReader.Parse(xaml) as InkCanvas;

            foreach(UIElement ui in curCanvas.Children)
            {
                if (ui.GetType() == typeof(ContentControl))
                {
                    if ((ui as ContentControl).Content.GetType() == typeof(CustomVideo))
                    {
                        var oldvideo = (ui as ContentControl).Content as CustomVideo;
                        var c = new CustomVideo();
                        c.Source = oldvideo.Source;
                        (ui as ContentControl).Content = c;
                    }
                }
            }
            curCanvas.EditingMode = InkCanvasEditingMode.None;
            var scaleTime = MainGrid.ActualHeight / 600;
            ScaleTransform scale = new ScaleTransform(scaleTime, scaleTime);
            curCanvas.RenderTransformOrigin = new Point(0.5, 0.5);
            curCanvas.RenderTransform = scale;
            curCanvas.HorizontalAlignment = HorizontalAlignment.Center;
            curCanvas.VerticalAlignment = VerticalAlignment.Center;
            numberOfVideosPlayed = 0;
            numberOfVideos = 0;
            MainGrid.Children.Clear();
            MainGrid.Children.Add(curCanvas);
            foreach (UIElement item in curCanvas.Children)
            {
                if (item.GetType() == typeof(ContentControl))
                {
                    if ((item as ContentControl).Content.GetType() == typeof(CustomVideo))
                    {
                        numberOfVideos++;
                    }
                }
            }
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (numberOfVideos == numberOfVideosPlayed)
                {
                    if (curIndex < slides.Count - 1)
                    {
                        curIndex++;
                        ChangeCanvas(slides[curIndex]);
                    }
                    else
                        Close();
                }
                else
                {
                    int i = 0;
                    foreach (UIElement item in curCanvas.Children)
                    {
                        if (item.GetType() == typeof(ContentControl))
                        {
                            if ((item as ContentControl).Content.GetType() == typeof(CustomVideo))
                            {
                                i++;
                                if (numberOfVideosPlayed < i)
                                {
                                    ((((item as ContentControl).Content as CustomVideo).Content as Canvas).Children[0] as MediaElement).Play();
                                    numberOfVideosPlayed++;
                                    return;
                                }
                                else
                                {
                                    ((((item as ContentControl).Content as CustomVideo).Content as Canvas).Children[0] as MediaElement).Pause();
                                }
                            }
                        }
                    }
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (curIndex > 0)
                {
                    curIndex--;
                    ChangeCanvas(slides[curIndex]);
                }
                else
                    Close();
            }
        }
    }
}
