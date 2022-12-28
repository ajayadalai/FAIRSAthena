using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class Message : Window
    {
        public Message(string purpose, string message)
        {
            try
            {
                InitializeComponent();
                if (!(Application.Current.MainWindow is Message))
                {
                    Owner = Application.Current.MainWindow;
                }
                purpose_.Text = purpose;

                if (!string.IsNullOrEmpty(purpose) && purpose != "Message")
                {
                    message_.Text = string.Format("{0}: {1}", purpose, message);
                }
                else
                {
                    message_.Text = message;
                }
            }
            catch
            {

            }
        }

        public static void Display(string purpose, string message)
        {
           
            Display(purpose, message, false);
        }

        public static void Display(string purpose, string message, bool widen)
        {
            Message msg = null;
            
                msg = new Message(purpose, message);
            


            if (widen)
            {
                // Widen window and subordinate elements
                int inc = 200;
                msg.purpose_.Width += inc;
                msg.message_.Width += inc;
                msg.messageWindow_.Width += inc;
                msg.grid_.Width += inc;

                // Move button over
                Thickness new_margin = msg.ok_.Margin;
                new_margin.Left += inc / 2;
                //msg.ok_.Margin = new_margin;
            }

            msg.ShowDialog();
        }
        private void OKClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
