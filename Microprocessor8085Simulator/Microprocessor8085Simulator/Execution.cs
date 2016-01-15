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
        /* Execute Step Comments:
                The temp variables Rs,Rd,Operand8,etc registers are defined in constructor Main() method to avoid memory leaks.
                But, this also means they will also retain the same value even after ExecuteStep() is called again.
                So, for any temp variables, modify all its constituent variables then only use it.
                e.g. Do not use Operand16 by modifying just its LB or HB.      
        */
        private void ExecuteStep()
        {
            IC++;
            CCC = CCC + Int32.Parse(I.Names[m[PC.DEC16].DEC8, 2]);

            if (changeInte == true)     //Interrupt control logic (as ei/de affects inte f/f only during next instruction)
            {
                if (changeInteTo == true)
                    inte = true;
                else
                    inte = false;

                changeInte = false;

                UpdateInterrupt();
            }

            Opcode.DEC8 = m[PC.DEC16].DEC8;

            //  * DATA TRANSFER GROUP *

            if (BitCombination(Opcode[7], Opcode[6]) == "01")   //MOV,HLT
            {
                //Except Rs=Rd=M 01110110 hlt
                if (BitCombination(Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "110110")
                {
                    //Halt Rs=Rd=M 01110110 hlt
                    Stop();
                    return;
                }
                else
                {
                    Rs.DEC8 = Get8BitRegisterValue(Opcode[2], Opcode[1], Opcode[0]);
                    Set8BitRegisterValue(Rs.DEC8, Opcode[5], Opcode[4], Opcode[3]);

                    PC.INX();
                }
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[2], Opcode[1], Opcode[0]) == "00110")  //MVI Rd,8BitData
            {
                PC.INX();

                Operand8.DEC8 = m[PC.DEC16].DEC8;
                Set8BitRegisterValue(Operand8.DEC8, Opcode[5], Opcode[4], Opcode[3]);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "000001")  //LXI Rp,16BitData
            {
                PC.INX();
                Operand16.LB.DEC8 = m[PC.DEC16].DEC8;

                PC.INX();
                Operand16.HB.DEC8 = m[PC.DEC16].DEC8;

                Set16BitRegisterValue(true, Operand16.DEC16, Opcode[5], Opcode[4]);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00111010")  //LDA
            {
                PC.INX();
                Operand16.LB.DEC8 = m[PC.DEC16].DEC8;

                PC.INX();
                Operand16.HB.DEC8 = m[PC.DEC16].DEC8;

                PSW.HB.DEC8 = m[Operand16.DEC16].DEC8;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00110010")  //STA
            {
                PC.INX();
                Operand16.LB.DEC8 = m[PC.DEC16].DEC8;

                PC.INX();
                Operand16.HB.DEC8 = m[PC.DEC16].DEC8;

                m[Operand16.DEC16].DEC8 = PSW.HB.DEC8;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00101010")  //LHLD
            {
                PC.INX();
                Operand16.LB.DEC8 = m[PC.DEC16].DEC8;

                PC.INX();
                Operand16.HB.DEC8 = m[PC.DEC16].DEC8;

                HL.LB.DEC8 = m[Operand16.DEC16].DEC8;

                Operand16.INX();
                HL.HB.DEC8 = m[Operand16.DEC16].DEC8;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00100010")  //SHLD
            {
                PC.INX();
                Operand16.LB.DEC8 = m[PC.DEC16].DEC8;

                PC.INX();
                Operand16.HB.DEC8 = m[PC.DEC16].DEC8;

                m[Operand16.DEC16].DEC8 = HL.LB.DEC8;

                Operand16.INX();
                m[Operand16.DEC16].DEC8 = HL.HB.DEC8;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00001010")  //LDAX B
            {
                PSW.HB.DEC8 = m[BC.DEC16].DEC8;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00011010")  //LDAX D
            {
                PSW.HB.DEC8 = m[DE.DEC16].DEC8;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11101010")  //XCHG
            {
                Operand16.DEC16 = HL.DEC16;
                HL.DEC16 = DE.DEC16;
                DE.DEC16 = Operand16.DEC16;

                PC.INX();
            }
            else if (Opcode.DEC8 == 219)  //IN 8bitIOPortAddress
            {
                PC.INX();

                PSW.HB.DEC8 = io[m[PC.DEC16].DEC8].DEC8;

                PC.INX();
            }
            else if (Opcode.DEC8 == 211)  //OUT 8bitIOPortAddress
            {
                PC.INX();

                io[m[PC.DEC16].DEC8].DEC8 = PSW.HB.DEC8;

                PC.INX();
            }
            //  * ARITHMETIC GROUP  *
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3]) == "10000")  //ADD Rs
            {
                Rs.DEC8 = Get8BitRegisterValue(Opcode[2], Opcode[1], Opcode[0]);

                if ((PSW.HB.GetLowerNibble() + Rs.GetLowerNibble()) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((PSW.HB.DEC8 + Rs.DEC8) > 255)
                {
                    PSW.LB[0] = true;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Rs.DEC8 - 256);
                }
                else
                {
                    PSW.LB[0] = false;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Rs.DEC8);
                }

                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3]) == "10001")  //ADC R
            {
                Rs.DEC8 = Get8BitRegisterValue(Opcode[2], Opcode[1], Opcode[0]);

                byte i;
                if (PSW.LB[0] == true)
                    i = 1;
                else
                    i = 0;

                if ((PSW.HB.GetLowerNibble() + Rs.GetLowerNibble() + i) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((PSW.HB.DEC8 + Rs.DEC8 + i) > 255)
                {
                    PSW.LB[0] = true;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Rs.DEC8 + i - 256);
                }
                else
                {
                    PSW.LB[0] = false;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Rs.DEC8 + i);
                }

                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11000110")  //ADI 8bitdata
            {
                PC.INX();

                Operand8.DEC8 = m[PC.DEC16].DEC8;

                if ((PSW.HB.GetLowerNibble() + Operand8.GetLowerNibble()) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((PSW.HB.DEC8 + Operand8.DEC8) > 255)
                {
                    PSW.LB[0] = true;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Operand8.DEC8 - 256);
                }
                else
                {
                    PSW.LB[0] = false;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Operand8.DEC8);
                }

                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11001110")  //ACI 8bitdata
            {
                PC.INX();

                Operand8.DEC8 = m[PC.DEC16].DEC8;

                byte i;
                if (PSW.LB[0] == true)
                    i = 1;
                else
                    i = 0;

                if ((PSW.HB.GetLowerNibble() + Operand8.GetLowerNibble() + i) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((PSW.HB.DEC8 + Operand8.DEC8 + i) > 255)
                {
                    PSW.LB[0] = true;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Operand8.DEC8 + i - 256);
                }
                else
                {
                    PSW.LB[0] = false;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Operand8.DEC8 + i);
                }

                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[2], Opcode[1], Opcode[0]) == "00100")  //INR R
            {
                Rs.DEC8 = Get8BitRegisterValue(Opcode[5], Opcode[4], Opcode[3]);

                if (Rs.GetLowerNibble() == 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if (Rs.DEC8 == 255)
                {
                    Rs.DEC8 = (byte)(0 + 1);
                }
                else
                {
                    Rs.DEC8 = (byte)(Rs.DEC8 + 1);
                }

                Set8BitRegisterValue(Rs.DEC8, Opcode[5], Opcode[4], Opcode[3]);
                UpdateP(Rs.DEC8);
                UpdateS(Rs.DEC8);
                UpdateZ(Rs.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3]) == "10010")  //SUB R
            {
                Rs.DEC8 = Get8BitRegisterValue(Opcode[2], Opcode[1], Opcode[0]);

                Rs.DEC8 = Rs.Get2sComplement();

                //In 2's complement addition, complement both CY and AC. Here, it is done directly in if conditional
                //by inverting true and false.

                if ((PSW.HB.GetLowerNibble() + Rs.GetLowerNibble()) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((PSW.HB.DEC8 + Rs.DEC8) > 255)
                {
                    PSW.LB[0] = false;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Rs.DEC8 - 256);
                }
                else
                {
                    PSW.LB[0] = true;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Rs.DEC8);
                }

                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3]) == "10011")  //SBB R
            {
                Rs.DEC8 = Get8BitRegisterValue(Opcode[2], Opcode[1], Opcode[0]);

                if (PSW.LB[0] == true)
                {
                    if (Rs.DEC8 == 255)
                        Rs.DEC8 = 0;
                    else
                        Rs.DEC8 = (byte)(Rs.DEC8 + 1);
                }

                Rs.DEC8 = Rs.Get2sComplement();

                if ((PSW.HB.GetLowerNibble() + Rs.GetLowerNibble()) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((PSW.HB.DEC8 + Rs.DEC8) > 255)
                {
                    PSW.LB[0] = false;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Rs.DEC8 - 256);
                }
                else
                {
                    PSW.LB[0] = true;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Rs.DEC8);
                }

                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11010110")  //SUI 8bitData
            {
                PC.INX();

                Operand8.DEC8 = m[PC.DEC16].DEC8;

                Operand8.DEC8 = Operand8.Get2sComplement();

                if ((PSW.HB.GetLowerNibble() + Operand8.GetLowerNibble()) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((PSW.HB.DEC8 + Operand8.DEC8) > 255)
                {
                    PSW.LB[0] = false;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Operand8.DEC8 - 256);
                }
                else
                {
                    PSW.LB[0] = true;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Operand8.DEC8);
                }

                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11011110")  //SBI 8BitDAta
            {
                PC.INX();

                Operand8.DEC8 = m[PC.DEC16].DEC8;

                if (PSW.LB[0] == true)
                {
                    if (Operand8.DEC8 == 255)
                        Operand8.DEC8 = 0;
                    else
                        Operand8.DEC8 = (byte)(Operand8.DEC8 + 1);
                }

                Operand8.DEC8 = Operand8.Get2sComplement();

                if ((PSW.HB.GetLowerNibble() + Operand8.GetLowerNibble()) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((PSW.HB.DEC8 + Operand8.DEC8) > 255)
                {
                    PSW.LB[0] = false;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Operand8.DEC8 - 256);
                }
                else
                {
                    PSW.LB[0] = true;
                    PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + Operand8.DEC8);
                }

                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[2], Opcode[1], Opcode[0]) == "00101")  //DCR R
            {
                /*Rs.DEC8 = Get8BitRegisterValue(Opcode[5], Opcode[4], Opcode[3]);

                Rs.DEC8 = Rs.Get2sComplement();

                if (Rs.GetLowerNibble() == 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if (Rs.DEC8 == 255)
                {
                    Rs.DEC8 = (byte)(0 + 1);
                }
                else
                {
                    Rs.DEC8 = (byte)(Rs.DEC8 + 1);
                }

                Set8BitRegisterValue(Rs.DEC8, Opcode[5], Opcode[4], Opcode[3]);
                UpdateP(Rs.DEC8);
                UpdateS(Rs.DEC8);
                UpdateZ(Rs.DEC8);

                PC.INX();
                 * */

                Rd.DEC8 = Get8BitRegisterValue(Opcode[5], Opcode[4], Opcode[3]);

                Rs.DEC8 = 1;                            //As 1 is substracted
                Rs.DEC8 = Rs.Get2sComplement();

                //In 2's complement addition, complement both CY and AC. Here, it is done directly in if conditional
                //by inverting true and false.

                if ((Rd.GetLowerNibble() + Rs.GetLowerNibble()) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((Rd.DEC8 + Rs.DEC8) > 255)
                {
                    PSW.LB[0] = false;
                    Rd.DEC8 = (byte)(Rd.DEC8 + Rs.DEC8 - 256);
                }
                else
                {
                    PSW.LB[0] = true;
                    Rd.DEC8 = (byte)(Rd.DEC8 + Rs.DEC8);
                }

                Set8BitRegisterValue(Rd.DEC8, Opcode[5], Opcode[4], Opcode[3]);

                UpdateP(Rd.DEC8);
                UpdateS(Rd.DEC8);
                UpdateZ(Rd.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "000011")  //INX Rp
            {
                switch (BitCombination(Opcode[5], Opcode[4]))
                {
                    case "00":
                        {
                            BC.INX();
                            break;
                        }
                    case "01":
                        {
                            DE.INX();
                            break;
                        }
                    case "10":
                        {
                            HL.INX();
                            break;
                        }
                    case "11":
                        {
                            SP.INX();
                            break;
                        }
                }
                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "001011")  //DCX Rp
            {
                switch (BitCombination(Opcode[5], Opcode[4]))
                {
                    case "00":
                        {
                            BC.DCX();
                            break;
                        }
                    case "01":
                        {
                            DE.DCX();
                            break;
                        }
                    case "10":
                        {
                            HL.DCX();
                            break;
                        }
                    case "11":
                        {
                            SP.DCX();
                            break;
                        }
                }
                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "001001")  //DAD Rp,16BitData
            {
                Operand16.DEC16 = Get16BitRegisterValue(true, Opcode[5], Opcode[4]);

                if ((HL.DEC16 + Operand16.DEC16) > 65535)
                {
                    PSW.LB[0] = true;
                    HL.DEC16 = (ushort)(HL.DEC16 + Operand16.DEC16 - 65536);
                }
                else
                {
                    PSW.LB[0] = false;
                    HL.DEC16 = (ushort)(HL.DEC16 + Operand16.DEC16);
                }

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00100111")  //DAA
            {
                if ((PSW.HB.GetLowerNibble() > 9) || (PSW.LB[4] == true))
                {
                    //Add 6 to lower nibble
                    if ((PSW.HB.GetLowerNibble() + 6) > 15)
                    {
                        PSW.LB[4] = true;
                        PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + 6 - 16); //6 is added, 16 is sub to neutralize overflow out of nibble
                    }
                    else
                    {
                        PSW.LB[4] = false;
                        PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + 6);
                    }
                }
                else
                    PSW.LB[4] = false;

                byte i;
                if (PSW.LB[4] == true)
                    i = 1;
                else
                    i = 0;
                if ((PSW.HB.GetHigherNibble() > 9) || (PSW.LB[0] == true))
                {
                    bool PreviousCY = PSW.LB[0];

                    //Add 6 to higher nibble
                    if ((PSW.HB.GetHigherNibble() + 6 + i) > 15)
                    {
                        PSW.LB[0] = true;
                        PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + (6 + i - 16) * 16);  //i is carry from lower nibble, Multiply by 16 to reach uppaer nibble ( i.e. up by 4 bits 2^4)
                    }
                    else
                    {
                        PSW.LB[0] = false;
                        PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 + (6 + i) * 16);
                    }

                    if (PreviousCY)                                             //Set CY if it was initially set 
                        PSW.LB[0] = true;
                }

                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            //   * LOGICAL GROUP *
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3]) == "10100")  //ANA R
            {
                Rs.DEC8 = Get8BitRegisterValue(Opcode[2], Opcode[1], Opcode[0]);

                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 & Rs.DEC8);

                PSW.LB[0] = false;
                PSW.LB[4] = true;
                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11100110")  //ANI 8bitData
            {
                PC.INX();

                Operand8.DEC8 = m[PC.DEC16].DEC8;

                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 & Operand8.DEC8);

                PSW.LB[0] = false;
                PSW.LB[4] = true;
                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3]) == "10110")  //ORA R
            {
                Rs.DEC8 = Get8BitRegisterValue(Opcode[2], Opcode[1], Opcode[0]);

                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 | Rs.DEC8);

                PSW.LB[0] = false;
                PSW.LB[4] = false;
                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11110110")  //ORI 8bitData
            {
                PC.INX();

                Operand8.DEC8 = m[PC.DEC16].DEC8;

                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 | Operand8.DEC8);

                PSW.LB[0] = false;
                PSW.LB[4] = false;
                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3]) == "10101")  //XRA R
            {
                Rs.DEC8 = Get8BitRegisterValue(Opcode[2], Opcode[1], Opcode[0]);

                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 ^ Rs.DEC8);

                PSW.LB[0] = false;
                PSW.LB[4] = false;
                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11101110")  //XRI 8bitData
            {
                PC.INX();

                Operand8.DEC8 = m[PC.DEC16].DEC8;

                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 ^ Operand8.DEC8);

                PSW.LB[0] = false;
                PSW.LB[4] = false;
                UpdateP(PSW.HB.DEC8);
                UpdateS(PSW.HB.DEC8);
                UpdateZ(PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00101111")  //CMA
            {
                PSW.HB.DEC8 = (byte)(~PSW.HB.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00111111")  //CMC
            {
                PSW.LB[0] = !PSW.LB[0];

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00110111")  //STC
            {
                PSW.LB[0] = true;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3]) == "10111")  //CMP R
            {
                Operand8.DEC8 = PSW.HB.DEC8;

                Rs.DEC8 = Get8BitRegisterValue(Opcode[2], Opcode[1], Opcode[0]);
                Rs.DEC8 = Rs.Get2sComplement();

                if ((Operand8.GetLowerNibble() + Rs.GetLowerNibble()) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((Operand8.DEC8 + Rs.DEC8) > 255)
                {
                    PSW.LB[0] = false;
                    Operand8.DEC8 = (byte)(Operand8.DEC8 + Rs.DEC8 - 256);
                }
                else
                {
                    PSW.LB[0] = true;
                    Operand8.DEC8 = (byte)(Operand8.DEC8 + Rs.DEC8);
                }
                UpdateP(Operand8.DEC8);
                UpdateS(Operand8.DEC8);
                UpdateZ(Operand8.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3]) == "10111")  //CPI 8bitData
            {
                Operand8.DEC8 = PSW.HB.DEC8;

                PC.INX();

                Rs.DEC8 = m[PC.DEC16].DEC8;
                Rs.DEC8 = Rs.Get2sComplement();

                if ((Operand8.GetLowerNibble() + Rs.GetLowerNibble()) > 15)
                    PSW.LB[4] = true;
                else
                    PSW.LB[4] = false;

                if ((Operand8.DEC8 + Rs.DEC8) > 255)
                {
                    PSW.LB[0] = false;
                    Operand8.DEC8 = (byte)(Operand8.DEC8 + Rs.DEC8 - 256);
                }
                else
                {
                    PSW.LB[0] = true;
                    Operand8.DEC8 = (byte)(Operand8.DEC8 + Rs.DEC8);
                }
                UpdateP(Operand8.DEC8);
                UpdateS(Operand8.DEC8);
                UpdateZ(Operand8.DEC8);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00000111")  //RLC
            {
                bool D7 = PSW.HB[7];
                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 << 1);
                PSW.HB[0] = D7;
                PSW.LB[0] = D7;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00001111")  //RRC
            {
                bool D0 = PSW.HB[0];
                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 >> 1);
                PSW.HB[7] = D0;
                PSW.LB[0] = D0;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00010111")  //RAL
            {
                bool D7 = PSW.HB[7];
                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 << 1);
                PSW.HB[0] = PSW.LB[0];
                PSW.LB[0] = D7;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "00011111")  //RAR
            {
                bool D0 = PSW.HB[0];
                PSW.HB.DEC8 = (byte)(PSW.HB.DEC8 >> 1);
                PSW.HB[7] = PSW.LB[0];
                PSW.LB[0] = D0;

                PC.INX();
            }
            //  * STACK AND MACHINE CONTROL GROUP   *
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "110101")  //PUSH Rp
            {
                //FOR Push/Pop and in Branching only , IsSP = false i.e. return PC
                Operand16.DEC16 = Get16BitRegisterValue(false, Opcode[5], Opcode[4]);

                SP.DCX();
                m[SP.DEC16].DEC8 = Operand16.HB.DEC8;

                SP.DCX();
                m[SP.DEC16].DEC8 = Operand16.LB.DEC8;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "110001")  //POP Rp
            {
                Operand16.LB.DEC8 = m[SP.DEC16].DEC8;
                SP.INX();

                Operand16.HB.DEC8 = m[SP.DEC16].DEC8;
                SP.INX();

                Set16BitRegisterValue(false, Operand16.DEC16, Opcode[5], Opcode[4]);

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11111001")  //SPHL
            {
                SP.DEC16 = HL.DEC16;

                PC.INX();
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11100011")  //XTHL
            {
                Operand16.DEC16 = HL.DEC16;

                HL.LB.DEC8 = m[SP.DEC16].DEC8;
                m[SP.DEC16].DEC8 = Operand16.LB.DEC8;

                if (SP.DEC16 == 255)    //Check for overflow
                {
                    HL.HB.DEC8 = m[0].DEC8;
                    m[0].DEC8 = Operand16.HB.DEC8;
                }
                else
                {
                    HL.HB.DEC8 = m[SP.DEC16 + 1].DEC8;
                    m[SP.DEC16 + 1].DEC8 = Operand16.HB.DEC8;
                }

                PC.INX();
            }
            else if (Opcode.DEC8 == 0)  //NOP
            {
                PC.INX();
            }
            else if (Opcode.DEC8 == 251)   //EI
            {
                changeInte = true;
                changeInteTo = true;

                PC.INX();
            }
            else if (Opcode.DEC8 == 251)   //DI
            {
                changeInte = true;
                changeInteTo = false;

                PC.INX();
            }
            else if (Opcode.DEC8 == 32)  //RIM
            {
                //Read sid and output to d7 bit of accumulator
                if (rbSid0.IsChecked == true)
                    PC.HB[7] = false;
                else
                    PC.HB[7] = true;

                //No logic for interrupts pending in simulator

                //Read inte
                if (inte == true)
                    PC.HB[3] = true;
                else
                    PC.HB[3] = false;

                //Reads masks
                if (interrupt[2] == true)   //M7.5
                    PC.HB[2] = true;
                else
                    PC.HB[2] = false;

                if (interrupt[1] == true)   //M6.5
                    PC.HB[1] = true;
                else
                    PC.HB[1] = false;
                
                if (interrupt[0] == true)   //M5.5
                    PC.HB[0] = true;
                else
                    PC.HB[0] = false;

                PC.INX();
                UpdateInterrupt();
            }
            else if (Opcode.DEC8 == 48)  //SIM
            {
                //Set sod=1 if sde=1 else sod=0
                if (PC.HB[6] == true)
                {
                    if (PC.HB[7] == true)
                        tbSod.Text = "1";
                    else
                        tbSod.Text = "0";
                }

                if (PC.HB[4] == true)   //Resets r7.5 f/f, else has no effect
                    r75 = false;

                if (PC.HB[3] == true)   //MSE
                {
                    if (PC.HB[2] == true)   //M'7.5
                        interrupt[2] = true;
                    else
                        interrupt[2] = false;

                    if (PC.HB[1] == true)   //M'6.5
                        interrupt[1] = true;
                    else
                        interrupt[1] = false;

                    if (PC.HB[0] == true)   //M'5.5
                        interrupt[0] = true;
                    else
                        interrupt[0] = false;
                }

                PC.INX();
                UpdateInterrupt();
            }
            //  * BRANCH GROUP  *
            //In branch group, take care of whether to execute PC.INX() or not at end.
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[5], Opcode[4], Opcode[3], Opcode[2], Opcode[1], Opcode[0]) == "11101001")  //PCHL
            {
                PC.DEC16 = HL.DEC16;
            }
            else if (Opcode.DEC8 == 195)  //JMP
            {
                PC.INX();
                Operand16.LB.DEC8 = m[PC.DEC16].DEC8;

                PC.INX();
                Operand16.HB.DEC8 = m[PC.DEC16].DEC8;

                PC.DEC16 = Operand16.DEC16;
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[2], Opcode[1], Opcode[0]) == "11010")  //Conditional Jump
            {
                if (CheckCondition(Opcode[5], Opcode[4], Opcode[3]) == true)
                {
                    PC.INX();
                    Operand16.LB.DEC8 = m[PC.DEC16].DEC8;

                    PC.INX();
                    Operand16.HB.DEC8 = m[PC.DEC16].DEC8;

                    PC.DEC16 = Operand16.DEC16;
                }
                else
                {
                    PC.INX();
                    PC.INX();
                    PC.INX();
                }
            }
            else if (Opcode.DEC8 == 205)  //CALL
            {
                //Storing address to be branched in temp register Operand16
                PC.INX();
                Operand16.LB.DEC8 = m[PC.DEC16].DEC8;

                PC.INX();
                Operand16.HB.DEC8 = m[PC.DEC16].DEC8;

                PC.INX();

                //Push contents of PC onto stack
                SP.DCX();
                m[SP.DEC16].DEC8 = PC.HB.DEC8;

                SP.DCX();
                m[SP.DEC16].DEC8 = PC.LB.DEC8;

                //Branch                
                PC.DEC16 = Operand16.DEC16;
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[2], Opcode[1], Opcode[0]) == "11100")  //Conditional CALL
            {
                if (CheckCondition(Opcode[5], Opcode[4], Opcode[3]) == true)
                {
                    //Storing address to be branched in temp register Operand16
                    PC.INX();
                    Operand16.LB.DEC8 = m[PC.DEC16].DEC8;

                    PC.INX();
                    Operand16.HB.DEC8 = m[PC.DEC16].DEC8;

                    PC.INX();

                    //Push contents of PC onto stack
                    SP.DCX();
                    m[SP.DEC16].DEC8 = PC.HB.DEC8;

                    SP.DCX();
                    m[SP.DEC16].DEC8 = PC.LB.DEC8;

                    //Branch                
                    PC.DEC16 = Operand16.DEC16;
                }
                else
                {
                    PC.INX();
                    PC.INX();
                    PC.INX();
                }
            }
            else if (Opcode.DEC8 == 201)  //RET
            {
                //Pop content from stack to temp RegisterPair Operand16 
                Operand16.LB.DEC8 = m[SP.DEC16].DEC8;
                SP.INX();

                Operand16.HB.DEC8 = m[SP.DEC16].DEC8;
                SP.INX();

                //Branch
                PC.DEC16 = Operand16.DEC16;
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[2], Opcode[1], Opcode[0]) == "11000")  //Conditional Return
            {
                if (CheckCondition(Opcode[5], Opcode[4], Opcode[3]) == true)
                {
                    Operand16.LB.DEC8 = m[SP.DEC16].DEC8;
                    SP.INX();

                    Operand16.HB.DEC8 = m[SP.DEC16].DEC8;
                    SP.INX();

                    //Branch
                    PC.DEC16 = Operand16.DEC16;
                }
                else
                {
                    PC.INX();
                }
            }
            else if (BitCombination(Opcode[7], Opcode[6], Opcode[2], Opcode[1], Opcode[0]) == "11111")  //RST N
            {
                PC.INX();

                //Push contents of PC onto stack
                SP.DCX();
                m[SP.DEC16].DEC8 = PC.HB.DEC8;

                SP.DCX();
                m[SP.DEC16].DEC8 = PC.LB.DEC8;

                //Branch
                switch (BitCombination(Opcode[5], Opcode[4], Opcode[3]))
                {
                    case "000":
                        {
                            PC.DEC16 = 0;
                            break;
                        }
                    case "001":
                        {
                            PC.DEC16 = 8;
                            break;
                        }
                    case "010":
                        {
                            PC.DEC16 = 16;
                            break;
                        }
                    case "011":
                        {
                            PC.DEC16 = 24;
                            break;
                        }
                    case "100":
                        {
                            PC.DEC16 = 32;
                            break;
                        }
                    case "101":
                        {
                            PC.DEC16 = 40;
                            break;
                        }
                    case "110":
                        {
                            PC.DEC16 = 48;
                            break;
                        }
                    case "111":
                        {
                            PC.DEC16 = 56;
                            break;
                        }
                }
            }
            else
            {
                //Unrecognised Instructions
                PC.INX();
            }

            RefreshMIO(false);     //Nofocus, as it will cause Stop button never to gain focus
            UpdateAll();
            //Rucurse in Fast Mode but in idle priority
            if (rbFast.IsChecked == true)
            {
                bStep.IsEnabled = false;
                if (IsStopped == false)
                {
                    bStart.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new ExecuteStepDelegate(ExecuteStep));
                }
            }
        }
    }
}