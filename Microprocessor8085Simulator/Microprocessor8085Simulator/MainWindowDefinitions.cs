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
using System.IO;
using System.Threading;
using Microsoft.Win32;

namespace Microprocessor8085Simulator
{
    public partial class MainWindow : Window
    {
        public static MyByteM M;                                        //Memory Pointer = (HL)
        public static MyByte16 BC, DE, HL, PC, SP, PSW;                 //Registers

        public static MyByte8[] m = new MyByte8[65536];
        public static MyByte8[] io = new MyByte8[256];

        public delegate void ExecuteStepDelegate();                     //Delegate used for Dispatcher

        public static TextBox[] lb = new TextBox[32];                   //Textboxes used as row,col memory labels on MemoryEditor 
        public static myTextBox2d[] tb = new myTextBox2d[256];          //Textboxes used to display 256 memlocations in a page MemoryEditor

        public static int hexStart;                                     //4 digit starting index of page of MemoryEditor
        public static ushort PCStartValue = 0;

        public static CultureInfo provider = CultureInfo.InvariantCulture;

        public static bool IsStopped;
        public static bool IsRbMemoryChecked;

        public static string LI = "";
        public static string NI = "";
        public static int IC;
        public static int CCC;

        public static bool inte = false;
        public static bool changeInte = false;                          //(whether to change inte)As ei,di affects inte f/f duting next instruction
        public static bool changeInteTo = false;                        //(whether to change inte to true or false)
        public static MyByte8 interrupt = new MyByte8();                //Used for rim/sim
        public static bool r75 = true;                                  //R7.5 f/f

        public static TextBox[] FArray;

        //ExecuteStep() is used many times in program. If you declare these variables as local variables of ExecuteStep(),
        //there will be a lot of memory leak.
        MyByte8 Opcode = new MyByte8();
        MyByte8 Rs = new MyByte8();
        MyByte8 Rd = new MyByte8();
        MyByte8 Operand8 = new MyByte8();
        MyByte8 OperandLB = new MyByte8();
        MyByte8 OperandHB = new MyByte8();
        MyByte16 Rpc = new MyByte16();
        MyByte16 Operand16 = new MyByte16();

        //Regular Expressions for Assembler
        //Note: '\s*\.org\s*(?<add>\d{1,4})h?' Take care. As ending h pattern is optional, the digit sequence named add
        //can extend to any length but only 4 digits will be matched. More digits will be ignored
        //But we want to ignore the higher digits, as higher digits will not fit in value.
        //So, change quantifier to + and apply ligic in code to ignore the higher digits

        //For 2 and 3 byte ins, 2nd and 3rd bytes are operands. Below operand means operand in opcode which are registers only.

        //Lines and label
        Regex lines = new Regex(@"\s*(?<ins>[0-9a-zA-Z\.:, \t]+)?(?(;).*?)\r\n", RegexOptions.IgnoreCase | RegexOptions.Multiline);//Only[a-z,A-Z,0-9.,(Dot for directives),Space AND ,] can appear in an instrictionLast section of Regex matches comments ,if a line is empty or contain only spaces,no match
        Regex emptyLine = new Regex(@"[0-9a-zA-Z\.:,]", RegexOptions.IgnoreCase);       //Exclude only spaces as compared to above        
        Regex label = new Regex(@"^\s*(?<label>\w+):", RegexOptions.IgnoreCase);

        //Assembler Directives
        Regex asmDirOrg = new Regex(@"\.org\s+(?<add>[a-f0-9]+)h?\s*#", RegexOptions.IgnoreCase);
        Regex asmDirBegin = new Regex(@"\.begin\s+(?<add>[a-f0-9]+)h?\s*#", RegexOptions.IgnoreCase);
        Regex asmDirEqu = new Regex(@"\.equ\s+(?<identifier>\w+)\s+(?<value>[a-f0-9]+)h?\s*#", RegexOptions.IgnoreCase);
        Regex asmDirSetByte = new Regex(@"\.setbyte\s+(?<add>[a-f0-9]+)h?\s*,\s*(?<value>[a-f0-9]+)h?\s*#", RegexOptions.IgnoreCase);
        Regex asmDirSetWord = new Regex(@"\.setword\s+(?<add>[a-f0-9]+)h?\s*,\s*(?<value>[a-f0-9]+)h?\s*#", RegexOptions.IgnoreCase);  //Set lowerbyte in add and higherbyte in (add+1)
        Regex asmDirFill = new Regex(@"\.fill\s+(?<addStart>[a-f0-9]+)h?\s*,\s*(?<addEnd>[a-f0-9]+)h?\s*,\s*(?<value>[a-f0-9]+)h?\s*#", RegexOptions.IgnoreCase);

        //Move  (Only instruction for 1 byte, 2 operands both registers)
        Regex mov = new Regex(@"mov\s+(?<Rd>[abcdehlm])\s*,\s*(?<Rs>[abcdehlm])\s*#", RegexOptions.IgnoreCase);

        //rst e.g. rst 0 to rst 7
        Regex rst = new Regex(@"rst\s+(?<Num>[0-7])\s*#", RegexOptions.IgnoreCase);

