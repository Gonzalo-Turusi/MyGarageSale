using MyGarageSale.Models;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MyGarageSale.Services;

public interface IEmailService
{
    Task<bool> SendPurchaseRequestEmailAsync(PurchaseRequest purchaseRequest);
    Task<bool> SendTestEmailAsync(string toEmail, string subject, string body);
    Task SendPurchaseRequestNotificationAsync(PurchaseRequest request, List<PurchaseRequestItem> items);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendPurchaseRequestEmailAsync(PurchaseRequest purchaseRequest)
    {
        try
        {
            var subject = $"Nueva Solicitud de Compra - MyGarageSale #{purchaseRequest.Id}";
            var body = BuildPurchaseRequestEmailBody(purchaseRequest);
            
            var adminEmail = _configuration["AdminEmail"] ?? "admin@mygaragesale.com";
            
            return await SendEmailAsync(adminEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending purchase request email for request {RequestId}", purchaseRequest.Id);
            return false;
        }
    }

    public async Task<bool> SendTestEmailAsync(string toEmail, string subject, string body)
    {
        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPurchaseRequestNotificationAsync(PurchaseRequest request, List<PurchaseRequestItem> items)
    {
        try
        {
            var subject = $"Nuevo Pedido - MyGarageSale #{request.Id}";
            var body = BuildEmailBody(request, items);
            
            // Intentar envío real de email
            var emailSent = await SendRealEmailAsync("gonsalot@gmail.com", subject, body);
            
            if (emailSent)
            {
                _logger.LogInformation("✅ EMAIL ENVIADO EXITOSAMENTE");
                _logger.LogInformation("Para: gonsalot@gmail.com");
                _logger.LogInformation("Asunto: {Subject}", subject);
            }
            else
            {
                // Si falla el envío real, al menos mostramos en logs
                _logger.LogWarning("⚠️  EMAIL NO ENVIADO - MOSTRANDO EN LOGS");
                _logger.LogInformation("=== EMAIL SIMULADO ===");
                _logger.LogInformation("Para: gonsalot@gmail.com");
                _logger.LogInformation("Asunto: {Subject}", subject);
                _logger.LogInformation("Cuerpo del email:");
                _logger.LogInformation(body);
                _logger.LogInformation("========================");
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación de pedido por email");
            throw;
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            // First try SendGrid (for Azure)
            var sendGridApiKey = _configuration["SendGrid:ApiKey"];
            if (!string.IsNullOrEmpty(sendGridApiKey))
            {
                return await SendEmailViaSendGridAsync(toEmail, subject, body, sendGridApiKey);
            }

            // Fallback to SMTP (for local development)
            var smtpHost = _configuration["Smtp:Host"];
            if (!string.IsNullOrEmpty(smtpHost))
            {
                return await SendEmailViaSmtpAsync(toEmail, subject, body);
            }

            // Development mode - just log the email
            _logger.LogInformation("EMAIL (Development Mode):\nTo: {ToEmail}\nSubject: {Subject}\nBody:\n{Body}", 
                toEmail, subject, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
            return false;
        }
    }

    private async Task<bool> SendEmailViaSendGridAsync(string toEmail, string subject, string body, string apiKey)
    {
        try
        {
            // TODO: Implement SendGrid when we deploy to Azure
            // For now, using the SMTP fallback
            _logger.LogInformation("SendGrid not implemented yet, falling back to SMTP");
            return await SendEmailViaSmtpAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via SendGrid");
            return false;
        }
    }

    private async Task<bool> SendEmailViaSmtpAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["Smtp:Host"] ?? "localhost";
            var smtpPort = _configuration.GetValue<int>("Smtp:Port", 587);
            var smtpUsername = _configuration["Smtp:Username"];
            var smtpPassword = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@mygaragesale.com";

            using var client = new SmtpClient(smtpHost, smtpPort);
            
            if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
            {
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;
            }

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, "MyGarageSale"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via SMTP to {ToEmail}", toEmail);
            
            // In development, log the email instead of failing
            if (_configuration.GetValue<bool>("IsDevelopment", true))
            {
                _logger.LogInformation("EMAIL (SMTP Failed - Development Mode):\nTo: {ToEmail}\nSubject: {Subject}\nBody:\n{Body}", 
                    toEmail, subject, body);
                return true;
            }
            
            return false;
        }
    }

    private string BuildPurchaseRequestEmailBody(PurchaseRequest purchaseRequest)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine("<h2>Nueva Solicitud de Compra</h2>");
        sb.AppendLine($"<p><strong>Solicitud #:</strong> {purchaseRequest.Id}</p>");
        sb.AppendLine($"<p><strong>Fecha:</strong> {purchaseRequest.CreatedAt:dd/MM/yyyy HH:mm}</p>");
        sb.AppendLine("<hr>");
        
        sb.AppendLine("<h3>Datos del Cliente</h3>");
        sb.AppendLine($"<p><strong>Nombre:</strong> {purchaseRequest.CustomerName}</p>");
        sb.AppendLine($"<p><strong>Email:</strong> {purchaseRequest.CustomerEmail}</p>");
        
        if (!string.IsNullOrEmpty(purchaseRequest.Comments))
        {
            sb.AppendLine($"<p><strong>Comentarios:</strong></p>");
            sb.AppendLine($"<p>{purchaseRequest.Comments.Replace("\n", "<br>")}</p>");
        }
        
        sb.AppendLine("<hr>");
        sb.AppendLine("<h3>Artículos Solicitados</h3>");
        sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0'>");
        sb.AppendLine("<tr><th>Artículo</th><th>Cantidad</th><th>Precio Unit.</th><th>Subtotal</th></tr>");
        
        decimal total = 0;
        foreach (var purchaseRequestItem in purchaseRequest.Items)
        {
            var subtotal = purchaseRequestItem.Item.Price * purchaseRequestItem.Quantity;
            total += subtotal;
            
            sb.AppendLine($"<tr>");
            sb.AppendLine($"<td>{purchaseRequestItem.Item.Title}</td>");
            sb.AppendLine($"<td>{purchaseRequestItem.Quantity}</td>");
            sb.AppendLine($"<td>${purchaseRequestItem.Item.Price:F2}</td>");
            sb.AppendLine($"<td>${subtotal:F2}</td>");
            sb.AppendLine($"</tr>");
        }
        
        sb.AppendLine($"<tr><td colspan='3'><strong>TOTAL</strong></td><td><strong>${total:F2}</strong></td></tr>");
        sb.AppendLine("</table>");
        
        sb.AppendLine("<hr>");
        sb.AppendLine("<p><em>Este email fue generado automáticamente por MyGarageSale</em></p>");
        sb.AppendLine("</body></html>");
        
        return sb.ToString();
    }

    private string BuildEmailBody(PurchaseRequest request, List<PurchaseRequestItem> items)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("🛒 NUEVO PEDIDO EN MYGARAGESALE");
        sb.AppendLine("=================================");
        sb.AppendLine();
        sb.AppendLine($"📧 Cliente: {request.CustomerName}");
        sb.AppendLine($"✉️  Email: {request.CustomerEmail}");
        sb.AppendLine($"📅 Fecha: {request.CreatedAt:dd/MM/yyyy HH:mm}");
        sb.AppendLine();
        
        if (!string.IsNullOrEmpty(request.Comments))
        {
            sb.AppendLine($"💬 Comentarios del cliente:");
            sb.AppendLine($"   {request.Comments}");
            sb.AppendLine();
        }
        
        sb.AppendLine("🛍️  ARTÍCULOS SOLICITADOS:");
        sb.AppendLine("-------------------------");
        
        decimal total = 0;
        foreach (var item in items)
        {
            var subtotal = item.Item.Price * item.Quantity;
            total += subtotal;
            
            sb.AppendLine($"• {item.Item.Title}");
            sb.AppendLine($"  Categoría: {item.Item.Category?.Name}");
            sb.AppendLine($"  Precio: ${item.Item.Price:F2}");
            sb.AppendLine($"  Cantidad: {item.Quantity}");
            sb.AppendLine($"  Subtotal: ${subtotal:F2}");
            sb.AppendLine();
        }
        
        sb.AppendLine($"💰 TOTAL DEL PEDIDO: ${total:F2}");
        sb.AppendLine();
        sb.AppendLine("📝 PRÓXIMOS PASOS:");
        sb.AppendLine("- Contactar al cliente para coordinar pago y entrega");
        sb.AppendLine("- Verificar disponibilidad de los artículos");
        sb.AppendLine("- Marcar como procesado en el sistema");
        
        return sb.ToString();
    }
    
    // Método para envío real de email (cuando tengas configuración SMTP)
    private async Task<bool> SendRealEmailAsync(string to, string subject, string body)
    {
        // Ejemplo de configuración (esto iría en appsettings.json)
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:Username"] ?? "";
        var smtpPass = _configuration["Email:Password"] ?? "";
        
        using var client = new SmtpClient(smtpHost, smtpPort);
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(smtpUser, smtpPass);
        
        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUser, "MyGarageSale"),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        
        mailMessage.To.Add(to);
        
        try
        {
            await client.SendMailAsync(mailMessage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail}", to);
            return false;
        }
    }
} 