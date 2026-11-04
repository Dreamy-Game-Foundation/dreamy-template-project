using System;

namespace Dreamy.Feature
{
    public readonly struct FeatureId : IEquatable<FeatureId>
    {
        public FeatureId(string value)
        {
            if (!TryParse(value, out FeatureId featureId))
            {
                throw new ArgumentException("Feature ID cannot be empty.", nameof(value));
            }

            Value = featureId.Value;
        }

        public string Value { get; }
        public bool Equals(FeatureId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is FeatureId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;

        public static bool TryParse(string value, out FeatureId featureId)
        {
            featureId = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            featureId = new FeatureId(value, true);
            return true;
        }

        private FeatureId(string value, bool isValidated)
        {
            Value = value;
        }
    }
}
