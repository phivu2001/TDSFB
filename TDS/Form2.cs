using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace TDS
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string idacc = textBox1.Text;
            string accesstocken = textBox2.Text;
            string cookie = textBox3.Text;
            string name = textBox4.Text;
            // Tạo đối tượng Account từ các giá trị nhập vào
            Account account = new Account
            {
                Name = name,
                Accesstocken = accesstocken,
                Idacc = idacc,
                Cookie = cookie
            };

            string FilePath = @"D:\Project\TDSS\file\Account.xml";
            AccountManager.WriteAccountsToFile(FilePath, account);
            MessageBox.Show("Đã lưu thông tin vào file.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public class Account
        {
            public string Name { get; set; }
            public string Idacc { get; set; }
            public string Accesstocken { get; set; }
            public string Cookie { get; set; }
        }

        public static class AccountManager
        {
            public static void WriteAccountsToFile(string filePath, Account account)
            {
                List<Account> accounts;

                if (File.Exists(filePath))
                {
                    accounts = ReadAccountsFromFile(filePath);
                    accounts.Add(account);
                }
                else
                {
                    accounts = new List<Account> { account };
                }

                XmlSerializer serializer = new XmlSerializer(typeof(List<Account>));

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, accounts);
                }
            }

            public static List<Account> ReadAccountsFromFile(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    return new List<Account>();
                }

                XmlSerializer serializer = new XmlSerializer(typeof(List<Account>));

                using (StreamReader reader = new StreamReader(filePath))
                {
                    return (List<Account>)serializer.Deserialize(reader);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var test = AccountManager.ReadAccountsFromFile(@"D:\Project\TDSS\file\test.xml");
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            int listViewWidth = 477;
            int listViewHeight = 79;
            int startX = 7;
            int startY = 299;
            int spacingX = 15; 
            int spacingY = 8;  

            for (int i = 0; i < 12; i++)
            {
                ListView listView = new ListView
                {
                    Size = new Size(listViewWidth, listViewHeight),
                    Location = new Point(startX + (i % 3) * (listViewWidth + spacingX),
                                         startY + (i / 3) * (listViewHeight + spacingY))
                };
                this.Controls.Add(listView);
            }
        }
    }
}
