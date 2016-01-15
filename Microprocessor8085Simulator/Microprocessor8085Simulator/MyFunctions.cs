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
        public static string GetBinaryFromHexString(string s)
        {
            return s;
        }

        public static string GetOctalFromHexString(string s)
        {
            return s;
        }

        private string BitCombination(params bool[] b)
        {
            //This method returns a string representing bitvalues passed as bool 
            string s = "";
            for (int i = 0; i <= b.Length - 1; i++)
            {
                if (b[i] == true)
                    s = s.Insert(i, "1");
                else
                    s = s.Insert(i, "0");
            }
            return s;
        }

        private bool CheckCondition(bool i, bool j, bool k)
        {
            switch (BitCombination(i, j, k))
            {
                case "000":     //NZ
                    {
                        if (PSW.LB[6] == false)
                            return true;
                        else
                            return false;
                    }
                case "001":     //Z
                    {
                        if (PSW.LB[6] == true)
                            return true;
                        else
                            return false;
                    }
                case "010":     //NC
                    {
                        if (PSW.LB[0] == false)
                            return true;
                        else
                            return false;
                    }
                case "011":     //C
                    {
                        if (PSW.LB[0] == true)
                            return true;
                        else
                            return false;
                    }
                case "100":     //PO
                    {
                        if (PSW.LB[2] == false)
                            return true;
                        else
                            return false;
                    }
                case "101":     //PE
                    {
                        if (PSW.LB[2] == true)
                            return true;
                        else
                            return false;
                    }
                case "110":     //P
                    {
                        if (PSW.LB[7] == false)
                            return true;
                        else
                            return false;
                    }
                case "111":     //M
                    {
                        if (PSW.LB[7] == true)
                            return true;
                        else
                            return false;
                    }
                default:
                    return false;
            }
        }
        private byte Get8BitRegisterValue(bool i, bool j, bool k)
        {
            switch (BitCombination(i, j, k))
            {
                case "000":
                    {
                        return BC.HB.DEC8;
                    }
                case "001":
                    {
                        return BC.LB.DEC8;

                    }
                case "010":
                    {
                        return DE.HB.DEC8;

                    }
                case "011":
                    {
                        return DE.LB.DEC8;

                    }
                case "100":
                    {
                        return HL.HB.DEC8;

                    }
                case "101":
                    {
                        return HL.LB.DEC8;

                    }
                case "110":
                    {
                        return M.DEC8;

                    }
                case "111":
                    {
                        return PSW.HB.DEC8;

                    }
                default:
                    return (byte)0;
            }
        }

        private void Set8BitRegisterValue(byte value, bool i, bool j, bool k)
        {
            switch (BitCombination(i, j, k))
            {
                case "000":
                    {
                        BC.HB.DEC8 = value;
                        break;
                    }
                case "001":
                    {
                        BC.LB.DEC8 = value;
                        break;

                    }
                case "010":
                    {
                        DE.HB.DEC8 = value;
                        break;
                    }
                case "011":
                    {
                        DE.LB.DEC8 = value;
                        break;
                    }
                case "100":
                    {
                        HL.HB.DEC8 = value;
                        break;
                    }
                case "101":
                    {
                        HL.LB.DEC8 = value;
                        break;
                    }
                case "110":
                    {
                        M.DEC8 = value;
                        break;
                    }
                case "111":
                    {
                        PSW.HB.DEC8 = value;
                        break;
                    }
            }
        }
        private ushort Get16BitRegisterValue(bool IsSP, bool i, bool j)
        {
            //Note: You need PC only in Push/Pop. So, value of IsSP is true everywhere else and false only in Push/Pop.
            switch (BitCombination(i, j))
            {
                case "00":
                    {
                        return BC.DEC16;
                    }
                case "01":
                    {
                        return DE.DEC16;
                    }
                case "10":
                    {
                        return HL.DEC16;
                    }
                case "11":
                    {
                        if (IsSP == true)
                            return SP.DEC16;
                        else
                        {
                            return PSW.DEC16;
                        }
                    }
                default:
                    return (ushort)0;
            }
        }

        private void Set16BitRegisterValue(bool IsSP, ushort value, bool i, bool j)
        {
            //Note: You need PC only in Push/Pop. So, value of IsSP is true everywhere else and false only in Push/Pop.
            switch (BitCombination(i, j))
            {
                case "00":
                    {
                        BC.DEC16 = value;
                        break;
                    }
                case "01":
                    {
                        DE.DEC16 = value;
                        break;
                    }
                case "10":
                    {
                        HL.DEC16 = value;
                        break;
                    }
                case "11":
                    {
                        if (IsSP == true)
                            SP.DEC16 = value;
                        else
                        {
                            PSW.DEC16 = value;
                        }
                        break;
                    }
            }
        }
    }
}