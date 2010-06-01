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
        public Window1()
        {
            InitializeComponent();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

           
            
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {


            axVLC.CreateControl();
            axVLC.playlist.clear();
            int id = axVLC.playlist.add(@"", "track1", new String[] { });
            axVLC.playlist.playItem(id);



        }
    }
}
