using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IT3201_Final_Project
{
    class EncryptAndDecrypt
    {
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

        public String generateRandomKey(int inputLen)
        {
            const int MAX = 15;
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            for(int i = 0; i < inputLen && i < MAX; i++)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        // The main Encryption. Put all the encryption steps here, in a correct order.
        // Caesar -> Transposition -> RSA
        public String Encrypt(String input, int inputLen, String key)
        {
            String output;

            output = CaesarCipherEncrypt(input, inputLen);
            output = TranspositionCipherEncrypt(input, key, '-');

            return output;
        }

        // The main Decryption. Put all the encryption steps here, in a correct order.
        // RSA -> Transposition -> Caesar
        /*
        public String Decrypt(String input, int inputLen, String key)
        {
            String output;

            return output;
        }
        */
        

        public String CaesarCipherEncrypt(String input, int shiftLength)
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

        private static int[] GetShiftIndexes(string key)
        {
            int keyLen = key.Length;
            int[] indexes = new int[keyLen];
            List<KeyValuePair<int, char>> sortedKey = new List<KeyValuePair<int, char>>();
            int i;

            for (i = 0; i < keyLen; ++i)
                sortedKey.Add(new KeyValuePair<int, char>(i, key[i]));

            sortedKey.Sort(
                delegate (KeyValuePair<int, char> pair1, KeyValuePair<int, char> pair2) {
                    return pair1.Value.CompareTo(pair2.Value);
                }
            );

            for (i = 0; i < keyLen; ++i)
            {
                indexes[sortedKey[i].Key] = i;
            }

            return indexes;
        }

        public String TranspositionCipherEncrypt(string input, string key, char padChar)
        {
            input = (input.Length % key.Length == 0) ? input : input.PadRight(input.Length - (input.Length % key.Length) + key.Length, padChar);
            StringBuilder output = new StringBuilder();
            int totalChars = input.Length;
            int totalCols = key.Length;
            int totalRows = (int)Math.Ceiling((double)totalChars / totalCols);
            char[,] rowCharacters = new char[totalRows, totalCols];
            char[,] colCharacters = new char[totalCols, totalRows];
            char[,] sortedColChars = new char[totalCols, totalRows];
            int currentRow, currentColumn, i, j;
            int[] shiftIndexes = GetShiftIndexes(key);

            for (i = 0; i < totalChars; ++i)
            {
                currentRow = i / totalCols;
                currentColumn = i % totalCols;
                rowCharacters[currentRow, currentColumn] = input[i];
            }

            for (i = 0; i < totalRows; ++i)
            {
                for (j = 0; j < totalCols; ++j)
                {
                    colCharacters[j, i] = rowCharacters[i, j];
                }  
            }
                

            for (i = 0; i < totalCols; ++i)
            {
                for (j = 0; j < totalRows; ++j)
                {
                    sortedColChars[shiftIndexes[i], j] = colCharacters[i, j];
                } 
            }

            for (i = 0; i < totalChars; ++i)
            {
                currentRow = i / totalRows;
                currentColumn = i % totalRows;
                output.Append(sortedColChars[currentRow, currentColumn]);
            }

            return output.ToString();
        }
    }
}
