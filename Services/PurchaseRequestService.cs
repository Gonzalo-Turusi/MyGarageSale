using Microsoft.EntityFrameworkCore;
using MyGarageSale.Data;
using MyGarageSale.Models;

namespace MyGarageSale.Services;

public interface IPurchaseRequestService
{
    Task<List<PurchaseRequest>> GetAllPurchaseRequestsAsync();
    Task<List<PurchaseRequest>> GetUnprocessedPurchaseRequestsAsync();
    Task<PurchaseRequest?> GetPurchaseRequestByIdAsync(int id);
    Task<PurchaseRequest> CreatePurchaseRequestAsync(PurchaseRequest purchaseRequest, List<PurchaseRequestItem> purchaseRequestItems);
    Task<bool> MarkAsProcessedAsync(int id);
    Task<bool> DeletePurchaseRequestAsync(int id);
}

public class PurchaseRequestService : IPurchaseRequestService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PurchaseRequestService> _logger;
    private readonly IEmailService _emailService;

    public PurchaseRequestService(AppDbContext context, ILogger<PurchaseRequestService> logger, IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<List<PurchaseRequest>> GetAllPurchaseRequestsAsync()
    {
        try
        {
            return await _context.PurchaseRequests
                .Include(pr => pr.Items)
                    .ThenInclude(pri => pri.Item)
                        .ThenInclude(i => i.Category)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all purchase requests");
            return new List<PurchaseRequest>();
        }
    }

    public async Task<List<PurchaseRequest>> GetUnprocessedPurchaseRequestsAsync()
    {
        try
        {
            return await _context.PurchaseRequests
                .Include(pr => pr.Items)
                    .ThenInclude(pri => pri.Item)
                        .ThenInclude(i => i.Category)
                .Where(pr => !pr.IsProcessed)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unprocessed purchase requests");
            return new List<PurchaseRequest>();
        }
    }

    public async Task<PurchaseRequest?> GetPurchaseRequestByIdAsync(int id)
    {
        try
        {
            return await _context.PurchaseRequests
                .Include(pr => pr.Items)
                    .ThenInclude(pri => pri.Item)
                        .ThenInclude(i => i.Category)
                .FirstOrDefaultAsync(pr => pr.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase request {RequestId}", id);
            return null;
        }
    }

    public async Task<PurchaseRequest> CreatePurchaseRequestAsync(PurchaseRequest purchaseRequest, List<PurchaseRequestItem> purchaseRequestItems)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Create the purchase request
            purchaseRequest.CreatedAt = DateTime.UtcNow;
            _context.PurchaseRequests.Add(purchaseRequest);
            await _context.SaveChangesAsync();

            // Add the items to the purchase request
            foreach (var item in purchaseRequestItems)
            {
                item.PurchaseRequestId = purchaseRequest.Id;
                _context.PurchaseRequestItems.Add(item);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Load the complete purchase request with all related data
            var completePurchaseRequest = await GetPurchaseRequestByIdAsync(purchaseRequest.Id);
            
            // Enviar notificación por email
            try
            {
                await _emailService.SendPurchaseRequestNotificationAsync(completePurchaseRequest!, completePurchaseRequest!.Items.ToList());
                _logger.LogInformation("Email notification sent for purchase request {RequestId}", purchaseRequest.Id);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send email notification for purchase request {RequestId}, but order was created successfully", purchaseRequest.Id);
            }
            
            _logger.LogInformation("Purchase request created successfully with ID {RequestId}", purchaseRequest.Id);
            return completePurchaseRequest ?? purchaseRequest;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating purchase request");
            throw;
        }
    }

    public async Task<bool> MarkAsProcessedAsync(int id)
    {
        try
        {
            var purchaseRequest = await _context.PurchaseRequests.FindAsync(id);
            if (purchaseRequest == null)
            {
                return false;
            }

            purchaseRequest.IsProcessed = true;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Purchase request marked as processed with ID {RequestId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking purchase request as processed {RequestId}", id);
            return false;
        }
    }

    public async Task<bool> DeletePurchaseRequestAsync(int id)
    {
        try
        {
            var purchaseRequest = await _context.PurchaseRequests
                .Include(pr => pr.Items)
                .FirstOrDefaultAsync(pr => pr.Id == id);
                
            if (purchaseRequest == null)
            {
                return false;
            }

            _context.PurchaseRequests.Remove(purchaseRequest);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Purchase request deleted successfully with ID {RequestId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting purchase request {RequestId}", id);
            return false;
        }
    }
} 