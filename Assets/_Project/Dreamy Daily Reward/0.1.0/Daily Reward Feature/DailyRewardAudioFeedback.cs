using Dreamy.Audio;
using Dreamy.DailyReward;
using Dreamy.Economy;
using UnityEngine;

namespace Dreamy.Feature.DailyReward.Integration
{
    public sealed class DailyRewardAudioFeedback : MonoBehaviour, IDailyRewardFeedback
    {
        [SerializeField] private string claimedAudioId;
        [SerializeField] private string failedAudioId;
        public void PlayClaimed(ResourceAmount reward)
        {
            if (DreamyAudio.IsInitialized && AudioKey.TryParse(claimedAudioId, out AudioKey key))
                DreamyAudio.Play(key);
        }

        public void PlayClaimFailed()
        {
            if (DreamyAudio.IsInitialized && AudioKey.TryParse(failedAudioId, out AudioKey key))
                DreamyAudio.Play(key);
        }
    }
}
