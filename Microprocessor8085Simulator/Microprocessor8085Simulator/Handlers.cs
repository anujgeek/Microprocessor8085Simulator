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
        private void bNext_Click(object sender, RoutedEventArgs e)
        {
            int curPageStartAddress = (Int32.Parse(tbGo.Text, NumberStyles.HexNumber) / 256) * 256;
            int nextPageStartAddress = curPageStartAddress + 256;
            tbGo.Text = nextPageStartAddress.ToString("X4");
            RefreshMIO(true);
        }

        private void bLast_Click(object sender, RoutedEventArgs e)
        {
            int curPageStartAddress = (Int32.Parse(tbGo.Text, NumberStyles.HexNumber) / 256) * 256;
            int lastPageStartAddress = curPageStartAddress - 1;
            tbGo.Text = lastPageStartAddress.ToString("X4");
            RefreshMIO(true);
        }
        
        private void go_Click(object sender, RoutedEventArgs e)
        {
            RefreshMIO(true);
        }

        private void tbGo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbGo.Text.Length == 0)
                {
                    if (rbMemory.IsChecked == true)
                        tbGo.Text = "0000";
                    else
                        tbGo.Text = "00";
                }
                RefreshMIO(true);
            }
        }

        private void rbMIO_Checked(object sender, RoutedEventArgs e)
        {
            if (rbMemory.IsChecked == true)
            {
                tbGo.MaxLength = 4;
                tbGo.Text = "0000";
                IsRbMemoryChecked = true;

                bNext.IsEnabled = true;
                bLast.IsEnabled = false;
            }
            else
            {

                bNext.IsEnabled = false;
                bLast.IsEnabled = false;

                tbGo.MaxLength = 2;
                tbGo.Text = "00";
                IsRbMemoryChecked = false;
            }
            RefreshMIO(true);
        }
        private void rbEM_Checked(object sender, RoutedEventArgs e)
        {
            /*This is common checked handler for both radiobuttons
            Through Xaml, We have set IsChecked property of one of the radiobuttons to be true
            That radio button first raises the Checked event
            Xaml executes sequentially, line by line
            Here rbFast radio button is accessed and rbStep radio button is checked
            So, rbFast should be declared first in Xaml since if rbStep is declared first
            it will raise the Checked event whose event handler accesses rbFast
            Which has not yet been declared in Xaml, thus causing a compiler error*/

            if (rbFast.IsChecked == true)
                bStep.IsEnabled = false;
        }



        private void bStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void bStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void bStep_Click(object sender, RoutedEventArgs e)
        {
            Step();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F1)
            {
                Start();
            }
            else if (e.Key == System.Windows.Input.Key.F2)
            {
                Step();
            }
            else if (e.Key == System.Windows.Input.Key.F3)
            {
                Stop();
            }
        }

        private void Start()
        {
            //This function will do initialisation part
            //Tt will not execute the first step in First mode but will in Fast mode

            //This check is necesssary as you can also start with F1 key
            if (IsStopped == false)
                return;

            IsStopped = false;

            bStart.IsEnabled = false;
            bStop.IsEnabled = true;
            rbFast.IsEnabled = false;
            rbStep.IsEnabled = false;

            LI = "";
            NI = "";
            IC = 0;
            CCC = 0;

            ResetRegisters();
            UpdateAll();

            //Logic to detect mode of execution
            if (rbFast.IsChecked == true)
            {
                bStep.IsEnabled = false;
                bStart.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ExecuteStepDelegate(ExecuteStep));
            }
            else
            {
                bStep.IsEnabled = true;
            }
        }

        private void Step()
        {
            //This check is necessary as you can also step with F2 key
            if ((rbStep.IsChecked == true) && (IsStopped == false))
                ExecuteStep();
        }

        private void Stop()
        {
            IsStopped = true;

            bStart.IsEnabled = true;
            bStop.IsEnabled = false;
            bStep.IsEnabled = false;
            rbFast.IsEnabled = true;
            rbStep.IsEnabled = true;

            UpdateAll();
            tbNI.Text = "";
            tbNIMenu.Text = "";
        }
    }
}