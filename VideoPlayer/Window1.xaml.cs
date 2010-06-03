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
using Microsoft.Win32;
using System.Threading;

namespace VideoPlayer {
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window {
		private int Volume {
			get;
			set;
		}

		private bool _isPlaying;
		private bool IsPlaying {
			get {
				return _isPlaying;
			}
			set {
				_isPlaying = value;
				if (value == false) {
					play.Content = "Play";
				} else {
					play.Content = "Pause";
				}
			}
		}

		bool IsOpened {
			get;
			set;
		}

		public Window1() {
			InitializeComponent();
			axVLC.CreateControl();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			axVLC.Visible = true;
			Volume = 50;
			IsOpened = false;
			IsPlaying = false;
		}

		private void play_Click(object sender, RoutedEventArgs e) {
			if (IsOpened) {
				if (IsPlaying) {
					togglePause();	//triggers pause
				
				} else {
                    togglePause();  //triggers play
				}
			}

		}

		private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			Volume = Convert.ToInt32(e.NewValue * 10);
			if (IsOpened) axVLC.Volume = Volume;
		}

		private void stop_Click(object sender, RoutedEventArgs e) {
			stopCurrentVideo();
		}

		private void open_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Video Files|*.avi;*.mpg;*.mpeg";
			openFileDialog.Title = "Select a Video File";
			Nullable<bool> result = openFileDialog.ShowDialog();
			if (result == true) {
                progressSlider.Value = 0;
				stopCurrentVideo();
				axVLC.playlist.clear();
				//Thread.Sleep(5000);
				int id = axVLC.playlist.add(openFileDialog.FileName, openFileDialog.Title, new String[] { });
				axVLC.playlist.playItem(id);
				IsPlaying = true;
				IsOpened = true;
			}
		}

		private void stopCurrentVideo() {
			if (IsOpened) {
				axVLC.playlist.stop();
				IsPlaying = false;
				IsOpened = false;
			}
		}

        private void togglePause()
        {
            axVLC.Volume = Volume;
            axVLC.playlist.togglePause();	
            IsPlaying = true;
        }

        private void fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (IsOpened)
            {
                axVLC.video.fullscreen = true;
            }
        }


        private void wfh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (IsOpened) axVLC.video.fullscreen = !axVLC.video.fullscreen;            
            }
            else if (e.Key == Key.Space)
            {
                togglePause();
            }
            else if (e.Key == Key.M)
            {
                axVLC.audio.mute = !axVLC.audio.mute;
            }
            else if (e.Key == Key.C)
            {
                axVLC.input.rate *= 1.1;
            }
            else if (e.Key == Key.X)
            {
                axVLC.input.rate /= 1.1;
            }
            else if (e.Key == Key.Left)
            {
                axVLC.input.Time -= 6000;
            }
            else if (e.Key == Key.Right)
            {
                axVLC.input.Time += 6000;
            }
        }

        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsOpened)
            {
                axVLC.input.Position = e.NewValue / 10;
            }
        }
	}
}
