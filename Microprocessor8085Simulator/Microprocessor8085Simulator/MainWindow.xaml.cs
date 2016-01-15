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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            int i, j, k, myHexNo;

            //Initialize initial state
            IsRbMemoryChecked = true;
            IsStopped = true;
            bStop.IsEnabled = false;
            bStep.IsEnabled = false;
            bR75.IsEnabled = false;
            bR65.IsEnabled = false;
            bR55.IsEnabled = false;
            bIntr.IsEnabled = false;

            //Initialize memory
            for (i = 0; i <= 65535; i++)
            {
                m[i] = new MyByte8();
            }

            //Initialize IO
            for (i = 0; i <= 255; i++)
            {
                io[i] = new MyByte8();
            }

            //Initialize Registers
            M = new MyByteM();
            BC = new MyByte16();
            DE = new MyByte16();
            HL = new MyByte16();
            PSW = new MyByte16();
            PC = new MyByte16();
            SP = new MyByte16();

            //Initialize TextBoxes
            FArray = new TextBox[] { tbF0, tbF1, tbF2, tbF3, tbF4, tbF5, tbF6, tbF7 };
            tbGo.Text = "0000";

            tbPCMenu.Text = "0000";
            //tbCIMenu
            //tbNIMenu
            //tbCI
            //tcNI
            tbPC.Text = "0000";
            tbSP.Text = "0000";
            //tbCCC
            //tbIC
            tbA.Text = "00";
            tbF.Text = "00";
            tbB.Text = "00";
            tbC.Text = "00";
            tbD.Text = "00";
            tbE.Text = "00";
            tbH.Text = "00";
            tbL.Text = "00";
            tbM.Text = "00";
            for (i = 0; i <= 7; i++)
                FArray[i].Text = "0";



            // Define the Rows
            RowDefinition[] r = new RowDefinition[17];
            for (i = 0; i <= 16; i++)
            {
                r[i] = new RowDefinition();
                myGrid.RowDefinitions.Add(r[i]);
            }

            //Define columns
            ColumnDefinition[] c = new ColumnDefinition[17];
            for (i = 0; i <= 16; i++)
            {
                c[i] = new ColumnDefinition();
                myGrid.ColumnDefinitions.Add(c[i]);
            }

            k = 0;
            myHexNo = 0x000;

            for (i = 1, j = 0; i <= 16; i++)
            {
                lb[k] = new TextBox();
                lb[k].TextAlignment = TextAlignment.Center;
                lb[k].Text = myHexNo.ToString("X3");
                myHexNo++;
                lb[k].Focusable = false;
                lb[k].Background = Brushes.BlanchedAlmond;


                Grid.SetRow(lb[k], i);
                Grid.SetColumn(lb[k], j);
                myGrid.Children.Add(lb[k]);
                k++;
            }

            for (i = 0, j = 1; j <= 16; j++)
            {
                lb[k] = new TextBox();
                lb[k].TextAlignment = TextAlignment.Center;
                lb[k].Text = (j - 1).ToString("X");
                lb[k].Focusable = false;
                lb[k].Background = Brushes.BlanchedAlmond;

                Grid.SetRow(lb[k], i);
                Grid.SetColumn(lb[k], j);
                myGrid.Children.Add(lb[k]);
                k++;
            }

            k = 0;
            for (i = 1; i <= 16; i++)
            {
                for (j = 1; j <= 16; j++)
                {
                    tb[k] = new myTextBox2d();
                    tb[k].Index = k;
                    tb[k].Text = "00";


                    Grid.SetRow(tb[k], i);
                    Grid.SetColumn(tb[k], j);
                    myGrid.Children.Add(tb[k]);
                    k++;
                }
            }

            rbMemory.IsChecked = true;

            IsSaved = true;
            CommandBinding cmdBindingPaste = new CommandBinding(ApplicationCommands.Paste);
            cmdBindingPaste.Executed += PasteCommandHandler;
            rtb1.CommandBindings.Add(cmdBindingPaste);

            CommandBinding cmdBindingExit = new CommandBinding(ApplicationCommands.Close);
            cmdBindingExit.Executed += ExitCommandHandler;
            CommandBindings.Add(cmdBindingExit);

            CommandBinding cmdBindingOpen = new CommandBinding(ApplicationCommands.Open);
            cmdBindingOpen.Executed += OpenCommandHandler;
            CommandBindings.Add(cmdBindingOpen);

            CommandBinding cmdBindingSave = new CommandBinding(ApplicationCommands.Save);
            cmdBindingSave.Executed += SaveCommandHandler;
            CommandBindings.Add(cmdBindingSave);
        }

        /*rtb1KeyDownEnter
         * For middle keyword split no highlight problem
         * Also To insert a line number an rtb1
         * Use rtb1.CaretPosition.InsertParagraphbreak()
        private void rtb1_KeyDown(object sender, KeyEventArgs e)
        {
            if (rtb1.Document == null)
                return;
            if (rtb1.CaretPosition.Paragraph == null)
                return;
            if (rtb1.CaretPosition.Paragraph.Inlines.FirstInline == null)
                return;

            FormatCurrentLine();

            if (e.Key == System.Windows.Input.Key.Return)
            {
                FormatCurrentLine();
                rtb1.CaretPosition.InsertParagraphBreak();
            }
        }
        */
    }
}