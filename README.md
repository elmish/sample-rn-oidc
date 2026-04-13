# Elmish OIDC React Native Sample

Minimal Expo/React Native app demonstrating [Fable.Elmish.OIDC.ReactNative](https://github.com/elmish/oidc) authentication with Microsoft Entra ID.

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20+
- [Expo CLI](https://docs.expo.dev/get-started/installation/) (`npx expo`)
- Expo Go app on your device, **or** a dev build

## Azure AD Setup

1. Run the setup script (requires [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)):

   ```bash
   ./setup-azure.sh
   ```

2. Copy the output `ClientId` and `TenantId` into `src/App.fs`.

## Development

```bash
# Install dependencies
npm install

# Compile F# → JS (watch mode)
npm run watch

# In another terminal, start Expo
npm start
```

Scan the QR code with Expo Go, or press `i`/`a` for simulators.

## How It Works

The app uses `Fable.Elmish.OIDC.ReactNative` with:

- **`ReactNativeNavigation.authSession`** — opens the identity provider in an in-app browser via `expo-web-browser`, captures the authorization code callback automatically
- **`ReactNativeRenewal.refreshToken`** — renews sessions using the `refresh_token` grant (requested via `offline_access` scope)
- **`ReactNative.memoryStorage`** — in-memory token storage (tokens are lost on app restart; swap with AsyncStorage for persistence)

### OIDC Flow

```
App ──LogIn──▸ authSession opens in-app browser
                  ▸ Microsoft login page
                  ▸ Redirect to app scheme
                  ▸ AuthCallback(code, state)
              ──▸ Token exchange (PKCE)
              ──▸ ID token validation (RSA signature)
              ──▸ Authenticated
```

## Project Structure

```
src/
  App.fs          Elmish app — OIDC integration, views, bootstrap
  Styles.fs       React Native styles
  app.fsproj      F# project — references Fable.Elmish.OIDC.ReactNative
```
