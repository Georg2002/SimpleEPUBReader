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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EPUBReader
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        bool IsGoingDown = false;
        Storyboard MoveUp;
        Storyboard MoveDown;

        public MainWindow2()
        {
            InitializeComponent();
            MoveUp = (Storyboard)this.FindResource("MoveUp");
            MoveDown = (Storyboard)this.FindResource("MoveDown");
            ViewerInteracter.Viewer = Viewer;
        }

        private void Base_MouseMove(object sender, MouseEventArgs e)
        {
            double CheckHeight = 25;
            Point Pos = e.GetPosition(Base);
            if (IsGoingDown)
            {
                CheckHeight = 50;
            }
            if (Pos.Y < CheckHeight && !IsGoingDown)
            {
                IsGoingDown = true;
                MoveUp.Stop();
                MoveDown.Begin();
            }
            else if (Pos.Y > CheckHeight && IsGoingDown)
            {
                MoveDown.Stop();
                MoveUp.Begin();
                IsGoingDown = false;
               
            }
        }

        private void ButtonLeft_Click(object sender, RoutedEventArgs e)
        {
            ViewerInteracter.SwitchLeft();
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e)
        {
            ViewerInteracter.SwitchRight();
        }

        private void Base_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Saver.Save();
        }
    }
}
