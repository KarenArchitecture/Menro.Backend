namespace Menro.Application.Common.Interfaces
{
    public interface IFileUrlService
    {
        string BuildFileUrl(string relativePath);
        string BuildIconUrl(string fileName);
        string BuildImageUrl(string fileName);
        string BuildProfileImageUrl(string fileName);
        string BuildAudioUrl(string fileName);
    }
}
