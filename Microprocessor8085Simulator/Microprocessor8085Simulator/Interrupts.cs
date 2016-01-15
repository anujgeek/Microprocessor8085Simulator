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
        private void bInterrrup_Click(object sender, RoutedEventArgs e)
        {            
            inte = false;       //Reset inte f/f
            
            //Push pc onto stack
            SP.DCX();
            m[SP.DEC16].DEC8 = PC.HB.DEC8;

            SP.DCX();
            m[SP.DEC16].DEC8 = PC.LB.DEC8;

            //Branch
            Button a = sender as Button;
            switch (a.Name)
            {                
                case "bR75":
                    {
                        PC.DEC16 = 60;
                        CCC = CCC + 18;
                        UpdateAll();
                        tbLI.Text = "RST 7.5";
                        tbLIMenu.Text = "RST 7.5";
                        break;
                    }
                case "bR65":
                    {
                        PC.DEC16 = 52;
                        CCC = CCC + 18;
                        UpdateAll();
                        tbLI.Text = "RST 6.5";
                        tbLIMenu.Text = "RST 6.5";
                        break;
                    }
                case "bR55":
                    {
                        PC.DEC16 = 44;
                        CCC = CCC + 18;
                        UpdateAll();
                        tbLI.Text = "RST 5.5";
                        tbLIMenu.Text = "RST 5.5";
                        break;
                    }
                case "bTrap":
                    {
                        PC.DEC16 = 36;
                        CCC = CCC + 18;
                        UpdateAll();
                        tbLI.Text = "TRAP";
                        tbLIMenu.Text = "TRAP";
                        break;
                    }
            }
            UpdateInterrupt();
        }

        private void bIntr_Click(object sender, RoutedEventArgs e)
        {
            WindowIntr my = new WindowIntr();
            bool? b = my.ShowDialog();
            if (b == true)
            {
                //Window closed by OK
                //You can access the address entered by string s = my.tbAdd.Text;

                CCC = CCC + 18;
                inte = false;

                //Push pc onto stack
                SP.DCX();
                m[SP.DEC16].DEC8 = PC.HB.DEC8;

                SP.DCX();
                m[SP.DEC16].DEC8 = PC.LB.DEC8;

                //Branch
                PC.DEC16 = (ushort)Int32.Parse(my.tbAdd.Text, NumberStyles.HexNumber);

                UpdateInterrupt();
                UpdateAll();
                tbLI.Text = "INTR";
                tbLIMenu.Text = "INTR";
            }
            else
            {
                //Window closed by canceling
            }
        }

        private void UpdateInterrupt()
        {
            if (inte == false)
            {
                bR75.IsEnabled = false;
                bR65.IsEnabled = false;
                bR55.IsEnabled = false;
                bIntr.IsEnabled = false;
            }
            else
            {
                bIntr.IsEnabled = true;

                if (r75 == true)
                {
                    if (interrupt[2] == true)      //M7.5
                        bR75.IsEnabled = false;
                    else
                        bR75.IsEnabled = true;
                }
                else
                    bR75.IsEnabled = false;

                if (interrupt[1] == true)      //M6.5
                    bR65.IsEnabled = false;
                else
                    bR65.IsEnabled = true;

                if (interrupt[0] == true)      //M5.5
                    bR55.IsEnabled = false;
                else
                    bR55.IsEnabled = true;
            }
        }
    }
}