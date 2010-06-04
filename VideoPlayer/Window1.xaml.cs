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
using System.ComponentModel;

namespace VideoPlayer {
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window {

		public Window1() {
			InitializeComponent();
			axVLC.CreateControl();
			labelPrintMessage.Content = "";
		}

		#region properties
		/// <summary>
		/// timer that erases the message printed in the video screen
		/// </summary>
		public BackgroundWorker MessageEraser {
			get;
			set;
		}

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
					play.Background = (ImageBrush) grid1.Resources["imageButtonPlay"];
				} else {
					play.Background = (ImageBrush) grid1.Resources["imageButtonPause"];
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
		#endregion

		#region wpf component events
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
					togglePause();	//the video is running - toggle between pause & play
				}
			} else {
				openVideo();
			}
		}

		private void stop_Click(object sender, RoutedEventArgs e) {
			stopCurrentVideo();

		}

		private void open_Click(object sender, RoutedEventArgs e) {
			openVideo();
		}

		private void fullscreen_Click(object sender, RoutedEventArgs e) {
			toggleFullscreen();
		}

		private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			Volume = Convert.ToInt32(e.NewValue * 10);
			if (IsOpened)
				axVLC.Volume = Volume;
		}

		private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (IsOpened && !IsStopped) {
				axVLC.input.Position = e.NewValue / 10;
			}
		}

		private void wfh_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				toggleFullscreen();
			} else if (e.Key == Key.Space) {
				togglePause();
			} else if (e.Key == Key.M) {
				axVLC.audio.mute = !axVLC.audio.mute;
			} else if (e.Key == Key.C) {
				axVLC.input.rate += 0.1;
			} else if (e.Key == Key.X) {
				axVLC.input.rate -= 0.1;
			} else if (e.Key == Key.Left) {
				axVLC.input.Time -= 6000;
			} else if (e.Key == Key.Right) {
				axVLC.input.Time += 6000;
			}
		}

		#endregion

		#region support methods
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
					progressSlider.Value = 0;
				}
			}
		}

		private void togglePause() {
			if (!IsStopped) {
				if (IsPlaying) {
					axVLC.playlist.togglePause();	//triggers pause
					IsPlaying = false;
					printMessage("Video paused.");
				} else {
					axVLC.Volume = Volume;
					axVLC.playlist.togglePause();	//triggers play
					IsPlaying = true;
					printMessage("Continue playing...");
				}
			}
		}

		private void toggleFullscreen() {
			if (IsPlaying)
				axVLC.video.fullscreen = !axVLC.video.fullscreen;
		}

			#region print message handling
		/// <summary>
		/// prints the message in the video screen and sets the timer to erase it
		/// </summary>
		void printMessage(string message) {
			if (MessageEraser != null) {
				MessageEraser.CancelAsync();
			}
			labelPrintMessage.Content = message;
			// the label Content is not empty, therefore the timer to erase the message needs to be set
			BackgroundWorker messageEraser = new BackgroundWorker();
			messageEraser.DoWork += new DoWorkEventHandler(messageEraser_DoWork);
			messageEraser.RunWorkerCompleted += new RunWorkerCompletedEventHandler(messageEraser_RunWorkerCompleted);
			messageEraser.WorkerSupportsCancellation = true;
			MessageEraser = messageEraser;
			MessageEraser.RunWorkerAsync();
		}

		/// <summary>
		/// erases the Content of the message covering the video screen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void messageEraser_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (!e.Cancelled) {
				labelPrintMessage.Content = "";
			}
		}

		/// <summary>
		/// waits 3 seconds and then triggers RunWorkerCompleted section (if cancelation wasn't pending)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void messageEraser_DoWork(object sender, DoWorkEventArgs e) {
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			Thread.Sleep(3000);
			if (backgroundWorker.CancellationPending) {
				e.Cancel = true;
			}
		}
			#endregion

		#endregion

	}
}
