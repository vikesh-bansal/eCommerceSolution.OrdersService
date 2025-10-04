using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersMicroservice.API.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
        private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
        private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
        private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
        public OrdersController(IOrdersService ordersService, IValidator<OrderAddRequest> orderAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator)
        {
            _ordersService = ordersService;
            _orderAddRequestValidator = orderAddRequestValidator;
            _orderUpdateRequestValidator = orderUpdateRequestValidator;
            _orderItemAddRequestValidator = orderItemAddRequestValidator;
            _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        }

        //GET: /api/Orders
        [HttpGet]
        public async Task<IEnumerable<OrderResponse?>> Get()
        {
            List<OrderResponse?> orders = await _ordersService.GetOrders();
            return orders;
        }
        //GET: /api/Orders/search/orderid/{orderID}
        [HttpGet("/api/Orders/search/orderid/{orderID}")]
        public async Task<OrderResponse?> GetByOrderId(Guid orderID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderID, orderID);
            OrderResponse? order = await _ordersService.GetOrderByCondition(filter);
            return order;
        }

        //GET: /api/Orders/search/productid/{productID}
        [HttpGet("/api/Orders/search/productid/{productID}")]
        public async Task<List<OrderResponse?>> GetOrdersByProductId(Guid productID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(x => x.OrderItems, Builders<OrderItem>.Filter.Eq(tempProduct => tempProduct.ProductID, productID));
            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return orders;
        }

        //GET: /api/Orders/search/orderDate/{orderDate}
        [HttpGet("/api/Orders/search/orderDate/{orderDate}")]
        public async Task<List<OrderResponse?>> GetOrdersByOrderDate(DateTime orderDate)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyy-MM-dd"));
            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return orders;
        }
        //POST: /api/Orders
        [HttpPost("/api/Orders")]
        public async Task<IActionResult?> AddOrder(OrderAddRequest order)
        {
            if (order == null) return BadRequest("Invalid order data"); 

            OrderResponse? orderResponse= await _ordersService.AddOrder(order);
            if (orderResponse == null)
            {
                return Problem("Error in adding product");
            }
            return Created($"api/Orders/search/orderid/{orderResponse?.OrderID}",orderResponse);

        }
        //PuT: /api/Orders
        [HttpPut("/api/Orders/{orderID}")]
        public async Task<IActionResult?> UpdateOrder(Guid orderID, OrderUpdateRequest order)
        {
            if (order == null) return BadRequest("Invalid order data");

            if (orderID != order.OrderID)
            {
                return BadRequest("OrderId in the URL doesn't match with the OrderId in the Request body");
            }

            OrderResponse? orderResponse = await _ordersService.UpdateOrder(order);
            if (orderResponse == null)
            {
                return Problem("Error in adding product");
            }
            return Ok(orderResponse);

        }

        //Delete: /api/Orders
        [HttpDelete("/api/Orders/{orderID}")]
        public async Task<IActionResult?> DeleteOrder(Guid orderID)
        {
            if (orderID == Guid.Empty) return BadRequest("Invalid order ID");
             

            bool isDeleted = await _ordersService.DeleteOrder(orderID);
            if (!isDeleted)
            {
                return Problem("Error in deleting product");
            }
            return Ok(isDeleted);

        }

        //GET: /api/Orders/search/userid/{userID}
        [HttpGet("/api/Orders/search/userid/{userID}")]
        public async Task<List<OrderResponse?>> GetOrdersByUserID(Guid userID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.UserID, userID);
            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return orders;
        }
    }
}
