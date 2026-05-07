namespace Menro.Application.Features.MusicPlayer.Dtos
{
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
