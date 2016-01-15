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
using System.Windows.Shapes;

namespace Microprocessor8085Simulator
{
    /// <summary>
    /// Interaction logic for SetStartAdd.xaml
    /// </summary>
    public partial class SetStartAdd : Window
    {
        public SetStartAdd()
        {
            InitializeComponent();
            newPC.Text = MainWindow.PCStartValue.ToString("X4");
            newPC.Focus();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
