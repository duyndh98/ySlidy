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

namespace ySlide
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
        
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(CustomVideo), new UIPropertyMetadata("00:00", SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CustomVideo customVideo = obj as CustomVideo;
            Uri uri;
            if (Uri.TryCreate(e.NewValue.ToString(), UriKind.Absolute, out uri))
            {
                customVideo.MediaPlayer.Source = uri;
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

            btnPlay.Background = Brushes.Transparent;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (MediaPlayer.Source != null)
            {
                if (MediaPlayer.NaturalDuration.HasTimeSpan)
                    label.Content = String.Format("{0} / {1}", MediaPlayer.Position.ToString(@"mm\:ss"), MediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
            }
            else label.Content = "No file selected...";
        }

        private void IsPlaying(bool flag)
        {
            isPlaying = flag;
            if (flag == false)
            {
                timeRunning = TimeSpan.FromSeconds(0);
                btnPlay.Opacity = 1;
                MediaPlayer.Pause();
            }
            else
            {
                MediaPlayer.Play();
                btnPlay.Opacity = 0;
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlaying)
            {
                IsPlaying(true);
            }
            else {
                IsPlaying(false);
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isPlaying)
            {
                IsPlaying(true);
            }
            else
            {
                IsPlaying(false);
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position = TimeSpan.FromSeconds(0);
            IsPlaying(false);
        }

        private void btnMoveBack_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position -= TimeSpan.FromSeconds(10);
        }

        private void btnMoveForward_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position += TimeSpan.FromSeconds(10);
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
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
                // Open document  
                MediaPlayer.Source = new Uri(dialog.FileName);
                btnPlay.IsEnabled = true;
            }
        }
    }
}
