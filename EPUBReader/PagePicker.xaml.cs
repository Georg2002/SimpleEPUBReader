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

namespace EPUBReader
{
    /// <summary>
    /// Interaction logic for ChapterPicker.xaml
    /// </summary>
    public partial class PagePicker : UserControl
    {   
        public PagePicker()
        {
            InitializeComponent();
            ViewerInteracter.PageChanged += RefreshIndicator;
            var Window = Application.Current.MainWindow;
            Window.SizeChanged += RefreshIndicator;
            Window.StateChanged += RefreshIndicator;
        }

        internal void SetNightmode(bool nightmode)
        {
            foreach (Button Child in MainGrid.Children)
            {
                Child.Style = GlobalSettings.CurrentButtonStyle;
            }
        }

        private void RefreshIndicator(object sender, EventArgs args)
        {
            string CurrentPage = ViewerInteracter.GetCurrentPage().ToString();
            string TotalPages = ViewerInteracter.GetTotalPages().ToString();
            PageIndicatorText.Text = CurrentPage + "/" + TotalPages + "\nfertig";
        }

        private void PageIndicator_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }      

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            int NeededColumn = 0;
            if (ViewerInteracter.RTL)
            {
                NeededColumn = 6;
            }
            var First = MainGrid.Children[0];
            if (Grid.GetColumn(First) != NeededColumn)
            {
                foreach (UIElement Child in MainGrid.Children)
                {
                    int Current = Grid.GetColumn(Child);
                    Grid.SetColumn(Child, 6 - Current);
                }

            }
            GlobalSettings.LeaveMenuDown = IsVisible;          
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            var Button = (Button)sender;
            string Name = Button.Name;
            string NumberString = Button.Name.Remove(0, 1);
            int Change = Convert.ToInt32(NumberString);
            if (Name.StartsWith("M"))
            {
                Change *= -1;
            }
            int NewPage = ViewerInteracter.GetCurrentPage() + Change;
            ViewerInteracter.SetPage(NewPage);
        }
    }
}
