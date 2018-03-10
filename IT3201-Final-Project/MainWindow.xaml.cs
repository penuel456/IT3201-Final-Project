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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IT3201_Final_Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        EncryptAndDecrypt ead = new EncryptAndDecrypt();

        private void EncryptBtn_Click(object sender, RoutedEventArgs e)
        {
            SHA1LabelEncrypt.Text = ead.Hash(PlaintextLabel.Text);
            KeyLabelEncrypt.Text = ead.generateRandomKey((PlaintextLabel.Text.Length / 2));

            CiphertextLabel.Text = ead.Encrypt(PlaintextLabel.Text, KeyLabelEncrypt.Text.Length, KeyLabelEncrypt.Text);

            Notification.Text = "Text Successfully encrypted.";
        }
    }
}
