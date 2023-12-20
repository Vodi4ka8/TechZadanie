﻿using System;
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

namespace TechZadanie.Features.File
{
    /// <summary>
    /// Логика взаимодействия для Description.xaml
    /// </summary>
    public partial class Description : Window
    {
        public string? Text {  get; set; }
        public Description()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Text = FolderNameBox.Text;

            Window.GetWindow(this).Close();
        }


    }
}
