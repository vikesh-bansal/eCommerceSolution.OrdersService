using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators
{
    public class OrderItemUpdateRequestValidator:AbstractValidator<OrderItemUpdateRequest>
    {
        public OrderItemUpdateRequestValidator()
        {
            RuleFor(temp => temp.ProductID).NotEmpty().WithErrorCode("Product ID can't be blank");
            RuleFor(temp => temp.UnitPrice).NotEmpty().WithErrorCode("Unit price can't be blank").GreaterThan(0).WithErrorCode("Unit price can't be less than or equal to zero");
            RuleFor(temp => temp.UnitPrice).NotEmpty().WithErrorCode("Quantity can't be blank").GreaterThan(0).WithErrorCode("Quantity can't be less than or equal to zero");
        }
    }
}
