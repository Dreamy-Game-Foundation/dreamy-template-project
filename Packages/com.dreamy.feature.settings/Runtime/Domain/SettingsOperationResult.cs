namespace Dreamy.Settings
{
    public readonly struct SettingsOperationResult
    {
        private SettingsOperationResult(SettingsOperationStatus status, string message)
        {
            Status = status;
            Message = message;
        }

        public SettingsOperationStatus Status { get; }
        public string Message { get; }
        public bool IsSuccess => Status == SettingsOperationStatus.Succeeded;

        public static SettingsOperationResult Succeeded(string message = null) =>
            new(SettingsOperationStatus.Succeeded, message);

        public static SettingsOperationResult Unavailable(string message = null) =>
            new(SettingsOperationStatus.Unavailable, message);

        public static SettingsOperationResult Failed(string message = null) =>
            new(SettingsOperationStatus.Failed, message);
    }

    public enum SettingsOperationStatus
    {
        Succeeded,
        Unavailable,
        Failed
    }
}
