# 🔧 Configuración de Venta Yenny y Gonzalo

## ⚠️ Configuración de Seguridad

**IMPORTANTE:** Nunca subas credenciales reales al repositorio. Este archivo te guiará para configurar la aplicación de manera segura.

## 📁 Archivos de Configuración

### 1. **appsettings.Development.json** (Local - NO se sube al repo)

Crea este archivo localmente con tus credenciales reales:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=garage_sale.db"
  },
  "AdminCredentials": {
    "Username": "tu-usuario-admin",
    "Password": "tu-password-seguro"
  },
  "AdminEmail": "tu-email@gmail.com",
  "IsDevelopment": true,
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "tu-email@gmail.com",
    "Password": "tu-app-password-gmail",
    "FromEmail": "tu-email@gmail.com",
    "EnableSsl": true
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "tu-email@gmail.com",
    "Password": "tu-app-password-gmail"
  }
}
```

### 2. **Variables de Entorno (Recomendado para Producción)**

```bash
# Admin Configuration
ADMIN_USERNAME=tu-usuario
ADMIN_PASSWORD=tu-password-seguro
ADMIN_EMAIL=tu-email@gmail.com

# Email Configuration
SMTP_USERNAME=tu-email@gmail.com
SMTP_PASSWORD=tu-app-password
```

## 🚀 Despliegue en Azure

### Azure App Service Configuration

En Azure, configura estas variables como **Application Settings**:

1. Ve a tu App Service > Configuration > Application settings
2. Agrega estas variables:

```
AdminCredentials__Username = tu-usuario
AdminCredentials__Password = tu-password-seguro
AdminEmail = tu-email@gmail.com
Smtp__Username = tu-email@gmail.com
Smtp__Password = tu-app-password
Email__Username = tu-email@gmail.com
Email__Password = tu-app-password
```

### Azure Key Vault (Más Seguro)

Para máxima seguridad, usa Azure Key Vault:

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    vaultUri: "https://tu-keyvault.vault.azure.net/",
    credential: new DefaultAzureCredential()
);
```

## 📧 Configuración de Gmail

1. Habilita verificación en dos pasos
2. Genera una **App Password** específica
3. Usa esa App Password, NO tu password de Gmail

## 🔍 Verificación

- ✅ `appsettings.json` - Puede estar en el repo (solo placeholders)
- ❌ `appsettings.Development.json` - NO debe estar en el repo
- ❌ `appsettings.Production.json` - NO debe estar en el repo
- ✅ Variables de entorno para producción

## 🛡️ Buenas Prácticas

1. **Nunca** hardcodees credenciales
2. Usa **variables de entorno** en producción
3. **Azure Key Vault** para secretos críticos
4. **App Passwords** para Gmail, no passwords reales
5. **Rota credenciales** regularmente 