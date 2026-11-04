using System;
using Dreamy.UI;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.Settings
{
    public sealed class RateUsPresenter : IPanelPresenter
    {
        private readonly ISettingsService service;
        private readonly IRateUsView view;
        private CancellationTokenSource lifetime;
        private bool isBound;
        private bool isSubmitting;
        private int rating;
        private int generation;

        public RateUsPresenter(ISettingsService service, IRateUsView view)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public event Action<int> PositiveRatingSubmitted;
        public event Action<SettingsOperationResult> SubmissionFailed;

        public void Show()
        {
            if (!isBound)
            {
                lifetime = new CancellationTokenSource();
                generation++;
                view.RatingSelected += SelectRating;
                view.RateRequested += SubmitRating;
                view.CloseRequested += Close;
                isBound = true;
            }

            view.SetRating(rating);
            view.SetInteractable(!isSubmitting && service.GetState().CanRequestReview);
        }

        public void Dispose()
        {
            if (!isBound) return;
            isBound = false;
            generation++;
            isSubmitting = false;
            view.RatingSelected -= SelectRating;
            view.RateRequested -= SubmitRating;
            view.CloseRequested -= Close;
            lifetime.Cancel();
            lifetime.Dispose();
            lifetime = null;
        }

        private void SelectRating(int selectedRating)
        {
            if (isSubmitting) return;
            rating = selectedRating < 1 ? 1 : selectedRating > 5 ? 5 : selectedRating;
            view.SetRating(rating);
        }

        private void SubmitRating()
        {
            if (isSubmitting || rating <= 0) return;
            if (rating < 4) { Close(); return; }
            if (!service.GetState().CanRequestReview) return;
            SubmitAsync(rating, generation, lifetime.Token).Forget();
        }

        private async UniTask SubmitAsync(int submittedRating, int requestGeneration, CancellationToken token)
        {
            isSubmitting = true;
            view.SetInteractable(false);
            try
            {
                SettingsOperationResult result = await service.SubmitRatingAsync(submittedRating, token);
                if (!isBound || requestGeneration != generation || token.IsCancellationRequested) return;
                if (result.IsSuccess)
                {
                    PositiveRatingSubmitted?.Invoke(submittedRating);
                    if (isBound && requestGeneration == generation) Close();
                }
                else SubmissionFailed?.Invoke(result);
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                if (isBound && requestGeneration == generation)
                    SubmissionFailed?.Invoke(SettingsOperationResult.Failed(exception.Message));
            }
            finally
            {
                if (isBound && requestGeneration == generation)
                {
                    isSubmitting = false;
                    view.SetInteractable(service.GetState().CanRequestReview);
                }
            }
        }

        private void Close()
        {
            Dispose();
            view.Close();
        }
    }
}
