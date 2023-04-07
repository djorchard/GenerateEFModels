//  DanTimer
//  Simple Timer Class
//
//  Ver 1.03    27/11/2022  Renamed Stopwatch (to stop all the conflicts) and created a new Timer class with event handling
//  Ver 1.02    13/02/2022  Added Restart method
//  Ver 1.00    14/01/2022  Dan Barnes Initial Version

using System.Timers;

namespace Dan
{

    internal class Timer :IDisposable
    {
        private readonly System.Timers.Timer timer;
        private readonly Action _onTick;
        private readonly bool _waitForPreviousTick;
        private bool ticking;

        // ReSharper disable once InconsistentNaming
        public Timer(Action OnTick,int intervalMs,bool waitForPreviousTick = true)
        {
            _waitForPreviousTick = waitForPreviousTick;
            _onTick = OnTick;
            timer = new System.Timers.Timer(intervalMs);
            timer.Elapsed += OnTickInternal;
            timer.Start();
        }

        private void OnTickInternal(object? sender, ElapsedEventArgs e)
        {
            if (_waitForPreviousTick)
            {
                while (ticking)
                {
                    Thread.Sleep(10);
                }
                ticking = true;
            }
            
            _onTick.Invoke();
            ticking = false;
        }

        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
        }
    }

    internal class Stopwatch
    {
        private DateTime _startTime;
        public Stopwatch()
        {
            _startTime = DateTime.Now;
        }

        public void Restart()
        {
            _startTime = DateTime.Now;
        }

        public double ElapsedMs => (DateTime.Now - _startTime).TotalMilliseconds;

        public double ElapsedSeconds => (DateTime.Now - _startTime).TotalSeconds;
        public double ElapsedMinutes => (DateTime.Now - _startTime).TotalMinutes;

        public double ElapsedHours => (DateTime.Now - _startTime).TotalHours;
    }
}