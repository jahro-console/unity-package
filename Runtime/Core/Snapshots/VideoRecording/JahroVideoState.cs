namespace JahroConsole
{
    public enum JahroVideoState
    {
        Idle,
        Recording,
        Stopping,
        Shadow,
        Committing,
        Error
    }

    public enum JahroVideoError
    {
        None,
        InsufficientDiskSpace,
        MaxDurationReached,
        FrameCaptureFailure,
        StorageLimitReached,
        Unknown
    }
}
