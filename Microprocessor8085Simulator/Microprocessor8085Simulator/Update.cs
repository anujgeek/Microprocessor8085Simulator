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
        private void UpdateAll()
        {
            LI = NI;
            NI = GetNI();
            tbPCMenu.Text = PC.DEC16.ToString("X4");
            tbLIMenu.Text = LI;
            tbNIMenu.Text = NI;

            tbLI.Text = LI;
            tbNI.Text = NI;
            tbPC.Text = PC.DEC16.ToString("X4");
            tbSP.Text = SP.DEC16.ToString("X4");
            tbCCC.Text = CCC.ToString();
            tbIC.Text = IC.ToString();
            tbA.Text = PSW.HB.DEC8.ToString("X2");
            tbF.Text = PSW.LB.DEC8.ToString("X2");
            tbB.Text = BC.HB.DEC8.ToString("X2");
            tbC.Text = BC.LB.DEC8.ToString("X2");
            tbD.Text = DE.HB.DEC8.ToString("X2");
            tbE.Text = DE.LB.DEC8.ToString("X2");
            tbH.Text = HL.HB.DEC8.ToString("X2");
            tbL.Text = HL.LB.DEC8.ToString("X2");
            tbM.Text = M.DEC8.ToString("X2");

            string s;
            for (int i = 0; i <= 7; i++)
            {
                if (PSW.LB[i] == true)
                    s = "1";
                else
                    s = "0";
                FArray[i].Text = s;
            }
        }

        private string GetNI()
        {
            string s="";
            byte v0, v1, v2;
            v0 = MainWindow.m[PC.DEC16].DEC8;       //v0=data at current memory
            if (I.Names[v0, 0] == "1")
            {
                return I.Names[v0, 1];
            }
            else if (I.Names[v0, 0] == "2")
            {
                if (PC.DEC16 != 65535)
                    v1 = MainWindow.m[PC.DEC16 + 1].DEC8;
                else
                    v1 = MainWindow.m[0].DEC8;
                return String.Concat(I.Names[v0, 1], v1.ToString("X2"));
            }
            else if (I.Names[v0, 0] == "3")
            {
                if (PC.DEC16 != 65535)
                    v1 = MainWindow.m[PC.DEC16 + 1].DEC8;
                else
                    v1 = MainWindow.m[0].DEC8;
                if ((PC.DEC16 + 1) != 65535)
                    v2 = MainWindow.m[PC.DEC16 + 2].DEC8;
                else
                    v2 = MainWindow.m[0].DEC8;

                return String.Concat(I.Names[v0, 1], v2.ToString("X2"), v1.ToString("X2"));
            }
            return s;
        }
        private void ResetRegisters()
        {
            PSW.DEC16 = 0;
            BC.DEC16 = 0;
            DE.DEC16 = 0;
            HL.DEC16 = 0;
            PC.DEC16 = PCStartValue;
            SP.DEC16 = 0;
            //Do not Reset M.DEC8 = 0; as doing this will also modify the memory location 
        }

        private void UpdateZ(byte i)
        {
            if (i != 0)
                PSW.LB[6] = false;
            else
                PSW.LB[6] = true;
        }
        private void UpdateS(byte i)
        {
            MyByte8 temp = new MyByte8();
            temp.DEC8 = i;
            if (temp[7] == true)
                PSW.LB[7] = true;
            else
                PSW.LB[7] = false;
        }
        private void UpdateP(byte i)
        {
            MyByte8 temp = new MyByte8();
            temp.DEC8 = i;
            int NoOfOnes = 0;
            for (int j = 0; j <= 7; j++)
            {
                if (temp[j] == true)
                    NoOfOnes++;
            }
            if (NoOfOnes % 2 == 0)
                PSW.LB[2] = true;
            else
                PSW.LB[2] = false;
        }

        public static void SetMIO(int index, int val)
        {
            if (IsRbMemoryChecked == true)
                m[hexStart + index].DEC8 = (byte)val;
            else
                io[index].DEC8 = (byte)val;
        }
        private void RefreshMIO(bool focus)
        {
            int a, b, i, myHexNo;
            string s;

            if (rbMemory.IsChecked == true)
            {
                a = Int32.Parse(tbGo.Text, NumberStyles.HexNumber);
                b = (Int32.Parse(tbGo.Text, NumberStyles.HexNumber) / 256) * 256;

                if (b == 65280)
                {
                    bNext.IsEnabled = false;
                    bLast.IsEnabled = true;
                }
                else if (b == 0)
                {
                    bLast.IsEnabled = false;
                    bNext.IsEnabled = true;
                }
                else
                {
                    bNext.IsEnabled = true;
                    bLast.IsEnabled = true;
                }

                s = b.ToString("X4");
                hexStart = Int32.Parse(s, NumberStyles.HexNumber);//4 digit
                s = s.Remove(3, 1);

                myHexNo = Int32.Parse(s, NumberStyles.HexNumber); //3 digit

                for (i = 0; i <= 15; i++)
                {
                    lb[i].Text = myHexNo.ToString("X3");
                    myHexNo++;
                }
                for (i = 0; i <= 255; i++)
                {
                    tb[i].Text = m[hexStart + i].DEC8.ToString("X2");
                }
                if (focus == true)
                    tb[a - b].Focus();
            }
            else
            {
                myHexNo = 0;
                for (i = 0; i <= 15; i++)
                {
                    lb[i].Text = myHexNo.ToString("X1");
                    myHexNo++;
                }
                for (i = 0; i <= 255; i++)
                {
                    tb[i].Text = io[i].DEC8.ToString("X2");
                }
                if (focus == true)
                    tb[Int32.Parse(tbGo.Text, NumberStyles.HexNumber)].Focus();
            }
        }
    }
}