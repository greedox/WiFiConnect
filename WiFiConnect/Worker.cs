using System;
using System.Threading;
using System.Threading.Tasks;

namespace WiFiConnect
{
    public class Worker
    {
        public bool IsWorks { get => (_token != null) && _token.IsCancellationRequested; }

        private int delay;
        private Task _workerThread;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

        public Worker(Action workerJob, int delay = 2500)
        {
            this.delay = delay;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            InitializeWorkerThread(workerJob);
        }

        public Worker(Action workerJob, CancellationTokenSource tokenSource)
        {
            _tokenSource = tokenSource;
            _token = _tokenSource.Token;
            InitializeWorkerThread(workerJob);
        }

        private void InitializeWorkerThread(Action workerJob)
        {
            _workerThread = new Task(() =>
            {
                while (!_token.IsCancellationRequested)
                {
                    workerJob();
                    Thread.Sleep(delay);
                }
            }, _token, TaskCreationOptions.LongRunning);
        }

        public void Start()
        {
            if (!IsWorks)
            {
                _workerThread.Start();
            }
        }

        public void Stop()
        {
            if (IsWorks)
            {
                _tokenSource.Cancel();
                _workerThread.Wait();
                _tokenSource.Dispose();
                _workerThread.Dispose();
                _tokenSource = null;
                _workerThread = null;
            }
        }
    }
}
