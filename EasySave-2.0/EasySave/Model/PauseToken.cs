using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Model
{
    public class PauseTokenSource
    {
        private volatile TaskCompletionSource<bool> _paused;

        public PauseToken Token => new PauseToken(this);

        public bool IsPaused => _paused != null;

        public void Pause()
        {
            if (_paused == null)
                _paused = new TaskCompletionSource<bool>();
        }

        public void Resume()
        {
            while (true)
            {
                var tcs = _paused;
                if (tcs == null) return;
                if (Interlocked.CompareExchange(ref _paused, null, tcs) == tcs)
                {
                    tcs.SetResult(true);
                    break;
                }
            }
        }

        public Task WaitWhilePausedAsync()
        {
            var tcs = _paused;
            return tcs?.Task ?? Task.CompletedTask;
        }
    }

    public struct PauseToken
    {
        private readonly PauseTokenSource _source;

        public PauseToken(PauseTokenSource source) { _source = source; }

        public bool IsPaused => _source?.IsPaused ?? false;

        public Task WaitWhilePausedAsync()
        {
            return _source?.WaitWhilePausedAsync() ?? Task.CompletedTask;
        }
    }
}