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

## Vinculación a Unity Gaming Services (UGS)
Para que Lobbies/Relay/Analytics funcionen, el proyecto debe estar vinculado:

- Opción A (recomendada): copia `ProjectSettings/UGS.link.template.json` a `ProjectSettings/UGS.link.json` (gitignored) y rellena:
	- `organizationId`: ID numérico de tu organización
	- `projectId`: GUID del proyecto UGS
	- `projectName`: nombre visible del proyecto
- Opción B: define variables de entorno antes de abrir Unity:
	- `UGS_ORG_ID`, `UGS_PROJECT_ID`, `UGS_PROJECT_NAME`

El editor aplicará el enlace en el arranque (Assets/Scripts/Editor/UGSLinkOnLoad.cs). También puedes ir a Tools > XCAPE > Setup Wizard > "Open Services Project Settings" y vincular manualmente.

## Estructura
- `Assets/` scripts, escenas, prefabs, materiales, modelos, audio
- `ProjectSettings/` configuración del proyecto
- `Packages/` paquetes y dependencias

## Builds
- Android: File > Build Settings > Android > Switch Platform > Build (AAB)
- iOS: File > Build Settings > iOS > Switch Platform > Build
- Automated: Push to main/master triggers CI builds via GitHub Actions

## Testing
- Tests: `Assets/Tests/EditMode/ScoreManagerTests.cs`
- Run: Window > General > Test Runner > EditMode > Run All
- CI: Tests run automatically on all PRs and pushes

## Monetización
- AdMob integrado (ver `ADMOB_SETUP.md` para configuración completa)
- Install: Tools > XCAPE > Setup Wizard > "Install Google Mobile Ads (AdMob)"
- Configurar App IDs y Ad Unit IDs en `AdManager.cs` y manifests

## Soporte
Issues y roadmap en `plan.yml`.
