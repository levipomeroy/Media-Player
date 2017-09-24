using Microsoft.Win32;
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
using System.Timers;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Reflection;

namespace Media_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer timer;
        private System.Windows.Threading.DispatcherTimer restingTimer;
        private string m_prevSize = "Normal";

        bool m_fullScreen = false;
        bool m_mediaIsPlaying = false;
        int restedTime = 0;
        bool m_isMuted = false;
        double m_speed = 1;
        int m_PicIndex = 0;
        FileInfo[] Files;
        string m_FolderName;
        bool m_IsPicture = false;


        public MainWindow()
        {
            InitializeComponent();
         
            VolumeSlider.Value = VolumeSlider.Maximum/ 2;
            MediaView.Volume = (VolumeSlider.Value/100);

            PlayOrPause.Background = new ImageBrush(PlayPic.Source);
            SkipBack.Background = new ImageBrush(BackPic.Source);
            SkipFoward.Background = new ImageBrush(FowardPic.Source);
            FullScreenBtn.Background = new ImageBrush(FullscreenPic.Source);
            muteBtn.Background = new ImageBrush(MutePic.Source);
            PrevPic.Background = new ImageBrush(LeftPic.Source);
            NextPic.Background = new ImageBrush(RightPic.Source);
            RotateBtn.Background = new ImageBrush(RotatePic.Source);


            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);

            restingTimer = new System.Windows.Threading.DispatcherTimer();
            restingTimer.Tick += new EventHandler(restingTimer_Tick);
            restingTimer.Interval = new TimeSpan(0, 0, 1);

            if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                MediaView.Source = new Uri(Application.Current.Properties["ArbitraryArgName"].ToString());
                MediaView.Play();
                PlayOrPause.Background = new ImageBrush(PausePic.Source);
                PlayOrPause.ToolTip = "Pause";
            }
        }

        private void restingTimer_Tick(object sender, EventArgs e)
        {
            restedTime++;
            if(restedTime >= 5)
            {
                restingTimer.Stop();
                ControlBar.Opacity = 0;
                this.Cursor = Cursors.None;
                restedTime = 0;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            string hours;
            string minutes;
            string seconds;

            timprogress.Value = MediaView.Position.TotalSeconds;


            if (MediaView.Position.TotalHours < 10)
            {
                hours = "0" + (int)MediaView.Position.TotalHours;
            }
            else
            {
                hours = ((int)MediaView.Position.TotalHours).ToString();
            }
            if (MediaView.Position.Minutes < 10)
            {
                minutes = "0" + (int)MediaView.Position.Minutes;
            }
            else
            {
                minutes = ((int)MediaView.Position.Minutes).ToString();
            }
            if (MediaView.Position.Seconds < 10)
            {
                seconds = "0" + (int)MediaView.Position.Seconds;
            }
            else
            {
                seconds = ((int)MediaView.Position.Seconds).ToString();
            }

            CurrentTime.Content = string.Format("{0}:{1}:{2}", hours, minutes, seconds);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();  // Create OpenFileDialog 

            dlg.Filter = "(All files (*.*)|*.*";    // Set filter for file extension

            Nullable<bool> result = dlg.ShowDialog();   // Display OpenFileDialog by calling ShowDialog method 

            if (result == true)
            {
                string filename = dlg.FileName;         //Get filename

                m_FolderName = new FileInfo(filename).Directory.FullName;

                DirectoryInfo d = new DirectoryInfo(m_FolderName);//Assuming Test is your Folder
                Files = d.GetFiles("*.jpg"); //Getting Text files

                if (Files.Length > 0)
                {
                    while (m_FolderName + "\\" + Files[m_PicIndex].ToString() != filename)
                    {
                        m_PicIndex++;
                    }
                }

                MediaView.Source = new Uri(filename);   //Open file in mediaElement
                MediaView.Play();
                m_mediaIsPlaying = true;

                PlayOrPause.Background = new ImageBrush(PausePic.Source);

                timer.Start();
                restingTimer.Start();
            }
        }

        private void PlayOrPause_Click(object sender, RoutedEventArgs e)
        {
            if (m_mediaIsPlaying)
            {
                MediaView.Pause();
                MusicElement.Pause();
                MediaView.SpeedRatio = m_speed;
                m_mediaIsPlaying = false;
                PlayOrPause.ToolTip = "Play";

                PlayOrPause.Background = new ImageBrush(PlayPic.Source);
                timer.Stop();
            }
            else
            {
                MediaView.Play();
                if (MediaView.Position < TimeSpan.FromSeconds(timprogress.Maximum))
                {
                    MusicElement.Play();
                }
                MediaView.SpeedRatio = m_speed;
                m_mediaIsPlaying = true;
                PlayOrPause.ToolTip = "Pause";

                PlayOrPause.Background = new ImageBrush(PausePic.Source);
                timer.Start();
                MediaView.SpeedRatio = m_speed;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaView.Volume = (VolumeSlider.Value /100);
        }

        private void MediaView_MediaOpened(object sender, RoutedEventArgs e)
        {
            string hours;
            string minutes;
            string seconds;

            if (MediaView.HasVideo && MediaView.HasAudio) //Its a video
            {
                MusicElement.Visibility = Visibility.Hidden;
                MusicElement.Stop();
                m_IsPicture = false;

                ControlBar.Visibility = Visibility.Visible;
                PicControl.Visibility = Visibility.Hidden;
            }
            else if (!MediaView.HasVideo && MediaView.HasAudio)  //Its a song
            {
                MusicElement.Visibility = Visibility.Visible;
                MusicElement.Play();
                m_IsPicture = false;

                ControlBar.Visibility = Visibility.Visible;
                PicControl.Visibility = Visibility.Hidden;
            }
            else    //Its an image
            {
                m_IsPicture = true;
                MusicElement.Visibility = Visibility.Hidden;
                MusicElement.Stop();
                ControlBar.Visibility = Visibility.Hidden;
                PicControl.Visibility = Visibility.Visible;
            }
            rotater.Angle = 0;

            if (MediaView.NaturalDuration.HasTimeSpan)
            {
                if(MediaView.NaturalDuration.TimeSpan.TotalHours < 10)
                {
                    hours = "0" + (int)MediaView.NaturalDuration.TimeSpan.TotalHours;
                }
                else
                {
                    hours = ((int)MediaView.NaturalDuration.TimeSpan.TotalHours).ToString();
                }
                if(MediaView.NaturalDuration.TimeSpan.Minutes < 10)
                {
                    minutes = "0" + (int)MediaView.NaturalDuration.TimeSpan.Minutes;
                }
                else
                {
                    minutes = ((int)MediaView.NaturalDuration.TimeSpan.Minutes).ToString();
                }
                if(MediaView.NaturalDuration.TimeSpan.Seconds < 10)
                {
                    seconds = "0" + (int)MediaView.NaturalDuration.TimeSpan.Seconds;
                }
                else
                {
                    seconds = ((int)MediaView.NaturalDuration.TimeSpan.Seconds).ToString();
                }

                TotalTime.Content = string.Format("{0}:{1}:{2}", hours, minutes, seconds);

                timprogress.Maximum = MediaView.NaturalDuration.TimeSpan.TotalSeconds;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {

            ControlBar.Opacity = 1;
            this.Cursor = Cursors.Arrow;
            restingTimer.Start();
            restedTime = 0;
        }

        private void SkipFoward_Click(object sender, RoutedEventArgs e)
        {
            MediaView.Position += TimeSpan.FromSeconds(30);
        }

        private void SkipBack_Click(object sender, RoutedEventArgs e)
        {
            MediaView.Position -= TimeSpan.FromSeconds(30);
            if(MediaView.Position < TimeSpan.FromSeconds(timprogress.Maximum))
            {
                MusicElement.Play();
            }
        }

        private void timprogress_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MediaView.Position = TimeSpan.FromSeconds(timprogress.Value);
            MusicElement.Play();
        }

        private void FullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!m_fullScreen)
            {
                layoutRoot.Children.Remove(MyMenu);
                FullScreenBtn.ToolTip = "Exit fullscreen";

                if (this.WindowState == WindowState.Normal)
                {
                    m_prevSize = "Normal";
                }
                else if (this.WindowState == WindowState.Maximized)
                {
                    m_prevSize = "Max";
                }

                if(this.WindowState == WindowState.Maximized)   //This is dumb but fixes error
                {
                    this.WindowState = WindowState.Minimized;
                }

                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.ResizeMode = ResizeMode.NoResize;
                MediaView.Position = TimeSpan.FromSeconds(timprogress.Value);

                m_fullScreen = true;
                FullScreenBtn.Background = new ImageBrush(UnscreenPic.Source);
            }
            else
            {
                FullScreenBtn.Background = new ImageBrush(FullscreenPic.Source);

                FullScreenBtn.ToolTip = "Fullscreen";

                this.Content = layoutRoot;
                layoutRoot.Children.Add(MyMenu);
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                if (m_prevSize == "Normal")
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                }
                this.ResizeMode = ResizeMode.CanResize;
                MediaView.Position = TimeSpan.FromSeconds(timprogress.Value);
                m_fullScreen = false;
                this.Cursor = Cursors.Arrow;
            }
        }

        private void myWindow_StateChanged(object sender, EventArgs e)
        {
            if (myWindow.WindowState == WindowState.Maximized)
            {
                timprogress.Width = 1000;

                SkipBack.Margin = new Thickness(550, 0, 0, 0);
                muteBtn.Margin = new Thickness(300, 0, 5, 0);
            }
            else
            {
                SkipBack.Margin = new Thickness(300, 0, 0, 0);
                muteBtn.Margin = new Thickness(90, 0, 0, 0);
                timprogress.Width = 500;
            }
        }

        private void uniform_Click(object sender, RoutedEventArgs e)
        {
            uniform.IsChecked = false;
            fill.IsChecked = false;
            toFill.IsChecked = false;

            MenuItem ViewOption = sender as MenuItem;
            ViewOption.IsChecked = true;

            switch (ViewOption.Header.ToString())
            {
                case "Normal":
                    MediaView.Stretch = Stretch.Uniform;
                    break;
                case "Stretch":
                    MediaView.Stretch = Stretch.Fill;
                    break;
                case "Super Stretch":
                    MediaView.Stretch = Stretch.UniformToFill;
                    break;
            }
        }

        private void muteBtn_Click(object sender, RoutedEventArgs e)
        {
            if(!m_isMuted)
            {
                m_isMuted = true;
                MediaView.Volume = 0;

                muteBtn.Background = new ImageBrush(MutePic.Source);

                muteBtn.ToolTip = "Unmute";
            }
            else
            {
                m_isMuted = false;
                MediaView.Volume = VolumeSlider.Value / 100;

                muteBtn.Background = new ImageBrush(UnMutePic.Source);


                muteBtn.ToolTip = "Mute";
            }
        }

        private void Speed_Click(object sender, RoutedEventArgs e)
        {
            Normal.IsChecked = false;
            Fast.IsChecked = false;
            Faster.IsChecked = false;
            Slow.IsChecked = false;

            MenuItem speed = sender as MenuItem;
            speed.IsChecked = true;

            switch (speed.Name)
            {
                case "Normal":
                    MediaView.SpeedRatio = 1;
                    m_speed = 1;
                    break;
                case "Fast":
                    MediaView.SpeedRatio = 1.5;
                    m_speed = 1.5;
                    break;
                case "Faster":
                    MediaView.SpeedRatio = 2.5;
                    m_speed = 2.5;
                    break;
                case "Slow":
                    MediaView.SpeedRatio = .5;
                    m_speed = .5;
                    break;
            }
        }

        private void myWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                PlayOrPause_Click(PlayOrPause, e);
                e.Handled = true;
            }
            else if(e.Key == Key.Left && m_IsPicture)
            {
                PrevPic_Click(PrevPic, e);
            }
            else if(e.Key == Key.Right && m_IsPicture)
            {
                NextPic_Click(NextPic, e);
            }
        }

        private void MusicElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MusicElement.Position = TimeSpan.FromSeconds(1);
            MusicElement.Play();
        }

        private void MediaView_MediaEnded(object sender, RoutedEventArgs e)
        {
            MusicElement.Pause();
        }

        private void NextPic_Click(object sender, RoutedEventArgs e)
        {
            if (m_PicIndex +1 < Files.Length)
            {
                m_PicIndex++;
                MediaView.Source = new Uri(m_FolderName + "\\" + Files[m_PicIndex].ToString());
            }
            else
            {
                m_PicIndex = 0;
                MediaView.Source = new Uri(m_FolderName + "\\" + Files[m_PicIndex].ToString());

            }
        }

        private void PrevPic_Click(object sender, RoutedEventArgs e)
        {
            if (m_PicIndex - 1 >= 0)
            {
                m_PicIndex--;
                MediaView.Source = new Uri(m_FolderName + "\\" + Files[m_PicIndex].ToString());
            }
            else
            {
                m_PicIndex = Files.Length -1;
                MediaView.Source = new Uri(m_FolderName + "\\" + Files[m_PicIndex].ToString());
            }
        }

        private void RotateBtn_Click(object sender, RoutedEventArgs e)
        {
            rotater.Angle += 90;
        }

    }
}
