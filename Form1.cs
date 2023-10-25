using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Management;

namespace Homework_26_10
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // константа, що ідентифікує повідомлення WM_SETTEXT
            const uint WM_SETTEXT = 0x0C;

            // імпортуємо функцію SendMEssage з бібліотеки user32.dll

            //[DllImport("user32.dll")]
            //public static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);

            /* список, в якому зберігатимуться об'єкти, з описом дочірніх процесів додатка */
            List<Process> Processes = new List<Process>();

            /* лічильник запущених процесів */
            int Counter = 0;


        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
    }
}
