using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;

namespace UnwindTicket
{
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
        }


        private void Configuration_Load(object sender, EventArgs e)
        {
            try
            {
                clsConfig.LoadConfig();
                txtURL.Text = clsConfig.URL;
                txtApikey.Text = clsConfig.APIKey;
                txtUsername.Text = clsConfig.UserName;
                txtPassword.Text = clsConfig.Password;

            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "Configuration_Load " + ex.Message + "\t" + ex.StackTrace);
            }

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if(txtURL.Text.Trim() == "")
                    MessageBox.Show("URL can not left empty");
                if(txtUsername.Text.Trim() == "")
                    MessageBox.Show("Username can not left empty");
                if(txtPassword.Text.Trim() == "")
                    MessageBox.Show("Password can not left empty");
                if (txtApikey.Text.Trim() == "")
                    MessageBox.Show("API key can not left empty");

                Logger.LogEntry("Information", "Update Start");
                string path = clsConfig.GetSaveFolderPath();
                if (path != "")
                {
                  path += "\\UTconfig.txt";
                  string info =  "URL|" + txtURL.Text + ";";
                         info += "UserName|" + txtUsername.Text + ";";
                         info += "Password|" + txtPassword.Text + ";";
                         info += "APIKey|" + txtApikey.Text + ";";
                    System.IO.File.WriteAllText(path, info);
                }
                clsConfig.LoadConfig();
                Logger.LogEntry("Information", "Update Finish");
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "btnUpdate_Click " + ex.Message + "\t" + ex.StackTrace);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "btnClear_Click " + ex.Message + "\t" + ex.StackTrace);
            }

        }

        

        

    }
}
