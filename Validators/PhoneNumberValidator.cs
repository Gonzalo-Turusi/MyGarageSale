using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MyGarageSale.Validators;

public class ArgentinePhoneAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true; // Let [Required] handle null validation

        var stringValue = value as string;
        if (string.IsNullOrWhiteSpace(stringValue))
            return true; // Let [Required] handle empty validation

        var phone = stringValue.Trim();
        
        // Patrón más flexible que permite:
        // - Código de país +54 (opcional)
        // - Número 9 (opcional) 
        // - Dígitos, espacios, guiones y paréntesis en cualquier orden
        // - Debe contener al menos 8 dígitos (números locales argentinos)
        var pattern = @"^[\+\(\)\s\-\d]+$";
        
        // Verificar que contenga solo caracteres válidos
        if (!Regex.IsMatch(phone, pattern))
            return false;
            
        // Contar los dígitos para asegurar que tenga al menos 8
        var digitCount = Regex.Matches(phone, @"\d").Count;
        return digitCount >= 8 && digitCount <= 15; // Máximo 15 según estándar internacional
    }

    public override string FormatErrorMessage(string name)
    {
        return $"El teléfono debe contener solo números, espacios, guiones y paréntesis, con al menos 8 dígitos. Ej: +54 9 11 1234-5678 o 11 1234 5678";
    }
} 