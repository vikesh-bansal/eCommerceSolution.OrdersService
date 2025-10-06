using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;

public class ProductDTOToOrderItemResponseMappingProfile:Profile
{
    public ProductDTOToOrderItemResponseMappingProfile() {
        CreateMap<ProductDTO, OrderItemResponse>().ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductID)).
                ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
    }
}
