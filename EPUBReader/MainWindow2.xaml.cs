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

        //check mouse coordinates and activate animation when below 50
        //increase check area to 100 as soon as it starts moving, decrease only when the tool bar is fully pulled in!!
        //!!!!!!!!!!!     
    }
}
