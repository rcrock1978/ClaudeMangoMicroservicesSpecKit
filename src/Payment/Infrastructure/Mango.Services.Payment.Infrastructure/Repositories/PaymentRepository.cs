namespace Mango.Services.Payment.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Mango.Services.Payment.Application.Interfaces;
using Mango.Services.Payment.Domain;
using Mango.Services.Payment.Infrastructure.Data;

/// <summary>
/// Repository implementation for Payment entity operations.
/// Provides data access for payment management with EF Core.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    /// <summary>
    /// Initializes a new instance of PaymentRepository.
    /// </summary>
    /// <param name="context">Payment database context</param>
    public PaymentRepository(PaymentDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<Payment?> GetByIdAsync(int paymentId)
    {
        if (paymentId <= 0)
            return null;

        return await _context.Payments
            .Include(p => p.Refunds)
            .Include(p => p.Logs)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<List<Payment>> GetByOrderIdAsync(int orderId)
    {
        if (orderId <= 0)
            return new List<Payment>();

        return await _context.Payments
            .Where(p => p.OrderId == orderId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<Payment>> GetByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new List<Payment>();

        return await _context.Payments
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return null;

        return await _context.Payments
            .Include(p => p.Refunds)
            .Include(p => p.Logs)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId && !p.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    /// <inheritdoc/>
    public async Task<Payment> UpdatePaymentAsync(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    /// <inheritdoc/>
    public async Task<Payment?> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string? transactionId = null)
    {
        var payment = await GetByIdAsync(paymentId);
        if (payment == null)
            return null;

        payment.Status = status;
        if (!string.IsNullOrWhiteSpace(transactionId))
            payment.TransactionId = transactionId;

        payment.UpdatedAt = DateTime.UtcNow;
        await UpdatePaymentAsync(payment);
        return payment;
    }

    /// <inheritdoc/>
    public async Task<PaymentRefund> RecordRefundAsync(PaymentRefund refund)
    {
        if (refund == null)
            throw new ArgumentNullException(nameof(refund));

        _context.PaymentRefunds.Add(refund);
        await _context.SaveChangesAsync();
        return refund;
    }

    /// <inheritdoc/>
    public async Task<List<PaymentRefund>> GetRefundsAsync(int paymentId)
    {
        if (paymentId <= 0)
            return new List<PaymentRefund>();

        return await _context.PaymentRefunds
            .Where(r => r.PaymentId == paymentId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<PaymentLog>> GetPaymentLogsAsync(int paymentId)
    {
        if (paymentId <= 0)
            return new List<PaymentLog>();

        return await _context.PaymentLogs
            .Where(l => l.PaymentId == paymentId && !l.IsDeleted)
            .OrderByDescending(l => l.Timestamp)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<PaymentLog> AddPaymentLogAsync(PaymentLog log)
    {
        if (log == null)
            throw new ArgumentNullException(nameof(log));

        _context.PaymentLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    /// <inheritdoc/>
    public async Task<List<Payment>> GetByStatusAsync(PaymentStatus status, int limit = 100)
    {
        return await _context.Payments
            .Where(p => p.Status == status && !p.IsDeleted)
            .OrderBy(p => p.CreatedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePaymentAsync(int paymentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null)
            return false;

        payment.IsDeleted = true;
        payment.DeletedAt = DateTime.UtcNow;
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> PaymentExistsAsync(int paymentId)
    {
        return await _context.Payments
            .AsNoTracking()
            .AnyAsync(p => p.Id == paymentId && !p.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
