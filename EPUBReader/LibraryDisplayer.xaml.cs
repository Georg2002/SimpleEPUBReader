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
    /// Interaction logic for LibraryDisplayer.xaml
    /// </summary>
    public partial class LibraryDisplayer : UserControl
    {
        public LibraryDisplayer()
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
            LibraryManager.SelectedItem = (BookDefinition)List.SelectedItem;
            LibraryManager.LoadSelectedBook();
            Visibility = Visibility.Hidden;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LibraryManager.InUse = IsVisible;
            if (IsVisible)
            {
               LibraryManager. UpdateCurrentBook();
                List.ItemsSource = LibraryManager.Books;             
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
                LibraryManager.Books.Remove((BookDefinition)List.SelectedItem);
                List.Items.Refresh();
            }
        }
    }
}
