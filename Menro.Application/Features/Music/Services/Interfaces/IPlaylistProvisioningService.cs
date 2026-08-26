using Menro.Domain.Entities.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Music.Services.Interfaces
{
    public interface IPlaylistProvisioningService
    {
        Task<Playlist> EnsureActivePlaylistAsync(int restaurantId);
    }
}