        //2 byte, Operand Register e.g. mvi a,33
        Regex mvi = new Regex(@"mvi\s+(?<Rd>[abcdehlm])\s*,\s*(?<Imm8bitData>[0-9a-f]+)h?\s*#", RegexOptions.IgnoreCase);

        //3 byte, Operand Register e.g. lxi b,3333
        Regex lxi = new Regex(@"lxi\s+(?<Rd>(b|d|h|(sp)))\s*,\s*(?<Imm16bitData>[0-9a-f]+)h?\s*#", RegexOptions.IgnoreCase);

        //1 byte, operand Register e.g. inr h, sub b, pop b, stax d, push psw, pop sp
        Regex singleByteOneOperandReg = new Regex(@"(?<name>((pop)|(push)|(stax)|(ldax)|(inx)|(dcx)|(dad)|(inr)|(dcr)|(add)|(adc)|(sub)|(sbb)|(ana)|(xra)|(ora)|(cmp)))\s+(?<Rs>a|b|c|d|e|h|l|m|(psw)|(sp))\s*#", RegexOptions.IgnoreCase);

        //2 byte, Zero Operand e.g. adi 33, out 20
        Regex imm8bitZeroOperand = new Regex(@"(?<name>((adi)|(aci)|(out)|(sui)|(in)|(sbi)|(ani)|(xri)|(ori)|(cpi)))\s+(?<Imm8bitData>[0-9a-f]+)h?\s*#", RegexOptions.IgnoreCase);

        //3 byte, zero operand e.g. shld 2000, jmp 2000, cc 2000
        Regex imm16bitZeroOperand = new Regex(@"(?<name>((shld)|(lhld)|(sta)|(lda)))\s+(?<Imm16bitData>[0-9a-f]+)h?\s*#", RegexOptions.IgnoreCase);

        //3 byte, zero operand that support label
        //This is only ins where a label name can be ambigious with an address e.g. ffab can be a label or a address
        //To resolve this ambiguity, addresses must be preceded by a 0 e.g. accc is a label while 0accc is an address
        Regex imm16bitZeroOperandSupportLabel = new Regex(@"(?<name>((jnz)|(jmp)|(cnz)|(jz)|(cz)|(call)|(jnc)|(cnc)|(jc)|(cc)|(jpo)|(cpo)|(jpe)|(cpe)|(jp)|(cp)|(jm)|(cm)))\s+(?([0])(0(?<Imm16bitData>[0-9a-f]+)h?)|(?<label>\w+))\s*#", RegexOptions.IgnoreCase);

        //1 byte, No operand e.g. pchl, rar, rlc, rc(return if carry)
        Regex singleByteZeroOperand = new Regex(@"\b(?<name>((nop)|(rlc)|(rrc)|(rar)|(ral)|(rim)|(daa)|(cma)|(sim)|(stc)|(cmc)|(hlt)|(rnz)|(rz)|(ret)|(rnc)|(rc)|(rpo)|(xthl)|(rpe)|(pchl)|(xchg)|(rp)|(di)|(rm)|(sphl)|(ei)))\b\s*#", RegexOptions.IgnoreCase);

        //Regular Expressions for Syntax Highlighling
        Regex register = new Regex(@"\b([abcdehlm]|(pc)|(sp)|(psw))\b", RegexOptions.IgnoreCase);
        MatchCollection registerMatches;
        Regex digit = new Regex(@"\b[0-9a-f]{2,5}h?\b", RegexOptions.IgnoreCase);
        MatchCollection digitMatches;
        Regex keyword = new Regex(@"\b((mov)|(rst)|(mvi)|(add)|(adi)|(adc)|(aci)|(sub)|(sbb)|(sui)|(sbi)|(inr)|(dcr)|(ana)|(ani)|(xra)|(xri)|(ora)|(ori)|(cmp)|(cpi)|(inx)|(dcx)|(lxi)|(dad)|(push)|(pop)|(ldax)|(stax)|(ral)|(rar)|(rlc)|(rrc)|(sim)|(rim)|(lda)|(sta)|(lhld)|(shld)|(xchg)|(in)|(out)|(jmp)|(jc)|(jnc)|(jz)|(jnz)|(jp)|(jm)|(jpe)|(jpo)|(call)|(cc)|(cnc)|(cz)|(cnz)|(cp)|(cm)|(cpe)|(cpo)|(ret)|(rc)|(rnc)|(rz)|(rnz)|(rp)|(rm)|(rpo)|(rpe)|(pchl)|(sphl)|(xthl)|(nop)|(ei)|(di)|(hlt)|(daa))\b", RegexOptions.IgnoreCase);
        MatchCollection keywordMatches;
        Regex comment = new Regex(@"(?(;).*?(?<comment>;.*))", RegexOptions.IgnoreCase);
        MatchCollection commentMatches;

        bool IsSaved;

        //Preferences
        public static bool? showBinaryValueTooltip = true;
        public static int toolTipFormatIndex = 0;           //0-binary,1-Dec,2-octal
    }
}