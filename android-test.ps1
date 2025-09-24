# XCAPE Android Setup and Test Script
# Automatiza configuración de Android SDK y testing en emulador

param(
    [switch]$Setup,
    [switch]$Build,
    [switch]$Deploy,
    [switch]$Test,
    [switch]$All
)

# Colores para output
function Write-ColorOutput($ForegroundColor, $Message) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    Write-Output $Message
    $host.UI.RawUI.ForegroundColor = $fc
}

function Write-Success($Message) { Write-ColorOutput Green "[OK] $Message" }
function Write-Warning($Message) { Write-ColorOutput Yellow "[WARN] $Message" }
function Write-Error($Message) { Write-ColorOutput Red "[ERROR] $Message" }
function Write-Info($Message) { Write-ColorOutput Cyan "[INFO] $Message" }

Write-Info "XCAPE Bowling - Android Emulator Setup and Test"
Write-Info "==============================================="

# Detectar rutas de Android SDK
$AndroidSdkPaths = @(
    "$env:ANDROID_HOME",
    "$env:LOCALAPPDATA\Android\Sdk",
    "$env:APPDATA\Android\Sdk",
    "C:\Android\Sdk",
    "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk"
)

$AndroidSdkPath = $null
foreach ($path in $AndroidSdkPaths) {
    if ($path -and (Test-Path $path)) {
        $AndroidSdkPath = $path
        break
    }
}

# Función para verificar herramientas Android
function Test-AndroidSetup {
    Write-Info "Verificando configuración de Android..."
    
    if (-not $AndroidSdkPath) {
        Write-Error "Android SDK no encontrado. Instala Android Studio primero."
        Write-Info "Descarga desde: https://developer.android.com/studio"
        return $false
    }
    
    Write-Success "Android SDK encontrado en: $AndroidSdkPath"
    
    # Verificar ADB
    $adbPath = Join-Path $AndroidSdkPath "platform-tools\adb.exe"
    if (Test-Path $adbPath) {
        Write-Success "ADB encontrado"
        
        # Verificar dispositivos conectados
        $devices = & $adbPath devices
        if ($devices -match "device$") {
            Write-Success "Emulador/dispositivo Android conectado"
            return $true
        } else {
            Write-Warning "No hay dispositivos Android conectados"
            Write-Info "Inicia un emulador en Android Studio (AVD Manager)"
            return $false
        }
    } else {
        Write-Error "ADB no encontrado en $adbPath"
        return $false
    }
}

# Función para verificar Unity
function Test-UnitySetup {
    Write-Info "Verificando configuración de Unity..."
    
    $UnityPaths = @(
        "C:\Program Files\Unity\Hub\Editor\2023.3.40f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\2023.3.*\Editor\Unity.exe"
    )
    
    $UnityPath = $null
    foreach ($path in $UnityPaths) {
        if (Test-Path $path) {
            $UnityPath = $path
            break
        }
    }
    
    if ($UnityPath) {
        Write-Success "Unity encontrado en: $UnityPath"
        return $UnityPath
    } else {
        Write-Error "Unity 2023.3.40f1 no encontrado"
        Write-Info "Instala Unity 2023.3.40f1 LTS con módulo Android"
        return $null
    }
}

# Función para build automático
function Invoke-UnityBuild {
    param([string]$UnityPath)
    
    Write-Info "Iniciando build de Android..."
    
    $buildPath = Join-Path $PSScriptRoot "build\Android"
    if (-not (Test-Path $buildPath)) {
        New-Item -ItemType Directory -Path $buildPath -Force | Out-Null
    }
    
    $arguments = @(
        "-batchmode",
        "-quit",
        "-projectPath", $PSScriptRoot,
        "-buildTarget", "Android",
        "-executeMethod", "XCAPE.Build.BuildAutomation.BuildAndroid",
        "-logFile", "build.log"
    )
    
    Write-Info "Ejecutando Unity build..."
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru
    
    if ($process.ExitCode -eq 0) {
        Write-Success "Build completado exitosamente"
        return $true
    } else {
        Write-Error "Build falló con código: $($process.ExitCode)"
        Write-Info "Revisa build.log para detalles"
        return $false
    }
}

