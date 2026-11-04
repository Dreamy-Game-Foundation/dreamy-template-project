namespace Dreamy.UI
{
    public interface IStaggerableTween
    {
        void SetStaggerDelay(float showDelay, float hideDelay);
        void ClearStaggerDelay();
    }
}
