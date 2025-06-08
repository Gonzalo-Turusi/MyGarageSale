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
    Task SendPurchaseConfirmationToCustomerAsync(PurchaseRequest request, List<PurchaseRequestItem> items);
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
            var adminEmail = _configuration["AdminEmail"] ?? "admin@mygaragesale.com";
            var subject = $"🛒 Nueva solicitud de compra - Pedido #{request.Id}";
            var body = BuildPurchaseRequestEmailBody(request, items);
            
            _logger.LogInformation("📧 Enviando notificación de compra al administrador: {AdminEmail}", adminEmail);
            
            var success = await SendEmailAsync(adminEmail, subject, body);
            
            if (success)
            {
                _logger.LogInformation("✅ Email de notificación enviado correctamente al administrador");
            }
            else
            {
                _logger.LogWarning("⚠️ No se pudo enviar el email de notificación al administrador");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error enviando notificación de compra al administrador");
        }
    }

    public async Task SendPurchaseConfirmationToCustomerAsync(PurchaseRequest request, List<PurchaseRequestItem> items)
    {
        try
        {
            var subject = $"✅ Confirmación de pedido #{request.Id} - MyGarageSale";
            var body = BuildCustomerConfirmationEmailBody(request, items);
            
            _logger.LogInformation("📧 Enviando confirmación de compra al cliente: {CustomerEmail}", request.CustomerEmail);
            
            var success = await SendEmailAsync(request.CustomerEmail, subject, body);
            
            if (success)
            {
                _logger.LogInformation("✅ Email de confirmación enviado correctamente al cliente");
            }
            else
            {
                _logger.LogWarning("⚠️ No se pudo enviar el email de confirmación al cliente");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error enviando confirmación de compra al cliente");
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            // Usar SMTP (Gmail) para envío de emails
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

    private string BuildPurchaseRequestEmailBody(PurchaseRequest purchaseRequest, List<PurchaseRequestItem>? items = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;'>");
        
        // Header
        sb.AppendLine("<div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 12px 12px 0 0; color: white;'>");
        sb.AppendLine("<h1 style='margin: 0; font-size: 28px;'>🛒 Nueva Solicitud de Compra</h1>");
        sb.AppendLine("<p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>MyGarageSale - Sistema de Ventas</p>");
        sb.AppendLine("</div>");
        
        // Información del pedido
        sb.AppendLine("<div style='background-color: #f8f9fa; padding: 25px; border-left: 4px solid #28a745;'>");
        sb.AppendLine("<h2 style='color: #28a745; margin-top: 0; font-size: 20px;'>📋 Detalles del Pedido</h2>");
        sb.AppendLine($"<div style='background: white; padding: 15px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>");
        sb.AppendLine($"<p style='margin: 8px 0;'><strong>📦 Número de Pedido:</strong> <span style='color: #007bff;'>#{purchaseRequest.Id}</span></p>");
        sb.AppendLine($"<p style='margin: 8px 0;'><strong>📅 Fecha:</strong> {purchaseRequest.CreatedAt:dd/MM/yyyy HH:mm}</p>");
        sb.AppendLine($"<p style='margin: 8px 0;'><strong>👤 Cliente:</strong> {purchaseRequest.CustomerName}</p>");
        sb.AppendLine($"<p style='margin: 8px 0;'><strong>📧 Email:</strong> <a href='mailto:{purchaseRequest.CustomerEmail}' style='color: #007bff;'>{purchaseRequest.CustomerEmail}</a></p>");
        sb.AppendLine($"<p style='margin: 8px 0;'><strong>📱 Teléfono:</strong> <a href='tel:{purchaseRequest.CustomerPhone}' style='color: #007bff;'>{purchaseRequest.CustomerPhone}</a></p>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        
        // Comentarios si existen
        if (!string.IsNullOrEmpty(purchaseRequest.Comments))
        {
            sb.AppendLine("<div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 20px; margin: 20px 0; border-radius: 8px;'>");
            sb.AppendLine("<h3 style='color: #856404; margin-top: 0; font-size: 18px;'>💬 Comentarios del Cliente</h3>");
            sb.AppendLine($"<div style='background: white; padding: 15px; border-radius: 6px; font-style: italic;'>");
            sb.AppendLine($"<p style='margin: 0;'>\"{purchaseRequest.Comments.Replace("\n", "<br>")}\"</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
        }
        
        // Artículos del pedido
        sb.AppendLine("<div style='margin: 25px 0;'>");
        sb.AppendLine("<h2 style='color: #495057; margin-bottom: 15px; font-size: 20px;'>🛍️ Artículos Solicitados</h2>");
        sb.AppendLine("<div style='background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>");
        sb.AppendLine("<table style='width: 100%; border-collapse: collapse;'>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white;'>");
        sb.AppendLine("<th style='padding: 15px; text-align: left; font-weight: 600;'>Artículo</th>");
        sb.AppendLine("<th style='padding: 15px; text-align: center; font-weight: 600;'>Cant.</th>");
        sb.AppendLine("<th style='padding: 15px; text-align: right; font-weight: 600;'>Precio Unit.</th>");
        sb.AppendLine("<th style='padding: 15px; text-align: right; font-weight: 600;'>Subtotal</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");
        
        decimal total = 0;
        bool isEven = false;
        var itemsToProcess = items ?? purchaseRequest.Items;
        foreach (var purchaseRequestItem in itemsToProcess)
        {
            var subtotal = purchaseRequestItem.Item.Price * purchaseRequestItem.Quantity;
            total += subtotal;
            var bgColor = isEven ? "#f8f9fa" : "white";
            
            sb.AppendLine($"<tr style='background-color: {bgColor};'>");
            sb.AppendLine($"<td style='padding: 12px 15px; border-bottom: 1px solid #dee2e6;'>");
            sb.AppendLine($"  <strong>{purchaseRequestItem.Item.Title}</strong>");
            if (!string.IsNullOrEmpty(purchaseRequestItem.Item.Category?.Name))
            {
                sb.AppendLine($"  <br><small style='color: #6c757d;'>📂 {purchaseRequestItem.Item.Category.Name}</small>");
            }
            sb.AppendLine($"</td>");
            sb.AppendLine($"<td style='padding: 12px 15px; text-align: center; border-bottom: 1px solid #dee2e6; font-weight: 600;'>{purchaseRequestItem.Quantity}</td>");
            sb.AppendLine($"<td style='padding: 12px 15px; text-align: right; border-bottom: 1px solid #dee2e6;'>${purchaseRequestItem.Item.Price:F2}</td>");
            sb.AppendLine($"<td style='padding: 12px 15px; text-align: right; border-bottom: 1px solid #dee2e6; font-weight: 600; color: #28a745;'>${subtotal:F2}</td>");
            sb.AppendLine($"</tr>");
            isEven = !isEven;
        }
        
        // Total
        sb.AppendLine("<tr style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white;'>");
        sb.AppendLine("<td colspan='3' style='padding: 15px; text-align: right; font-weight: 700; font-size: 16px;'>TOTAL DEL PEDIDO:</td>");
        sb.AppendLine($"<td style='padding: 15px; text-align: right; font-weight: 700; font-size: 20px;'>${total:F2}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        
        // Acciones recomendadas
        sb.AppendLine("<div style='background: linear-gradient(135deg, #17a2b8 0%, #138496 100%); color: white; padding: 20px; border-radius: 8px; margin: 25px 0;'>");
        sb.AppendLine("<h3 style='margin-top: 0; font-size: 18px;'>⚡ Acciones Recomendadas</h3>");
        sb.AppendLine("<ul style='margin: 10px 0; padding-left: 20px;'>");
        sb.AppendLine($"<li style='margin: 8px 0;'>📞 <strong>Contactar al cliente:</strong> <a href='mailto:{purchaseRequest.CustomerEmail}' style='color: #ffc107; text-decoration: underline;'>{purchaseRequest.CustomerEmail}</a> | <a href='tel:{purchaseRequest.CustomerPhone}' style='color: #ffc107; text-decoration: underline;'>{purchaseRequest.CustomerPhone}</a></li>");
        sb.AppendLine("<li style='margin: 8px 0;'>✅ <strong>Verificar disponibilidad</strong> de todos los artículos</li>");
        sb.AppendLine("<li style='margin: 8px 0;'>🤝 <strong>Coordinar método de pago</strong> y entrega</li>");
        sb.AppendLine("<li style='margin: 8px 0;'>🏷️ <strong>Marcar como procesado</strong> en el sistema</li>");
        sb.AppendLine("</ul>");
        sb.AppendLine("</div>");
        
        // Footer
        sb.AppendLine("<div style='text-align: center; padding: 20px; border-top: 2px solid #dee2e6; margin-top: 30px; background-color: #f8f9fa;'>");
        sb.AppendLine("<p style='color: #6c757d; margin: 0; font-size: 14px;'>🤖 Email generado automáticamente por <strong>MyGarageSale</strong></p>");
        sb.AppendLine($"<p style='color: #6c757d; margin: 5px 0 0 0; font-size: 12px;'>Pedido recibido el {purchaseRequest.CreatedAt:dd/MM/yyyy} a las {purchaseRequest.CreatedAt:HH:mm}</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("</body></html>");
        
        return sb.ToString();
    }

    private string BuildCustomerConfirmationEmailBody(PurchaseRequest request, List<PurchaseRequestItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        
        // Header
        sb.AppendLine("<div style='background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 8px; margin-bottom: 20px;'>");
        sb.AppendLine("<h1 style='color: #28a745; margin: 0;'>✅ ¡Pedido Confirmado!</h1>");
        sb.AppendLine("<p style='margin: 10px 0 0 0; font-size: 18px;'>Gracias por tu compra en MyGarageSale</p>");
        sb.AppendLine("</div>");
        
        // Información del pedido
        sb.AppendLine("<div style='background-color: #e9ecef; padding: 15px; border-radius: 8px; margin-bottom: 20px;'>");
        sb.AppendLine($"<h2 style='color: #495057; margin-top: 0;'>📋 Información del Pedido</h2>");
        sb.AppendLine($"<p><strong>Número de Pedido:</strong> #{request.Id}</p>");
        sb.AppendLine($"<p><strong>Fecha:</strong> {request.CreatedAt:dd/MM/yyyy HH:mm}</p>");
        sb.AppendLine($"<p><strong>Cliente:</strong> {request.CustomerName}</p>");
        sb.AppendLine("</div>");
        
        // Artículos del pedido
        sb.AppendLine("<h2 style='color: #495057;'>🛍️ Artículos del Pedido</h2>");
        sb.AppendLine("<table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr style='background-color: #f8f9fa;'>");
        sb.AppendLine("<th style='border: 1px solid #dee2e6; padding: 12px; text-align: left;'>Artículo</th>");
        sb.AppendLine("<th style='border: 1px solid #dee2e6; padding: 12px; text-align: center;'>Cantidad</th>");
        sb.AppendLine("<th style='border: 1px solid #dee2e6; padding: 12px; text-align: right;'>Precio Unit.</th>");
        sb.AppendLine("<th style='border: 1px solid #dee2e6; padding: 12px; text-align: right;'>Subtotal</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");
        
        decimal total = 0;
        foreach (var item in items)
        {
            var subtotal = item.Item.Price * item.Quantity;
            total += subtotal;
            
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style='border: 1px solid #dee2e6; padding: 12px;'>{item.Item.Title}</td>");
            sb.AppendLine($"<td style='border: 1px solid #dee2e6; padding: 12px; text-align: center;'>{item.Quantity}</td>");
            sb.AppendLine($"<td style='border: 1px solid #dee2e6; padding: 12px; text-align: right;'>${item.Item.Price:F2}</td>");
            sb.AppendLine($"<td style='border: 1px solid #dee2e6; padding: 12px; text-align: right;'>${subtotal:F2}</td>");
            sb.AppendLine("</tr>");
        }
        
        // Total
        sb.AppendLine("<tr style='background-color: #f8f9fa; font-weight: bold;'>");
        sb.AppendLine("<td colspan='3' style='border: 1px solid #dee2e6; padding: 12px; text-align: right;'>TOTAL:</td>");
        sb.AppendLine($"<td style='border: 1px solid #dee2e6; padding: 12px; text-align: right; color: #28a745; font-size: 18px;'>${total:F2}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
        
        // Comentarios si existen
        if (!string.IsNullOrEmpty(request.Comments))
        {
            sb.AppendLine("<div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 8px; margin-bottom: 20px;'>");
            sb.AppendLine("<h3 style='color: #856404; margin-top: 0;'>💬 Tus Comentarios</h3>");
            sb.AppendLine($"<p style='margin-bottom: 0;'>{request.Comments.Replace("\n", "<br>")}</p>");
            sb.AppendLine("</div>");
        }
        
        // Próximos pasos
        sb.AppendLine("<div style='background-color: #d1ecf1; border: 1px solid #b8daff; padding: 15px; border-radius: 8px; margin-bottom: 20px;'>");
        sb.AppendLine("<h3 style='color: #0c5460; margin-top: 0;'>📞 Próximos Pasos</h3>");
        sb.AppendLine("<p>🔄 <strong>Procesando tu pedido:</strong> Estamos revisando tu solicitud</p>");
        sb.AppendLine("<p>📧 <strong>Te contactaremos:</strong> En las próximas horas para coordinar</p>");
        sb.AppendLine("<p>💳 <strong>Pago y entrega:</strong> Acordaremos el método de pago y entrega</p>");
        sb.AppendLine("</div>");
        
        // Footer
        sb.AppendLine("<div style='text-align: center; padding: 20px; border-top: 1px solid #dee2e6; margin-top: 30px;'>");
        sb.AppendLine("<p style='color: #6c757d; margin: 0;'>Gracias por elegirnos 🙏</p>");
        sb.AppendLine("<p style='color: #6c757d; margin: 5px 0 0 0; font-size: 14px;'>");
        sb.AppendLine("Este email fue generado automáticamente por <strong>MyGarageSale</strong>");
        sb.AppendLine("</p>");
        sb.AppendLine("</div>");
        
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