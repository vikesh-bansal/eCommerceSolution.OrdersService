using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
public class OrderUpdateRequestValidator:AbstractValidator<OrderUpdateRequest>
{
    public OrderUpdateRequestValidator()
    {
        //OrderID
        RuleFor(temp => temp.OrderID).NotEmpty().WithErrorCode("Order ID can't be blank");
        //UserID
        RuleFor(temp => temp.UserID).NotEmpty().WithErrorCode("User ID can't be blank");
        //Order Date
        RuleFor(temp => temp.OrderDate).NotEmpty().WithErrorCode("Order Date can't be blank");
        //Order Items
        RuleFor(temp => temp.OrderItems).NotEmpty().WithErrorCode("Order Items can't be blank");

    }
}
