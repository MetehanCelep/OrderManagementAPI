using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OrderManagementAPI.Data;
using OrderManagementAPI.DTOs;
using OrderManagementAPI.Models;
using System.Text.Json;

namespace OrderManagementAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ProductService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _cacheKey = "products";

        public ProductService(ApplicationDbContext context, IMapper mapper,
            IDistributedCache cache, ILogger<ProductService> logger, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ApiResponse<List<ProductDto>>> GetProductsAsync(string category = null)
        {
            try
            {
                var cacheKey = string.IsNullOrEmpty(category) ? _cacheKey : $"{_cacheKey}_{category}";

                // Try to get from Redis
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var cachedProducts = JsonSerializer.Deserialize<List<ProductDto>>(cachedData);
                    _logger.LogInformation("Products loaded from Redis cache");
                    return ApiResponse<List<ProductDto>>.Success(cachedProducts);
                }

                var query = _context.Products.Where(p => p.Status == true);

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(p => p.Category.ToLower() == category.ToLower());
                }

                var products = await query.ToListAsync();
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                // Cache to Redis for 10 minutes
                var cacheMinutes = _configuration.GetValue<int>("Cache:ProductCacheMinutes", 10);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheMinutes)
                };

                var serializedData = JsonSerializer.Serialize(productDtos);
                await _cache.SetStringAsync(cacheKey, serializedData, options);

                _logger.LogInformation($"Products loaded from database and cached to Redis. Count: {productDtos.Count}");
                return ApiResponse<List<ProductDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return ApiResponse<List<ProductDto>>.Failed("Ürünler getirilirken hata oluþtu", "PRODUCT_ERROR");
            }
        }
    }
}