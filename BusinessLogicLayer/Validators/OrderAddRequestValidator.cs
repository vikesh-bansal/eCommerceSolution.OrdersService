﻿using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;
 

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators
{
    public class OrderAddRequestValidator:AbstractValidator<OrderAddRequest>
    {
        public OrderAddRequestValidator() {
            //UserID
            RuleFor(temp => temp.UserID).NotEmpty().WithErrorCode("User ID can't be blank");
            //Order Date
            RuleFor(temp => temp.OrderDate).NotEmpty().WithErrorCode("Order Date can't be blank");
            //Order Items
            RuleFor(temp => temp.OrderItems).NotEmpty().WithErrorCode("Order Items can't be blank");

        }
    }
}
