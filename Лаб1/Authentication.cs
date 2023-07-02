using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Лаб1
{
    public partial class Authentication : Form
    {
        bool flagHash = false; //проверка на хэширование
        public Authentication()
        {
            InitializeComponent();
        }

        private string GetHeshPassword(string password)
        {
            StringBuilder newPassword = new StringBuilder();
            var hashAlg = MD5.Create();

            byte[] hash = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < hash.Length; i++)
            {
                newPassword.Append(hash[i].ToString("X2"));
            }
            flagHash = true;
            return newPassword.ToString();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            const string IP = "127.0.0.1";
            const int port = 8080;
            string login = "login " + textBox1.Text;

            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);

            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var message = login;
            var data = Encoding.UTF8.GetBytes(message);

            tcpSocket.Connect(tcpEndPoint);
            tcpSocket.Send(data);

            var buffer = new byte[256];
            var size = 0;
            var answer = new StringBuilder();

            // получаем ответ от сервера 
            do
            {
                size = tcpSocket.Receive(buffer);
                answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
            }
            while (tcpSocket.Available > 0);


            if (answer.ToString() != "0")
            {
                byte[] numb = ConvertToByte(answer.ToString());
                BigInteger answerNumb = new BigInteger(numb);
                if (answerNumb < 0)
                    answerNumb = -answerNumb;
                textBox3.Text = answerNumb.ToString();
            }
            else
            {
                MessageBox.Show("Вы не зарегистрированы!");
                Registration newForm = new Registration();
                newForm.Show();
                this.Hide();
                   
            }
            answer.Clear();
            tcpSocket.Shutdown(SocketShutdown.Both);
            tcpSocket.Close();
        }

        private byte[] ConvertToByte(string answer)
        {
            byte[] numb = new byte[answer.Length];

            for (int i = 0; i < answer.Length; i++)
            {
                numb[i] = Convert.ToByte(answer[i]);
            }
            return numb;
        }
        private void button3_Click(object sender, System.EventArgs e)
        {
            if (textBox3.Text != " " && textBox4.Text != " ")
            {
                textBox3.Text = GetHeshPassword(textBox3.Text);
                textBox4.Text = GetHeshPassword(textBox4.Text);                
            }
            else
                MessageBox.Show("Не правильно введены данные!", "Ошибка!");
        }

        private void button4_Click(object sender, System.EventArgs e)
        {
            if (textBox3.Text != " " && textBox4.Text != " " && flagHash)
            {
                const string IP = "127.0.0.1";
                const int port = 8080;

                string resultLine = "numb " + textBox3.Text + " password " + textBox4.Text;

                var tcpEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);

                var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                var message = resultLine;
                var data = Encoding.UTF8.GetBytes(message);

                tcpSocket.Connect(tcpEndPoint);
                tcpSocket.Send(data);

                var buffer = new byte[256];
                var size = 0;
                var answer = new StringBuilder();
                // получаем ответ от сервера 
                do
                {
                    size = tcpSocket.Receive(buffer);
                    answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (tcpSocket.Available > 0);

                if (answer.ToString() != "0")
                    MessageBox.Show(answer.ToString());
                else
                {
                    textBox3.Text = "";
                    textBox4.Text = "";
                    MessageBox.Show("Не правильный данные! Еще раз проверьте пользователя, пароль или зарегистрируйтесь!", "Ошибка!");
                }


                answer.Clear();
                tcpSocket.Shutdown(SocketShutdown.Both);
                tcpSocket.Close();
            }
            else
                MessageBox.Show("Не правильно введены данные!", "Ошибка!");
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
