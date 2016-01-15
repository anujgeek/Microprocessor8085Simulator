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
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        public Preferences()
        {
            InitializeComponent();
            cbShowBinaryValueToolTip.IsChecked = MainWindow.showBinaryValueTooltip;
            cbFormat.SelectedIndex = MainWindow.toolTipFormatIndex;
        }

        private void bPrefOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void cbShowBinaryValueToolTip_Checked(object sender, RoutedEventArgs e)
        {
            cbFormat.IsEnabled = true;
        }

        private void cbShowBinaryValueToolTip_Unchecked(object sender, RoutedEventArgs e)
        {
            cbFormat.IsEnabled = false;
        }
    }
}
