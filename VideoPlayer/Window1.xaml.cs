using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AXVLC;

namespace VideoPlayer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private int Volume { get; set; }
        private bool _isPlaying;
        private bool IsPlaying 
        { 
            get {return _isPlaying;}
            set 
            {
                _isPlaying = value;
                if (value == false)
                {
                    play.Content = "Play";
                }
                else
                {
                    play.Content = "Pause";
                }
            } 
        }
        bool IsOpened { get; set; }

        public Window1()
        {
            InitializeComponent();
            axVLC.CreateControl();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            axVLC.Visible = true;
            Volume = 50;
            IsOpened = false;
            IsPlaying = false;
           
            
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            if (IsOpened)
            {
                if (!IsPlaying)
                {


                    axVLC.Volume = Volume;
                    axVLC.playlist.togglePause();
                    IsPlaying = true;

                }
                else
                {
                    axVLC.playlist.togglePause();
                    IsPlaying = false;
                } 
            }

        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = Convert.ToInt32(e.NewValue * 10);
            if (IsOpened) axVLC.Volume = Volume;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            if (IsOpened)
            {
                axVLC.playlist.stop();
                IsPlaying = false;
                IsOpened = false;
            }
        }

        private void open_Click(object sender, RoutedEventArgs e)
        {
            int id = axVLC.playlist.add(@"C:/test.avi", "track1", new String[] { });

            axVLC.playlist.playItem(id);

            
            IsPlaying = true;
            IsOpened = true;

        }
    }
}
