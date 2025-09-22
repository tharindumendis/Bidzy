using System.Security.Claims;
using Bidzy.API.DTOs;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController(IAuctionRepository auctionRepository, ISearchhistoryRepository searchhistoryRepository) : ControllerBase
    {
        private readonly IAuctionRepository _auctionRepository = auctionRepository;
        private readonly ISearchhistoryRepository _searchRepository = searchhistoryRepository;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SearchAuctions([FromQuery] AuctionSearchParams searchParams)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _auctionRepository.SearchAuctionsAsync(searchParams);
            try
            {
                await _searchRepository.SaveSearchAsync(searchParams.Title, Guid.Parse(userId));
            }
            catch
            {
                Console.WriteLine("search save error");
            }
            return Ok(new
            {
                currentPage = searchParams.Page,
                pageSize = searchParams.PageSize,
                totalCount = result.TotalCount,
                totalPages = (int)Math.Ceiling(result.TotalCount / (double)searchParams.PageSize),
                items = result.Items.Select(x => x.ToReadDto())
            });
        }



        [HttpGet("guess")]
        public async Task<IActionResult> SearchGuessAuctions([FromQuery] AuctionSearchParams searchParams)
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
