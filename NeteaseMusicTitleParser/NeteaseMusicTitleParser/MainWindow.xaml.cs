using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace NeteaseMusicTitleParser
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public DispatcherTimer systemProcessTick;
        public string stringMusicTitle = "";

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
        [DllImport("user32.dll")]
        static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);
        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rectCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            int left;
            int top;
            int right;
            int bottom;
        }
        public GUITHREADINFO? GetGuiThreadInfo(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
            {
                uint threadId = GetWindowThreadProcessId(hwnd, IntPtr.Zero);
                GUITHREADINFO guiThreadInfo = new GUITHREADINFO();
                guiThreadInfo.cbSize = Marshal.SizeOf(guiThreadInfo);
                if (GetGUIThreadInfo(threadId, ref guiThreadInfo) == false)
                    return null;
                return guiThreadInfo;
            }
            return null;
        }

        protected void SendText(string text)
        {
            IntPtr hwnd = GetForegroundWindow();
            try
            {
                if (String.IsNullOrEmpty(text))
                    return;
                GUITHREADINFO? guiInfo = GetGuiThreadInfo(hwnd);
                if (guiInfo != null)
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        SendMessage(guiInfo.Value.hwndFocus, 0x0102, (IntPtr)(int)text[i], IntPtr.Zero);
                    }
                }
            }
            catch (Exception ex)
            {
                TextBlock_MusicTitle.Text += "\n";
                TextBlock_MusicTitle.Text += ex.Message;
            }
            
        }

        public MainWindow()
        {
            InitializeComponent();
            InitialSystemTimer();
        }
        public void InitialSystemTimer()
        {
            if (systemProcessTick == null)
            {
                systemProcessTick = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1.0)
                };
                systemProcessTick.Tick += SystemProcessTask;
                systemProcessTick.Start();
            }
        }

        //定时器处理任务
        public void SystemProcessTask(object sender, EventArgs e)
        {
            string stringTitleCache = GetMusicTitle();
            if (stringMusicTitle != stringTitleCache)
            {
                stringMusicTitle = stringTitleCache;
                TextBlock_MusicTitle.Text = stringMusicTitle;
                try
                {
                    SendText(stringMusicTitle);
                    WriteTextFile(stringMusicTitle);
                }
                catch (Exception ex)
                {
                    TextBlock_MusicTitle.Text += "\n";
                    TextBlock_MusicTitle.Text += ex.Message;
                }

            }
        }

        private void WriteTextFile(string stringMusicTitle)
        {
            try
            {
                System.IO.File.WriteAllText(@".\NeteaseMusicTitle.txt", "正在播放：" + stringMusicTitle);
            }
            catch (Exception)
            {

            }
        }

        public string GetMusicTitle()
        {
            try
            {
                Process[] processMusic = Process.GetProcessesByName("cloudmusic");
                int proIndex = 0;
                foreach (Process pro in processMusic)
                {
                    if (processMusic[proIndex].MainWindowTitle.Length > 0)
                    {
                        return processMusic[proIndex].MainWindowTitle;
                    }
                    proIndex++;
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }

        }


    }
}
