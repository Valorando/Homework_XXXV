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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Homework_26_10
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);

        const uint WM_SETTEXT = 0x0C;

        List<Process> Processes = new List<Process>();

        int Counter = 0;

        public Form1()
        {
            InitializeComponent();
            LoadAvailableAssemblies();
        }

        /* метод, що завантажує доступні виконувані файли з домашньої директорії проєкту */
        void LoadAvailableAssemblies()
        {
            // назва файлу складання поточного додатка
            string except = new FileInfo(Application.ExecutablePath).Name;

            // отримуємо назву файлу без розширення
            except = except.Substring(0, except.IndexOf("."));

            // отримуємо усі *.exe файли з домашньої директорії
            string[] files = Directory.GetFiles(Application.StartupPath, "*.exe");

            foreach (var file in files)
            {
                // отримуємо ім'я файлу
                string fileName = new FileInfo(file).Name;

                /* якщо ім'я афйлу не містить імені виконуваного файлу проєкту,
                   тоді воно додається до списку */

                if (fileName.IndexOf(except) == -1)
                    AvailableAssemblies.Items.Add(fileName);
            }
        }


        /* метод, що запускає процес для виконання і що зберігає об'єкт, який його описує */
        void RunProcess(string AssamblyName)
        {
            // запускаємо процес на основі виконуваного файлу.
            Process proc = Process.Start(AssamblyName);

            // додаємо процес до списку
            Processes.Add(proc);

            /* перевіряємо, чи став створений процес дочірнім
               по відношенню до поточного і, якщо став, виводимо MessageBox */
            if (Process.GetCurrentProcess().Id == GetParentProcessId(proc.Id))
                MessageBox.Show(proc.ProcessName + " дійсно, дочірній процес поточного процесу!");

            /* вказуємо, що процес має генерувати події */
            proc.EnableRaisingEvents = true;

            // додаємо обробник на подію завершення процесу
            proc.Exited += proc_Exited;

            /* розміщуємо новий текст головного вікна дочірнього процесу */
            SetChildWindowText(proc.MainWindowHandle, "Chils process #" + (++Counter));

            /* перевіряємо, чи запускали ми екземпляр такого додатка * і,
               якщо немає, то додаємо до списку запущених додатків */
            if (!StartedAssemblies.Items.Contains(proc.ProcessName))
                StartedAssemblies.Items.Add(proc.ProcessName);

            /* видаляємо додаток зі списку доступних додатків */
            AvailableAssemblies.Items.Remove(AvailableAssemblies.SelectedItem);
        }


        //=========
        private void UpdateAvailableAssemblies(string assemblyName)
        {
            if (AvailableAssemblies.InvokeRequired)
            {
                // Если это не главный поток, вызываем метод снова в главном потоке
                AvailableAssemblies.Invoke(new Action(() => UpdateAvailableAssemblies(assemblyName)));
            }
            else
            {
                // Этот код будет выполнен в главном потоке
                AvailableAssemblies.Items.Add(assemblyName);
            }
        }


        /* метод обгортання для надсилання повідомлення WM_SETTEXT */
        void SetChildWindowText(IntPtr Handle, string text)
        {
            SendMessage(Handle, WM_SETTEXT, 0, text);
        }

        /* метод, який отримує PID батьківського процесу (використовує WMI) */
        int GetParentProcessId(int Id)
        {
            int parentId = 0;
            using (ManagementObject obj = new ManagementObject("win32_process.handle=" + Id.ToString()))
            {
                obj.Get();
                parentId = Convert.ToInt32(obj["ParentProcessId"]);
            }
            return parentId;
        }

        private void proc_Exited(object sender, EventArgs e)
        {
            Process proc = sender as Process;

            // забираємо процес зі списку запущених додатків
            //StartedAssemblies.Items.Remove(proc.ProcessName);
            if (StartedAssemblies.InvokeRequired)
            {
                StartedAssemblies.BeginInvoke(new MethodInvoker(() => StartedAssemblies.Items.Remove(proc.ProcessName)));
            }
            else
            {
                StartedAssemblies.Items.Remove(proc.ProcessName);
            }

            // додаємо процес до списку доступних додатків
            //AvailableAssemblies.Items.Add(proc.ProcessName);
            UpdateAvailableAssemblies(proc.ProcessName);

            // забираємо процес зі списку дочірніх процесів
            Processes.Remove(proc);

            // зменшуємо лічильник дочірніх процесів на 1
            Counter--;
            int index = 0;

            /* змінюємо текст для головних вікон усіх дочірніх процесів */
            foreach (var p in Processes)
                SetChildWindowText(p.MainWindowHandle, "Child process #" + ++index);
        }

        // оголошення делегата, що приймає параметр типу Process
        delegate void ProcessDelegate(Process proc);

        /* метод, який виконує прохід по всіх дочірніх процесах із заданим
           ім'ям і виконує для цих процесів заданий делегатом метод */
        void ExecuteOnProcessesByName(string ProcessName, ProcessDelegate func)
        {
            /* отримуємо список запущених в операційній системі процесів */
            Process[] processes = Process.GetProcessesByName(ProcessName);
            foreach (var process in processes)

                /* якщо PID батьківського процесу дорівнює PID поточного процесу */
                if (Process.GetCurrentProcess().Id == GetParentProcessId(process.Id))

                    // запускаємо метод
                    func(process);
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            RunProcess(AvailableAssemblies.SelectedItem.ToString());
        }

        void Kill(Process proc)
        {
            proc.Kill();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            ExecuteOnProcessesByName(StartedAssemblies.SelectedItem.ToString(), Kill);
            StartedAssemblies.Items.Remove(StartedAssemblies.SelectedItem);
        }

        void CloseMainWindow(Process proc)
        {
            proc.CloseMainWindow();
        }

        private void CloseWindowButton_Click(object sender, EventArgs e)
        {
            ExecuteOnProcessesByName(StartedAssemblies.SelectedItem.ToString(), CloseMainWindow);
            StartedAssemblies.Items.Remove(StartedAssemblies.SelectedItem);
        }

        void Refresh(Process proc)
        {
            proc.Refresh();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ExecuteOnProcessesByName(StartedAssemblies.SelectedItem.ToString(), Refresh);
        }

        private void AvailableAssemblies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AvailableAssemblies.SelectedItems.Count == 0)
                StartButton.Enabled = false;
            else
                StartButton.Enabled = true;
        }

        private void StartedAssemblies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (StartedAssemblies.SelectedItems.Count == 0)
            {
                StopButton.Enabled = false;
                RefreshButton.Enabled = false;
                CloseWindowButton.Enabled = false;
            }
            else
            {
                StopButton.Enabled = true;
                RefreshButton.Enabled = true;
                CloseWindowButton.Enabled = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var proc in Processes)
                proc.Kill();
        }

        private void RunCalc_Click(object sender, EventArgs e)
        {
            RunProcess("calc.exe");
        }
    }
}
