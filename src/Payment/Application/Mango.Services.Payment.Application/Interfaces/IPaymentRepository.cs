namespace Mango.Services.Payment.Application.Interfaces;

using Mango.Services.Payment.Domain;

/// <summary>
/// Repository interface for Payment entity operations.
/// Provides data access abstraction for payment management.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Get a payment by ID.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Payment if found, null otherwise</returns>
    Task<Payment?> GetByIdAsync(int paymentId);

    /// <summary>
    /// Get all payments for an order.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>List of payments for the order</returns>
    Task<List<Payment>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Get all payments for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of payments for the user</returns>
    Task<List<Payment>> GetByUserIdAsync(string userId);

    /// <summary>
    /// Get a payment by transaction ID.
    /// </summary>
    /// <param name="transactionId">Transaction ID from gateway</param>
    /// <returns>Payment if found, null otherwise</returns>
    Task<Payment?> GetByTransactionIdAsync(string transactionId);

    /// <summary>
    /// Create a new payment record.
    /// </summary>
    /// <param name="payment">Payment entity to create</param>
    /// <returns>Created payment with ID assigned</returns>
    Task<Payment> CreatePaymentAsync(Payment payment);

    /// <summary>
    /// Update an existing payment record.
    /// </summary>
    /// <param name="payment">Payment entity with updates</param>
    /// <returns>Updated payment</returns>
    Task<Payment> UpdatePaymentAsync(Payment payment);

    /// <summary>
    /// Update payment status and transaction ID.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="status">New status</param>
    /// <param name="transactionId">Transaction ID from gateway</param>
    /// <returns>Updated payment</returns>
    Task<Payment?> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string? transactionId = null);

    /// <summary>
    /// Record a refund for a payment.
    /// </summary>
    /// <param name="refund">Refund entity to record</param>
    /// <returns>Created refund record</returns>
    Task<PaymentRefund> RecordRefundAsync(PaymentRefund refund);

    /// <summary>
    /// Get refunds for a payment.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>List of refunds</returns>
    Task<List<PaymentRefund>> GetRefundsAsync(int paymentId);

    /// <summary>
    /// Get payment audit logs.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>List of payment logs</returns>
    Task<List<PaymentLog>> GetPaymentLogsAsync(int paymentId);

    /// <summary>
    /// Add an audit log entry for a payment.
    /// </summary>
    /// <param name="log">Log entry to add</param>
    /// <returns>Created log entry</returns>
    Task<PaymentLog> AddPaymentLogAsync(PaymentLog log);

    /// <summary>
    /// Get payments with a specific status.
    /// </summary>
    /// <param name="status">Payment status to filter by</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>List of payments with specified status</returns>
    Task<List<Payment>> GetByStatusAsync(PaymentStatus status, int limit = 100);

    /// <summary>
    /// Delete a payment record (soft delete).
    /// </summary>
    /// <param name="paymentId">Payment ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeletePaymentAsync(int paymentId);

    /// <summary>
    /// Check if a payment exists.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>True if payment exists</returns>
    Task<bool> PaymentExistsAsync(int paymentId);

    /// <summary>
    /// Save all changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync();
}
