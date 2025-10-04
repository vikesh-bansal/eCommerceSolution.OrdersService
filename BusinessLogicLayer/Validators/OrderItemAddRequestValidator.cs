using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public  class OrderItemAddRequestValidator:AbstractValidator<OrderItemAddRequest>
{
   public OrderItemAddRequestValidator()
    {
        RuleFor(temp => temp.ProductID).NotEmpty().NotEmpty().WithErrorCode("Product ID can't be blank");
        RuleFor(temp => temp.UnitPrice).NotEmpty().WithErrorCode("Unit price can't be blank").GreaterThan(0).WithErrorCode("Unit price can't be less than or equal to zero");
        RuleFor(temp => temp.Quantity).NotEmpty().WithErrorCode("Quantity can't be blank").GreaterThan(0).WithErrorCode("Quantity can't be less than or equal to zero");

    }
}
