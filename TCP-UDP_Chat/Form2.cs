using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Collections;

namespace sLanCS
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            // kill all threads
            Environment.Exit(Environment.ExitCode);
        }

        public static Hashtable clientsList = new Hashtable();
        public TcpListener serverSocket;
        public TcpClient clientSocket;
        public Thread ctThread;
        public string value = null;

        // starting the server
        public void Start(int port)
        {
            serverSocket = new TcpListener(port);
            clientSocket = new TcpClient();
            clientSocket = default(TcpClient);

            serverSocket.Start();
            richTextBox1.AppendText(">> sLanCS Server started at port " + port + " ...\r\n");

            ctThread = new Thread(joinClient);
            ctThread.Start();
        }
        
        // new connection handler
        public void joinClient()
        {
            // listning for new client connection
            while (true)
            {
                try
                {
                    clientSocket = serverSocket.AcceptTcpClient();
	
					// prawidlowe rozwiazanie to
					// byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                    byte[] bytesFrom = new byte[10025];
                    string dataFromClient = null;

                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                    // estabilishing new connection
                    try
                    {
                        clientsList.Add(dataFromClient, clientSocket);
                        mvp(true);
                        value = "> " + dataFromClient + " joined lobby (" + label3.Text + " zombies online)\r\n";
                        broadcast(dataFromClient + " joined lobby (" + label3.Text + " zombies online)\r\n", dataFromClient, false);
                        msg();

                        handleClient client = new handleClient();
                        client.startClient(clientSocket, dataFromClient, clientsList, this);
                    }
                    catch
                    {
                        value = "> " + dataFromClient + " tried connect to lobby (disconnected -- same nick)\r\n";
                        // message that sad zombie!
                        Byte[] broadcastBytes = null;
                        broadcastBytes = Encoding.ASCII.GetBytes(dataFromClient + " tried connect to lobby (disconnected -- same nick)\r\n");
                        networkStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                        // + rest
                        broadcast(dataFromClient + " tried connect to lobby (disconnected -- same nick)\r\n", "", false);
                        msg();
                        clientSocket.Client.Disconnect(true);
                    }
                }
                catch (Exception ex)
                {
                    value = ex.ToString();
                    msg();
                }
            }

            // server crashed, should never happen
            value = ">> sLanCS Server stopped ...\r\n";
            msg();
            if(clientSocket.Connected)  clientSocket.Client.Disconnect(true);
            if (serverSocket.Server.Connected) serverSocket.Server.Disconnect(true);
            button1.Enabled = true;
            label1.Visible = false;
            label3.Visible = false;
        }

        // broadcasting messages
        public static void broadcast(string msg, string uName, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                try
                {
                    NetworkStream broadcastStream = broadcastSocket.GetStream();
                    Byte[] broadcastBytes = null;

                    if (flag == true) broadcastBytes = Encoding.ASCII.GetBytes(uName + " says: " + msg + "\r\n");
                    else broadcastBytes = Encoding.ASCII.GetBytes(msg);

                    broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                    broadcastStream.Flush();
                }
                catch { }
            }
        }


        public class handleClient : Form2
        {
            TcpClient clientSocket;
            string clNo;
            Hashtable clientsList;

            // creating thread for another client's socket
            public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList, Form2 f2)
            {
                this.clientSocket = inClientSocket;
                this.clNo = clineNo;
                this.clientsList = cList;
                // required for thread of thread communication [otherwise handling item will be empty]
                this.richTextBox1 = f2.richTextBox1;
                this.label3 = f2.label3;

                Thread ctThrd = new Thread(doChat);
                ctThrd.Start();
            }

            // broadcasting client messages via chat room
            private void doChat()
            {
                byte[] bytesFrom = new byte[10025];
                string dataFromClient = null;
                Byte[] sendBytes = null;
                string serverResponse = null;

                while (true)
                {
                    try
                    {
                        NetworkStream networkStream = clientSocket.GetStream();
                        networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                        if (SocketConnected(clientSocket))
                        {
                            value = "> " + clNo + ": " + dataFromClient + "\r\n";
                            msg();

                            broadcast(dataFromClient, clNo, true);
                        }
                        else throw new Exception("DC/left");
                    }
                    catch (Exception ex)
                    {
                        mvp(false);
                        value = "> " + clNo + " left lobby (" + label3.Text + " zombies online)\r\n";
                        msg();
                        broadcast(clNo + " left lobby (" + label3.Text + " zombies online)\r\n", clNo, false);
                        clientsList.Remove(clNo);
                        clientSocket.Client.Disconnect(true);
                        break;
                    }
                }
            }
        }

        // multithread communication
        public void msg()
        {
            if (this.richTextBox1.InvokeRequired)
            {
                this.richTextBox1.Invoke(new Action(delegate ()
                {
                    this.richTextBox1.AppendText(this.value);
                    this.richTextBox1.SelectionStart = this.richTextBox1.Text.Length;
                    this.richTextBox1.ScrollToCaret();
                })); 
            }
            else
            {
                richTextBox1.AppendText(this.value);
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
        }

        public void mvp(Boolean isNewConnection)
        {
            if (this.label3.InvokeRequired)
            {
                this.label3.Invoke(new Action(delegate ()
                {
                    if(isNewConnection == true) this.label3.Text = Convert.ToString(Int32.Parse(this.label3.Text) + 1);
                    else this.label3.Text = Convert.ToString(Int32.Parse(this.label3.Text) - 1);
                }));
            }
            else
            {
                if (isNewConnection == true) label3.Text = Convert.ToString(Int32.Parse(label3.Text) + 1);
                else label3.Text = Convert.ToString(Int32.Parse(label3.Text) - 1);
            }
        }

        // check if client is still connected
        bool SocketConnected(TcpClient s)
        {
            bool part1 = s.Client.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            label1.Visible = true;
            label3.Visible = true;

            Start(Int32.Parse(textBox2.Text));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText("> Master: " + Convert.ToString(textBox1.Text) + "\r\n");
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            broadcast(Convert.ToString(textBox1.Text), "Master", true);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}