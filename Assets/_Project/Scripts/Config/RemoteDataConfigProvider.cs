using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.DataConfig;
using Newtonsoft.Json;
using UnityEngine;

namespace Dreamy.Template
{
    public sealed class RemoteDataConfigProvider : IRemoteConfigProvider
    {
        public UniTask<string> FetchJsonAsync(
            string documentName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                //string json = DreamySDK.RemoteConfig_GetData<string>(documentName);
                //return UniTask.FromResult(json);
                return UniTask.FromResult<string>(null);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[RemoteDataConfigProvider] Failed to fetch remote config '{documentName}': {ex.Message}");
                return UniTask.FromResult<string>(null);
            }
        }
    }
}