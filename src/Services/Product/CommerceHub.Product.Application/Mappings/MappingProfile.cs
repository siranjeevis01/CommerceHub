using AutoMapper;
using CommerceHub.Product.Application.Commands;
using CommerceHub.Product.Application.DTOs;
using ProductEntity = CommerceHub.Product.Domain.Entities.Product;
using CommerceHub.Product.Domain.Entities;

namespace CommerceHub.Product.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProductEntity, ProductDto>()
            .ForMember(d => d.CompareAtPrice, o => o.MapFrom(s => s.ComparePrice))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : (double?)null))
            .ForMember(d => d.TotalReviews, o => o.MapFrom(s => s.Reviews.Count))
            .ForMember(d => d.Variants, o => o.MapFrom(s => s.Variants.OrderBy(v => v.Id)));

        CreateMap<ProductEntity, ProductListDto>()
            .ForMember(d => d.CompareAtPrice, o => o.MapFrom(s => s.ComparePrice))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : (double?)null));

        CreateMap<ProductVariant, ProductVariantDto>();
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ParentCategoryName, o => o.MapFrom(s => s.ParentCategory != null ? s.ParentCategory.Name : null))
            .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products.Count));

        CreateMap<Brand, BrandDto>()
            .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products.Count));

        CreateMap<CreateProductCommand, ProductEntity>()
            .ForMember(d => d.ComparePrice, o => o.MapFrom(s => s.CompareAtPrice))
            .ForMember(d => d.Slug, o => o.Ignore())
            .ForMember(d => d.ShortDescription, o => o.MapFrom(s => s.Description))
            .ForMember(d => d.LongDescription, o => o.Ignore())
            .ForMember(d => d.MainImageUrl, o => o.Ignore())
            .ForMember(d => d.GalleryImages, o => o.Ignore())
            .ForMember(d => d.StockQuantity, o => o.Ignore())
            .ForMember(d => d.StockStatus, o => o.Ignore())
            .ForMember(d => d.IsFeatured, o => o.Ignore())
            .ForMember(d => d.IsPublished, o => o.Ignore())
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Brand, o => o.Ignore())
            .ForMember(d => d.Variants, o => o.Ignore())
            .ForMember(d => d.AttributeValues, o => o.Ignore())
            .ForMember(d => d.Reviews, o => o.Ignore());

        CreateMap<CreateProductVariantDto, ProductVariant>()
            .ForMember(d => d.ComparePrice, o => o.Ignore())
            .ForMember(d => d.StockQuantity, o => o.Ignore())
            .ForMember(d => d.Attributes, o => o.Ignore())
            .ForMember(d => d.ProductId, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore());

        CreateMap<UpdateProductCommand, ProductEntity>()
            .ForMember(d => d.ComparePrice, o => o.MapFrom(s => s.CompareAtPrice))
            .ForMember(d => d.Slug, o => o.Ignore())
            .ForMember(d => d.ShortDescription, o => o.MapFrom(s => s.Description))
            .ForMember(d => d.LongDescription, o => o.Ignore())
            .ForMember(d => d.MainImageUrl, o => o.Ignore())
            .ForMember(d => d.GalleryImages, o => o.Ignore())
            .ForMember(d => d.StockQuantity, o => o.Ignore())
            .ForMember(d => d.StockStatus, o => o.Ignore())
            .ForMember(d => d.IsFeatured, o => o.Ignore())
            .ForMember(d => d.IsPublished, o => o.Ignore())
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Brand, o => o.Ignore())
            .ForMember(d => d.Variants, o => o.Ignore())
            .ForMember(d => d.AttributeValues, o => o.Ignore())
            .ForMember(d => d.Reviews, o => o.Ignore());

        CreateMap<CreateCategoryCommand, Category>()
            .ForMember(d => d.Slug, o => o.Ignore())
            .ForMember(d => d.ParentCategory, o => o.Ignore())
            .ForMember(d => d.SubCategories, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore())
            .ForMember(d => d.Attributes, o => o.Ignore());

        CreateMap<UpdateCategoryCommand, Category>()
            .ForMember(d => d.Slug, o => o.Ignore())
            .ForMember(d => d.ParentCategory, o => o.Ignore())
            .ForMember(d => d.SubCategories, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore())
            .ForMember(d => d.Attributes, o => o.Ignore());

        CreateMap<CreateBrandCommand, Brand>()
            .ForMember(d => d.Products, o => o.Ignore());

        CreateMap<UpdateBrandCommand, Brand>()
            .ForMember(d => d.Products, o => o.Ignore());
    }
}
