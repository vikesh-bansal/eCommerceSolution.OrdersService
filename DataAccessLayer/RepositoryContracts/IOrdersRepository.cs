using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
public interface IOrdersRepository
{
    /// <summary>
    /// Retrieves all orders asynchronously
    /// </summary>
    /// <returns>Returns all orders from the orders collection</returns>
    Task<IEnumerable<Order>> GetOrders();
    /// <summary>
    /// Retrieves all Orders based on the specified condition async
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>Returning a collection of matching orders</returns>
    Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// The condition to filter Orders
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>Returning matching order</returns>
    Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// Adds a new Order into the Orders collection asynchronously
    /// </summary>
    /// <param name="order">The orders to be added</param>
    /// <returns>Returnes the added Order object or null if unsuccessfull</returns>
    Task<Order?> AddOrder(Order order);

    /// <summary>
    /// Update an existing Order
    /// </summary>
    /// <param name="order">The order to be added</param>
    /// <returns>Returns the updated order object</returns>
    Task<Order?> UpdateOrder(Order order);

    /// <summary>
    /// Deleted the order asynchronously
    /// </summary>
    /// <param name="order">The Order ID based on which we need to delete the order</param>
    /// <returns>Returns true if the deletion is successfull, false otherwise</returns>
    Task<bool> DeleteOrder(Guid orderID);

}