# Función para deploy al emulador
function Invoke-Deploy {
    Write-Info "Desplegando al emulador..."
    
    $adbPath = Join-Path $AndroidSdkPath "platform-tools\adb.exe"
    $apkPath = Join-Path $PSScriptRoot "build\Android\XCAPE.apk"
    
    if (-not (Test-Path $apkPath)) {
        Write-Error "APK no encontrado en: $apkPath"
        Write-Info "Ejecuta build primero con: .\android-test.ps1 -Build"
        return $false
    }
    
    Write-Info "Instalando APK..."
    $result = & $adbPath install -r $apkPath 2>&1
    
    if ($result -match "Success") {
        Write-Success "APK instalado correctamente"
        
        Write-Info "Iniciando aplicación..."
        & $adbPath shell am start -n com.xcapestudio.xcapebowling/.UnityPlayerActivity
        Write-Success "¡XCAPE Bowling iniciado en el emulador!"
        return $true
    } else {
        Write-Error "Error instalando APK: $result"
        return $false
    }
}

# Función para testing automático
function Invoke-AutoTest {
    Write-Info "Iniciando tests automáticos..."
    
    $adbPath = Join-Path $AndroidSdkPath "platform-tools\adb.exe"
    
    # Esperar que la app cargue
    Start-Sleep -Seconds 3
    
    Write-Info "Simulando input de test..."
    
    # Tap en botón Play (coordenadas aproximadas para landscape)
    & $adbPath shell input tap 960 540
    Start-Sleep -Seconds 2
    
    # Simular lanzamiento de bola (swipe)
    & $adbPath shell input swipe 500 800 500 300 500
    Start-Sleep -Seconds 3
    
    Write-Info "Test básico completado"
    Write-Info "Verifica manualmente que:"
    Write-Success "- App se abrio sin crashes"
    Write-Success "- Main Menu es visible"
    Write-Success "- Scene transition funciono"
    Write-Success "- Ball physics responden"
}

# Main execution
if ($All -or $Setup) {
    if (-not (Test-AndroidSetup)) {
        Write-Error "Setup de Android falló. Corrige los errores y reintenta."
        exit 1
    }
}

$UnityPath = $null
if ($All -or $Build) {
    $UnityPath = Test-UnitySetup
    if (-not $UnityPath) {
        Write-Error "Setup de Unity falló. Instala Unity 2023.3.40f1 LTS."
        exit 1
    }
    
    if (-not (Invoke-UnityBuild $UnityPath)) {
        Write-Error "Build falló. Revisa logs para detalles."
        exit 1
    }
}

if ($All -or $Deploy) {
    if (-not (Test-AndroidSetup)) {
        Write-Error "No hay emulador conectado. Inicia uno primero."
        exit 1
    }
    
    if (-not (Invoke-Deploy)) {
        Write-Error "Deploy falló."
        exit 1
    }
}

if ($All -or $Test) {
    Invoke-AutoTest
}

if (-not ($Setup -or $Build -or $Deploy -or $Test -or $All)) {
    Write-Info "Uso: .\android-test.ps1 [-Setup] [-Build] [-Deploy] [-Test] [-All]"
    Write-Info ""
    Write-Info "Opciones:"
    Write-Info "  -Setup   Verificar configuración de Android SDK y emulador"
    Write-Info "  -Build   Compilar XCAPE para Android"
    Write-Info "  -Deploy  Instalar y ejecutar en emulador"
    Write-Info "  -Test    Ejecutar tests automáticos básicos"
    Write-Info "  -All     Ejecutar todas las opciones"
    Write-Info ""
    Write-Info "Ejemplo: .\android-test.ps1 -All"
}
