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
        private List<TextRange> registerAllMatches = new List<TextRange>();
        private List<TextRange> digitAllMatches = new List<TextRange>();
        private List<TextRange> keywordAllMatches = new List<TextRange>();
        private List<TextRange> commentAllMatches = new List<TextRange>();

        private void rtb1_TextChanged(object sender, TextChangedEventArgs e)
        {
            //These checks are necessary to invoke FormatCurrentLinefunction as it uses Caret related properties
            if (rtb1.Document == null)
                return;
            if (rtb1.CaretPosition.Paragraph == null)
                return;
            if (rtb1.CaretPosition.Paragraph.Inlines.FirstInline == null)
                return;

            FormatCurrentLine();

            IsSaved = false;
        }

        public void FormatCurrentLine()
        {
            /*These checks are necessary to invoke FormatCurrentLinefunction as it uses Caret related properties
            and is included in TextChange event handler. So, if you need to execute this function elsewhere, include these checks.
            
            if (rtb1.Document == null)
                return;
            if (rtb1.CaretPosition.Paragraph == null)
                return;
            if (rtb1.CaretPosition.Paragraph.Inlines.FirstInline == null)
                return;
            */

            int i;
            string s;
            Run r;

            TextRange paragraphRange = new TextRange(rtb1.CaretPosition.Paragraph.ContentStart, rtb1.CaretPosition.Paragraph.ContentEnd);
            paragraphRange.ClearAllProperties();

            r = (Run)(rtb1.CaretPosition.Paragraph.Inlines.FirstInline);
            if (r.Text == null)
                return;
            s = r.Text;

            registerMatches = register.Matches(s);
            digitMatches = digit.Matches(s);
            keywordMatches = keyword.Matches(s);
            commentMatches = comment.Matches(s);

            for (i = 0; i <= registerMatches.Count - 1; i++)
            {
                TextRange temp = new TextRange(r.ContentStart.GetPositionAtOffset(registerMatches[i].Index, LogicalDirection.Forward), r.ContentStart.GetPositionAtOffset(registerMatches[i].Index + registerMatches[i].Length, LogicalDirection.Forward));
                registerAllMatches.Add(temp);
            }

            for (i = 0; i <= digitMatches.Count - 1; i++)
            {
                TextRange temp = new TextRange(r.ContentStart.GetPositionAtOffset(digitMatches[i].Index, LogicalDirection.Forward), r.ContentStart.GetPositionAtOffset(digitMatches[i].Index + digitMatches[i].Length, LogicalDirection.Forward));
                digitAllMatches.Add(temp);
            }

            for (i = 0; i <= keywordMatches.Count - 1; i++)
            {
                TextRange temp = new TextRange(r.ContentStart.GetPositionAtOffset(keywordMatches[i].Index, LogicalDirection.Forward), r.ContentStart.GetPositionAtOffset(keywordMatches[i].Index + keywordMatches[i].Length, LogicalDirection.Forward));
                keywordAllMatches.Add(temp);
            }

            for (i = 0; i <= commentMatches.Count - 1; i++)
            {
                TextRange temp = new TextRange(r.ContentStart.GetPositionAtOffset(commentMatches[i].Groups["comment"].Index, LogicalDirection.Forward), r.ContentStart.GetPositionAtOffset(commentMatches[i].Groups["comment"].Index + commentMatches[i].Groups["comment"].Length, LogicalDirection.Forward));
                commentAllMatches.Add(temp);
            }

            Format();
        }

        public void FormatWholeDocument()
        {
            int i;
            string s;
            TextPointer navigator;
            TextPointerContext context;
            Run r;

            TextRange documentRange = new TextRange(rtb1.Document.ContentStart, rtb1.Document.ContentEnd);      //Make a textRange document point to the entire document
            documentRange.ClearAllProperties();                                                                 //Clears all formatting, so all all inlines within a paragraph are converted to a single inline but all paragraphs are kept

            navigator = rtb1.Document.ContentStart;
            while (navigator.CompareTo(rtb1.Document.ContentEnd) < 0)
            {
                context = navigator.GetPointerContext(LogicalDirection.Backward);
                if (context == TextPointerContext.ElementStart && navigator.Parent is Run)
                {
                    r = (Run)navigator.Parent;
                    s = r.Text;

                    registerMatches = register.Matches(s);
                    digitMatches = digit.Matches(s);
                    keywordMatches = keyword.Matches(s);
                    commentMatches = comment.Matches(s);

                    for (i = 0; i <= registerMatches.Count - 1; i++)
                    {
                        TextRange temp = new TextRange(r.ContentStart.GetPositionAtOffset(registerMatches[i].Index, LogicalDirection.Forward), r.ContentStart.GetPositionAtOffset(registerMatches[i].Index + registerMatches[i].Length, LogicalDirection.Forward));
                        registerAllMatches.Add(temp);
                    }

                    for (i = 0; i <= digitMatches.Count - 1; i++)
                    {
                        TextRange temp = new TextRange(r.ContentStart.GetPositionAtOffset(digitMatches[i].Index, LogicalDirection.Forward), r.ContentStart.GetPositionAtOffset(digitMatches[i].Index + digitMatches[i].Length, LogicalDirection.Forward));
                        digitAllMatches.Add(temp);
                    }

                    for (i = 0; i <= keywordMatches.Count - 1; i++)
                    {
                        TextRange temp = new TextRange(r.ContentStart.GetPositionAtOffset(keywordMatches[i].Index, LogicalDirection.Forward), r.ContentStart.GetPositionAtOffset(keywordMatches[i].Index + keywordMatches[i].Length, LogicalDirection.Forward));
                        keywordAllMatches.Add(temp);
                    }

                    for (i = 0; i <= commentMatches.Count - 1; i++)
                    {
                        TextRange temp = new TextRange(r.ContentStart.GetPositionAtOffset(commentMatches[i].Groups["comment"].Index, LogicalDirection.Forward), r.ContentStart.GetPositionAtOffset(commentMatches[i].Groups["comment"].Index + commentMatches[i].Groups["comment"].Length, LogicalDirection.Forward));
                        commentAllMatches.Add(temp);
                    }
                }
                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
            }

            Format();
        }

        public void Format()
        {
            int i;

            //Remove textchangehandler as formatting triggers textchange
            rtb1.TextChanged -= rtb1_TextChanged;

            for (i = 0; i <= registerAllMatches.Count - 1; i++)
            {
                registerAllMatches[i].ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Tomato));
            }
            registerAllMatches.Clear();

            for (i = 0; i <= digitAllMatches.Count - 1; i++)
            {
                digitAllMatches[i].ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Brown));
            }
            digitAllMatches.Clear();

            for (i = 0; i <= keywordAllMatches.Count - 1; i++)
            {
                keywordAllMatches[i].ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.SkyBlue));
            }
            keywordAllMatches.Clear();

            for (i = 0; i <= commentAllMatches.Count - 1; i++)
            {
                commentAllMatches[i].ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Green));
            }
            commentAllMatches.Clear();

            //Resubscribing 
            rtb1.TextChanged += rtb1_TextChanged;
        }
    }
}