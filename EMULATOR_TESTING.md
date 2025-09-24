# XCAPE Emulator Setup & Testing Guide

## Configuración Rápida para Testing en Emulador

### 1. Instalar Android Studio y SDK
1. Descarga [Android Studio](https://developer.android.com/studio)
2. Durante instalación, asegúrate de instalar:
   - Android SDK
   - Android SDK Platform-Tools
   - Android Emulator
   - HAXM (Intel) o Hyper-V (AMD)

### 2. Configurar Variables de Entorno
Añade a tu PATH del sistema:
```
C:\Users\[TU_USUARIO]\AppData\Local\Android\Sdk\platform-tools
C:\Users\[TU_USUARIO]\AppData\Local\Android\Sdk\tools
```

### 3. Crear Emulador Android
1. Abre Android Studio
2. Ve a `Tools > AVD Manager`
3. Crea un dispositivo virtual:
   - **Dispositivo**: Pixel 7 Pro (resolución alta, landscape ideal para bowling)
   - **API**: Android 13 (API 33) o superior
   - **RAM**: 4GB mínimo
   - **Storage**: 8GB mínimo

### 4. Verificar Configuración
```powershell
# Verificar ADB funciona
adb devices

# Debería mostrar:
# List of devices attached
# emulator-5554    device
```

## Testing Steps en Unity

### Quick Test (Scene directo)
1. Abre Unity con el proyecto XCAPE
2. Ve a `Assets/Scenes/GamePlay.unity`
3. Inicia el emulador Android
4. En Unity: `File > Build Settings > Android`
5. Click `Build and Run`

### Full Game Test (Main Menu flow)
1. Asegurar Build Settings tiene ambas escenas:
   - `MainMenu.unity` (index 0)
   - `GamePlay.unity` (index 1)
2. `File > Build Settings > Build and Run`
3. Probar flujo completo: Main Menu → Play → Bowling

### Multiplayer Test (UGS)
1. Abre `MainMenu.unity`
2. Busca el `LobbyPanel` (debería estar inactivo)
3. Activar panel en Inspector
4. `Build and Run`
5. Test: Create Lobby → Join Code → Ready/Start

## Debugging en Emulador

### Unity Remote (Alternativa rápida)
1. Instalar "Unity Remote 5" en emulador desde Play Store
2. En Unity: `Edit > Project Settings > XR Plug-in Management > Unity Remote`
3. `Window > General > Game > Remote` para testing inmediato sin builds

### Logcat (Ver logs en tiempo real)
```powershell
# Instalar logcat viewer
adb logcat -s Unity

# O filtrar por XCAPE
adb logcat | findstr "XCAPE"
```

### Performance Profiling
1. En Unity: `Window > Analysis > Profiler`
2. Build con `Development Build` checked
3. Connect to emulator via IP

## Troubleshooting Common Issues

### "No devices found"
```powershell
# Reiniciar ADB
adb kill-server
adb start-server
adb devices
```

### Emulator muy lento
- Habilitar Hardware Acceleration en BIOS (VT-x/AMD-V)
- Aumentar RAM del emulador
- Usar sistema de archivos rápido (no encrypted)

### Build errors
- Verificar API Level mínimo en `PlayerSettings` (24+)
- Limpiar `Library` folder si hay problemas extraños
- Verificar NDK instalado correctamente

### AdMob testing
- Test IDs están preconfigurados en `AdManager.cs`
- Internet funciona en emulador por defecto
- Logs de AdMob aparecen en logcat

## Scripts de Automatización

Crea estos batch files para testing rápido:

### `start_emulator.bat`
```batch
@echo off
cd /d "%ANDROID_HOME%\emulator"
emulator -avd Pixel_7_Pro_API_33 -gpu host
```

### `quick_build.bat`
```batch
@echo off
echo Building XCAPE for Android...
"C:\Program Files\Unity\Hub\Editor\2023.3.40f1\Editor\Unity.exe" ^
  -batchmode ^
  -quit ^
  -projectPath "C:\Apps\xcape" ^
  -buildTarget Android ^
  -executeMethod XCAPE.Build.BuildAutomation.BuildAndroid
echo Build complete!
```

### `deploy_and_test.bat`
```batch
@echo off
adb devices
adb install -r "build\Android\XCAPE.apk"
adb shell am start -n com.xcapestudio.xcapebowling/.UnityPlayerActivity
echo Game launched on emulator!
```

## Expected Test Results

### ✅ Basic Functionality
- [ ] App launches without crashes
- [ ] Main Menu renders correctly
- [ ] Settings panel opens/closes
- [ ] Scene transition to GamePlay works

### ✅ Gameplay Core
- [ ] Ball spawns and physics work
- [ ] Touch controls respond (click/drag for power/direction)
- [ ] Pins knock down realistically
- [ ] Score calculation is accurate
- [ ] Frame progression works correctly

### ✅ UI/UX
- [ ] Scorecard updates in real-time
- [ ] Pause menu functional
- [ ] Game Over screen shows final score
- [ ] Settings persist between sessions

### ✅ Networking (if UGS configured)
- [ ] Create Lobby shows Join Code
- [ ] Players list updates correctly
- [ ] Ready/Start flow works
- [ ] Host can start game when all ready

### ✅ AdMob (Test Mode)
- [ ] Banner appears at bottom (after delay)
- [ ] Interstitial shows after 3rd game
- [ ] Rewarded video loads and plays
- [ ] Premium toggle hides ads

## Performance Targets

### Mobile Optimized Settings
- **Target FPS**: 60 (configured in GameManager)
- **Resolution**: Adaptive (URP handles scaling)
- **Memory**: <2GB RAM usage
- **Battery**: Moderate drain (physics-heavy game)

### Emulator Specific
- **Input Latency**: <100ms (mouse simulates touch)
- **Physics**: Should run smooth at 60fps
- **Loading**: Scene transitions <3 seconds
- **Network**: UGS calls <500ms (localhost advantage)

¿Quieres que configure automáticamente algunas de estas opciones o prefieres que empecemos con una build directa al emulador?
