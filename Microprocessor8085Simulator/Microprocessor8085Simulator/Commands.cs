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
        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            rtb1.Document.Blocks.Clear();
        }        

        void ExitCommandHandler(object sender, RoutedEventArgs e)
        {
            if (IsSaved == false)
            {
                MessageBoxResult result = MessageBox.Show(this, "Save changes to current document?", "Save Current Document", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
                if (result == MessageBoxResult.No)
                    Application.Current.Shutdown();
                if (result == MessageBoxResult.Yes)
                {
                    SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = "Document"; // Default file name
                    dlg.DefaultExt = ".text"; // Default file extension
                    dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

                    // Show save file dialog box
                    Nullable<bool> saveResult = dlg.ShowDialog();

                    // Process save file dialog box results
                    if (saveResult == true)
                    {
                        // Save document
                        string filename = dlg.FileName;

                        TextRange range;
                        FileStream fStream;
                        range = new TextRange(rtb1.Document.ContentStart, rtb1.Document.ContentEnd);
                        fStream = new FileStream(filename, FileMode.Create);
                        range.Save(fStream, DataFormats.Text);
                        fStream.Close();

                        IsSaved = true;
                        Application.Current.Shutdown();
                    }
                }
            }
            else
                Application.Current.Shutdown();
        }
        void PasteCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            rtb1.Paste();
            FormatWholeDocument();
        }
        private void OpenCommandHandler(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;

                TextRange range;
                FileStream fStream;
                if (File.Exists(filename))
                {
                    tabControl1.SelectedIndex = 1;
                    range = new TextRange(rtb1.Document.ContentStart, rtb1.Document.ContentEnd);
                    fStream = new FileStream(filename, FileMode.OpenOrCreate);
                    range.Load(fStream, DataFormats.Text);
                    fStream.Close();                    
                }
                FormatWholeDocument();
            }
        }
        private void SaveCommandHandler(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".text"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                TextRange range;
                FileStream fStream;
                range = new TextRange(rtb1.Document.ContentStart, rtb1.Document.ContentEnd);
                fStream = new FileStream(filename, FileMode.Create);
                range.Save(fStream, DataFormats.Text);
                fStream.Close();

                IsSaved = true;
            }
        }
    }
}