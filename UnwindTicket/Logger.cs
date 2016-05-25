using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UnwindTicket
{
    class Logger
    {
        public static int SendMail(string mailBody, string Subject, string vToEmail)
        {
            int functionReturnValue = 0;
            try
            {
                if (string.IsNullOrEmpty(mailBody) || mailBody == "")
                    return functionReturnValue;
                //' Is no data present then

                string toEmail = "";
                string ccEmail = "";
                string BccEmail = "";

                if (string.IsNullOrEmpty(vToEmail))
                {

                    toEmail = ConfigurationManager.AppSettings["ToEmail"];
                    //.Replace(";", ",")
                    ccEmail = ConfigurationManager.AppSettings["CCEmail"].Replace(";", ",");
                }

                string fromEmail = ConfigurationManager.AppSettings["SystemEmail"];
                string smtpServer = ConfigurationManager.AppSettings["SMTPServer"];
                string smtpUser = ConfigurationManager.AppSettings["SMTPUser"];
                string smtpPass = ConfigurationManager.AppSettings["SMTPPassword"];
                string smtpPort = ConfigurationManager.AppSettings["SMTPPort"];
                string SubjectPostfix = ConfigurationManager.AppSettings["ClientName"];


                System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();

                // mailMessage.To.Add(toEmail)
              //  LogEntry("Information", "SendMail:  Creating Mail message ");

                if (toEmail.Contains(";"))
                {

                    string[] eTo = toEmail.Split(';');
                    for (int i = 0; i <= eTo.Length - 1; i++)
                    {
                        mailMessage.To.Add(eTo[i]);
                    }
                }
                else
                {
                    mailMessage.To.Add(toEmail);
                }
                if (!string.IsNullOrEmpty(ccEmail))
                {
                    mailMessage.CC.Add(ccEmail);
                }

                if (!string.IsNullOrEmpty(BccEmail))
                {
                    mailMessage.Bcc.Add(BccEmail);
                }

                mailMessage.From = new System.Net.Mail.MailAddress(fromEmail, "Otastech");
                mailMessage.Subject = Subject + ":" + SubjectPostfix;
                mailMessage.Body = mailBody + "-" + SubjectPostfix;
                mailMessage.Priority = System.Net.Mail.MailPriority.High;
                mailMessage.IsBodyHtml = false;

                LogEntry("Information", "SendMail:  Creating SMTP Object ");

                System.Net.Mail.SmtpClient SMTPClientObj = new System.Net.Mail.SmtpClient();
                SMTPClientObj.UseDefaultCredentials = false;
                SMTPClientObj.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                SMTPClientObj.Host = smtpServer;
                //"smtp.gmail.com"
                SMTPClientObj.Port = Convert.ToInt16(smtpPort);
                //smtpPort '587
                SMTPClientObj.EnableSsl = true;

                if (!string.IsNullOrEmpty(toEmail))
                {
                    SMTPClientObj.Send(mailMessage);
                }
                return 1;
            }
            catch (System.Exception ex)
            {
                LogEntry("Error", "SendMail: " + ex.Message + "\n" + ex.StackTrace);
            }
            return functionReturnValue;
        }


        //public static int SendMail(string subject, string body, string ccAddress = "", string attachment = "", bool isHTMLBody = false)
        //{
        //    try
        //    {
        //        string mailTo = ConfigurationManager.AppSettings["ToEmail"];
        //        string mailFrom = ConfigurationManager.AppSettings["SystemEmail"];
        //        string SMTPUser = ConfigurationManager.AppSettings["SMTPUser"];
        //        string SMTPPassword = ConfigurationManager.AppSettings["SMTPPassword"];
        //        string SMTPPort = ConfigurationManager.AppSettings["SMTPPort"];
        //        string SMTPServer = ConfigurationManager.AppSettings["SMTPServer"];

        //        if (string.IsNullOrEmpty(mailTo))
        //            return 0;

        //        System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();

        //        if (mailTo.Contains(";"))
        //        {
        //            string[] eTo = mailTo.Split(';');
        //            for (int i = 0; i < eTo.Length; i++)
        //            {
        //                mailMessage.To.Add(eTo[i]);
        //            }
        //        }
        //        else
        //        {
        //            mailMessage.To.Add(mailTo);
        //        }

        //        mailMessage.From = new System.Net.Mail.MailAddress(mailFrom, "OTAS Technologies");
        //        mailMessage.Subject = subject.Replace(Environment.NewLine, " ");
        //        mailMessage.Body = body;
        //        mailMessage.Priority = System.Net.Mail.MailPriority.High;
        //        mailMessage.IsBodyHtml = isHTMLBody;

        //        if (!string.IsNullOrEmpty(attachment))
        //        {
        //            string[] strAttachments = attachment.Split('|');
        //            if (strAttachments != null && strAttachments.Length > 0)
        //            {
        //                for (int i = 0; i < strAttachments.Length; i++)
        //                {
        //                    if (strAttachments[i] != string.Empty)
        //                    {
        //                        mailMessage.Attachments.Add(new System.Net.Mail.Attachment(strAttachments[i]));
        //                    }
        //                }
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(ccAddress))
        //            mailMessage.CC.Add(ccAddress);

        //        System.Net.Mail.SmtpClient MyMailServer = new System.Net.Mail.SmtpClient();
        //        MyMailServer.UseDefaultCredentials = false;
        //        MyMailServer.Credentials = new System.Net.NetworkCredential(SMTPUser, SMTPPassword);
        //        MyMailServer.Host = SMTPServer;
        //        MyMailServer.Port = Convert.ToInt16(SMTPPort);
        //        MyMailServer.EnableSsl = true;

        //        if (!string.IsNullOrEmpty(mailTo))
        //            MyMailServer.Send(mailMessage);

        //        return 1;
        //    }
        //    catch (Exception)
        //    {
        //        return 0;
        //    }
        //}



        public static void LogEntry(string msgType, string strLog, bool Mail = false)
        {
            try
            {

                string LocalHistoryFilePath = AppDomain.CurrentDomain.BaseDirectory;
                if (System.IO.Directory.Exists(LocalHistoryFilePath + @"\Log\") == false)
                {
                    System.IO.Directory.CreateDirectory(LocalHistoryFilePath + @"Log\");
                }

                System.IO.StreamWriter sw = new System.IO.StreamWriter(LocalHistoryFilePath + @"\Log\" + DateTime.Today.Date.ToString("ddMMyyyy") + ".log", true);
                switch (msgType)
                {
                    case "Information":
                        //''''********** Remove next line b'caz msg will be write to file, no listview now.
                        sw.WriteLine("Information" + "\t" + DateTime.Now.ToString() + "\t" + strLog);
                        break; // TODO: might not be correct. Was : Exit Select
                    case "Error":
                        sw.WriteLine("Error" + "\t" + DateTime.Now.ToString() + "\t" + strLog);
                        break; // TODO: might not be correct. Was : Exit Select
                    default:
                        sw.WriteLine("Other" + "\t" + DateTime.Now.ToString() + "\t" + strLog);
                        break; // TODO: might not be correct. Was : Exit Select
                }
                sw.Flush();
                sw.Close();
                if (Mail)
                    SendMail(strLog, msgType, "");
            }
            catch
            { }

        }

       
    }
}
