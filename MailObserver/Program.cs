using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Newtonsoft.Json;

namespace MailObserver
{
    class Program
    {
        private static User user = new User();
        private static List<IMail> messages = new List<IMail>();
        static void Main(string[] args)
        {
            Console.WriteLine("Введите логин:");

            bool loginIsCorrect = true;
            string login = "";

            while(loginIsCorrect)
            {
                login = Console.ReadLine();
                if (login == "")
                {
                    Console.Write("Неправильно введен логин!");
                }
                else
                {
                    loginIsCorrect = false;
                    user.Login = login;
                }
            }

            Console.WriteLine("Введите пароль!");

            bool passwordIsCorrect = true;
            string password = "";

            while (passwordIsCorrect)
            {
                password = Console.ReadLine();
                if (user.Password == "")
                {
                    Console.Write("Неправильно введен пароль!");
                }
                else
                {
                    passwordIsCorrect = false;
                    user.Password = password;
                }
            }

            messages = ReadJsonFile();
            if (messages.Count > 0)
            {
                foreach (var i in messages)
                {
                    Console.WriteLine("Тема: " + i.Subject);
                    Console.Write("От кого: ");
                    foreach (var sender in i.From)
                    {
                        Console.Write("Отправитель: " + sender.Address);
                    }
                    Console.WriteLine(i.Text);
                    if (i.Date != null)//
                    {
                        Console.WriteLine(i.Date.Value);
                    }

                    foreach (var j in i.Attachments)
                    {
                        Console.WriteLine(j.FileName);
                    }
                }
            }
            else
            {
                using (Imap imap = new Imap())
                {
                    imap.ConnectSSL("imap.gmail.com");
                    imap.UseBestLogin(user.Login, user.Password);
                    imap.SelectInbox();

                    List<long> uids = imap.GetAll();
                    uids.Reverse();

                    foreach (var i in uids)
                    {
                        var message = imap.GetMessageByUID(i);
                        IMail imail = new MailBuilder().CreateFromEml(message);

                        messages.Add(imail);
                    }

                    WriteJsonFile();

                    foreach (var i in messages)
                    {
                        Console.WriteLine("Тема: " + i.Subject);
                        Console.Write("От кого: ");
                        foreach (var sender in i.From)
                        {
                            Console.Write("Отправитель: " + sender.Address);
                        }
                        Console.WriteLine(i.Text);
                        if (i.Date != null)
                        {
                            Console.WriteLine(i.Date.Value);
                        }

                        foreach (var j in i.Attachments)
                        {
                            Console.WriteLine(j.FileName);
                        }
                    }
                }
            }
            Console.ReadLine();
        }


        #region JsonReadAndWriteFile
        private static void WriteJsonFile()
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\files\");
            string serialized = JsonConvert.SerializeObject(messages);
            File.WriteAllBytes(Directory.GetCurrentDirectory() + @"\files\" + "email.json", Encoding.Default.GetBytes(serialized));
        }

        private static List<IMail> ReadJsonFile()
        {
            try
            {
                byte[] data = File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\files\" + "email.json");
                string json = Encoding.Default.GetString(data);
                List<IMail> newMessages = JsonConvert.DeserializeObject<List<IMail>>(json);
                return newMessages;
            }
            catch
            {
                return new List<IMail>();
            }
        }
        #endregion
    }
}
