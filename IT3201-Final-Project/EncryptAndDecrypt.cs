using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IT3201_Final_Project
{
    class EncryptAndDecrypt
    {
        UnicodeEncoding ByteConverter = new UnicodeEncoding();
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
        String oldhash;
        public String Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // Uppercase SHA-1 hash
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public void storeInFile(String ciphertext, String hash)
        {
            // Storing ciphertext and SHA-1 hash
            String path = Path.Combine(Environment.CurrentDirectory, "IT3201-text.txt");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter writetext = File.CreateText(path))
            {
                writetext.WriteLine(ciphertext);
                writetext.WriteLine(hash);
            }

            // Storing public key
            path = Path.Combine(Environment.CurrentDirectory, "IT3201-pubKey.pem");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter writetext = File.CreateText(path))
            {
                writetext.WriteLine(RSA.ToXmlString(false));
            }

            // Storing private key
            path = Path.Combine(Environment.CurrentDirectory, "IT3201-privKey.pem");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter writetext = File.CreateText(path))
            {
                writetext.WriteLine(RSA.ToXmlString(true));
            }
        }

        public String generateRandomKey(int inputLen)
        {
            const int MAX = 10;
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            for(int i = 0; i < inputLen && i < MAX; i++)
            {
                res.Append(alphabet[rnd.Next(alphabet.Length)]);
            }
            return res.ToString();
        }

        // The main Encryption method. Put all the encryption steps here, in a correct order.
        public String Encrypt(String input, int inputLen, String key)
        {
            String output;

            output = CaesarCipher(input, inputLen);
            output = TranspositionCipher(output, key, '-');
            output = RSAEncrypt(ByteConverter.GetBytes(output), RSA.ExportParameters(false), false);

            return output;
        }

        public String CaesarCipher(String input, int shiftLength)
        {
            StringBuilder output = new StringBuilder();
            char[] buffer = input.ToCharArray();

            for (int i = 0; i < buffer.Length; i++)
            {
                char letter = buffer[i];

                // Checks for spaces.
                if (!char.IsWhiteSpace(letter))
                {
                    letter = (char)(letter + shiftLength);

                    if (letter > 'z')
                    {
                        letter = (char)(letter - 26);
                    }
                    else if (letter < 'a')
                    {
                        letter = (char)(letter + 26);
                    }

                    output.Append(letter);           
                }
                else
                {
                    output.Append(" ");
                }
                
            }
            return output.ToString();
        }

        private int[] GetShiftIndexes(string key)
        {
            int keyLength = key.Length;
            int[] indexes = new int[keyLength];
            List<KeyValuePair<int, char>> sortedKey = new List<KeyValuePair<int, char>>();
            int i;

            for (i = 0; i < keyLength; ++i)
                sortedKey.Add(new KeyValuePair<int, char>(i, key[i]));

            sortedKey.Sort(
                delegate (KeyValuePair<int, char> pair1, KeyValuePair<int, char> pair2) {
                    return pair1.Value.CompareTo(pair2.Value);
                }
            );

            for (i = 0; i < keyLength; ++i)
                indexes[sortedKey[i].Key] = i;

            return indexes;
        }

        public String TranspositionCipher(string input, string key, char padChar)
        {
            input = (input.Length % key.Length == 0) ? input : input.PadRight(input.Length - (input.Length % key.Length) + key.Length, padChar);
            StringBuilder output = new StringBuilder();
            int totalChars = input.Length;
            int totalColumns = key.Length;
            int totalRows = (int)Math.Ceiling((double)totalChars / totalColumns);
            char[,] rowChars = new char[totalRows, totalColumns];
            char[,] colChars = new char[totalColumns, totalRows];
            char[,] sortedColChars = new char[totalColumns, totalRows];
            int currentRow, currentColumn, i, j;
            int[] shiftIndexes = GetShiftIndexes(key);

            for (i = 0; i < totalChars; ++i)
            {
                currentRow = i / totalColumns;
                currentColumn = i % totalColumns;
                rowChars[currentRow, currentColumn] = input[i];
            }

            for (i = 0; i < totalRows; ++i)
                for (j = 0; j < totalColumns; ++j)
                    colChars[j, i] = rowChars[i, j];

            for (i = 0; i < totalColumns; ++i)
                for (j = 0; j < totalRows; ++j)
                    sortedColChars[shiftIndexes[i], j] = colChars[i, j];

            for (i = 0; i < totalChars; ++i)
            {
                currentRow = i / totalRows;
                currentColumn = i % totalRows;
                output.Append(sortedColChars[currentRow, currentColumn]);
            }

            return output.ToString();
        }

        // REFERENCE: RSA ciphertext length is 172
        public String RSAEncrypt(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKey);
                    encryptedData = RSA.Encrypt(Data, DoOAEPPadding);
                }
                return Convert.ToBase64String(encryptedData);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        //Decryption Area from this point downwards


        public String[] readCiphandHash()
        {
            String path = Path.Combine(Environment.CurrentDirectory, "IT3201-text.txt");
            String[] enctext = new String[2];
            using (StreamReader readtext = new StreamReader(path))
            {
                enctext[0] = readtext.ReadLine();   // Ciphertext
                enctext[1] = readtext.ReadLine();  // OLD HASH
                oldhash = enctext[1];
                //writetext.WriteLine(ciphertext);
                //writetext.WriteLine(hash);
                return enctext;
            }
        }

        public byte[] bytesToDecrypt(String ciph)
        {
            var btd = Convert.FromBase64String(ciph);

            return btd;
        }

        public void getPrivKey()
        {
            String path = Path.Combine(Environment.CurrentDirectory, "IT3201-privKey.pem");
            var pem = File.ReadAllText(path);
            RSA.FromXmlString(pem);
        }

        public String Decrypt(String input, int inputLen, String key)
        {
            String output;
            byte[] bytestoDec = bytesToDecrypt(input);
            getPrivKey();
             output = RSADecrypt(bytestoDec, RSA.ExportParameters(true), false);
            if (output != null)
            {
                output = DecryptTranspo(output, key);
                output = DecryptCaesar(output, inputLen);
            }
            else
            {
                MainWindow mw = new MainWindow();
                mw.Notification.Text = "Message is unverified. File may be corrupted or tampered.";
            }
             
            return output;
        }

         public String RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKeyInfo);
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }

                return ByteConverter.GetString(decryptedData);
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }

        }

        public String DecryptTranspo(string input, string key)
        {
            StringBuilder output = new StringBuilder();
            int totalChars = input.Length;
            int totalColumns = (int)Math.Ceiling((double)totalChars / key.Length);
            int totalRows = key.Length;
            char[,] rowChars = new char[totalRows, totalColumns];
            char[,] colChars = new char[totalColumns, totalRows];
            char[,] unsortedColChars = new char[totalColumns, totalRows];
            int currentRow, currentColumn, i, j;
            int[] shiftIndexes = GetShiftIndexes(key);

            for (i = 0; i < totalChars; ++i)
            {
                currentRow = i / totalColumns;
                currentColumn = i % totalColumns;
                rowChars[currentRow, currentColumn] = input[i];
            }

            for (i = 0; i < totalRows; ++i)
                for (j = 0; j < totalColumns; ++j)
                    colChars[j, i] = rowChars[i, j];

            for (i = 0; i < totalColumns; ++i)
                for (j = 0; j < totalRows; ++j)
                    unsortedColChars[i, j] = colChars[i, shiftIndexes[j]];

            for (i = 0; i < totalChars; ++i)
            {
                currentRow = i / totalRows;
                currentColumn = i % totalRows;
                output.Append(unsortedColChars[currentRow, currentColumn]);
                if (output.ToString().Contains("-"))
                {
                    output.Replace("-", null);
                }
            }

            
            return output.ToString();
        }

        public String DecryptCaesar(String input, int shiftLength)
        {
            StringBuilder output = new StringBuilder();
            char[] buffer = input.ToCharArray();

            for (int i = 0; i < buffer.Length; i++)
            {
                char letter = buffer[i];

                // Checks for spaces.
                if (!char.IsWhiteSpace(letter))
                {
                    letter = (char)(letter - shiftLength);

                    if (letter > 'z')
                    {
                        letter = (char)(letter - 26);
                    }
                    else if (letter < 'a')
                    {
                        letter = (char)(letter + 26);
                    }

                    output.Append(letter);
                }
                else
                {
                    output.Append(" ");
                }

            }
            return output.ToString();
        }
    }
}
