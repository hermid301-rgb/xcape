# AdMob Integration Guide

Este proyecto incluye integración completa con Google AdMob para monetización.

## Setup Inicial

1. **Instalar AdMob SDK**:
   - En Unity: `Tools > XCAPE > Setup Wizard > Install Google Mobile Ads (AdMob)`
   - O manualmente: `Window > Package Manager > + > Add from Git URL > https://github.com/googleads/googleads-mobile-unity.git`

2. **Crear cuenta AdMob**:
   - Ve a [AdMob Console](https://apps.admob.com/)
   - Crea una nueva app para Android e iOS
   - Obtén tu App ID y Ad Unit IDs

3. **Configurar IDs de producción**:
   - Edita `Assets/Scripts/Monetization/AdManager.cs`
   - Reemplaza los test IDs con tus IDs reales:
     ```csharp
     [SerializeField] private string androidBannerAdUnitId = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
     [SerializeField] private string androidInterstitialAdUnitId = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
     [SerializeField] private string androidRewardedAdUnitId = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
     ```

4. **Configurar App IDs**:
   - **Android**: Edita `Assets/Plugins/Android/AndroidManifest.xml`
     ```xml
     <meta-data
         android:name="com.google.android.gms.ads.APPLICATION_ID"
         android:value="ca-app-pub-XXXXXXXXXXXXXXXX~XXXXXXXXXX"/>
     ```
   - **iOS**: Los App IDs se configuran automáticamente en build via `iOSPostProcess.cs`

## Tipos de Anuncios

### Banner Ads
- Se muestran automáticamente en el bottom de la pantalla
- Se ocultan automáticamente para usuarios premium
- Control manual via `AdManager.ShowBannerAd()` / `HideBannerAd()`

### Interstitial Ads
- Se muestran cada 3 partidas completadas (configurable)
- Llamar `AdManager.ShowInterstitialAd()` al final del juego
- Evento `OnInterstitialClosed` para continuar flujo

### Rewarded Ads
- Para obtener vidas extra, puntos bonus, etc.
- Llamar `AdManager.ShowRewardedAd(callback)` 
- Callback recibe `true` si el usuario completó el video

## Integración en Código

```csharp
// Obtener referencia al AdManager
var adManager = GameManager.Instance?.AdManager;

// Mostrar banner al entrar al juego
adManager?.ShowBannerAd();

// Mostrar interstitial al completar partida
adManager?.ShowInterstitialAd();

// Mostrar rewarded para bonus
adManager?.ShowRewardedAd((success) => {
    if (success) {
        // Dar recompensa al jugador
        GiveExtraLife();
    }
});

// Verificar si hay anuncio rewarded disponible
bool canShowReward = adManager?.IsRewardedAdReady() ?? false;
```

## Test Mode

- Por defecto está habilitado test mode (`testMode = true` en AdManager)
- Los test IDs están preconfigurados para desarrollo
- Cambiar a `false` para builds de producción

## Premium Users

- Los usuarios premium (`IsPremium = true`) no ven anuncios
- Se controla via PlayerPrefs o sistema IAP
- Usar `AdManager.SetPremiumStatus(true)` para activar

## Troubleshooting

### Android
- Verificar que `AndroidManifest.xml` tiene los permisos correctos
- Asegurar que Google Play Services está actualizado en el dispositivo
- Verificar que App ID coincide con AdMob console

### iOS
- Verificar que App ID está en Info.plist (se hace automáticamente)
- Para iOS 14+, verificar SKAdNetwork IDs en `iOSPostProcess.cs`
- Verificar App Tracking Transparency si usas personalización

### General
- Verificar internet connection
- Revisar logs de Unity Console para errores de AdMob
- Asegurar que Ad Unit IDs son correctos y corresponden a la plataforma

## Optimización

- Los anuncios se precargan automáticamente para mejor experiencia
- Banner se oculta cuando app va a background
- Frecuencia de interstitials es configurable (`interstitialFrequency`)
- Timeouts y retry logic integrados

## Compliance

- GDPR: AdMob maneja automáticamente en Europa
- COPPA: Configurar `SetTagForChildDirectedTreatment` si necesario
- CCPA: AdMob maneja automáticamente en California
- Verificar app-ads.txt en tu dominio web (requerido por AdMob)
