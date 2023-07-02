using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Лаб1
{
    public partial class Registration : Form
    {
        Database db = new Database();
        public Registration()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {    
            var login = textBox1.Text.ToLower();
            var password = textBox2.Text.ToLower();
            

            if (!checkUser(login, password) && !checkEmptyParametrs(login, password) && checkLengthPassword(password))
            {
                string queryString = $"insert into customersDB(login_user, password_user) values ('{login}', '{GetHeshPassword(password)}')";

                SqlCommand command = new SqlCommand(queryString, db.GetConnection());
                db.OpenConnection();

                if (command.ExecuteNonQuery() == 1)
                {
                    MessageBox.Show("Аккаунт успешно зарегистрирован!");
                }
                else
                {
                    MessageBox.Show("Аккаунт не создан");
                }
                db.CloseConnection();
            }
        }


        private bool checkLengthPassword(string password)
        {
            if (password.Length < 8)
            {
                MessageBox.Show("Длина пароля должна быть больше 8 символов!");
                return false;
            }
            else
                return true;
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
            return newPassword.ToString();
        }

        private bool checkEmptyParametrs(string login, string password)
        {
            if (login == "")
            {
                MessageBox.Show("Введите логин!");
                return true;
            }
            else if (password == "")
            {
                MessageBox.Show("Введите пароль!");
                return true;
            }
            else
                return false;

        }

        private bool checkUser(string login, string password)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();
            string queryString = $"select id_user, login_user, password_user from customersDB where login_user = '{login}'";

            SqlCommand command = new SqlCommand(queryString, db.GetConnection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Такой аккаунт уже существует!");
                return true;
            }
            else
                return false;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Authentication newForm = new Authentication();
            newForm.Show();
            this.Hide();
        }
    }
}
