using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace Microprocessor8085Simulator
{
    public partial class MainWindow : Window
    {
        private void MenuItemPreferences_Click(object sender, RoutedEventArgs e)
        {
            Preferences PrefWindow = new Preferences();
            bool? result = PrefWindow.ShowDialog();

            if (result == true)
            {
                showBinaryValueTooltip = PrefWindow.cbShowBinaryValueToolTip.IsChecked;
                toolTipFormatIndex = PrefWindow.cbFormat.SelectedIndex;
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            About myAbout = new About();
            myAbout.ShowDialog();
        }

        private void MenuItemClearMemory_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i <= 65535; i++)
            {
                m[i].DEC8 = 0;
            }
            RefreshMIO(false);

            MessageBox.Show("Memory Cleared", "Memory Cleared");    
        }

        private void MenuSetStartAdd_Click(object sender, RoutedEventArgs e)
        {
            SetStartAdd mySetStartAdd = new SetStartAdd();
            bool? b = mySetStartAdd.ShowDialog();
            if (b == true)
            {
                PCStartValue = (ushort)Int32.Parse(mySetStartAdd.newPC.Text, NumberStyles.HexNumber);
                tbPC.Text = PCStartValue.ToString("X4");
            }
        }
    }
}