using System;
using System.Collections.Generic;
using System.IO;
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
        byte[] decryptionbytes;
        String ciphertodecrypt;
        String oldhash;

        private void EncryptBtn_Click(object sender, RoutedEventArgs e)
        {
            if(Plaintextbox.Text.Length >= 3)
            {
                SHA1LabelEncrypt.Text = ead.Hash(Plaintextbox.Text);
                KeyLabelEncrypt.Text = ead.generateRandomKey(Plaintextbox.Text.Length);

                CiphertextLabel.Text = ead.Encrypt(Plaintextbox.Text, KeyLabelEncrypt.Text.Length, KeyLabelEncrypt.Text);
                ead.storeInFile(CiphertextLabel.Text, SHA1LabelEncrypt.Text);

                Notification.Text = "Text Successfully encrypted";
            }
            else
            {
                Notification.Text = "Plain text minimum of 3 characters only.";
            }
        }

        private void openFileBtn_Click(object sender, RoutedEventArgs e)
        {
            Stream stream = null;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            String[] ciphandhash = { };

            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
 
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                if((stream = dlg.OpenFile()) != null)
                {
                    using (stream)
                    {

                        ciphandhash = ead.readCiphandHash();
                        ciphertodecrypt = ciphandhash[0];
                        //SHA1LabelDecrypt.Text = ciphandhash[1];
                        oldhash = ciphandhash[1];
                    }
                } 
            }
        }

        private void DecryptBtn_Click(object sender, RoutedEventArgs e)
        {
            //PlaintextLabel.Text = ciphertodecrypt;
            String result;
            if(KeyLabelDecrypt.Text.Length >= 0)
            {
                if(ciphertodecrypt.Length != 172 || ciphertodecrypt == null)
                {
                    Notification.Text = "Message is unverified";
                }
                else
                {
                    result = ead.Decrypt(ciphertodecrypt, KeyLabelDecrypt.Text.Length, KeyLabelDecrypt.Text);
                    PlaintextLabel.Text = result;
                    SHA1LabelDecrypt.Text = ead.Hash(PlaintextLabel.Text);
                }
               
                if(String.Equals(oldhash, SHA1LabelDecrypt.Text))
                {
                    Notification.Text = "MESSAGE VERIFIED!!";
                }
                else
                {
                    Notification.Text = "Message is unverified";
                }
            }
            else
            {
                Notification.Text = "Please input the key before decrypting.";
            }
            
            
            
        }
    }
}
