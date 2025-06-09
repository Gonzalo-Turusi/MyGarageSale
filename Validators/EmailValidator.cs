using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MyGarageSale.Validators;

public class StrictEmailAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true; // Let [Required] handle null validation

        var stringValue = value as string;
        if (string.IsNullOrWhiteSpace(stringValue))
            return true; // Let [Required] handle empty validation

        var email = stringValue.Trim();
        
        // Patrón más estricto para emails válidos
        // Permite: usuario@dominio.com, usuario.nombre@dominio.com.ar, etc.
        // No permite: espacios, caracteres especiales inválidos, etc.
        var pattern = @"^[a-zA-Z0-9]([a-zA-Z0-9._-])*[a-zA-Z0-9]@[a-zA-Z0-9]([a-zA-Z0-9.-])*[a-zA-Z0-9]\.[a-zA-Z]{2,}$";
        
        return Regex.IsMatch(email, pattern);
    }

    public override string FormatErrorMessage(string name)
    {
        return "Ingresa un email válido (ejemplo: usuario@dominio.com)";
    }
} 