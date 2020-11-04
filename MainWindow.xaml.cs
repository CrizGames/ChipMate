
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ChipMate
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            emu.rgb = args.Contains("--rgb");

            //ChooseROM();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            emu.interpreter.keyboard.OnKeyDown(e.Key);
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            emu.interpreter.keyboard.OnKeyUp(e.Key);
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_OpenRom_Click(object sender, RoutedEventArgs e)
        {
            ChooseROM();
        }

        public void ChooseROM()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Choose a ROM File";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.RestoreDirectory = true;
            dialog.Filter = "ch8 files (*.ch8)|*.ch8|All files (*.*)|*.*";
            dialog.FilterIndex = 1;

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                emu.RunROM(dialog.FileName);
            }
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/CrizGames/ChipMate");
        }
    }
}
