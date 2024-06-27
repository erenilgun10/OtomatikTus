using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ototus
{
    public partial class Form1 : Form
    {
        private Keys selectedKey1 = Keys.None;
        private Keys selectedKey2 = Keys.None;
        private int interval = 1000; 
        private bool isRunning = false;
        private Thread keyPressThread;

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const int KEYEVENTF_KEYDOWN = 0x0000; 
        private const int KEYEVENTF_KEYUP = 0x0002;   

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyPress += Form1_KeyPress;
        }
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '-')
            {
                btnStop_Click(sender, e);
            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            tus1.Click += new EventHandler(Tus1_Click);
            tus2.Click += new EventHandler(Tus2_Click);
        }

        private void Tus1_Click(object sender, EventArgs e)
        {
            selectedKey1 = SelectKey();
            label1.Text = selectedKey1.ToString();
        }

        private void Tus2_Click(object sender, EventArgs e)
        {
            selectedKey2 = SelectKey();
            label2.Text = selectedKey2.ToString();
        }

        private Keys SelectKey()
        {
            using (var form = new KeyCaptureForm())
            {
                form.ShowDialog();
                return form.SelectedKey;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (selectedKey1 == Keys.None && selectedKey2 == Keys.None)
            {
                MessageBox.Show("Bir tuş seçin");
                return;
            }

            if (!int.TryParse(textBox1.Text, out interval) || interval < 1 || interval > 1000)
            {
                MessageBox.Show("Lütfen 1 ile 1000 arasında geçerli bir ms değeri girin.");
                return;
            }
            
            if (isRunning)
            {
                MessageBox.Show("Program zaten çalışıyor.");
                return;
            }

            isRunning = true;
            keyPressThread = new Thread(AutoKeyPress);
            keyPressThread.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isRunning = false;
            keyPressThread?.Abort();
            keyPressThread = null;

            if (keyPressThread == null || !keyPressThread.IsAlive)
            {
                keyPressThread = new Thread(AutoKeyPress);
                keyPressThread.Start();
            }


        }
        private void AutoKeyPress()
        {
            while (isRunning)
            {
                if (selectedKey1 != Keys.None)
                {
                    keybd_event((byte)selectedKey1, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                    Thread.Sleep(50);
                    keybd_event((byte)selectedKey1, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }

                if (selectedKey2 != Keys.None)
                {
                    keybd_event((byte)selectedKey2, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                    Thread.Sleep(50);
                    keybd_event((byte)selectedKey2, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }

                Thread.Sleep(interval);
            }
        }
    }

    public class KeyCaptureForm : Form
    {
        public Keys SelectedKey { get; private set; } = Keys.None;

        public KeyCaptureForm()
        {
            this.KeyDown += new KeyEventHandler(KeyCaptureForm_KeyDown);
            this.Text = "Tuşa Basın";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoSize = true; 
        }

        private void KeyCaptureForm_KeyDown(object sender, KeyEventArgs e)
        {
            SelectedKey = e.KeyCode;
            this.Close();
        }
    }
}
