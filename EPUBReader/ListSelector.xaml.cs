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
    /// Interaction logic for ListSelector.xaml
    /// </summary>
    public partial class ListSelector : UserControl
    {
       public int SelectedIndex;
        public List<string> ShownList;
        public event EventHandler ItemSelected;
        public event EventHandler ItemDeleted;

        public ListSelector()
        {
            InitializeComponent();
        }

        internal void SetNightmode(bool nightmode)
        {
            ButtonBack.Style = GlobalSettings.CurrentButtonStyle;
            ButtonOk.Style = GlobalSettings.CurrentButtonStyle;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            SelectedIndex = List.SelectedIndex;
            ItemSelected.Invoke(this, null);
            Visibility = Visibility.Hidden;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            GlobalSettings.LeaveMenuDown = IsVisible;
            if (IsVisible)
            {
                List.ItemsSource = ShownList;
                List.Items.Refresh();
            }
            else
            {       
                List.ItemsSource = null;
            }
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {               
            if (List.SelectedItem != null)
            {
                SelectedIndex = List.SelectedIndex;
                ItemDeleted.Invoke(this, null);        
                List.Items.Refresh();
            }
        }
    }
}
