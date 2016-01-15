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
        private void bDisassembler_Click(object sender, RoutedEventArgs e)
        {
            TextRange tr2 = new TextRange(rtb3.Document.ContentStart, rtb3.Document.ContentEnd);
            tr2.Text = "";                                                                          //Deletes previous text in rtb3

            //Draws a table in rtb3 showing the disaasembly

            //Basic table
            rtb3.Document = new FlowDocument();
            Table t = new Table();
            rtb3.Document.Blocks.Add(t);
            t.Background = Brushes.White;
            t.TextAlignment = TextAlignment.Center;
            t.FontSize = 12;

            //Columns
            t.Columns.Add(new TableColumn());
            GridLength c0 = new GridLength(55);
            t.Columns[0].Width = c0;
            t.Columns[0].Background = Brushes.AliceBlue;
            t.Columns.Add(new TableColumn());
            GridLength c1 = new GridLength(240);
            t.Columns[1].Width = c1;
            t.Columns[1].Background = Brushes.White;
            t.Columns.Add(new TableColumn());
            GridLength c2 = new GridLength(35);
            t.Columns[2].Width = c2;
            t.Columns[2].Background = Brushes.Orange;

            //Header row
            int rc = 0;                             //rowcount
            t.RowGroups.Add(new TableRowGroup());
            t.RowGroups[rc].Rows.Add(new TableRow());
            TableRow cr = t.RowGroups[0].Rows[rc];   // Alias the current working row for easy reference
            cr.FontSize = 14;
            cr.FontWeight = System.Windows.FontWeights.Bold;
            cr.Cells.Add(new TableCell(new Paragraph(new Run("Address"))));
            cr.Cells.Add(new TableCell(new Paragraph(new Run("Instructions"))));
            cr.Cells.Add(new TableCell(new Paragraph(new Run("Hex Code"))));

            //TableData
            ushort startAdd = (ushort)Int32.Parse(tbStartAddress.Text, NumberStyles.HexNumber);
            ushort endAdd = (ushort)Int32.Parse(tbEndAddress.Text, NumberStyles.HexNumber);
            byte v0 = 0, v1 = 0, v2 = 0;
            string s;

            MyByte16 i = new MyByte16();
            i.DEC16 = startAdd;
            while(true)
            {                
                v0 = MainWindow.m[i.DEC16].DEC8;       //v0=data at current memory

                if (I.Names[v0, 0] == "1")
                {
                    t.RowGroups[0].Rows.Add(new TableRow());
                    rc++;
                    cr = t.RowGroups[0].Rows[rc];
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(i.DEC16.ToString("X4")))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(I.Names[v0, 1]))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(v0.ToString("X2")))));
                }
                else if (I.Names[v0, 0] == "2")
                {
                    if (i.DEC16 != 65535)
                        v1 = MainWindow.m[i.DEC16 + 1].DEC8;
                    else
                        v1 = MainWindow.m[0].DEC8;
                    s = String.Concat(I.Names[v0, 1], v1.ToString("X2"));

                    t.RowGroups[0].Rows.Add(new TableRow());
                    rc++;
                    cr = t.RowGroups[0].Rows[rc];
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(i.DEC16.ToString("X4")))));                    
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(s))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(v0.ToString("X2")))));
                    i.INX();

                    t.RowGroups[0].Rows.Add(new TableRow());
                    rc++;
                    cr = t.RowGroups[0].Rows[rc];
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(i.DEC16.ToString("X4")))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(v1.ToString("X2")))));
                }
                else if (I.Names[v0, 0] == "3")
                {
                    if (i.DEC16 != 65535)
                        v1 = MainWindow.m[i.DEC16 + 1].DEC8;
                    else
                        v1 = MainWindow.m[0].DEC8;
                    if ((i.DEC16+1) != 65535)
                        v2 = MainWindow.m[i.DEC16 + 2].DEC8;
                    else
                        v2 = MainWindow.m[0].DEC8;

                    s = String.Concat(I.Names[v0, 1], v2.ToString("X2"), v1.ToString("X2"));

                    t.RowGroups[0].Rows.Add(new TableRow());
                    rc++;
                    cr = t.RowGroups[0].Rows[rc];
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(i.DEC16.ToString("X4")))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(s))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(v0.ToString("X2")))));
                    i.INX();

                    t.RowGroups[0].Rows.Add(new TableRow());
                    rc++;
                    cr = t.RowGroups[0].Rows[rc];
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(i.DEC16.ToString("X4")))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(v1.ToString("X2")))));
                    i.INX();

                    t.RowGroups[0].Rows.Add(new TableRow());
                    rc++;
                    cr = t.RowGroups[0].Rows[rc];
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(i.DEC16.ToString("X4")))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                    cr.Cells.Add(new TableCell(new Paragraph(new Run(v2.ToString("X2")))));
                }

                if (i.DEC16 == endAdd)
                    break;
                else
                    i.INX();
            }
        }
    }
}