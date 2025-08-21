using AutoMapper;
using OrderManagementAPI.DTOs;
using OrderManagementAPI.Entities;

namespace OrderManagementAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();
        }
    }
}