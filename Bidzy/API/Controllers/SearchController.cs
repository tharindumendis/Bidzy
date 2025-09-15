using Bidzy.API.DTOs;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController(IAuctionRepository auctionRepository) : ControllerBase
    {
        private readonly IAuctionRepository _auctionRepository = auctionRepository;

        [HttpGet]
        public async Task<IActionResult> SearchAuctions([FromQuery] AuctionSearchParams searchParams)
        {
            var result = await _auctionRepository.SearchAuctionsAsync(searchParams);

            return Ok(new
            {
                currentPage = searchParams.Page,
                pageSize = searchParams.PageSize,
                totalCount = result.TotalCount,
                totalPages = (int)Math.Ceiling(result.TotalCount / (double)searchParams.PageSize),
                items = result.Items.Select(x => x.ToReadDto())
            });
        }
    }
}
