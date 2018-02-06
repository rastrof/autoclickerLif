using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Windows.Media;

namespace AutoClickerLif
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static SpeechSynthesizer _speaker = new SpeechSynthesizer();
        private static AutoClicker _autoClicker = new AutoClicker(_speaker);
        
        // this is not how you're supposed to do it, WPF likes databinding, but I cant be arsed atm.
        private static string _typeRadio = String.Empty;
        private static Int32 _quiverSize = 31;
        
        
        
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                

                if  (key == Keys.F2)
                {                    
                    Start();
                } else if (key == Keys.F5)
                {                    
                    Stop();
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }        

        private static void Start()
        {
            if (_speaker.State == SynthesizerState.Speaking)
            {
                Debug.WriteLine("Already speaking to user, returning");
                return;
            }

            // already busy ?
            if (_autoClicker.Running)
            {
                Debug.WriteLine("Already running, stopping");
                return;
            }             
            
            AutoClickType clicktype = AutoClickType.Unknown;
            if (Enum.TryParse(_typeRadio, out clicktype))
            {
                _speaker.SpeakAsync("start auto click");
                _autoClicker.Start(clicktype, _quiverSize);
            }else
            {
                _speaker.SpeakAsync("Error, Please select an option to begin");                
                Debug.WriteLine("parsing Radio failed");
            }

        }

        private static void Stop()
        {

            if (_speaker.State == SynthesizerState.Speaking)
            {
                Debug.WriteLine("Already speaking to user, returning");
                return;
            }

            // already stopped ?
            if (!_autoClicker.Running)
            {
                _speaker.SpeakAsync("auto click already stopped");
                return;
            }             

            _speaker.SpeakAsync("stop auto click");
            _autoClicker.Stop();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public MainWindow()
        {
            _hookID = SetHook(_proc);

            InitializeComponent();

            //_speaker.SelectVoice()

            foreach (InstalledVoice voice in _speaker.GetInstalledVoices())
            {
                VoiceInfo info = voice.VoiceInfo;
                Debug.WriteLine(" Voice Name: " + info.Name);

                if (info.Name.Equals("Microsoft Zira Desktop"))
                {
                    _speaker.SelectVoice(info.Name);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton r = sender as System.Windows.Controls.RadioButton;

            if (r.IsChecked == true)
            {
                _typeRadio = r.Name;
            }
        }

        private void QuiverChanged(object sender, System.Windows.Input.KeyEventArgs e)
        {

            System.Windows.Controls.TextBox t = sender as System.Windows.Controls.TextBox;

            int i = 31;
            if (Int32.TryParse(t.Text, out i))
            {
                t.Background = Brushes.White;
            }
            else
            {
                t.Background = Brushes.Red;
            }

            _quiverSize = i;
        }
    }
}
