namespace Dreamy.Feature
{
    public interface IFeatureView<in TState>
    {
        void Render(TState state);
    }
}
