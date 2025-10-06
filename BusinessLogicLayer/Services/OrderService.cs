using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;

public class OrderService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly UsersMicroserviceClient _usersMicroserviceClient;
    private readonly ProductsMicroserviceClient _productsMicroserviceClient;
    public OrderService(IOrdersRepository ordersRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator, IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator, UsersMicroserviceClient usersMicroserviceClient, ProductsMicroserviceClient productsMicroserviceClient)
    {
        _ordersRepository = ordersRepository;
        _mapper = mapper;
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
    }
    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        //Check for null parameter
        if (orderAddRequest == null)
        {
            throw new ArgumentNullException(nameof(orderAddRequest));
        }
        //Validate OrderAddRequest using Fluent Validations

        ValidationResult orderAddRequestValidation = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!orderAddRequestValidation.IsValid)
        {
            string errors = string.Join(",", orderAddRequestValidation.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errors);
        }
        List<ProductDTO?> products = new List<ProductDTO?>();
        //Validate order items using Fluent Validation
        foreach (OrderItemAddRequest _orderItemAddRequest in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(_orderItemAddRequest);
            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }

            //TO DO: Add logic for checking if UserId exists in products microservice
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductId(_orderItemAddRequest.ProductID);
            if (product == null)
            {
                throw new ArgumentException("Invalid Product ID");
            }
            products.Add(product);
        }

        // TO DO: Add logic for checking if UserID exists in Users microservice

        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);
        if (user == null)
        {
            throw new ArgumentException("Invalid User ID");
        }

        //Convert data from orderaddrequest to order and post
        Order orderInput = _mapper.Map<Order>(orderAddRequest);

        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

        //Invoke repository
        Order? addedOrder = await _ordersRepository.AddOrder(orderInput);

        if (addedOrder == null)
        {
            return null;
        }
        var addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder);
        if (addedOrderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in addedOrderResponse.OrderItems)
            {
                ProductDTO? productDTO = products.Where(t => t.ProductID == orderItemResponse.ProductID).FirstOrDefault();
                if (productDTO != null)
                {
                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }
        }
        return addedOrderResponse;
    }

    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        Order? existingOrder = await _ordersRepository.GetOrderByCondition(filter);
        if (existingOrder == null)
        {
            return false;
        }
        bool isDeleted = await _ordersRepository.DeleteOrder(orderID);
        return isDeleted;
    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order? order = await _ordersRepository.GetOrderByCondition(filter);
        if (order == null) { return null; }

        OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);
        //To Do: Load ProductName and Category in OrderItem 
        await LoadProductAndUserDetail(orderResponse);

        return orderResponse;
    }

    private async Task LoadProductAndUserDetail(OrderResponse orderResponse)
    {
        //To Do: Load ProductName and Category in OrderItem 
        if (orderResponse != null)
        {

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductId(orderItemResponse.ProductID);
                if (productDTO != null)
                {
                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }
            //To Do: Load Person name and Email Address
            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }
    }

    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrders();

        List<OrderResponse?> orderResponses = _mapper.Map<List<OrderResponse?>>(orders);
        //To Do: Load ProductName and Category in each OrderItem
        await LoadProductsAndUserDetail(orderResponses);

        return orderResponses;
    }

    private async Task LoadProductsAndUserDetail(List<OrderResponse?> orderResponses)
    {
        //To Do: Load ProductName and Category in each OrderItem
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductId(orderItemResponse.ProductID);
                if (productDTO != null)
                {
                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }
            //To Do: Load Person name and Email Address
            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }

        }
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByCondition(filter);

        List<OrderResponse?> orderResponses = _mapper.Map<List<OrderResponse?>>(orders);

        //To Do: Load ProductName and Category in each OrderItem
        await LoadProductsAndUserDetail(orderResponses);
        return orderResponses;
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        }

        ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errors);
        }
        List<ProductDTO?> products = new List<ProductDTO?>();
        foreach (OrderItemUpdateRequest orderUdateRequest in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderUdateRequest);
            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(t => t.ErrorMessage));
                throw new ArgumentException(errors);
            }

            //TO DO: Add logic for checking if UserId exists in products microservice
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductId(orderUdateRequest.ProductID);
            if (product == null)
            {
                throw new ArgumentException("Invalid Product ID");
            }
            products.Add(product);
        }
        //To Do: Add logic to check if UserID is valid or not via UsersMicroservice  
        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);
        if (user == null)
        {
            throw new ArgumentException("Invalid User ID");
        }

        // Map orderupdate request with order
        Order orderToUpdate = _mapper.Map<Order>(orderUpdateRequest);
        foreach (OrderItem orderItem in orderToUpdate.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }
        orderToUpdate.TotalBill = orderToUpdate.OrderItems.Sum(x => x.TotalPrice);

        Order? updatedOrder = await _ordersRepository.UpdateOrder(orderToUpdate);
        if (updatedOrder == null)
        {
            return null;
        }

        var updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder);
        if (updatedOrderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in updatedOrderResponse.OrderItems)
            {
                ProductDTO? productDTO = products.Where(t => t.ProductID == orderItemResponse.ProductID).FirstOrDefault();
                if (productDTO != null)
                {
                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }
        }

        return _mapper.Map<OrderResponse>(updatedOrderResponse);
    }
}
