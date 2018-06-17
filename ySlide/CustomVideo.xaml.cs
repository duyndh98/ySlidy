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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ySlidy
{
    /// <summary>
    /// Interaction logic for CustomVideo.xaml
    /// </summary>
    public partial class CustomVideo : UserControl
    {
        public string Source
        {
            get
            {
                return GetValue(SourceProperty).ToString();
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }
        
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(CustomVideo), new UIPropertyMetadata("Đường dẫn file", SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CustomVideo customVideo = obj as CustomVideo;
            Uri uri;
            if (Uri.TryCreate(e.NewValue.ToString(), UriKind.Absolute, out uri))
            {
                ((customVideo.Content as Canvas).Children[0] as MediaElement).Source = uri;
            }
        }

        private TimeSpan timeRunning = TimeSpan.FromSeconds(0);
        private bool isPlaying = false;

        public CustomVideo()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //if (MediaPlayer.Source != null)
            //{
            //    if (MediaPlayer.NaturalDuration.HasTimeSpan)
            //        label.Content = String.Format("{0} / {1}", MediaPlayer.Position.ToString(@"mm\:ss"), MediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
            //}
            //else label.Content = "No file selected...";
        }

        public void IsPlaying(bool flag)
        {
            //isPlaying = flag;
            //if (flag == false)
            //{
            //    timeRunning = TimeSpan.FromSeconds(0);
            //    btnPlay.Opacity = 1;
            //    MediaPlayer.Pause();
            //}
            //else
            //{
            //    MediaPlayer.Play();
            //    btnPlay.Opacity = 0;
            //}
        }

        public void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isPlaying)
            {
                isPlaying = true;
                ((sender as Canvas).Children[0] as MediaElement).Pause();
            }
            else
            {
                isPlaying = false;
                ((sender as Canvas).Children[0] as MediaElement).Play();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var canvas = this.Content as Canvas;
            var media = canvas.Children[0] as MediaElement;

            media.Height = canvas.ActualHeight;
            media.Width = canvas.ActualWidth;
        }

        //public void btnStop_Click(object sender, RoutedEventArgs e)
        //{
        //    MediaPlayer.Position = TimeSpan.FromSeconds(0);
        //    IsPlaying(false);
        //}

        //public void btnMoveBack_Click(object sender, RoutedEventArgs e)
        //{
        //    MediaPlayer.Position -= TimeSpan.FromSeconds(10);
        //}

        //public void btnMoveForward_Click(object sender, RoutedEventArgs e)
        //{
        //    MediaPlayer.Position += TimeSpan.FromSeconds(10);
        //}

        //public void btnOpen_Click(object sender, RoutedEventArgs e)
        //{
        //    // Configure open file dialog box 
        //    Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
        //    dialog.FileName = "Videos"; // Default file name 
        //    dialog.DefaultExt = ".WMV"; // Default file extension 
        //    dialog.Filter = "MP4 Files (*.mp4)|*.mp4"; // Filter files by extension  

        //    // Show open file dialog box 
        //    Nullable<bool> result = dialog.ShowDialog();

        //    // Process open file dialog box results  
        //    if (result == true)
        //    {
        //        // Open document  
        //        MediaPlayer.Source = new Uri(dialog.FileName);
        //        btnPlay.IsEnabled = true;
        //    }
        //}
    }
}
