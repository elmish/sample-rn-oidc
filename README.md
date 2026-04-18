# Elmish OIDC React Native Sample

Minimal Expo/React Native app demonstrating [Fable.Elmish.OIDC.ReactNative](https://github.com/elmish/oidc) authentication with Microsoft Entra ID.

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20+
- [Expo Go](https://expo.dev/go) app on your device or simulator

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

# In another terminal, start Expo Go on iOS simulator
npm run ios
```

Or run `npm start` and scan the QR code with Expo Go on your device.

## How It Works

The app uses `Fable.Elmish.OIDC.ReactNative` (`1.0.0-beta.2`) with:

- **`Api.create`** — factory that wires up crypto, HTTP, encoding, timer, and refresh-token renewal into a ready-to-use `{| init; update; subscribe |}` record
- **`ReactNative.Navigation.authSession`** — opens the identity provider in an in-app browser via `expo-web-browser`, captures the authorization code callback automatically
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
