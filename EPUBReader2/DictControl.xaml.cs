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
using WatconWrapper;

namespace EPUBReader2
{
    /// <summary>
    /// Interaction logic for DictControl.xaml
    /// </summary>
    public partial class DictControl : UserControl
    {
        JapDictionary Dict;
        MainWindow main;
        bool Active = false;

        public DictControl()
        {
            InitializeComponent();
        }

        public void Init(MainWindow main)
        {
            this.main = main;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Dict = new JapDictionary();
        }

        public void ActiveSet(bool Set)
        {
            Active = Set;
            if (txtSelection.Text != "")
            {
                SelectionChanged("");
            }
        }

        public async void SelectionChanged(string Text)
        {
            if (!Active) return;
            txtSelection.Text = Text;
            var Results = await Dict.Lookup(Text);
            List.Items.Clear();
            foreach (var Result in Results)
            {
                List.Items.Add(new DictResultStruct(Result));
            }
        }

        private void MoveSelection(int Front, int End)
        {
            if (Active) main.DictSelectionMoved(Front, End);
        }

        private void clrBtn_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(0, -1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(-1, 0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MoveSelection(0, 1);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MoveSelection(1, 0);
        }
    }

    public struct DictResultStruct
    {
        public string Writing { get; set; }
        public string Reading { get; set; }
        public string Meaning { get; set; }
        public RadialGradientBrush Brush { get; set; }
        public string Type { get; set; }
        public DictResultStruct(DictWord word)
        {
            Writing = string.IsNullOrEmpty(word.WrittenForm) ? word.Readings : word.WrittenForm;
            Reading = word.Readings;
            Meaning = string.IsNullOrEmpty(word.Meanings) ? "/" : word.Meanings;
            Color Color;
            switch (word.Type)
            {
                case VocabType.Word:
                    Color = Colors.LightGreen;
                    break;
                case VocabType.Kanji:
                    Color = Colors.LightPink;
                    break;
                case VocabType.Name:
                    Color = Colors.LightBlue;
                    break;
                default:
                    Color = new Color();
                    break;
            }
            Brush = new RadialGradientBrush(Colors.Transparent, Color);
            Brush.RadiusX = Brush.RadiusY = 1;
            Type = word.Type.ToString();
        }
    }
}