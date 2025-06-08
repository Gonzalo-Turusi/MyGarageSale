# 🏠 MyGarageSale - Venta de Garage Online

**MyGarageSale** es una aplicación web moderna desarrollada con **.NET 9** y **Blazor Server**, diseñada para gestionar una venta de garage online de forma elegante y eficiente. Permite a los visitantes navegar por un catálogo de artículos y facilita al propietario la carga rápida de fotos, descripciones y precios.

## 🌟 Características Principales

### 🛍️ **Para Compradores**
- **Catálogo Visual**: Navegación intuitiva con imágenes de alta calidad
- **Búsqueda y Filtros**: Por categorías, precios y disponibilidad
- **Carrito de Compras**: Gestión de artículos de interés
- **Galería de Imágenes**: Múltiples fotos por artículo con carousel interactivo
- **Solicitud de Compra**: Sistema de contacto directo con el vendedor
- **Responsive Design**: Optimizado para móviles y escritorio

### 🔧 **Para Administradores**
- **Panel de Admin**: Gestión completa de inventario
- **Upload de Imágenes**: Soporte para múltiples imágenes por artículo
- **Gestión de Categorías**: Organización flexible del catálogo
- **Sistema de Notificaciones**: Email automático de solicitudes de compra
- **Control de Estado**: Marcar artículos como vendidos
- **Estadísticas**: Panel de control con métricas básicas

## 🚀 Tecnologías Utilizadas

- **Backend**: .NET 9, Blazor Server, Entity Framework Core
- **Frontend**: Blazor Components, Tailwind CSS
- **Base de Datos**: SQLite (desarrollo) / Azure SQL (producción)
- **Almacenamiento**: Imgur API + Local Fallback para imágenes
- **Email**: SMTP (Gmail)
- **Hosting**: Microsoft Azure App Service
- **CI/CD**: GitHub Actions (configurado)

## 📁 Estructura del Proyecto

```
MyGarageSale/
├── Components/           # Componentes Blazor
│   ├── Pages/           # Páginas principales
│   ├── Admin/           # Panel administrativo
│   └── Layout/          # Plantillas y layout
├── Services/            # Lógica de negocio
│   ├── ItemService      # Gestión de artículos
│   ├── CategoryService  # Gestión de categorías
│   ├── CartService      # Carrito de compras
│   ├── EmailService     # Notificaciones
│   ├── AuthService      # Autenticación admin
│   └── ImageUpload*     # Servicios de imágenes
├── Models/              # Modelos de datos
├── Data/                # Contexto de base de datos
├── wwwroot/             # Archivos estáticos
└── Properties/          # Configuración de Azure
```

## 🛠️ Instalación y Configuración

### **Requisitos Previos**
- .NET 9 SDK
- Node.js (para Tailwind CSS)
- Visual Studio 2022 o VS Code

### **Instalación Local**

1. **Clonar el repositorio**
```bash
git clone https://github.com/tu-usuario/MyGarageSale.git
cd MyGarageSale
```

2. **Restaurar dependencias**
```bash
dotnet restore
npm install
```

3. **Configurar variables locales** (ver [CONFIGURACION.md](CONFIGURACION.md))
```bash
# Crear appsettings.Development.json con tus credenciales
```

4. **Ejecutar la aplicación**
```bash
dotnet run
```

La aplicación estará disponible en `https://localhost:5001`

## 🔑 Configuración de Seguridad

⚠️ **IMPORTANTE**: Para configuración detallada de credenciales, emails y despliegue en Azure, consulta [CONFIGURACION.md](CONFIGURACION.md)

## 🌐 Despliegue

### **Azure App Service**
La aplicación está configurada para despliegue automático en Azure:

1. **GitHub Actions**: Configurado para CD/CI automático
2. **Azure SQL**: Base de datos en producción
3. **Variables de Entorno**: Configuradas de forma segura
4. **Custom Domain**: Soporte para dominio personalizado

### **Script de Despliegue**
```bash
# Ejecutar deploy-azure.ps1 para despliegue manual
./deploy-azure.ps1
```

## 📧 Sistema de Notificaciones

- **Email Automático**: Notificación inmediata de solicitudes de compra
- **Templates HTML**: Emails profesionales con información detallada
- **Configuración SMTP**: Gmail con App Password seguro
- **Error Handling**: Logs detallados y fallbacks

## 🎨 Interfaz de Usuario

- **Design System**: Tailwind CSS con componentes personalizados
- **Tema Moderno**: Colores profesionales y tipografía cuidada
- **Animaciones**: Transiciones suaves y micro-interacciones
- **Iconografía**: Emojis y iconos para mejor UX
- **Mobile First**: Diseño responsive desde el primer momento

## 📊 Funcionalidades Avanzadas

### **Sistema de Imágenes**
- **Dual Upload**: Imgur API como principal, almacenamiento local como fallback
- **Múltiples Imágenes**: Hasta 5 imágenes por artículo
- **Carousel Interactivo**: Navegación fluida entre imágenes
- **Optimización**: Compresión automática y carga lazy

### **Carrito de Compras**
- **Persistencia**: Mantiene estado entre sesiones
- **Gestión Inteligente**: Validación de disponibilidad
- **UX Optimizada**: Feedback visual inmediato

## 🔧 Mantenimiento

### **Logs y Monitoreo**
- **Structured Logging**: Logs detallados con categorización
- **Error Tracking**: Manejo de excepciones centralizado
- **Performance**: Métricas de rendimiento básico

### **Base de Datos**
- **Migrations**: Entity Framework para esquema
- **Seed Data**: Datos de ejemplo para desarrollo
- **Backup**: Estrategia de respaldo en Azure

## 📈 Estadísticas del Proyecto

- **Componentes**: 15+ componentes Blazor reutilizables
- **Servicios**: 8 servicios especializados
- **Endpoints**: API REST completa
- **Responsive**: 100% compatible con dispositivos móviles
- **Performance**: Carga < 2s en promedio

## 🤝 Contribución

Este es un proyecto privado, pero si tienes sugerencias:

1. Crear un issue describiendo la mejora
2. Fork del repositorio
3. Crear rama feature con la implementación
4. Enviar pull request con descripción detallada

## 📄 Licencia

MIT License - Ver [LICENSE](LICENSE) para más detalles.

## 📞 Soporte

Para soporte técnico o consultas sobre el proyecto:
- **GitHub Issues**: Para bugs y mejoras
- **Email**: Configurado en las variables de entorno
- **Documentación**: [CONFIGURACION.md](CONFIGURACION.md) para setup detallado

---

**Desarrollado con ❤️ usando .NET 9 y Blazor**
