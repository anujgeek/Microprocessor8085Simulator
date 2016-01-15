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
    /*Important Note: 
    Classes MyByte8, MyByteM and MyByte16 implement a logic to read/write fields dec8, lb, hb or dec16 in Properties DEC8, LB, HB or DEC16.
    So, except inside Property definition and in constructor, do not use the backing fields dec8, lb, hb or dec16. 
    Always use properties DEC8 or DEC16.*/

    public class MyByte8
    {
        protected byte dec8;
        public byte DEC8
        {
            get { return dec8; }
            set { dec8 = value; }
        }
        public MyByte8()
        {
            DEC8 = (byte)0;
        }
        public byte GetLowerNibble()
        {
            byte i, j;
            i = (byte)(DEC8 << 4);
            j = (byte)(i >> 4);
            return j;
        }
        public byte GetHigherNibble()
        {
            byte i = (byte)(DEC8 >> 4);
            return i;
        }
        public byte Get2sComplement()
        {
            byte i;
            i = (byte)(~DEC8);
            if (i == (byte)127)
                i = 0;
            else
                i = (byte)(i + 1);
            return i;
        }
        //Indexer
        public bool this[int index]
        {
            get
            {
                return (DEC8 & (1 << index)) != 0;
            }
            set
            {
                if (value)                                  //Turn the bit on if value is true; otherwise, turn it off
                    DEC8 |= (byte)(1 << index);
                else
                    DEC8 &= (byte)~(1 << index);
            }
        }
    }

    public class MyByteM : MyByte8
    {
        public MyByteM()
            : base()
        {

        }
        public new byte DEC8
        {
            get
            {
                base.dec8 = MainWindow.m[MainWindow.HL.DEC16].DEC8;
                return base.dec8;
            }
            set
            {
                base.dec8 = value;
                MainWindow.m[MainWindow.HL.DEC16].DEC8 = value;     //Updating memory                
            }
        }
    }
    public class MyByte16
    {
        private ushort dec16;
        public ushort DEC16
        {
            get
            {
                dec16 = (ushort)(hb.DEC8 * 256 + lb.DEC8);
                return dec16;
            }
            set
            {
                dec16 = value;
                hb.DEC8 = GetHigherByte();
                lb.DEC8 = GetLowerByte();

                /*Why this wont work? Though dec16 is set, but though currently LB and HB are not currently set.
                GetHigherByte() and GetLowerByte() uses DEC16. So, it arrives in its GET accessor, but GET accessor
                of DEC16 uses LB and HB to determine value of itself, which are not currently set.
                So, modify LB and HB in their own properties.
                Also, take care to not to fall in infinite recursion i.e. DEC16 using DEC8 and again DEC8 using DEC16
                It will cause stack overflow exception. Design logic such that only one determines its value from other
                */
            }
        }
        public MyByte16()
        {
            dec16 = (ushort)0;
            lb = new MyByte8();
            hb = new MyByte8();
        }
        private MyByte8 lb, hb;
        public MyByte8 LB
        {
            get { return lb; }
            set { lb = value; }
        }
        public MyByte8 HB
        {
            get { return hb; }
            set { hb = value; }
        }
        private byte GetLowerByte()
        {
            ushort i = (ushort)(this.dec16 << 8);
            byte j = (byte)(i >> 8);
            return j;
        }
        private byte GetHigherByte()
        {
            byte i = (byte)(this.dec16 >> 8);
            return i;
        }
        public void INX()
        {
            if (DEC16 == 65535)
                DEC16 = (ushort)0;
            else
                DEC16 = (ushort)(DEC16 + 1);
        }
        public void DCX()
        {
            if (DEC16 == 0)
                DEC16 = (ushort)65535;
            else
                DEC16 = (ushort)(DEC16 - 1);
        }
        public ushort Get2sComplement()
        {
            ushort i;
            i = (ushort)(~DEC16);
            if (i == (ushort)65535)
                i = 0;
            else
                i = (ushort)(i + 1);
            return i;
        }
        //Indexer
        public bool this[int index]
        {
            get
            {
                return (DEC16 & (1 << index)) != 0;
            }
            set
            {
                if (value)                                  //Turn the bit on if value is true; otherwise, turn it off
                    DEC16 |= (ushort)(1 << index);
                else
                    DEC16 &= (ushort)~(1 << index);
            }
        }
    }

    public class myTextBox2d : TextBox
    {
        private int index;
        public int Index                //Index is the serial index number of myTextBox, used to distingush them in MemoryEditor
        {
            get { return index; }
            set { index = value; }
        }
        public myTextBox2d()
            : base()
        {
            this.MaxLength = 2;
            this.TextAlignment = TextAlignment.Center;
            this.TextChanged += TextChangedHandler;
            this.GotFocus += GotFocusHandler;
            this.LostFocus += LostFocusHandler;
            this.KeyDown += EnterKeyHitHandler;
            this.ToolTipOpening += ToolTipOpeningHandler;
            this.ToolTip = "0";
        }

        void ToolTipOpeningHandler(object sender, ToolTipEventArgs e)
        {
            if (this.Text.Length <= 0)
            {
                e.Handled = true;
                return;
            }
            if (MainWindow.showBinaryValueTooltip == true)
            {
                //Update tooltip text
                if (MainWindow.toolTipFormatIndex == 0)
                {

                    string s = Convert.ToString(Byte.Parse(this.Text, NumberStyles.HexNumber), 2);  //Binary
                    int i = s.Length;           //inserting a string at Length apeends the string
                    while (true)
                    {
                        if ((i - 4) > 0)
                        {
                            i = i - 4;
                            s = s.Insert(i, " ");
                        }
                        else
                            break;
                    }
                    this.ToolTip = s;


                }
                else if (MainWindow.toolTipFormatIndex == 1)
                    this.ToolTip = Int32.Parse(this.Text, NumberStyles.HexNumber);                      //Decimal
                else
                    this.ToolTip = Convert.ToString(Byte.Parse(this.Text, NumberStyles.HexNumber), 8);  //Octal
            }
            else
            {
                e.Handled = true;
            }
        }

        public void EnterKeyHitHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SetData();
                if (this.Index != 255)
                    MainWindow.tb[Index + 1].Focus();
            }
        }
        public void GotFocusHandler(object sender, RoutedEventArgs e)
        {
            this.Select(0, this.MaxLength);
        }

        public void LostFocusHandler(object sender, RoutedEventArgs e)
        {
            SetData();
        }

        private void SetData()
        {
            while (this.Text.Length < this.MaxLength)
            {
                this.Text = this.Text.Insert(0, "0");
            }
            MainWindow.SetMIO(this.Index, Int32.Parse(this.Text, NumberStyles.HexNumber));
        }

        public void TextChangedHandler(object sender, TextChangedEventArgs args)
        {
            if (this.Text.Length != 0)
            {
                TextChange[] a = new TextChange[1];
                args.Changes.CopyTo(a, 0);
                if (a[0].AddedLength != 0)
                {
                    if (Uri.IsHexDigit(this.Text[a[0].Offset]) == false)
                    {
                        this.Text = this.Text.Remove(a[0].Offset, 1);
                        this.Text = this.Text.ToUpper();
                        this.Select(a[0].Offset, 0);
                    }
                    else
                    {
                        this.Text = this.Text.ToUpper();
                        this.Select(a[0].Offset + 1, 0);
                    }
                }
            }
        }
    }

    public class myTextBox4d : TextBox
    {
        public myTextBox4d()
            : base()
        {
            this.MaxLength = 4;
            this.TextAlignment = TextAlignment.Center;
            this.TextChanged += TextChangedHandler;
            this.GotFocus += GotFocusHandler;
            this.LostFocus += LostFocusHandler;
            this.ToolTipOpening += ToolTipOpeningHandler;
            this.ToolTip = "0";

        }

        void ToolTipOpeningHandler(object sender, ToolTipEventArgs e)
        {
            if (this.Text.Length <= 0)
            {
                e.Handled = true;
                return;
            }
            if (MainWindow.showBinaryValueTooltip == true)
            {
                //Update tooltip text
                if (MainWindow.toolTipFormatIndex == 0)
                {
                    string s = Convert.ToString(UInt16.Parse(this.Text, NumberStyles.HexNumber), 2);        //Binary
                    int i = s.Length;           //inserting a string at Length apeends the string
                    while (true)
                    {
                        if ((i - 4) > 0)
                        {
                            i = i - 4;
                            s = s.Insert(i, " ");
                        }
                        else
                            break;
                    }
                    this.ToolTip = s;
                }
                else if (MainWindow.toolTipFormatIndex == 1)
                    this.ToolTip = Int32.Parse(this.Text, NumberStyles.HexNumber);                          //Decimal
                else
                    this.ToolTip = Convert.ToString(UInt16.Parse(this.Text, NumberStyles.HexNumber), 8);    //Octal
            }
            else
            {
                e.Handled = true;
            }
        }

        public void GotFocusHandler(object sender, RoutedEventArgs e)
        {
            this.Select(0, this.MaxLength);
        }
        public void LostFocusHandler(object sender, RoutedEventArgs e)
        {
            while (this.Text.Length < this.MaxLength)
            {
                this.Text = this.Text.Insert(0, "0");
            }
        }
        public void TextChangedHandler(object sender, TextChangedEventArgs args)
        {
            if (this.Text.Length != 0)
            {
                TextChange[] a = new TextChange[1];
                args.Changes.CopyTo(a, 0);
                if (a[0].AddedLength != 0)
                {
                    if (Uri.IsHexDigit(this.Text[a[0].Offset]) == false)
                    {
                        this.Text = this.Text.Remove(a[0].Offset, 1);
                        this.Text = this.Text.ToUpper();
                        this.Select(a[0].Offset, 0);
                    }
                    else
                    {
                        this.Text = this.Text.ToUpper();
                        this.Select(a[0].Offset + 1, 0);
                    }
                }
            }
        }
    }
}