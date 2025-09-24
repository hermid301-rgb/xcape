# XCAPE Quick Test Guide

## Opción 1: Build and Run directo desde Unity (Recomendado)

### Pasos simples:
1. **Abrir Unity**: Doble click en `xcape` desde Unity Hub
2. **Verificar escenas**: 
   - File > Build Settings
   - Asegurar que MainMenu.unity está en índice 0
   - GamePlay.unity en índice 1
3. **Configurar Android**:
   - File > Build Settings > Android
   - Click "Switch Platform" (si no está activo)
4. **Build and Run**:
   - Conectar emulador Android o dispositivo físico
   - Click "Build and Run"
   - Seleccionar carpeta (ej: `builds/android`)
   - Unity construirá e instalará automáticamente

### Durante el build:
- Unity mostrará progreso en la barra inferior
- Puede tomar 5-15 minutos la primera vez
- El emulador se abrirá automáticamente cuando esté listo

## Opción 2: Unity Remote (Testing inmediato)

### Para testing sin builds:
1. **Instalar Unity Remote 5**:
   - En emulador: abrir Play Store
   - Buscar "Unity Remote 5"
   - Instalar y abrir

2. **Configurar Unity**:
   - Edit > Project Settings > XR Plug-in Management
   - Expandir "Unity Remote"
   - Device: "Any Android Device"

3. **Testing**:
   - Window > General > Game
   - En Game view, click dropdown y seleccionar "Remote"
   - Play en Unity Editor
   - El juego aparecerá en el emulador en tiempo real

## Opción 3: Testing desde Editor (Más rápido)

### Para desarrollo rápido:
1. **Abrir GamePlay scene**: Assets/Scenes/GamePlay.unity
2. **Simular touch con mouse**: Los controles están configurados para mouse/touch
3. **Play**: Click botón Play en Unity
4. **Interactuar**:
   - Click y drag para lanzar bola
   - Probar física y scoring

## Troubleshooting Común

### "No Android device found"
```
1. Abrir Android Studio
2. Tools > AVD Manager
3. Crear/Iniciar emulador Android
4. En Windows: verificar que emulador aparece en dispositivos
```

### "Build failed"
```
1. Verificar Android SDK path en Unity:
   Edit > Preferences > External Tools
2. Instalar Android Build Support en Unity Hub
3. Verificar API Level mínimo (24) en Player Settings
```

### Performance lento en emulador
```
1. Configurar emulador con:
   - RAM: 4GB+
   - Hardware acceleration enabled
   - Graphics: Hardware - GLES 2.0
2. En Unity Player Settings:
   - Graphics API: OpenGLES3 (prioritario)
   - Multithreaded Rendering: enabled
```

## Expected Results ✅

### Gameplay básico funcionando:
- [x] App inicia sin crashes
- [x] Main Menu visible y funcional  
- [x] Settings panel abre/cierra
- [x] Transición a GamePlay scene
- [x] Ball spawn y física
- [x] Touch controls (click/drag)
- [x] Pin knockdown physics
- [x] Score calculation
- [x] Game over flow

### UGS/Networking (si configurado):
- [x] Create Lobby muestra Join Code
- [x] Players list se actualiza
- [x] Ready/Start flow

### AdMob (Test mode):
- [x] Banner aparece en bottom
- [x] Interstitial después de 3 juegos
- [x] Rewarded video disponible

## Quick Commands

### Si tienes Android SDK instalado:
```powershell
# Verificar dispositivos
adb devices

# Ver logs en tiempo real
adb logcat -s Unity

# Instalar APK manualmente
adb install -r builds/android/XCAPE.apk
```

### Performance monitoring:
1. Unity Profiler: Window > Analysis > Profiler
2. Build con "Development Build" + "Autoconnect Profiler"
3. Connect al dispositivo para ver performance en tiempo real

¿Prefieres empezar con Unity Remote (más rápido) o hacer un build completo al emulador?
