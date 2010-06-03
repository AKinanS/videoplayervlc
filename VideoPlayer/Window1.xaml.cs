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

		/// <summary>
		/// actual video volume
		/// </summary>
		private int Volume {
			get;
			set;
		}

		/// <summary>
		/// the VLC library id of a last played video
		/// </summary>
		public int LastPlayedID {
			get;
			set;
		}

		private bool _isPlaying;
		/// <summary>
		/// tells whether the video is being played at the time and sets
		/// the play button appearence according to the current state
		/// </summary>
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

		/// <summary>
		/// tells whether the video is stopped at the time - ie. if true,
		/// than no video is currently loaded and therefore can't be manipulated with.
		/// </summary>
		public bool IsStopped {
			get;
			set;
		}

		/// <summary>
		/// tells whether the some video is currently loaded in the library
		/// </summary>
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
			IsStopped = true;
            wfh.Focus();
		}

		private void play_Click(object sender, RoutedEventArgs e) {
			if (IsOpened) { // there is some video in the playlist
				if (IsStopped) {	// the video is not running - play from beginning
					playVideo(LastPlayedID);
				} else {
					if (IsPlaying) {	//the video is running - toggle between pause & play
						axVLC.playlist.togglePause();	//triggers pause
						IsPlaying = false;
					} else {
						axVLC.Volume = Volume;
						axVLC.playlist.togglePause();	//triggers play
						IsPlaying = true;
					}
				}
			} else {
				openVideo();
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
			openVideo();
		}

		/// <summary>
		/// opens the video via openFileDialog and adds it into the library
		/// </summary>
		/// <returns>playlist id of the added video file</returns>
		private void openVideo() {
			int id = -1;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Video Files|*.avi;*.mpg;*.mpeg";
			openFileDialog.Title = "Select a Video File";
			Nullable<bool> result = openFileDialog.ShowDialog();
			if (result == true) {
                progressSlider.Value = 0;
				stopCurrentVideo();
				axVLC.playlist.clear();
				id = axVLC.playlist.add(openFileDialog.FileName, openFileDialog.Title, new String[] { });
				IsOpened = true;
				if (IsPlaying) {
					stopCurrentVideo();
				}
				playVideo(id);
			}
		}

		/// <summary>
		/// plays video from playlist according to the given id
		/// </summary>
		/// <param name="id">the identificator of the video in th VLC playlist</param>
		private void playVideo(int id) {
			axVLC.playlist.playItem(id);
			LastPlayedID = id;
			IsPlaying = true;
			IsStopped = false;
			}

		/// <summary>
		/// stops current video in case there is some being played
		/// </summary>
		private void stopCurrentVideo() {
			if (IsOpened) {
				if (!IsStopped) {
					axVLC.playlist.stop();
					IsPlaying = false;
					IsStopped = true;
				}
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
                wfh.Focus();
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
                axVLC.input.rate += 0.1;
            }
            else if (e.Key == Key.X)
            {
                axVLC.input.rate -= 0.1;
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
