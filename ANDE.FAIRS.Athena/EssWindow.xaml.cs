using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ANDE.FAIRS.Athena
{
    /// <summary>
    /// Interaction logic for EssWindow.xaml
    /// </summary>
    public partial class EssWindow : Window
    {
        public EssWindow()
        {
            InitializeComponent();
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_SYSCOMMAND = 0x112;
        private const int SC_MAXIMIZE = 0xF030;
        private void ReturnToInitiateClick(object sender, RoutedEventArgs e)
        {
            Process p = Process.Start("genefinder.exe");
            while (p.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(1);
            }

            SetParent(p.MainWindowHandle, CBox.Handle);
            SendMessage(p.MainWindowHandle, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
        }

        private void QuitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowsFormsHost_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }
    }
}
