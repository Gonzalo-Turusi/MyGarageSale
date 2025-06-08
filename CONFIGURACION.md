# 🔧 Guía de Configuración - MyGarageSale

## 📋 Índice
- [Configuración de Seguridad](#configuración-de-seguridad)
- [Configuración Local](#configuración-local)  
- [Configuración de Azure](#configuración-de-azure)
- [Sistema de Email](#sistema-de-email)
- [Upload de Imágenes](#upload-de-imágenes)
- [Base de Datos](#base-de-datos)
- [Variables de Entorno](#variables-de-entorno)
- [Verificación y Testing](#verificación-y-testing)

---

## ⚠️ Configuración de Seguridad

**CRÍTICO**: Nunca subas credenciales reales al repositorio público. Este archivo te guiará para configurar la aplicación de manera segura.

### 🛡️ Principios de Seguridad
- ✅ Usa **variables de entorno** en producción
- ✅ **Azure Key Vault** para secretos críticos  
- ✅ **App Passwords** para Gmail, no passwords reales
- ✅ **Rota credenciales** cada 3-6 meses
- ❌ **Nunca** hardcodees credenciales en el código

---

## 🏠 Configuración Local

### 1. **appsettings.Development.json** (Local - NO se sube al repo)

**Ubicación**: Raíz del proyecto  
**Estado**: Incluido en .gitignore

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=garage_sale.db"
  },
  "AdminCredentials": {
    "Username": "tu-usuario-admin",
    "Password": "tu-password-seguro-min-8-chars"
  },
  "AdminEmail": "tu-email@gmail.com",
  "IsDevelopment": true,
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "tu-email@gmail.com",
    "Password": "tu-app-password-gmail-16-chars",
    "FromEmail": "tu-email@gmail.com",
    "EnableSsl": true
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "tu-email@gmail.com",
    "Password": "tu-app-password-gmail-16-chars"
  },
  "Imgur": {
    "ClientId": "tu-imgur-client-id-opcional"
  }
}
```

### 2. **Setup Inicial Local**

```bash
# 1. Clonar repositorio
git clone https://github.com/tu-usuario/MyGarageSale.git
cd MyGarageSale

# 2. Crear archivo de configuración local
# Copia el template de arriba y personaliza con tus datos
New-Item -Path "appsettings.Development.json" -ItemType File

# 3. Instalar dependencias
dotnet restore
npm install

# 4. Ejecutar aplicación
dotnet run
```

---

## ☁️ Configuración de Azure

### **Azure App Service - Variables de Aplicación**

Configurar en: **Portal Azure > App Service > Configuration > Application Settings**

| Variable | Valor | Descripción |
|----------|-------|-------------|
| `AdminCredentials__Username` | `tu-usuario` | Usuario admin |
| `AdminCredentials__Password` | `tu-password-seguro` | Password admin |
| `AdminEmail` | `tu-email@gmail.com` | Email del admin |
| `Smtp__Host` | `smtp.gmail.com` | Servidor SMTP |
| `Smtp__Port` | `587` | Puerto SMTP |
| `Smtp__Username` | `tu-email@gmail.com` | Usuario SMTP |
| `Smtp__Password` | `tu-app-password` | Password SMTP |
| `Smtp__FromEmail` | `tu-email@gmail.com` | Email remitente |
| `Smtp__EnableSsl` | `true` | SSL habilitado |
| `Email__SmtpHost` | `smtp.gmail.com` | Host email |
| `Email__SmtpPort` | `587` | Puerto email |
| `Email__Username` | `tu-email@gmail.com` | Usuario email |
| `Email__Password` | `tu-app-password` | Password email |
| `Imgur__ClientId` | `tu-imgur-id` | ID de Imgur |

### **Azure SQL Database**

```json
{
  "ConnectionStrings": {
    "AzureSqlConnection": "Server=tcp:tu-server.database.windows.net,1433;Initial Catalog=MyGarageSaleDB;Persist Security Info=False;User ID=tu-usuario;Password=tu-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### **Azure Key Vault (Recomendado)**

```csharp
// En Program.cs - Ya configurado
builder.Configuration.AddAzureKeyVault(
    vaultUri: "https://tu-keyvault.vault.azure.net/",
    credential: new DefaultAzureCredential()
);
```

**Secretos en Key Vault:**
- `admin-username`
- `admin-password`  
- `smtp-password`
- `database-connection`

---

## 📧 Sistema de Email

### **Gmail Setup (Recomendado)**

1. **Habilitar 2FA** en tu cuenta Gmail
2. **Generar App Password**:
   - Google Account > Security > 2-Step Verification > App Passwords
   - Seleccionar "Mail" y generar password de 16 caracteres
3. **Usar App Password** en configuración, NO tu password normal



### **Sistema de Email Configurado**

El sistema utiliza **Gmail SMTP** únicamente y incluye:
- ✅ **Templates HTML profesionales** para emails
- ✅ **Confirmación automática** al cliente
- ✅ **Notificación inmediata** al admin de nuevas solicitudes
- ✅ **Error handling** robusto con logs detallados
- ✅ **Modo desarrollo** que loguea emails sin enviarlos

---

## 🖼️ Upload de Imágenes

### **Sistema Dual Configurado**

1. **Imgur API** (Principal)
2. **Almacenamiento Local** (Fallback)

### **Imgur Setup (Recomendado)**

1. Crear cuenta en [Imgur](https://imgur.com)
2. Ir a [Imgur API](https://api.imgur.com/oauth2/addclient)
3. Registrar aplicación:
   - **Application name**: MyGarageSale
   - **Authorization type**: Anonymous usage without user authorization
   - **Authorization callback URL**: No requerido
   - **Application website**: Tu dominio
   - **Email**: Tu email
4. Copiar **Client ID** a configuración

```json
{
  "Imgur": {
    "ClientId": "tu-client-id-imgur"
  }
}
```

### **Configuración de Upload**

- **Tamaño máximo**: 5MB por imagen
- **Formatos soportados**: JPG, PNG, GIF, BMP
- **Máximas imágenes**: 5 por artículo
- **Compresión**: Automática por Imgur
- **Fallback**: Almacenamiento local en `/wwwroot/uploads/`

---

## 🗃️ Base de Datos

### **Desarrollo (SQLite)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=garage_sale.db"
  }
}
```

### **Producción (Azure SQL)**
```json
{
  "ConnectionStrings": {
    "AzureSqlConnection": "Server=tcp:servidor.database.windows.net,1433;Initial Catalog=BD;User ID=usuario;Password=password;Encrypt=True;"
  }
}
```

### **Modelos de Datos**

| Tabla | Descripción | Campos Principales |
|-------|-------------|-------------------|
| `Items` | Artículos en venta | Id, Title, Description, Price, ImagePath, IsSold |
| `Categories` | Categorías | Id, Name, Description |
| `ItemImages` | Imágenes múltiples | Id, ItemId, ImagePath, AltText, Order |
| `PurchaseRequests` | Solicitudes de compra | Id, ItemId, BuyerEmail, BuyerName, Status |

---

## 🔐 Variables de Entorno

### **Desarrollo (.env o appsettings.Development.json)**
```bash
# Admin
ADMIN_USERNAME=admin
ADMIN_PASSWORD=password123
ADMIN_EMAIL=admin@gmail.com

# Email  
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=admin@gmail.com
SMTP_PASSWORD=app-password-16-chars

# Database
CONNECTION_STRING=Data Source=garage_sale.db

# Imgur (Opcional)
IMGUR_CLIENT_ID=tu-client-id
```

### **Producción (Azure App Settings)**
Todas las variables anteriores pero con valores reales de producción.

---

## ✅ Verificación y Testing

### **Checklist de Configuración**

#### Local Development
- [ ] `appsettings.Development.json` creado
- [ ] Credenciales admin configuradas
- [ ] Email SMTP configurado y testado
- [ ] Base de datos SQLite funcionando
- [ ] Imgur opcional configurado
- [ ] Aplicación arranca sin errores

#### Azure Production  
- [ ] Variables de entorno configuradas
- [ ] Azure SQL Database conectada
- [ ] Email funcionando en producción
- [ ] SSL/HTTPS habilitado
- [ ] Domain custom configurado (opcional)
- [ ] Logs y monitoreo funcionando

### **Testing Manual**

```bash
# Test 1: Aplicación arranca
dotnet run
# ✅ Acceder a https://localhost:5001

# Test 2: Admin login
# ✅ /admin - Login con credenciales configuradas

# Test 3: Upload imagen  
# ✅ Subir imagen en panel admin

# Test 4: Email
# ✅ Enviar solicitud de compra y verificar email

# Test 5: Base de datos
# ✅ Crear/editar/eliminar artículos
```

### **Logs para Debugging**

La aplicación incluye logging detallado:
```csharp
// Ver logs en desarrollo
dotnet run --verbosity detailed

// Logs incluyen:
// 🖼️ Upload de imágenes
// 📧 Envío de emails  
// 🔐 Autenticación admin
// 💾 Operaciones de BD
// ⚡ Performance
```

---

## 🚨 Troubleshooting Común

### **Error: No se pueden enviar emails**
- ✅ Verificar App Password de Gmail (16 caracteres)
- ✅ Confirmar 2FA habilitado en Gmail
- ✅ Revisar configuración SMTP en variables

### **Error: No se suben imágenes**
- ✅ Verificar Client ID de Imgur
- ✅ Confirmar permisos en `/wwwroot/uploads/`
- ✅ Revisar tamaño de archivo (max 5MB)

### **Error: Admin login falla**
- ✅ Verificar credenciales en configuración
- ✅ Confirmar variables de entorno en Azure
- ✅ Revisar logs de autenticación

### **Error: Base de datos**
- ✅ Verificar cadena de conexión
- ✅ Confirmar migraciones aplicadas
- ✅ Revisar permisos de BD en Azure

---

## 📚 Recursos Adicionales

- 🔗 [Documentación .NET 9](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-9)
- 🔗 [Blazor Server Guide](https://docs.microsoft.com/aspnet/core/blazor/hosting-models)
- 🔗 [Azure App Service](https://docs.microsoft.com/azure/app-service/)
- 🔗 [Imgur API Docs](https://apidocs.imgur.com/)
- 🔗 [Gmail App Passwords](https://support.google.com/accounts/answer/185833)

---

**📞 Soporte**: Si tienes problemas con la configuración, revisa los logs de la aplicación y consulta esta documentación. Para issues específicos, crea un ticket en GitHub. 