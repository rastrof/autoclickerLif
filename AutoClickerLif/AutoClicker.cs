
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Timers;
using WindowsInput;

namespace AutoClickerLif
{

    public enum AutoClickType { Unknown, Slinger, Archer, Melee,Fists }

    public class AutoClicker
    {

        private const int WIGGLE_AMOUNT = 20;
        private const int MELEE_HOLD_DURATION = 360;

        private Timer _eventTimer = new Timer();
        private Timer _holdTimer = new Timer();

        private bool _holdButton = false;
        private bool _autoStop = false;
        private int _autoStopCounter = 0;
        private int _autoStopNumber = 0;
        private int _originalDelayTime = 0;

        private InputSimulator _sim = new InputSimulator();

        private static SpeechSynthesizer _speaker;

        private bool _STOPPED = true;

        public bool Running
        {
            get
            {
                return !_STOPPED;
            }   
            
            private set { }         
        }

        public AutoClicker(SpeechSynthesizer speaker)
        {
            _eventTimer.Elapsed += EventTimer_Elapsed;
            _holdTimer.Elapsed += HoldTimer_Elapsed;

            _speaker = speaker;
        }
                
        private void EventTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_STOPPED) return;

            _sim.Mouse.LeftButtonDown();

            if (!_holdButton)
            {
                _sim.Mouse.LeftButtonUp();
            }
            else
            {               
                _eventTimer.Stop();
                _holdTimer.Start();
            }

            //randomize a bit to evade detection
            _eventTimer.Interval = _originalDelayTime + new System.Random().Next(-200, 200);
        }

        private void HoldTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_STOPPED) return;

            _holdTimer.Stop();

            _sim.Mouse.LeftButtonUp();

            _eventTimer.Start();

            if (_autoStop)
            {
                _autoStopCounter++;

                Debug.WriteLine("Autostop run " + _autoStopCounter);

                if (_autoStopCounter > _autoStopNumber)
                {
                    Stop();
                    _speaker.SpeakAsync("Auto stopped, probably out of ammo");
                }
            }            
        }


        public void Start(AutoClickType clickType,int quiverSize = 31)
        {
            _STOPPED = false;

            Debug.WriteLine("Quiversize " + quiverSize);

            switch (clickType)
            {
                case AutoClickType.Unknown:
                    Debug.WriteLine("Cannot Autoclick unknown");
                    return;
                case AutoClickType.Slinger:

                    _originalDelayTime = 3000;
                    _eventTimer.Interval = _originalDelayTime;
                    _holdTimer.Interval = 2000;
                    _holdButton = true;

                    _autoStop = true;
                    _autoStopCounter = 0;
                    _autoStopNumber = quiverSize;

                    break;
                case AutoClickType.Archer:

                    _originalDelayTime = 2500;
                    _eventTimer.Interval = _originalDelayTime;
                    _holdTimer.Interval = 5000;
                    _holdButton = true;

                    _autoStop = true;
                    _autoStopCounter = 0;
                    _autoStopNumber = quiverSize;

                    break;
                case AutoClickType.Fists:

                    _originalDelayTime = 1050;
                    _eventTimer.Interval = _originalDelayTime;
                    _holdTimer.Interval = MELEE_HOLD_DURATION;
                    _holdButton = true;
                    _autoStop = false;
                    break;
                case AutoClickType.Melee:

                    _originalDelayTime = 1550;
                    _eventTimer.Interval = _originalDelayTime;
                    _holdTimer.Interval = MELEE_HOLD_DURATION;
                    _holdButton = true;
                    _autoStop = false;
                    break;
                default:
                    return;
            }

            _speaker.Speak(clickType.ToString() + " , weapon ready !");
            _eventTimer.Start();            
        }

        public void Stop()
        {
            _STOPPED = true;

            _eventTimer.Interval = 300000;
            _holdTimer.Interval = 300000;
            _holdTimer.Stop();
            _eventTimer.Stop();
            
            _sim.Mouse.LeftButtonUp();
        }

        ~AutoClicker()
        {
            Stop();
            _eventTimer.Dispose();
            _holdTimer.Dispose();
        }
    }   

}
