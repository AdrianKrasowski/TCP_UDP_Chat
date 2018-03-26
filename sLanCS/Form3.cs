using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace sLanCS
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        System.Net.Sockets.TcpClient clientSocket;
        NetworkStream serverStream = default(NetworkStream);
        string readData = null;

        // DC on demand
        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;

            if (clientSocket.Connected) clientSocket.Client.Disconnect(true);
        }

        // connect to server
        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button1.Enabled = false;

            readData = ">> Connecting to server ...\r\n";
            msg();
            clientSocket = new System.Net.Sockets.TcpClient();
            // anti-freezing
            try
            {
                clientSocket.Connect(textBox1.Text, Int32.Parse(textBox2.Text));

                serverStream = clientSocket.GetStream();

                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox3.Text + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                Thread ctThread = new Thread(getMessage);
                ctThread.Start();
            }
            catch
            // check if connected successfully (or not)
            {
                button2.Enabled = false;
                button1.Enabled = true;

                readData = ">> Connection failure\r\n";
                msg();
            }
        }

        // thread for getting message from stream
        private void getMessage()
        {
            while (true)
            {
                try
                {
                    serverStream = clientSocket.GetStream();
                    int buffSize = 0;
                    byte[] inStream = new byte[10025];
                    buffSize = clientSocket.ReceiveBufferSize;
                    serverStream.Read(inStream, 0, buffSize);
                    string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                    char temp = (char)0;

                    // if message isnt empty (not clean DC)
                    if (String.Compare(returndata[0].ToString(), temp.ToString()) != 0)
                    {
                        readData = "> " + returndata + "\r\n";
                        msg();
                        // cut '{name} says: {proc} '
                        string tempS = returndata;
                        tempS = tempS.Substring(0, tempS.IndexOf("\r\n"));
                        for (int z = 0; z < 2; z++) tempS = tempS.Substring(tempS.IndexOf(" ") + 1);
                        // looking for MC calc command
                        if (tempS.IndexOf("mc ") == 0)
                        {
                            tempS = tempS.Substring(tempS.IndexOf(" ") + 1);
                            // get vars
                            double x1 = Convert.ToDouble(tempS.Substring(0, tempS.IndexOf(" ") +1));
                            tempS = tempS.Substring(tempS.IndexOf(" ") + 1);
                            double x2 = Convert.ToDouble(tempS.Substring(0, tempS.IndexOf(" ") + 1));
                            tempS = tempS.Substring(tempS.IndexOf(" ") + 1);
                            int n = Convert.ToInt32(tempS);

                            // check if they're correct
                            readData = "> x1 = " + x1 + ", x2 = " + x2 + ", n = " + n + ", calculating...\r\n";
                            msg();
                            if (clientSocket.Connected)
                            {
                                // calc + send + bye
                                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(MC(x1, x2, n) + "$");
                                serverStream.Write(outStream, 0, outStream.Length);
                                serverStream.Flush();
                            }

                        }
                    }
                    else
                    {
                        // throw ex
                        throw new Exception("DC");
                    }
                }
                catch (FormatException fEx)
                {
                    // wrong args
                    readData = "> Received wrong arguments!\r\n";
                    msg();
                }
                catch (Exception ex)
                {
                    // DC'd, close socket
                    readData = ">> Disconnected from the chat server\r\n";
                    msg();
                    button1.Enabled = true;
                    button2.Enabled = false;
                    if(clientSocket.Connected) clientSocket.Client.Disconnect(true);
                    break;
                }
            }
        }

        // seed generator
        static int GetSeed()
        {
            Random r = new Random();
            return Environment.TickCount * Thread.CurrentThread.ManagedThreadId + r.Next(-500, 500);
        }

        // MC
        private double MC(double x1, double x2, int n)
        {
            int count = 0;
            Random r = new Random(GetSeed());

            for (int i = 0; i < n; i++)
            {
                double x = r.NextDouble() * (x2 - x1) + x1;
                double y = r.NextDouble() * 2 - 1;

                if (y > 0 && Math.Sin(x) > y) count++;
                else if (y < 0 && Math.Sin(x) < y)  count--;
            }

            return (double)count / n * (x2 - x1) * 2;
        }

        // handle messages from thread
        private void msg()
        {
            if (this.richTextBox1.InvokeRequired)
            {
                this.richTextBox1.Invoke(new Action(delegate ()
                {
                    this.richTextBox1.AppendText(this.readData);
                    this.richTextBox1.SelectionStart = this.richTextBox1.Text.Length;
                    this.richTextBox1.ScrollToCaret();
                }));
            }
            else
            {
                richTextBox1.AppendText(this.readData);
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
        }
    }
}