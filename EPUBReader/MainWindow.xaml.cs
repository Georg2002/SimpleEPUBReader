﻿using EPUBRenderer;
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
using System.Windows.Shapes;

namespace EPUBReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var PageSettings = new EPUBParser.EpubSettings();         
           PageSettings.Language = "ja";
           PageSettings.Vertical = true;
           PageSettings.RTL = true;
            RenderLinePart.PageSettings = PageSettings;
            InitializeComponent();
        }
    }
}