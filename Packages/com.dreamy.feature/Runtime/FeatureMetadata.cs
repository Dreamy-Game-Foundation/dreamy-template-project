using System;

namespace Dreamy.Feature
{
    public readonly struct FeatureMetadata
    {
        public FeatureMetadata(FeatureId id, FeatureState state)
        {
            if (!id.Equals(default))
            {
                Id = id;
                State = state;
                return;
            }

            throw new ArgumentException("Feature ID must be valid.", nameof(id));
        }

        public FeatureId Id { get; }
        public FeatureState State { get; }
    }
}
