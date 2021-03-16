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

namespace EPUBReader2
{
    /// <summary>
    /// Interaction logic for MenuControl.xaml
    /// </summary>
    public partial class MenuControl : UserControl
    {
        public MenuControl()
        {
            InitializeComponent();
          
        }

        public void SetToChapters(List<string> Chapters)
        {
            ListBox.ItemsSource = GetItems(Chapters, false);
        }

        public void SetToLibrary(List<string> Titles)
        {
            ListBox.ItemsSource = GetItems(Titles, true);
        }


        private List<ListItemStruct>  GetItems(List<string> TextList,bool Visible)
        {
            var Res = new List<ListItemStruct>();
            for (int i = 0; i < TextList.Count; i++)
            {
                var Text = TextList[i];          
                Res.Add(new ListItemStruct(Text, Visible,i));
            }
            return Res;
        }
    }

    struct ListItemStruct
    {
        public string Text { get; set; }
        public Visibility Vis { get; set; }
        public int Index { get; set; }
        public ListItemStruct(string Text, bool Visible,int Index)
        {
            this.Text = Text;
            Vis = Visible ? Visibility.Visible : Visibility.Collapsed;
            this.Index = Index;
        }
    }
}
