using AutoMapper;
using Magnise_Test_Task.Data;
using Magnise_Test_Task.Interfaces;
using Magnise_Test_Task.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Magnise_Test_Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketController : ControllerBase
    {
        private readonly MarketDbContext _context;
        private readonly IAssetService _assetService;
        private readonly IMapper _mapper;

        public MarketController(MarketDbContext context, IAssetService assetService,  IMapper mapper)
        {
            _context = context;
            _assetService = assetService; 
            _mapper = mapper;
        }

        // GET: api/market/assets
        [HttpGet("assets")]
        public async Task<IActionResult> GetAssets()
        {
            var assets = await _context.Assets
                .Include(a => a.Mappings)
                .ToListAsync();

            var assetDTOs = _mapper.Map<List<AssetDTO>>(assets);

            return Ok(assetDTOs);
        }

        // GET: api/market/price? assetIds = { assetIds }
        [HttpGet("price")]
        public async Task<IActionResult> GetPrices([FromQuery] List<Guid> assetIds)
        {
            if (assetIds == null || assetIds.Count == 0)
            {
                return BadRequest("No asset IDs provided.");
            }

            var priceResponses = await _assetService.GetAssetPriceAsync(assetIds);
            return Ok(priceResponses);
        }
    }
}
