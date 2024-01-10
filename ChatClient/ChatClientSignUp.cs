using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ChatUygulamasi.ChatClient
{
    public partial class ChatClientSignUp : Form
    {
        public ChatClientSignUp()
        {
            InitializeComponent();
        }

        private void ChatClientSignUp_Load(object sender, EventArgs e)
        {

        }

        private void SignUp()
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("Lütfen boş alanları doldurun");
                return;
            }

            SqlConnection baglanti = new SqlConnection(SqlCon.ConnectionString);
            baglanti.Open();
            string sql = string.Format($"SELECT * FROM Userr WHERE Username = '{textBox1.Text}' ");

            SqlCommand komut = new SqlCommand(sql, baglanti);
            var result = komut.ExecuteReader();
            if (result.Read())
            {
                result.Close();
                MessageBox.Show("Böyle bir kullanıcı bulunmakta");
                return;
            }
            else
            {
                result.Close();

                sql = string.Format($@"insert into [dbo].[Userr] ([Username], [Pass]) 
                    values ('{textBox1.Text}', '{textBox2.Text}')");

                SqlCommand komut2 = new SqlCommand(sql, baglanti);
                var insertResult = komut2.ExecuteNonQuery();
                if (insertResult > 0)
                {
                    MessageBox.Show("Başarıyla Kaydedildi");
                    this.Close();
                }
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            SignUp();
        }
    }
}
