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
        class Instructions
        {
            public string label;
            public string branchToLabel;
            public int startAddress;
            public int nob;
            public int lineNo;
            public MyByte8 opcode;
            public MyByte16 operand;    //For single operand, operand is in LB of operand
            public bool IsArgError;
            public bool IsUnrecognizedInstruction;

            public Instructions()
            {
                label = null;
                startAddress = 0;
                nob = 0;
                lineNo = 0;
                opcode = new MyByte8();
                operand = new MyByte16();
                IsArgError = false;                   //Caused only if a data value is of invalid length or if data characters are invalid(not[0-9a-f])  ... except for rst
                IsUnrecognizedInstruction = false;  //Last else block: Caused if an invalid reg is invalid or ins name itself is unrecognized and for invalid data of rst (i.e. not[0-7])
            }
        }
        List<Instructions> instructions = new List<Instructions>();

        private void bAssemble_Click(object sender, RoutedEventArgs e)
        {
            #region AsmComment
            /*//Note: '\s*\.org\s*(?<add>\d{1,4})h?' Take care. As ending h pattern is optional, the digit sequence named add
            //can extend to any length but only 4 digits will be matched. More digits will be ignored
            //But we want to ignore the higher digits, as higher digits will not fit in value.
            //So, change quantifier to + and apply ligic in code to ignore the higher digits

            //For 2 and 3 byte ins, 2nd and 3rd bytes are operands. Below operand means operand in opcode which are registers only.

            //Lines and label
            Regex lines = new Regex(@"\s*(?<ins>[0-9a-zA-Z\.:, ]+)(?(;).*?)\r\n", RegexOptions.IgnoreCase | RegexOptions.Multiline);//Only[a-z,A-Z,0-9.,(Dot for directives),Space AND ,] can appear in an instrictionLast section of Regex matches comments ,if a line is empty or contain only spaces,no match
            Regex emptyLine = new Regex(@"[0-9a-zA-Z\.:,]", RegexOptions.IgnoreCase);       //Exclude only spaces as compared to above
            Regex label = new Regex(@"^\s*(?<label>\w+):", RegexOptions.IgnoreCase);

            //Assembler Directives
             * Note: Assembler directives are executed while assembling only and not while program is executed
            Regex asmDirOrg = new Regex(@"\.org\s*(?<add>\d+)h?", RegexOptions.IgnoreCase);
            Regex asmDirBegin = new Regex(@"\.begin\s*(?<add>\d+)h?", RegexOptions.IgnoreCase);
            Regex asmDirEqu = new Regex(@"\.equ\s*(?<identifier>\w+)\s*(?<value>\d+)h?", RegexOptions.IgnoreCase);
            Regex asmDirSetByte = new Regex(@"\.setbyte\s*(?<add>\d+)\s*,\s*(?<value>\d+)h?", RegexOptions.IgnoreCase);
            Regex asmDirSetWord = new Regex(@"\.setword\s*(?<add>\d+)\s*,\s*(?<value>\d+)h?", RegexOptions.IgnoreCase);  //Set lowerbyte in add and higherbyte in (add+1)
            Regex asmDirFill = new Regex(@"\.fill\s*(?<addStart>\d+)\s*,\s*(?<addEnd>\d+)\s*,\s*(?<value>\d{1,2})h?", RegexOptions.IgnoreCase);

            //1 byte, No operand e.g. pchl, rar, rlc, rc(return if carry)
            Regex singleByteZeroOperand = new Regex(@"\b(?<name>\w+)\b", RegexOptions.IgnoreCase);

            //Move  (Only instruction for 1 byte, 2 operands both registers)
            Regex mov = new Regex(@"mov\s+(?<Rd>[abcdehlm]),(?<Rs>[abcdehlm])", RegexOptions.IgnoreCase);

            //1 byte, operand Register e.g. inr h, sub b, pop b, stax d, push psw, pop sp
            Regex singleByteOneOperandReg = new Regex(@"(?<name>\w{3,4})\s+(?<Rs>a|b|c|d|e|h|l|m|(psw)|(sp))", RegexOptions.IgnoreCase);

            //1 byte, operand single digit e.g. rst 0 to rst 7
            Regex singleByteOneOperandNum = new Regex(@"(?<name>\w{3})\s+(?<Num>[0-7])", RegexOptions.IgnoreCase);

            //2 byte, Zero Operand e.g. adi 33, out 20
            Regex imm8bitZeroOperand = new Regex(@"(?<name>\w{3})\s+(?<Imm8bitData>[0-9a-f]+)h?", RegexOptions.IgnoreCase);

            //2 byte, Operand Register e.g. mvi a,33
            Regex imm8bitReg = new Regex(@"(?<name>\w{3})\s+(?<Rd>[abcdehlm]),(?<Imm8bitData>[0-9a-f]+)h?", RegexOptions.IgnoreCase);

            //3 byte, zero operand e.g. shld 2000, jmp 2000, cc 2000
            Regex imm16bitZeroOperand = new Regex(@"(?<name>\w{2,4})\s+(?<Imm16bitData>[0-9a-f]+)h?", RegexOptions.IgnoreCase);

            //3 byte, Operand Register e.g. lxi b,3333
            Regex imm16bitReg = new Regex(@"lxi\s+(?<Rd>b|d|h|(psw)|(sp)|(pc)),(?<Imm16bitData>[0-9a-f]+)h?", RegexOptions.IgnoreCase);
            */

            //To colour individual words, use document.blocks.[individual blocks].inlines.foreground
            //Paragraph[] h =(Paragraph[])rtb1.Document.Blocks.ToArray();
            #endregion

            /*Note: How to recognize error: For last else, Unrecognized Instruction -> nob=0 and startadd = -1
            For outofbound value, value is rounded to 2 or 4 digits (No error)
             */

            //Note: Here ,Count is used as counter for placing addresses
            ResetRegisters();
            MyByte16 Count = new MyByte16();
            Count.DEC16 = 0;                    //As we have to begin to fill memory only from 0 and not from PCStartValue(i.e. Instruction pointer)
            instructions.Clear();                                                                   //Deletes all previously assembled instructions

            TextRange tr1 = new TextRange(rtb1.Document.ContentStart, rtb1.Document.ContentEnd);
            string WholeDoc = String.Copy(tr1.Text);
            MatchCollection lineMatches = lines.Matches(WholeDoc);

            for (int i = 0; i <= lineMatches.Count - 1; i++)
            {                
                if (emptyLine.IsMatch(lineMatches[i].Groups["ins"].Value) == false)   //Detecting empty line
                    continue;                

                Instructions temp = new Instructions();

                string s = lineMatches[i].Groups["ins"].Value;
                s = s.Insert(s.Length, "#");     //Used to check that nothing should come betn a valid ins and end of line

                //Note: Labels should be error for asm dir as a label points to a mem address. So, check if (nob=0 && label!=null)->Error 
                if (label.IsMatch(s))
                    temp.label = label.Match(s).Groups["label"].Value.ToLower();

                if (asmDirOrg.IsMatch(s))
                {
                    //Check for invalid arg
                    int NumIfHex = 0;
                    string add = asmDirOrg.Match(s).Groups["add"].Value;
                    if (add.Length <= 4 && add.Length >= 0 && Int32.TryParse(add, NumberStyles.HexNumber, provider, out NumIfHex))
                    {
                        Count.DEC16 = (ushort)NumIfHex;   //The next ins is placed at this add
                    }
                    else
                    {
                        temp.IsArgError = true;
                    }

                    temp.nob = 0;
                    temp.lineNo = i;
                }
                else if (asmDirBegin.IsMatch(s))
                {
                    //Check for invalid arg
                    int NumIfHex = 0;
                    string add = asmDirBegin.Match(s).Groups["add"].Value;
                    if (add.Length <= 4 && add.Length >= 0 && Int32.TryParse(add, NumberStyles.HexNumber, provider, out NumIfHex))
                    {
                        PCStartValue = (ushort)NumIfHex;  //Set IP to this add
                    }
                    else
                    {
                        temp.IsArgError = true;
                    }

                    temp.nob = 0;
                    temp.lineNo = i;
                }
                else if (asmDirSetByte.IsMatch(s))
                {
                    //Check for invalid arg
                    int NumIfHexAdd = 0, NumIfHexValue = 0;
                    string add = asmDirSetByte.Match(s).Groups["add"].Value;
                    string value = asmDirSetByte.Match(s).Groups["value"].Value;
                    if (add.Length <= 4 && add.Length >= 0 && Int32.TryParse(add, NumberStyles.HexNumber, provider, out NumIfHexAdd) && value.Length <= 2 && value.Length >= 0 && Int32.TryParse(value, NumberStyles.HexNumber, provider, out NumIfHexValue))
                    {
                        ushort addNum = (ushort)NumIfHexAdd;
                        byte valueNum = (byte)NumIfHexValue;
                        m[addNum].DEC8 = valueNum;
                    }
                    else
                    {
                        temp.IsArgError = true;
                    }

                    temp.nob = 0;
                    temp.lineNo = i;
                }
                else if (asmDirSetWord.IsMatch(s))
                {
                    //Check for invalid arg
                    int NumIfHexAdd = 0, NumIfHexValue = 0;
                    string add = asmDirSetWord.Match(s).Groups["add"].Value;
                    string value = asmDirSetWord.Match(s).Groups["value"].Value;
                    if (add.Length <= 4 && add.Length >= 0 && Int32.TryParse(add, NumberStyles.HexNumber, provider, out NumIfHexAdd) && value.Length <= 2 && value.Length >= 0 && Int32.TryParse(value, NumberStyles.HexNumber, provider, out NumIfHexValue))
                    {
                        MyByte16 addNum = new MyByte16();
                        addNum.DEC16 = (ushort)NumIfHexAdd;
                        byte valueNum = (byte)NumIfHexValue;
                        MyByte16 myValue = new MyByte16();

                        m[addNum.DEC16].DEC8 = myValue.LB.DEC8;
                        addNum.INX();
                        m[addNum.DEC16].DEC8 = myValue.HB.DEC8;
                    }
                    else
                    {
                        temp.IsArgError = true;
                    }

                    temp.nob = 0;
                    temp.lineNo = i;
                }
                else if (asmDirFill.IsMatch(s))
                {
                    //Check for invalid arg
                    int NumIfHexAddStart = 0, NumIfHexAddEnd = 0, NumIfHexValue = 0;
                    string addStart = asmDirFill.Match(s).Groups["addStart"].Value;
                    string addEnd = asmDirFill.Match(s).Groups["addEnd"].Value;
                    string value = asmDirFill.Match(s).Groups["value"].Value;
                    if (addStart.Length <= 4 && addStart.Length >= 0 && Int32.TryParse(addStart, NumberStyles.HexNumber, provider, out NumIfHexAddStart) && addEnd.Length <= 4 && addEnd.Length >= 0 && Int32.TryParse(addEnd, NumberStyles.HexNumber, provider, out NumIfHexAddEnd) && value.Length <= 2 && value.Length >= 0 && Int32.TryParse(value, NumberStyles.HexNumber, provider, out NumIfHexValue))
                    {
                        ushort addStartNum = (ushort)NumIfHexAddStart;
                        ushort addEndNum = (ushort)NumIfHexAddEnd;
                        byte valueNum = (byte)NumIfHexValue;

                        for (int c = addStartNum; c <= addEndNum; c++)
                            m[c].DEC8 = valueNum;
                    }
                    else
                    {
                        temp.IsArgError = true;
                    }

                    temp.nob = 0;
                    temp.lineNo = i;
                }
                /*else if (asmDirEqu.IsMatch(s))
                {
                    //Not necessary directive

                    //Replce the EquString by its value in tr1.text and reflect it in rtb2
                    //string identifier = asmDirEqu.Match(s).Groups["identifier"].Value;
                    //string value = asmDirEqu.Match(s).Groups["value"].Value;
                    //WholeDoc.Replace(identifier,value);

                    temp.nob = 0;
                    temp.lineNo = i;
                }*/
                else if (mov.IsMatch(s))
                {
                    temp.startAddress = Count.DEC16;
                    temp.nob = 1;
                    temp.lineNo = i;
                    temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 64);

                    string rd, rs;
                    rd = mov.Match(s).Groups["Rd"].Value.ToLower();
                    rs = mov.Match(s).Groups["Rs"].Value.ToLower();

                    switch (rs)
                    {
                        case "b":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 0);
                                break;
                            }
                        case "c":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 1);
                                break;
                            }
                        case "d":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 2);
                                break;
                            }
                        case "e":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 3);
                                break;
                            }
                        case "h":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 4);
                                break;
                            }
                        case "l":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 5);
                                break;
                            }
                        case "m":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 6);
                                break;
                            }
                        case "a":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 7);
                                break;
                            }
                    }

                    switch (rd)
                    {
                        case "b":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 0);
                                break;
                            }
                        case "c":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 8);
                                break;
                            }
                        case "d":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 16);
                                break;
                            }
                        case "e":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 24);
                                break;
                            }
                        case "h":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 32);
                                break;
                            }
                        case "l":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 40);
                                break;
                            }
                        case "m":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 48);
                                break;
                            }
                        case "a":
                            {
                                temp.opcode.DEC8 = (byte)(temp.opcode.DEC8 + 56);
                                break;
                            }
                    }
                }
                else if (rst.IsMatch(s))
                {
                    temp.startAddress = Count.DEC16;
                    temp.nob = 1;
                    temp.lineNo = i;

                    string num = rst.Match(s).Groups["Num"].Value.ToLower();

                    switch (num)
                    {
                        case "0":
                            {
                                temp.opcode.DEC8 = 199;
                                break;
                            }
                        case "1":
                            {
                                temp.opcode.DEC8 = 207;
                                break;
                            }
                        case "2":
                            {
                                temp.opcode.DEC8 = 215;
                                break;
                            }
                        case "3":
                            {
                                temp.opcode.DEC8 = 223;
                                break;
                            }
                        case "4":
                            {
                                temp.opcode.DEC8 = 231;
                                break;
                            }
                        case "5":
                            {
                                temp.opcode.DEC8 = 239;
                                break;
                            }
                        case "6":
                            {
                                temp.opcode.DEC8 = 247;
                                break;
                            }
                        case "7":
                            {
                                temp.opcode.DEC8 = 255;
                                break;
                            }
                    }
                }
                else if (mvi.IsMatch(s))
                {
                    temp.startAddress = Count.DEC16;
                    temp.nob = 2;
                    temp.lineNo = i;

                    //Check for invalid arg
                    int NumIfHex = 0;
                    string immData = mvi.Match(s).Groups["Imm8bitData"].Value;
                    if (immData.Length <= 2 && immData.Length >= 0 && Int32.TryParse(immData, NumberStyles.HexNumber, provider, out NumIfHex))
                    {
                        temp.operand.LB.DEC8 = (byte)NumIfHex;
                    }
                    else
                    {
                        temp.IsArgError = true;
                    }

                    string rd = mvi.Match(s).Groups["Rd"].Value.ToLower();

                    switch (rd)
                    {
                        case "b":
                            {
                                temp.opcode.DEC8 = 6;
                                break;
                            }
                        case "c":
                            {
                                temp.opcode.DEC8 = 14;
                                break;
                            }
                        case "d":
                            {
                                temp.opcode.DEC8 = 22;
                                break;
                            }
                        case "e":
                            {
                                temp.opcode.DEC8 = 30;
                                break;
                            }
                        case "h":
                            {
                                temp.opcode.DEC8 = 38;
                                break;
                            }
                        case "l":
                            {
                                temp.opcode.DEC8 = 46;
                                break;
                            }
                        case "m":
                            {
                                temp.opcode.DEC8 = 54;
                                break;
                            }
                        case "a":
                            {
                                temp.opcode.DEC8 = 62;
                                break;
                            }
                    }
                }
                else if (lxi.IsMatch(s))
                {
                    temp.startAddress = Count.DEC16;
                    temp.nob = 3;
                    temp.lineNo = i;

                    //Check for invalid arg
                    int NumIfHex = 0;
                    string immData = lxi.Match(s).Groups["Imm16bitData"].Value;
                    if (immData.Length <= 4 && immData.Length >= 0 && Int32.TryParse(immData, NumberStyles.HexNumber, provider, out NumIfHex))
                    {
                        temp.operand.DEC16 = (ushort)NumIfHex;
                    }
                    else
                    {
                        temp.IsArgError = true;
                    }

                    string rd = lxi.Match(s).Groups["Rd"].Value.ToLower();

                    switch (rd)
                    {
                        case "b":
                            {
                                temp.opcode.DEC8 = 1;
                                break;
                            }
                        case "d":
                            {
                                temp.opcode.DEC8 = 17;
                                break;
                            }
                        case "h":
                            {
                                temp.opcode.DEC8 = 33;
                                break;
                            }
                        case "sp":
                            {
                                temp.opcode.DEC8 = 49;
                                break;
                            }
                        default:
                            {
                                temp.IsUnrecognizedInstruction = true;
                                break;
                            }
                    }
                }
                else if (singleByteOneOperandReg.IsMatch(s))
                {
                    temp.startAddress = Count.DEC16;
                    temp.nob = 1;
                    temp.lineNo = i;

                    string name = singleByteOneOperandReg.Match(s).Groups["name"].Value.ToLower();
                    string rs = singleByteOneOperandReg.Match(s).Groups["Rs"].Value.ToLower();

                    switch (name)
                    {
                        case "push":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 197;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 213;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 229;
                                            break;
                                        }
                                    case "psw":
                                        {
                                            temp.opcode.DEC8 = 245;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "pop":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 193;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 209;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 225;
                                            break;
                                        }
                                    case "psw":
                                        {
                                            temp.opcode.DEC8 = 241;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "stax":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 2;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 18;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "ldax":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 10;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 26;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "inx":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 3;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 19;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 35;
                                            break;
                                        }
                                    case "sp":
                                        {
                                            temp.opcode.DEC8 = 51;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "dcx":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 11;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 27;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 43;
                                            break;
                                        }
                                    case "sp":
                                        {
                                            temp.opcode.DEC8 = 59;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "dad":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 9;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 25;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 41;
                                            break;
                                        }
                                    case "sp":
                                        {
                                            temp.opcode.DEC8 = 57;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "inr":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 4;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 12;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 20;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 28;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 36;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 44;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 52;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 60;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "dcr":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 5;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 13;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 21;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 29;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 37;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 45;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 53;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 61;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "add":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 128;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 129;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 130;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 131;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 132;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 133;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 134;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 135;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "adc":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 136;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 137;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 138;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 139;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 140;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 141;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 142;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 143;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "sub":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 144;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 145;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 146;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 147;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 148;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 149;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 150;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 151;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "sbb":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 152;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 153;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 154;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 155;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 156;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 157;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 158;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 159;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "ana":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 160;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 161;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 162;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 163;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 164;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 165;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 166;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 167;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "xra":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 168;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 169;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 170;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 171;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 172;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 173;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 174;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 175;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "ora":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 176;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 177;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 178;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 179;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 180;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 181;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 182;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 183;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "cmp":
                            {
                                switch (rs)
                                {
                                    case "b":
                                        {
                                            temp.opcode.DEC8 = 184;
                                            break;
                                        }
                                    case "c":
                                        {
                                            temp.opcode.DEC8 = 185;
                                            break;
                                        }
                                    case "d":
                                        {
                                            temp.opcode.DEC8 = 186;
                                            break;
                                        }
                                    case "e":
                                        {
                                            temp.opcode.DEC8 = 187;
                                            break;
                                        }
                                    case "h":
                                        {
                                            temp.opcode.DEC8 = 188;
                                            break;
                                        }
                                    case "l":
                                        {
                                            temp.opcode.DEC8 = 189;
                                            break;
                                        }
                                    case "m":
                                        {
                                            temp.opcode.DEC8 = 190;
                                            break;
                                        }
                                    case "a":
                                        {
                                            temp.opcode.DEC8 = 191;
                                            break;
                                        }
                                    default:
                                        {
                                            temp.IsUnrecognizedInstruction = true;
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                }
                else if (imm8bitZeroOperand.IsMatch(s))
                {
                    temp.startAddress = Count.DEC16;
                    temp.nob = 2;
                    temp.lineNo = i;

                    //Check for invalid arg
                    int NumIfHex = 0;
                    string immData = imm8bitZeroOperand.Match(s).Groups["Imm8bitData"].Value;
                    if (immData.Length <= 2 && immData.Length >= 0 && Int32.TryParse(immData, NumberStyles.HexNumber, provider, out NumIfHex))
                    {
                        temp.operand.LB.DEC8 = (byte)NumIfHex;
                    }
                    else
                    {
                        temp.IsArgError = true;
                    }

                    string name = imm8bitZeroOperand.Match(s).Groups["name"].Value.ToLower();

                    switch (name)
                    {
                        case "adi":
                            {
                                temp.opcode.DEC8 = 198;
                                break;
                            }
                        case "aci":
                            {
                                temp.opcode.DEC8 = 206;
                                break;
                            }
                        case "out":
                            {
                                temp.opcode.DEC8 = 211;
                                break;
                            }
                        case "sui":
                            {
                                temp.opcode.DEC8 = 214;
                                break;
                            }
                        case "in":
                            {
                                temp.opcode.DEC8 = 219;
                                break;
                            }
                        case "sbi":
                            {
                                temp.opcode.DEC8 = 222;
                                break;
                            }
                        case "ani":
                            {
                                temp.opcode.DEC8 = 230;
                                break;
                            }
                        case "xri":
                            {
                                temp.opcode.DEC8 = 238;
                                break;
                            }
                        case "ori":
                            {
                                temp.opcode.DEC8 = 246;
                                break;
                            }
                        case "cpi":
                            {
                                temp.opcode.DEC8 = 254;
                                break;
                            }
                    }
                }
                else if (imm16bitZeroOperand.IsMatch(s))
                {
                    temp.startAddress = Count.DEC16;
                    temp.nob = 3;
                    temp.lineNo = i;

                    //Check for invalid arg
                    int NumIfHex = 0;
                    string immData = imm16bitZeroOperand.Match(s).Groups["Imm16bitData"].Value;
                    if (immData.Length <= 4 && immData.Length >= 0 && Int32.TryParse(immData, NumberStyles.HexNumber, provider, out NumIfHex))
                    {
                        temp.operand.DEC16 = (ushort)NumIfHex;
                    }
                    else
                    {
                        temp.IsArgError = true;
                    }

                    string name = imm16bitZeroOperand.Match(s).Groups["name"].Value.ToLower();

                    switch (name)
                    {
                        case "shld":
                            {
                                temp.opcode.DEC8 = 34;
                                break;
                            }
                        case "lhld":
                            {
                                temp.opcode.DEC8 = 42;
                                break;
                            }
                        case "sta":
                            {
                                temp.opcode.DEC8 = 50;
                                break;
                            }
                        case "lda":
                            {
                                temp.opcode.DEC8 = 58;
                                break;
                            }
                    }
                }
                else if (imm16bitZeroOperandSupportLabel.IsMatch(s))
                {
                    temp.startAddress = Count.DEC16;
                    temp.nob = 3;
                    temp.lineNo = i;

                    Match myMatch = imm16bitZeroOperandSupportLabel.Match(s);
                    if (myMatch.Groups["label"].Success)
                    {
                        temp.branchToLabel = myMatch.Groups["label"].Value.ToLower();   //Case insensitive label
                    }
                    else
                    {
                        //Check for invalid arg
                        int NumIfHex = 0;
                        string immData = imm16bitZeroOperandSupportLabel.Match(s).Groups["Imm16bitData"].Value;
                        if (immData.Length <= 4 && immData.Length >= 0 && Int32.TryParse(immData, NumberStyles.HexNumber, provider, out NumIfHex))
                        {
                            temp.operand.DEC16 = (ushort)NumIfHex;
                        }
                        else
                        {
                            temp.IsArgError = true;
                        }
                    }

                    string name = imm16bitZeroOperandSupportLabel.Match(s).Groups["name"].Value.ToLower();

                    switch (name)
                    {
                        case "jnz":
                            {
                                temp.opcode.DEC8 = 194;
                                break;
                            }
                        case "jmp":
                            {
                                temp.opcode.DEC8 = 195;
                                break;
                            }
                        case "cnz":
                            {
                                temp.opcode.DEC8 = 196;
                                break;
                            }
                        case "jz":
                            {
                                temp.opcode.DEC8 = 202;
                                break;
                            }
                        case "cz":
                            {
                                temp.opcode.DEC8 = 204;
                                break;
                            }
                        case "call":
                            {
                                temp.opcode.DEC8 = 205;
                                break;
                            }
                        case "jnc":
                            {
                                temp.opcode.DEC8 = 210;
                                break;
                            }
                        case "cnc":
                            {
                                temp.opcode.DEC8 = 212;
                                break;
                            }
                        case "jc":
                            {
                                temp.opcode.DEC8 = 218;
                                break;
                            }
                        case "cc":
                            {
                                temp.opcode.DEC8 = 220;
                                break;
                            }
                        case "jpo":
                            {
                                temp.opcode.DEC8 = 226;
                                break;
                            }
                        case "cpo":
                            {
                                temp.opcode.DEC8 = 228;
                                break;
                            }
                        case "jpe":
                            {
                                temp.opcode.DEC8 = 236;
                                break;
                            }
                        case "cpe":
                            {
                                temp.opcode.DEC8 = 238;
                                break;
                            }
                        case "jp":
                            {
                                temp.opcode.DEC8 = 242;
                                break;
                            }
                        case "cp":
                            {
                                temp.opcode.DEC8 = 244;
                                break;
                            }
                        case "jm":
                            {
                                temp.opcode.DEC8 = 250;
                                break;
                            }
                        case "cm":
                            {
                                temp.opcode.DEC8 = 252;
                                break;
                            }
                    }
                }
                else if (singleByteZeroOperand.IsMatch(s))
                {
                    temp.startAddress = Count.DEC16;
                    temp.nob = 1;
                    temp.lineNo = i;

                    string name = singleByteZeroOperand.Match(s).Groups["name"].Value.ToLower();

                    switch (name)
                    {
                        case "nop":
                            {
                                temp.opcode.DEC8 = 0;
                                break;
                            }
                        case "rlc":
                            {
                                temp.opcode.DEC8 = 7;
                                break;
                            }
                        case "rrc":
                            {
                                temp.opcode.DEC8 = 15;
                                break;
                            }
                        case "ral":
                            {
                                temp.opcode.DEC8 = 23;
                                break;
                            }
                        case "rar":
                            {
                                temp.opcode.DEC8 = 31;
                                break;
                            }
                        case "rim":
                            {
                                temp.opcode.DEC8 = 32;
                                break;
                            }
                        case "daa":
                            {
                                temp.opcode.DEC8 = 39;
                                break;
                            }
                        case "cma":
                            {
                                temp.opcode.DEC8 = 47;
                                break;
                            }
                        case "sim":
                            {
                                temp.opcode.DEC8 = 48;
                                break;
                            }
                        case "stc":
                            {
                                temp.opcode.DEC8 = 55;
                                break;
                            }
                        case "cmc":
                            {
                                temp.opcode.DEC8 = 63;
                                break;
                            }
                        case "hlt":
                            {
                                temp.opcode.DEC8 = 118;
                                break;
                            }
                        case "rnz":
                            {
                                temp.opcode.DEC8 = 192;
                                break;
                            }
                        case "rz":
                            {
                                temp.opcode.DEC8 = 200;
                                break;
                            }
                        case "ret":
                            {
                                temp.opcode.DEC8 = 201;
                                break;
                            }
                        case "rnc":
                            {
                                temp.opcode.DEC8 = 208;
                                break;
                            }
                        case "rc":
                            {
                                temp.opcode.DEC8 = 216;
                                break;
                            }
                        case "rpo":
                            {
                                temp.opcode.DEC8 = 224;
                                break;
                            }
                        case "xthl":
                            {
                                temp.opcode.DEC8 = 227;
                                break;
                            }
                        case "rpe":
                            {
                                temp.opcode.DEC8 = 232;
                                break;
                            }
                        case "pchl":
                            {
                                temp.opcode.DEC8 = 233;
                                break;
                            }
                        case "xchg":
                            {
                                temp.opcode.DEC8 = 235;
                                break;
                            }
                        case "rp":
                            {
                                temp.opcode.DEC8 = 240;
                                break;
                            }
                        case "di":
                            {
                                temp.opcode.DEC8 = 243;
                                break;
                            }
                        case "rm":
                            {
                                temp.opcode.DEC8 = 248;
                                break;
                            }
                        case "sphl":
                            {
                                temp.opcode.DEC8 = 249;
                                break;
                            }
                        case "ei":
                            {
                                temp.opcode.DEC8 = 251;
                                break;
                            }
                    }
                }
                else
                {
                    //Compiler Error : Unrecogized Instruction
                    temp.IsUnrecognizedInstruction = true;
                    temp.lineNo = i;
                }

                instructions.Add(temp);
                for (int k = 0; k <= temp.nob - 1; k++)
                {
                    Count.INX();
                }
            }

            ShowCompiledCode();
        }

        public void ReplaceLabel()
        {
            //This function replaces labels

            for (int i = 0; i <= instructions.Count - 1; i++)
            {
                int j;
                Instructions c = instructions[i];
                if (c.nob == 3 && c.branchToLabel != null)      //Instrction contains branchtolabel
                {
                    for (j = 0; j <= instructions.Count - 1; j++)
                    {
                        if (c.branchToLabel == instructions[j].label)
                        {
                            c.operand.DEC16 = (ushort)instructions[j].startAddress;
                            break;
                        }
                    }
                }
                else
                {
                    continue;
                }

                if (j == instructions.Count)    //Means no match found
                {
                    c.IsArgError = true;
                }
            }
        }

        private void ShowCompiledCode()
        {
            ReplaceLabel();

            bool AnyError = false;
            bLoad.IsEnabled = false;

            TextRange tr2 = new TextRange(rtb2.Document.ContentStart, rtb2.Document.ContentEnd);
            tr2.Text = "";                                                                          //Deletes previous text in rtb2

            //Draws a table in rtb2 showing the compiled code

            //Basic table
            rtb2.Document = new FlowDocument();
            Table t = new Table();
            rtb2.Document.Blocks.Add(t);
            t.Background = Brushes.White;
            t.TextAlignment = TextAlignment.Center;
            t.FontSize = 12;

            //Columns
            t.Columns.Add(new TableColumn());
            GridLength c0 = new GridLength(55);
            t.Columns[0].Width = c0;
            t.Columns[0].Background = Brushes.AliceBlue;
            t.Columns.Add(new TableColumn());
            GridLength c1 = new GridLength(55);
            t.Columns[1].Width = c1;
            t.Columns[1].Background = Brushes.BlanchedAlmond;
            t.Columns.Add(new TableColumn());
            GridLength c2 = new GridLength(240);
            t.Columns[2].Width = c2;
            t.Columns[2].Background = Brushes.White;
            t.Columns.Add(new TableColumn());
            GridLength c3 = new GridLength(35);
            t.Columns[3].Width = c3;
            t.Columns[3].Background = Brushes.Orange;

            //Header row
            int rc = 0;         //rowcount
            t.RowGroups.Add(new TableRowGroup());
            t.RowGroups[rc].Rows.Add(new TableRow());
            TableRow currentRow = t.RowGroups[0].Rows[0];   // Alias the current working row for easy reference
            currentRow.FontSize = 14;
            currentRow.FontWeight = System.Windows.FontWeights.Bold;
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Label"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Address"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Instructions"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Hex Code"))));

            //TableData
            for (int i = 0; i <= instructions.Count - 1; i++)
            {
                Instructions c = instructions[i];
                t.RowGroups[0].Rows.Add(new TableRow());
                rc++;
                TableRow cr = t.RowGroups[0].Rows[rc];

                if (c.label != null && c.nob == 0)  //Error : Current line is asmDir but contains label
                {
                    AnyError = true;
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                    string sName = "Error on Line ";
                    sName = String.Concat(sName, i.ToString("X4"));
                    sName = String.Concat(sName, " Labels not supported on AsmDir and Blank lines as they are not in memory");
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(sName))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                }
                else if (c.IsUnrecognizedInstruction)
                {
                    AnyError = true;
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run("Error : Unrecognized Instruction"))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                }
                else if (c.IsArgError)
                {
                    AnyError = true;
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run("Error : Invalid Argument"))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                }
                else
                {
                    if (c.nob == 1)
                    {
                        if (c.label == null)
                            cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                        else
                            cr.Cells.Add(new TableCell(new Paragraph(new Run(c.label.ToUpper()))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(c.startAddress.ToString("X4")))));
                        string sName = I.Names[c.opcode.DEC8, 1];
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(sName))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(c.opcode.DEC8.ToString("X2")))));
                    }
                    else if (c.nob == 2)
                    {
                        if (c.label == null)
                            cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                        else
                            cr.Cells.Add(new TableCell(new Paragraph(new Run(c.label.ToUpper()))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(c.startAddress.ToString("X4")))));
                        string sName = I.Names[c.opcode.DEC8, 1];
                        sName = String.Concat(sName, c.operand.LB.DEC8.ToString("X2"));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(sName))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(c.opcode.DEC8.ToString("X2")))));

                        t.RowGroups[0].Rows.Add(new TableRow());
                        rc++;
                        cr = t.RowGroups[0].Rows[rc];
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                        if (c.startAddress != 65535)
                            cr.Cells.Add(new TableCell(new Paragraph(new Run((c.startAddress + 1).ToString("X4")))));
                        else
                            cr.Cells.Add(new TableCell(new Paragraph(new Run("0000"))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(c.operand.LB.DEC8.ToString("X2")))));

                    }
                    else if (c.nob == 3)
                    {
                        if (c.label == null)
                            cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                        else
                            cr.Cells.Add(new TableCell(new Paragraph(new Run(c.label.ToUpper()))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(c.startAddress.ToString("X4")))));
                        string sName = I.Names[c.opcode.DEC8, 1];
                        sName = String.Concat(sName, c.operand.DEC16.ToString("X4"));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(sName))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(c.opcode.DEC8.ToString("X2")))));

                        t.RowGroups[0].Rows.Add(new TableRow());
                        rc++;
                        cr = t.RowGroups[0].Rows[rc];
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                        if (c.startAddress != 65535)
                            cr.Cells.Add(new TableCell(new Paragraph(new Run((c.startAddress + 1).ToString("X4")))));
                        else
                            cr.Cells.Add(new TableCell(new Paragraph(new Run("0000"))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(c.operand.LB.DEC8.ToString("X2")))));

                        t.RowGroups[0].Rows.Add(new TableRow());
                        rc++;
                        cr = t.RowGroups[0].Rows[rc];
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                        if (c.startAddress != 65535)
                            cr.Cells.Add(new TableCell(new Paragraph(new Run((c.startAddress + 1).ToString("X4")))));
                        else
                            cr.Cells.Add(new TableCell(new Paragraph(new Run("0000"))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                        cr.Cells.Add(new TableCell(new Paragraph(new Run(c.operand.HB.DEC8.ToString("X2")))));

                    }
                }
            }

            if (AnyError == true)
                bLoad.IsEnabled = false;
            else
                bLoad.IsEnabled = true;
        }

        private void bLoad_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i <= instructions.Count - 1; i++)
            {
                if (instructions[i].nob == 0)
                {
                    //Asm dir
                }
                else
                {
                    m[instructions[i].startAddress].DEC8 = instructions[i].opcode.DEC8;
                    if (instructions[i].nob == 2)
                    {
                        m[instructions[i].startAddress + 1].DEC8 = instructions[i].operand.LB.DEC8;
                    }
                    else if (instructions[i].nob == 3)
                    {
                        m[instructions[i].startAddress + 1].DEC8 = instructions[i].operand.LB.DEC8;
                        m[instructions[i].startAddress + 2].DEC8 = instructions[i].operand.HB.DEC8;
                    }
                }
            }
            RefreshMIO(false);
        }
    }
}