# XCAPE Bowling (Unity 2023.3 LTS)

Proyecto Unity móvil (Android/iOS) con URP, Netcode, AdMob e IAPs.

## Requisitos
- Unity Hub + Unity 2023.3.40f1 LTS con módulos Android e iOS
- Android Studio (SDK/NDK) / Xcode (macOS)
- VS Code + C# Dev Kit

## Primer arranque
1. Abre la carpeta en Unity Hub y selecciona 2023.3.40f1.
2. Unity descargará paquetes del `Packages/manifest.json`.
3. Al iniciar, ve a Tools > XCAPE > Setup Wizard para auto-configurar (scripting symbols, URP, Build Settings Android/iOS).

## Estructura
- `Assets/` scripts, escenas, prefabs, materiales, modelos, audio
- `ProjectSettings/` configuración del proyecto
- `Packages/` paquetes y dependencias

## Builds
- Android: File > Build Settings > Android > Switch Platform > Build (AAB)
- iOS: File > Build Settings > iOS > Switch Platform > Build

## Soporte
Issues y roadmap en `plan.yml`.
