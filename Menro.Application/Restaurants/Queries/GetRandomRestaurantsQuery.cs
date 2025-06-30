using MediatR;
using Menro.Application.Restaurants.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Queries
{
    public class GetRandomRestaurantsQuery : IRequest<List<RestaurantDto>>
    {
        public int Count { get; set; } = 10;
    }
}
