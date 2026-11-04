using System;

namespace Dreamy.Settings
{
    public interface IRateUsView
    {
        event Action<int> RatingSelected;
        event Action RateRequested;
        event Action CloseRequested;

        void SetRating(int rating);
        void SetInteractable(bool interactable);
        void Close();
    }
}
