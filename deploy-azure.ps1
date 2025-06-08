# Script para desplegar MyGarageSale en Azure
# Configuración actualizada con los valores que funcionaron exitosamente

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppName,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "Brazil South"  # Óptimo para Argentina
)

Write-Host "🚀 Iniciando despliegue de MyGarageSale en Azure..." -ForegroundColor Green
Write-Host "📍 Ubicación: $Location (optimizado para Argentina)" -ForegroundColor Cyan

# 1. Crear grupo de recursos
Write-Host "📁 Creando grupo de recursos..." -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location

# 2. Crear App Service Plan (Free Tier)
Write-Host "🏗️ Creando App Service Plan gratuito..." -ForegroundColor Yellow
az appservice plan create --name "$AppName-plan" --resource-group $ResourceGroupName --sku F1 --location $Location

# 3. Crear Web App
Write-Host "🌐 Creando Web App..." -ForegroundColor Yellow
az webapp create --resource-group $ResourceGroupName --plan "$AppName-plan" --name $AppName --runtime "dotnet:9"

# 4. Configurar variables de entorno
Write-Host "⚙️ Configurando variables de entorno..." -ForegroundColor Yellow
az webapp config appsettings set --resource-group $ResourceGroupName --name $AppName --settings `
    "ASPNETCORE_ENVIRONMENT=Production" `
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE=true" `
    "SCM_DO_BUILD_DURING_DEPLOYMENT=true"

# 5. Publicar aplicación
Write-Host "📦 Publicando aplicación..." -ForegroundColor Yellow
dotnet publish -c Release -o ./publish --nologo

# 6. Crear archivo ZIP para despliegue
Write-Host "🗜️ Creando archivo ZIP..." -ForegroundColor Yellow
Compress-Archive -Path ./publish/* -DestinationPath ./app.zip -Force

# 7. Desplegar aplicación
Write-Host "🚀 Desplegando aplicación..." -ForegroundColor Yellow
az webapp deploy --resource-group $ResourceGroupName --name $AppName --src-path "./app.zip" --type zip

# 8. Mostrar información final
$appUrl = az webapp show --resource-group $ResourceGroupName --name $AppName --query defaultHostName --output tsv
Write-Host ""
Write-Host "✅ ¡Despliegue completado exitosamente!" -ForegroundColor Green
Write-Host "🌍 Tu aplicación está disponible en: https://$appUrl" -ForegroundColor Cyan
Write-Host "💰 Costo: $0.00 USD (Plan F1 gratuito)" -ForegroundColor Green

# Limpiar archivos temporales
Remove-Item -Path ./publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ./app.zip -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "🎉 ¡Tu Garage Sale está en la nube!" -ForegroundColor Green
Write-Host ""
Write-Host "⚡ CONFIGURACIÓN RÁPIDA (5 minutos):" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. 🖼️  IMGUR (Para imágenes persistentes):"
Write-Host "   - https://api.imgur.com/oauth2/addclient → Crear app gratuita"
Write-Host "   - az webapp config appsettings set --resource-group $ResourceGroupName --name $AppName --settings Imgur__ClientId=TU_CLIENT_ID"
Write-Host ""
Write-Host "2. 📧 EMAIL (Gmail SMTP - más fácil):"
Write-Host "   - Habilita 'App Passwords' en tu Gmail"
Write-Host "   - az webapp config appsettings set --resource-group $ResourceGroupName --name $AppName --settings Smtp__Username=tu@gmail.com Smtp__Password=tu-app-password"
Write-Host ""
Write-Host "3. 👤 ADMIN:"
Write-Host "   - az webapp config appsettings set --resource-group $ResourceGroupName --name $AppName --settings AdminCredentials__Username=admin AdminCredentials__Password=tu-password" 