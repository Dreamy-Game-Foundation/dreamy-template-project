using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.DataConfig;

namespace Dreamy.Template
{
    public sealed class RemoteDataConfigProvider : IRemoteConfigProvider
    {
        public UniTask<string> FetchJsonAsync(
            string documentName,
            CancellationToken cancellationToken = default)
        {
            // Return null until a remote SDK adapter is connected.
            // RemoteConfigSource treats it as missing and falls back to Resources.
            return UniTask.FromResult<string>(null);
        }
    }
}
