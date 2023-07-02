using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;


namespace Server
{
    class Server
    {
        static char[] GenerateRandomNumb(ref BigInteger randNumb)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            int countByte = 128; // 16 байт
            byte[] randByte = new byte[countByte];

            rng.GetBytes(randByte);
            randNumb = new BigInteger(randByte);
            char[] res = ConvertToChar(randByte);            
            return res;
        }

        static char[] ConvertToChar(byte[] randByte)
        {
            char[] res = new char[randByte.Length];

            for (int i = 0; i < randByte.Length; i++)
            {
                res[i] = (char)randByte[i];
            }

            return res;
        }

        static bool CheckUser(Database db, string login)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string[] curLogin = login.Split(' ');
            string queryString = $"select login_user from customersDB where login_user = '{curLogin[1]}'";

            SqlCommand command = new SqlCommand(queryString, db.GetConnection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
                return true;
            else
                return false;
        }

        static bool CheckUserData(Database db, string checkLine, BigInteger randNumb)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string[] numbAndPassword = checkLine.Split(' ');

            // под вторым индексом находится пароль пользователя
            string queryString = $"select password_user from customersDB where password_user = '{numbAndPassword[3]}'"; 

            SqlCommand command = new SqlCommand(queryString, db.GetConnection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0 && CheckNumbHash(numbAndPassword[1], randNumb))
                return true;
            else
                return false;
        }

        static bool CheckNumbHash(string checkLine, BigInteger randNumb)
        {
            return checkLine == GetHeshPassword(randNumb.ToString());
        }

        static string GetHeshPassword(string password)
        {
            StringBuilder newPassword = new StringBuilder();
            var hashAlg = MD5.Create();

            byte[] hash = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < hash.Length; i++)
            {
                newPassword.Append(hash[i].ToString("X2"));
            }
            return newPassword.ToString();
        }

        static string KeyWord(string data)
        {
            string[] line = data.Split(' ');

            foreach(string word in line)
            {
                if (word == "login")
                    return "login";
                if (word == "password")
                    return "password";
            }
            return " ";
        }

        static void Main(string[] args)
        {
            const string IP = "127.0.0.1";
            const int port = 8080;
            int numQueue = 5;


            var data = new StringBuilder();
            Database db = new Database();

            BigInteger numbRandom = new BigInteger();

            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);

            //tcp-сокет в режиме ожидания
            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(tcpEndPoint);
            tcpSocket.Listen(numQueue);


            char[] randNumb = GenerateRandomNumb(ref numbRandom);
            if(numbRandom < 0)
                numbRandom = -numbRandom;

            while (true)
            {
                var listener = tcpSocket.Accept();
                var buffer = new byte[256];
                var size = 0;

                do
                {
                    size = listener.Receive(buffer);
                    data.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (tcpSocket.Available > 0);



                string keyword = KeyWord(data.ToString());

                if (keyword == "login")
                {                
                    if(CheckUser(db, data.ToString()))
                        listener.Send(Encoding.UTF8.GetBytes(randNumb));
                    else
                        listener.Send(Encoding.UTF8.GetBytes("0"));
                }
                if (keyword == "password")
                {
                    if(CheckUserData(db, data.ToString(), numbRandom))
                        listener.Send(Encoding.UTF8.GetBytes("Успешная аутентификация"));
                    else
                        listener.Send(Encoding.UTF8.GetBytes("0"));
                }
                

                Console.WriteLine(data);
                Console.WriteLine();

                data.Clear();
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();


            }
        }
    }
}
