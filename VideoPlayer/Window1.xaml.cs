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
			BackgroundWorker videoGuard = new BackgroundWorker();
			videoGuard.WorkerReportsProgress = true;
			videoGuard.DoWork += new DoWorkEventHandler(videoGuard_DoWork);
			videoGuard.ProgressChanged += new ProgressChangedEventHandler(videoGuard_ProgressChanged);
			videoGuard.RunWorkerAsync();
		}

		#region properties
		/// <summary>
		/// timer that erases the message printed in the video screen
		/// </summary>
		private BackgroundWorker MessageEraser {
			get;
			set;
		}

		/// <summary>
		/// watches the video every few miliseconds and adjusts the UI
		/// </summary>
		public BackgroundWorker VideoGuard {
			get;
			set;
		}

		/// <summary>
		/// tells whether the change of the progressSlider is caused by the program - ie. inner functionality
		/// caused the slider movement. Handles the loop between indicating a video progress and moving the
		/// actual position of the played video.
		/// </summary>
		public bool ProgressInnerReport {
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
		/// 
		private bool _isStopped;
		public bool IsStopped {
			get {
				return _isStopped;
			}
			set {
				progressSlider.IsEnabled = !value;
				_isStopped = value;
			}
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
		/// <summary>
		/// initializes basic values
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Loaded(object sender, RoutedEventArgs e) {
			axVLC.Visible = true;
			Volume = 50;
			IsOpened = false;
			IsPlaying = false;
			IsStopped = true;
			wfh.Focus();
		}

		/// <summary>
		/// plays currently opened video from the library on the button click
		/// ,calls open video dialog if no video is opened or toggles between
		/// pause and play mode if the video is already being played.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// stops current video if opened and playing on the button click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void stop_Click(object sender, RoutedEventArgs e) {
			stopCurrentVideo();
		}

		/// <summary>
		/// adds new video via open dialog on the button click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void open_Click(object sender, RoutedEventArgs e) {
			openVideo();
		}

		/// <summary>
		/// switches into the fullscreen on the button click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void fullscreen_Click(object sender, RoutedEventArgs e) {
			toggleFullscreen();
		}

		/// <summary>
		/// sets the current video volume according to the new position of volumeSlider
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			Volume = Convert.ToInt32(e.NewValue * 10);
			if (IsOpened)
				axVLC.Volume = Volume;
		}

		/// <summary>
		/// moves the video forward & backward according to the new position of the slider
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (IsOpened && !IsStopped) {
				if (!ProgressInnerReport) { // slider wasn't moved by inner program functionality
					axVLC.input.Position = e.NewValue / 10;
				} else {
					ProgressInnerReport = false;
				}
			}
		}

		/// <summary>
		/// key press handler - defines functionality of key commands
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void wfh_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) 
            {
				toggleFullscreen();
			} 
            else if (e.Key == Key.Space) 
            {
				togglePause();
			} 
            else if (e.Key == Key.M) 
            {
				axVLC.audio.mute = !axVLC.audio.mute;
                if (axVLC.audio.mute) printMessage("Audio muted");
                else printMessage("Audio unmuted");
			} 
            else if (e.Key == Key.C) 
            {
                axVLC.input.rate += 0.1f;
                printMessage("Speed rate: " + Math.Floor(axVLC.input.rate*10) / 10 + "×");
			} 
            else if (e.Key == Key.X) 
            {
				axVLC.input.rate -= 0.1f;
                printMessage("Speed rate: " + Math.Floor(axVLC.input.rate*10)/10 + "×");
			} 
            else if (e.Key == Key.Left) 
            {
                int speed = 6000;
                axVLC.input.Time -= speed;
                printMessage("Position " + Math.Floor(Convert.ToDouble(speed / 1000)) + "s backward");
			} 
            else if (e.Key == Key.Right) 
            {
                int speed = 6000;
                axVLC.input.Time += speed;
                printMessage("Position " + Math.Floor(Convert.ToDouble(speed / 1000)) + "s forward");
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
			openFileDialog.Filter = "Video Files|*.avi;*.mpg;*.mpeg;*.divx;*.mkv;*.mov;*.wmv|All files|*.*";
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
					printMessage("Video stopped");
				}
			}
		}

		/// <summary>
		/// toggles between the pause or play state of the current video if opened & playing
		/// </summary>
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

		/// <summary>
		/// toggles between fullscreen & window mode
		/// </summary>
		private void toggleFullscreen() {
			if (IsPlaying) {
				axVLC.video.toggleFullscreen();
			}
		}

			#region print message handling
		/// <summary>
		/// prints the message in the video screen and sets the timer to erase it
		/// </summary>
		public void printMessage(string message) {
			if (message != null) {
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
		}

		/// <summary>
		/// erases the Content of the message covering the video screen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void messageEraser_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (!e.Cancelled) {
				labelPrintMessage.Content = "";
			}
		}

		/// <summary>
		/// waits 3 seconds and then triggers RunWorkerCompleted section (if cancelation wasn't pending)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void messageEraser_DoWork(object sender, DoWorkEventArgs e) {
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			Thread.Sleep(3000);
			if (backgroundWorker.CancellationPending) {
				e.Cancel = true;
			}
		}
		#endregion

			#region video guard events
		void videoGuard_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			if (IsOpened) {
				if (progressSlider.Value > 0 && !axVLC.playlist.isPlaying && IsPlaying) { //handles the end of a video
					stopCurrentVideo();
				} else {
					ProgressInnerReport = true;
                    try
                    {
                        progressSlider.Value = axVLC.input.Position * 10;
                    }
                    catch
                    {
                    }
				}
			}
		}

		void videoGuard_DoWork(object sender, DoWorkEventArgs e) {
			BackgroundWorker mySender = sender as BackgroundWorker;
			while (true) {
				mySender.ReportProgress(1);
				Thread.Sleep(500);
			}
		}
			#endregion

		#endregion

	}
}
