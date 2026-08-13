namespace Menro.Web.Hubs
{
    public static class MusicHubEvents
    {
        public const string TrackRequested = "RequestCreated";
        public const string TrackApproved = "RequestApproved";
        public const string TrackRejected = "RequestRejected";
        public const string PlaylistChanged = "PlaylistChanged";
        public const string PlaybackChanged = "PlaybackChanged";
    }
}