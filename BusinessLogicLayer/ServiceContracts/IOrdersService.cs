
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;

public interface IOrdersService
{
    /// <summary>
    /// Retrieves the list of orders from the orders repository
    /// </summary>
    /// <returns>Returns list of OrderResponse objects</returns>
    Task<List<OrderResponse?>> GetOrders();
    /// <summary>
    /// Retrieves list of order from the orders with given condition
    /// </summary>
    /// <param name="filter"></param>
    /// <returns> Returns the matching orders as orderresponse objects</returns>
    Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter);
    /// <summary>
    /// Returns a single order that matches with given condition
    /// </summary>
    /// <param name="filter"></param>
    /// <returns> Return the matching order as orderresponse object or null if not found</returns>
    Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter);
    /// <summary>
    /// Add (inserts) order into the collection using orders repository
    /// </summary>
    /// <param name="orderAddRequest">Ordet to insert</param>
    /// <returns>Returns OrderResponse object that contain order details after inserting; or returns null if insertion is unsuccessfull.</returns>
    Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest);
    /// <summary>
    /// Update existing order based on order id
    /// </summary>
    /// <param name="orderUpdateRequest">Order data to update</param>
    /// <returns>Returns order object after successfull updation; otherwise null</returns>
    Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest);
    /// <summary>
    /// Deletes an existing order based on given order id
    /// </summary>
    /// <param name="orderId">OrderId to search and delete</param>
    /// <returns>Returns true if the deletion is successfull; otherwise false</returns>
    Task<bool> DeleteOrder(Guid orderId);
}
