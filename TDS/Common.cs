using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TDS
{
    public static class Common
    {
        public static List<string> lstSocial = new List<string>() { "Facebook", "Tiktok", "Intagram" };
        public static List<string> lstFieldsFacebook = new List<string>() { "follow", "like", "likegiare", "likesieure", "reaction", "comment", "share", "reactcmt", "group", "page" };
        public static List<string> lstFieldsTiktok = new List<string>() { "tiktok_like", "tiktok_follo", "tiktok_comment" };
        public static List<string> lstFieldsIntagram = new List<string>() { "instagram_like", "instagram_follow", "instagram_comment" };
    }
    public enum eSocial : int
    {
        Facebook = 0,
        Tiktok = 1,
        Intagram = 2
    }
    public class Account
    {
        public string NameAcc { get; set; }
        public string Idacc { get; set; }
        public string AccessToken { get; set; }
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

}
