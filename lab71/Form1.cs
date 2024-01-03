using System;
using System.Text;
using System.Windows.Forms;

namespace lab71
{
    public partial class Form1 : Form
    {
        private uint[] H = { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };
        private uint[] K = { 0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5, 0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
        0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3, 0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
        0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC, 0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
        0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7, 0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
        0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13, 0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
        0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3, 0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
        0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5, 0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
        0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208, 0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2 };

        public Form1()
        {
            InitializeComponent();
        }

        // Функция циклического сдвига вправо
        static uint ROTRIGHT(uint a, byte b)
        {
            return (((a) >> (b)) | ((a) << (32 - (b))));
        }

        private void button1_Click(object sender, EventArgs ee)
        {
            if (textBox1.Text.Length == 0) // нет сообщения
            {
                MessageBox.Show("Ошибка! Сообщение не должно быть пустым!");
                return;
            }
            if (textBox1.Text.Length >= Math.Pow(2, 64)) // большая длина сообщения
            {
                MessageBox.Show("Ошибка! Длина сообщения должна быть меньше 2^64");
                return;
            }
            // Предварительная обработка
            string message = textBox1.Text;
            byte[] m = Encoding.UTF8.GetBytes(message);
            long size64 = m.Length * 8; // длина сообщения в битах
            int check = 512;
            while (true)
            {
                if ((size64 + 64) >= check)
                    check += 512;
                else
                    break;
            }
            long k = check - size64 - sizeof(long)*8;
            byte[] additionalMas = new byte[k / 8];
            additionalMas[0] = 128;
            for (int i = 1; i < additionalMas.Length; i++)
            {
                additionalMas[i] = 0;
            }
            byte[] finalMessage = new byte[m.Length + additionalMas.Length + sizeof(long)];
            for (int i = 0; i < m.Length; i++)
            {
                finalMessage[i] = m[i];
            }
            for (int i = 0; i < additionalMas.Length; i++)
            {
                finalMessage[i + m.Length] = additionalMas[i];
            }
            byte[] size64bytes = BitConverter.GetBytes(size64);
            Array.Reverse(size64bytes); // bitconverter возвращает число наоборот
            for (int i = 0; i < size64bytes.Length; i++)
            {
                finalMessage[i + additionalMas.Length + m.Length] = size64bytes[i];
            }
            uint[] HH = new uint[H.Length];
            for (int z = 0; z < HH.Length; z++)
            {
                HH[z] = H[z];
            }
            // Обработка сообщения порциями по 512 бит
            for (int i = 0; i < finalMessage.Length/64; i++)
            {
                byte[,] words = new byte[16, 4];
                for (int j = 0; j < 16; j++)
                {
                    for (int r = 0; r < 4; r++)
                    {
                        words[j, r] = finalMessage[64 * i + 4 * j + r];
                    }
                }
                uint[] w = new uint[64];
                for (int j = 0; j < 16; j++)
                {
                    byte[] wordByte = new byte[4];
                    for (int r = 0; r < 4; r++)
                    {
                        wordByte[r] = words[j, r];
                    }
                    Array.Reverse(wordByte); // bitconverter возвращает число наоборот
                    w[j] = BitConverter.ToUInt32(wordByte, 0);
                }
                for (int j = 16; j < 64; j++)
                {
                    uint s0 = (ROTRIGHT(w[j - 15], 7)) ^ (ROTRIGHT(w[j - 15], 18)) ^ (w[j - 15] >> 3);
                    uint s1 = (ROTRIGHT(w[j - 2], 17)) ^ (ROTRIGHT(w[j - 2], 19)) ^ (w[j - 2] >> 10);
                    w[j] = w[j - 16] + s0 + w[j - 7] + s1;
                }   
                uint a = HH[0];
                uint b = HH[1];
                uint c = HH[2];
                uint d = HH[3];
                uint e = HH[4];
                uint f = HH[5];
                uint g = HH[6];
                uint h = HH[7];
                for (int j = 0; j < 64; j++)
                {
                    uint sigma0 = (ROTRIGHT(a, 2)) ^ (ROTRIGHT(a, 13)) ^ (ROTRIGHT(a, 22));
                    uint Ma = (a & b) ^ (a & c) ^ (b & c);
                    uint t2 = sigma0 + Ma;
                    uint sigma1 = (ROTRIGHT(e, 6)) ^ (ROTRIGHT(e, 11)) ^ (ROTRIGHT(e, 25));
                    uint Ch = (e & f) ^ ((~e) & g);
                    uint t1 = h + sigma1 + Ch + K[j] + w[j];
                    h = g;
                    g = f;
                    f = e;
                    e = d + t1;
                    d = c;
                    c = b;
                    b = a;
                    a = t1 + t2;
                }
                HH[0] = HH[0] + a;
                HH[1] = HH[1] + b;
                HH[2] = HH[2] + c;
                HH[3] = HH[3] + d;
                HH[4] = HH[4] + e;
                HH[5] = HH[5] + f;
                HH[6] = HH[6] + g;
                HH[7] = HH[7] + h;
            }
            string result = "";
            for (int i=0; i<HH.Length; i++)
            {
                result += HH[i].ToString("X");
            }
            label3.Text = result;
        }
    }
}









