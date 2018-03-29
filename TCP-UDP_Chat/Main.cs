using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sLanCS
{
    public partial class Main : Form
    {
        // TODO: Usunac wzorce
        Factory f = new sLanCSFactory();

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sLanCS server = f.FactoryMethod("Server");
            server.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sLanCS client = f.FactoryMethod("Client");
            client.Show();
            this.Hide();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // "Message:" 
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close(); // exit button
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.sendMessage();
        }

        private void sendMessage()
        {
            //if (this.client.Connected && this.txtNewMessage.Text.Trim() != "")
            //{
            //    this.client.SendCommand(new Proshot.CommandClient.Command(Proshot.CommandClient.CommandType.Message, IPAddress.Broadcast, this.txtNewMessage.Text));
            //    this.txtMessages.Text += this.client.NetworkName + ": " + this.txtNewMessage.Text.Trim() + Environment.NewLine;
            //    this.txtNewMessage.Text = "";
            //    this.txtNewMessage.Focus();
            //}
        }
    }
}