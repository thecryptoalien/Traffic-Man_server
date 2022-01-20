using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
// extras
using Microsoft.Extensions.Logging;
using System.Security;
using System.Windows;



using Ninja.WebSockets;
using SuperWebSocket;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace TrafficManServer
{
    public partial class Form1 : Form
    {
        // vars and shiz
        private string wsPassphrase = "lllooonnngggaaassssssSSTTRRIINNGG12345678900987654321";
        public DataTable dtConnected = new DataTable();
        int TogMove;
        int MValX;
        int MValY;
        public const char horizontalBar = (char)0x2015;
        public const char enDash = (char)0x2013;
        public const char emDash = (char)0x2014;
        public const char quoteSingleRight = (char)0x274c;
        public const char BallotBox = (char)0x2610;
        public const char HeavyMinus = (char)0x2582;   //268a        - 271a 2795 2796
        public const char Square4Corners = (char)0x25a2;

        //private static WebSocketClientFactory wsClient;
        private static WebSocketServer wsServer;
        public int wsPort = 8980;
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
        int nLeftRect, // x-coordinate of upper-left corner
        int nTopRect, // y-coordinate of upper-left corner
        int nRightRect, // x-coordinate of lower-right corner
        int nBottomRect, // y-coordinate of lower-right corner
        int nWidthEllipse, // height of ellipse
        int nHeightEllipse // width of ellipse
        );
        public Form1()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 5, 5));
            //label13.Text = Square4Corners.ToString();
            label12.Text = HeavyMinus.ToString();
            label11.Text = quoteSingleRight.ToString();
            wsServer = new WebSocketServer();
            //wsClient = new WebSocketClientFactory();
            // server setup 
            wsServer.Setup(wsPort);
            wsServer.NewSessionConnected += WsServer_NewSessionConnected;
            wsServer.NewMessageReceived += WsServer_NewMessageReceived;
            wsServer.NewDataReceived += WsServer_NewDataReceived;
            wsServer.SessionClosed += WsServer_SessionClosed;
            // add column to datatable and connect to datagrid            
            dtConnected.Columns.Add("Connected Clients", typeof(String));
            dataGridView1.DataSource = dtConnected;
            dataGridView1.Columns[0].Width = 248;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.Black;
            dataGridView1.Columns[0].HeaderCell.Style.SelectionForeColor = Color.Black;
            dataGridView1.Columns[0].HeaderCell.Style.SelectionBackColor = Color.LightGray;
            dataGridView1.Columns[0].HeaderCell.Style.ForeColor = Color.LightGray;
            dataGridView1.Columns[0].HeaderCell.Style.BackColor = Color.Black;
            dataGridView1.RowHeadersDefaultCellStyle.ForeColor = Color.LightGray;
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.Black;
            dataGridView1.DefaultCellStyle.ForeColor = Color.LightGray;
            dataGridView1.DefaultCellStyle.BackColor = Color.Black;
            //Change cell font
            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                c.DefaultCellStyle.Font = new Font("Segoe UI", 8.5F, GraphicsUnit.Pixel);//Segoe UI Semibold, 8.25pt, style=Bold
            }
            // test shiz
            

        }
        // Form Load
        private void Form1_Load(object sender, EventArgs e)
        {
            // do stuff
            DebugBox("App Started...");


        }
        // Websocket Stuffs

        private void WsServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            this.Invoke(new Action(() => webBox("Session " + session.SessionID + " Closed...")));
            // remove from data table
            try
            {
                DataRow[] row = dtConnected.Select("[Connected Clients] = '" + session.SessionID + "'");
                int rowNum = dtConnected.Rows.IndexOf(row[0]);
                dtConnected.Rows.RemoveAt(rowNum);
                this.Invoke(new Action(() => dataGridView1.Refresh()));
                this.Invoke(new Action(() => wsConCountUpdate()));
            }
            catch
            {
                this.Invoke(new Action(() => webBox("Session Closed Error!")));
                this.Invoke(new Action(() => wsConCountUpdate()));
            }
        }

        private void WsServer_NewDataReceived(WebSocketSession session, byte[] value)
        {
            this.Invoke(new Action(() => webBox("New Data Received From: " + session.SessionID)));
        }

        private void WsServer_NewMessageReceived(WebSocketSession session, string value)
        {
            this.Invoke(new Action(() => webBox("New Message Received: " + value + " From: " + session.SessionID)));
            if (value == "Hello Server")
            {
                session.Send("Hello Client " + session.SessionID);
            }
            else
            {
                // decryption0
                string dMsg = StringCipher.Decrypt(value, wsPassphrase);
                this.Invoke(new Action(() => webBox("Decrypted0 Message: " + dMsg)));
                // split0
                string[] spl0 = dMsg.Split('@');
                // handle command
                switch (spl0[0])
                {
                    case "ECHO":
                        this.Invoke(new Action(() => webBox("Command Received: ECHO")));
                        // decrypt echo message
                        string dpMsg = StringCipher.Decrypt(spl0[1], wsPassphrase);
                        this.Invoke(new Action(() => webBox("Decrypted ECHO Message: " + dpMsg)));
                        // echo message
                        session.Send(dpMsg);

                        break;
                    default:
                        this.Invoke(new Action(() => webBox("Message Did not contain a command...")));
                        break;

                }
                //this.Invoke(new Action(() => webBox("Decrypted0 Message0: " + spl0[0])));
                //this.Invoke(new Action(() => webBox("Decrypted0 Message1: " + spl0[1])));
                //string[] spl1 = spl0[0].Split(':');
                //this.Invoke(new Action(() => webBox("Command Message: " + spl1[0])));
                //this.Invoke(new Action(() => webBox("Command Message Args: " + spl1[1])));
                //string[] argSpl = spl1[1].Split('-');
                //this.Invoke(new Action(() => webBox("Command Arg Strings: " + argSpl[0])));
                //this.Invoke(new Action(() => webBox("Command Arg Bools: " + argSpl[1])));
                //this.Invoke(new Action(() => webBox("Command Arg Ints: " + argSpl[2])));
                // decryption1
                //string dpMsg = StringCipher.Decrypt(spl0[1], wsPassphrase);
                //this.Invoke(new Action(() => webBox("Decrypted1 Message: " + dpMsg)));
                // split decrypted1 message  for each loop??
                //string[] words = dpMsg.Split('$');
                //this.Invoke(new Action(() => webBox("Command String0: " + words[0])));
                //this.Invoke(new Action(() => webBox("Command String1: " + words[1])));
                //this.Invoke(new Action(() => webBox("Command String2: " + words[2])));
                //this.Invoke(new Action(() => webBox("Command Bool0: " + words[3])));
                //this.Invoke(new Action(() => webBox("Command Bool1: " + words[4])));
                //this.Invoke(new Action(() => webBox("Command Int0: " + words[5])));

            }
        }

        private void WsServer_NewSessionConnected(WebSocketSession session)
        {
            this.Invoke(new Action(() => webBox("New Session " + session.SessionID + " Connected...")));
            // populate datatable
            DataRow dr = dtConnected.NewRow();
            dr["Connected Clients"] = session.SessionID;
            dtConnected.Rows.Add(dr);
            this.Invoke(new Action(() => dataGridView1.Refresh()));
            this.Invoke(new Action(() => wsConCountUpdate()));
            //var session = wsServer.GetAppSessionByID(sessionID);
            //if (session != null)
                // send message
            session.Send("Test send to client......");

        }

        // test shiz



        // WebSocket Connection Count Updater
        public void wsConCountUpdate()
        {
            label10.Text = dtConnected.Rows.Count.ToString();
        }

        // WebSocket Box 
        public void webBox(string webMsg)
        {
            string TimeStamp = DateTime.Now.ToString("@-hh:mm:ss-MM-dd-yyyy-> ");
            richTextBox1.Focus();
            richTextBox1.AppendText(TimeStamp + webMsg + Environment.NewLine);

        }

        // Debug Box 
        public void DebugBox(string dBugMsg)
        {
            string TimeStamp = DateTime.Now.ToString("@-hh:mm:ss-MM-dd-yyyy-> ");
            richTextBox3.Focus();
            richTextBox3.AppendText(TimeStamp + dBugMsg + Environment.NewLine);

        }
        // Start WebSocket Server
        private void button1_Click(object sender, EventArgs e)
        {
            DebugBox("Starting WebSocket Server...");
            wsServer.Start();
            webBox("Server is running on port " + wsPort + "...");
            //wsTask = StartWebServer();
            //wsTask.Wait();

        }
        // Stop WebSocket Server
        private void button2_Click(object sender, EventArgs e)
        {
            DebugBox("Stopping WebSocket Server...");
            wsServer.Stop();
            webBox("Server running on port " + wsPort + " Stopped...");
            //wsTask.Dispose();
        }
        // Start Api Server
        private void button3_Click(object sender, EventArgs e)
        {
            DebugBox("Starting Api Server...");

        }
        // Stop Api Server
        private void button4_Click(object sender, EventArgs e)
        {
            DebugBox("Stopping Api Server...");

        }
        // Send Test Button
        private void button6_Click(object sender, EventArgs e)
        {
            // Send Test - to selected user
            DebugBox("Send Test to Selected user...");
            // get session ids from datagrid view
            DataGridViewSelectedRowCollection rows = dataGridView1.SelectedRows;
            for (int i = 0; i < rows.Count; i++)
            {
                // get indexed session id
                string sessionID = (string)rows[i].Cells[0].Value;
                this.Invoke(new Action(() => webBox("Sending Message To " + sessionID)));
                // connect to session
                var session = wsServer.GetAppSessionByID(sessionID);
                if (session != null)
                    // send message
                    session.Send("Test send to client......");
            }




        }
        // Disconnect Button
        private void button5_Click(object sender, EventArgs e)
        {
            // Disconnect selected user
            DebugBox("Disconnecting Selected user...");
            DataGridViewSelectedRowCollection rows = dataGridView1.SelectedRows;
            for (int i = 0; i < rows.Count; i++)
            {
                // get indexed session id
                string sessionID = (string)rows[i].Cells[0].Value;
                this.Invoke(new Action(() => webBox("Sending Disconnect Message To " + sessionID)));
                // connect to session
                var session = wsServer.GetAppSessionByID(sessionID);
                string disReason = "Just Because I Said SO!";
                string enDisReason = StringCipher.Encrypt(disReason, wsPassphrase);
                string[] disMsgA = { "DISCONNECT", enDisReason };
                string disMsg = string.Join("@", disMsgA);
                string enDisMsg = StringCipher.Encrypt(disMsg, wsPassphrase);
                if (session != null)
                    // send disconnect message                    
                    session.Send(enDisMsg);
            }
        }

        // encryption shiz
        public static class StringCipher
        {
            // This constant is used to determine the keysize of the encryption algorithm in bits.
            // We divide this by 8 within the code below to get the equivalent number of bytes.
            private const int Keysize = 256;

            // This constant determines the number of iterations for the password bytes generation function.
            private const int DerivationIterations = 1000;

            public static string Encrypt(string plainText, string passPhrase)
            {
                // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
                // so that the same Salt and IV values can be used when decrypting.  
                var saltStringBytes = Generate256BitsOfRandomEntropy();
                var ivStringBytes = Generate256BitsOfRandomEntropy();
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
                {
                    var keyBytes = password.GetBytes(Keysize / 8);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                                {
                                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                    cryptoStream.FlushFinalBlock();
                                    // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                    var cipherTextBytes = saltStringBytes;
                                    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Convert.ToBase64String(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }

            public static string Decrypt(string cipherText, string passPhrase)
            {
                // Get the complete stream of bytes that represent:
                // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
                var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
                // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
                var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
                // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
                var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
                // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
                var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

                using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
                {
                    var keyBytes = password.GetBytes(Keysize / 8);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                        {
                            using (var memoryStream = new MemoryStream(cipherTextBytes))
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                                {
                                    var plainTextBytes = new byte[cipherTextBytes.Length];
                                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }

            private static byte[] Generate256BitsOfRandomEntropy()
            {
                var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    // Fill the array with cryptographically secure random bytes.
                    rngCsp.GetBytes(randomBytes);
                }
                return randomBytes;
            }
        }
        // close button
        private void label11_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label12_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            TogMove = 1;
            MValX = e.X;
            MValY = e.Y;


        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            TogMove = 0;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (TogMove == 1)
            {
                this.SetDesktopLocation(MousePosition.X - MValX, MousePosition.Y - MValY);
            }
        }

    }    
}
