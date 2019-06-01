using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LC.Survey.Core
{
    abstract public class GlobalCommonHelper
    {
        #region General Methods

        /// <summary>
        /// Take any string and encrypt it using SHA1 then
        /// return the encrypted data
        /// </summary>
        /// <param name="data">input text you will enterd to encrypt it</param>
        /// <returns>return the encrypted text as hexadecimal string</returns>
        public string GetSHA1HashData(string data)
        {
            //create new instance of md5
            SHA1 sha1 = SHA1.Create();

            //convert the input text to array of bytes
            byte[] hashData = sha1.ComputeHash(Encoding.Default.GetBytes(data));

            //create new instance of StringBuilder to save hashed data
            StringBuilder returnValue = new StringBuilder();

            //loop for each byte and add it to StringBuilder
            for (int i = 0; i < hashData.Length; i++)
            {
                returnValue.Append(hashData[i].ToString());
            }

            // return hexadecimal string
            return returnValue.ToString();
        }

        /// <summary>
        /// Creates a slug url from string .
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        public string GetSlugURLFromString(string phrase)
        {
            string str = RemoveAccent(phrase).ToLower();
            // invalid chars
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens
            return str;
        }

        /// <summary>
        /// Delete file by specified path.
        /// </summary>
        /// <param name="path">path of file.</param>
        public void DeleteTargetFile(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }
        }

        /// <summary>
        /// Sent email to target email address with attachment.
        /// </summary>
        /// <param name="toEmail">Email addresses of
        /// one or multiple receipients semi colon (;) separated values.</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <returns>True | False</returns>
        public bool SendEmailToTarget(string toEmail, string subject, string body)
        {

            bool success = false;
            try
            {
                SmtpClient SmtpServer = new SmtpClient();
                MailMessage mail = new MailMessage();

                SmtpServer.Credentials = new NetworkCredential(
                    Convert.ToString(ConfigurationManager.AppSettings["fromEmail"]),
                    Convert.ToString(ConfigurationManager.AppSettings["fromPassword"]));

                SmtpServer.Host = Convert.ToString
                (ConfigurationManager.AppSettings["hostName"]);
                SmtpServer.Port = Convert.ToInt32
                (ConfigurationManager.AppSettings["portNumber"]);

                if (Convert.ToBoolean
                (ConfigurationManager.AppSettings["isEnableSSL"]) == true)
                    SmtpServer.EnableSsl = true;

                mail.From = new MailAddress(Convert.ToString
                (ConfigurationManager.AppSettings["senderName"]));

                string[] multiEmails = toEmail.Split(';');
                foreach (string email in multiEmails)
                {
                    mail.To.Add(email);
                }

                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = body;
                SmtpServer.Send(mail);
                mail.Dispose();
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Sent email to target email address with attachment.
        /// </summary>
        /// <param name="toEmail">Email addresses of
        /// one or multiple receipients semi colon (;) separated values.</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <param name="body">Email attachment file path</param>
        /// <returns>True | False</returns>
        public bool SendEmailToTarget(string toEmail, string subject, string body, string attachmentPath)
        {

            bool success = false;
            try
            {
                SmtpClient SmtpServer = new SmtpClient();
                MailMessage mail = new MailMessage();

                SmtpServer.Credentials = new NetworkCredential(
                    Convert.ToString(ConfigurationManager.AppSettings["fromEmail"]),
                    Convert.ToString(ConfigurationManager.AppSettings["fromPassword"]));

                SmtpServer.Host = Convert.ToString
                (ConfigurationManager.AppSettings["hostName"]);
                SmtpServer.Port = Convert.ToInt32
                (ConfigurationManager.AppSettings["portNumber"]);

                if (Convert.ToBoolean(ConfigurationManager.AppSettings["isEnableSSL"]) == true)
                    SmtpServer.EnableSsl = true;

                mail.From = new MailAddress(Convert.ToString
                (ConfigurationManager.AppSettings["senderName"]));

                string[] multiEmails = toEmail.Split(';');
                foreach (string email in multiEmails)
                {
                    mail.To.Add(email);
                }

                Attachment attachment;
                attachment = new System.Net.Mail.Attachment(attachmentPath);
                mail.Attachments.Add(attachment);

                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = body;
                SmtpServer.Send(mail);
                mail.Dispose();
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Strips tags
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Formatted text</returns>
        public string RemoveHtmlFromString(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = Regex.Replace(text, @"(>)(\r|\n)*(<)", "><");
            text = Regex.Replace(text, "(<[^>]*>)([^<]*)", "$2");


            return text;
        }

        /// <summary>
        /// Verifies that a string is in valid e-mail format
        /// </summary>
        /// <param name="email">Email to verify</param>
        /// <returns>true if the string is a valid e-mail address and false if it's not</returns>
        //        public bool IsValidEmail(string email)
        //            {
        //                if (String.IsNullOrEmpty(email))
        //                    return false;

        //                email = email.Trim();
        //                var result = Regex.IsMatch(email, "^(?:[\\w\\!\\#\\$\\%\\&\\

        //                '\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\

        //                '\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9]
        //                (?:[a-zA-Z0-9\\-] (?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9]
        //        (?:[a-zA-Z0-9\\-] (?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01] ?\\d{1, 2}|2[0-4]
        //            \\d|25[0-5])\\.){3}
        //(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))$", RegexOptions.IgnoreCase);
        //            return result;
        //        }

        /// <summary>
        /// Returns Allowed HTML only.
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Allowed HTML</returns>
        public string EnsureOnlyAllowedHtml(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            const string allowedTags = "br,hr,b,i,u,a,div,ol,ul,li,blockquote,img,span,p,em," +
                                        "strong,font,pre,h1,h2,h3,h4,h5,h6,address,cite";

            var m = Regex.Matches(text, "<.*?>", RegexOptions.IgnoreCase);
            for (int i = m.Count - 1; i >= 0; i--)
            {
                string tag = text.Substring(m[i].Index + 1, m[i].Length - 1).Trim().ToLower();

                if (!IsValidTag(tag, allowedTags))
                {
                    text = text.Remove(m[i].Index, m[i].Length);
                }
            }

            return text;
        }

        #endregion

        #region Internal Processing Private Methods

        private string RemoveAccent(string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        private bool IsValidTag(string tag, string tags)
        {
            string[] allowedTags = tags.Split(',');
            if (tag.IndexOf("javascript") >= 0) return false;
            if (tag.IndexOf("vbscript") >= 0) return false;
            if (tag.IndexOf("onclick") >= 0) return false;

            var endchars = new char[] { ' ', '>', '/', '\t' };

            int pos = tag.IndexOfAny(endchars, 1);
            if (pos > 0) tag = tag.Substring(0, pos);
            if (tag[0] == '/') tag = tag.Substring(1);

            foreach (string aTag in allowedTags)
            {
                if (tag == aTag) return true;
            }

            return false;
        }

        #endregion
    }
}
