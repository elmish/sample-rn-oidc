module App

(**
 - title: OIDC React Native Sample
 - tagline: Elmish OIDC authentication with Microsoft Entra ID on React Native
*)

open System
open Fable.Core
open Fable.Core.JsInterop
open Elmish
open Elmish.OIDC
open Elmish.OIDC.Types

[<Literal>]
let ClientId = "YOUR_CLIENT_ID"

[<Literal>]
let TenantId = "YOUR_TENANT_ID"

let private redirectUri =
    // Expo's AuthSession proxy redirect — works in Expo Go and dev builds
    // For production, use your app's custom scheme e.g. "sample-rn-oidc://callback"
    emitJsExpr () "(() => { try { return require('expo-auth-session').makeRedirectUri(); } catch(e) { return 'sample-rn-oidc://callback'; } })()"

let oidcOptions : Options =
    { clientId = ClientId
      authority = $"https://login.microsoftonline.com/{TenantId}/v2.0"
      scopes = [ "openid"; "profile"; "email"; "offline_access" ]
      redirectUri = redirectUri
      postLogoutRedirectUri = None
      silentRedirectUri = None
      renewBeforeExpirySeconds = 60
      clockSkewSeconds = 300
      allowedAlgorithms = [ "RS256" ] }

type UserInfo =
    { name: string
      email: string }

let private getUserInfo (userinfoEndpoint: string) (accessToken: string) : Async<UserInfo> =
    async {
        let! response = ReactNative.http.getText userinfoEndpoint
        // In a real app you'd parse JSON properly; this is a sample
        let json : obj = JS.JSON.parse response
        return
            { name = json?name |> Option.ofObj |> Option.defaultValue (string json?sub)
              email = json?preferred_username |> Option.ofObj |> Option.defaultValue "" }
    }

// ── Platform ────────────────────────────────────────────────────────────

let private createPlatform () : Platform =
    let storage = ReactNative.memoryStorage ()
    let nav = ReactNativeNavigation.authSession oidcOptions.redirectUri
    let platform =
        { crypto = ReactNative.crypto
          encoding = ReactNative.encoding
          http = ReactNative.http
          navigation = nav
          renewal = Unchecked.defaultof<RenewalStrategy>
          storage = storage
          timer = ReactNative.timer }
    { platform with renewal = ReactNativeRenewal.refreshToken platform }

let platform = createPlatform ()

// ── Elmish ──────────────────────────────────────────────────────────────

type Model =
    { oidc: Model<UserInfo> }

type Msg =
    | OidcMsg of Msg<UserInfo>

let init () =
    let oidcModel, oidcCmd = Oidc.initPlatform platform oidcOptions
    { oidc = oidcModel }, Cmd.map OidcMsg oidcCmd

let update (msg: Msg) (model: Model) =
    match msg with
    | OidcMsg m ->
        let m', c = Oidc.updatePlatform platform oidcOptions getUserInfo m model.oidc
        { model with oidc = m' }, Cmd.map OidcMsg c

let subscribe (model: Model) =
    Oidc.subscribePlatform platform model.oidc |> Sub.map "oidc" OidcMsg

// ── Views ───────────────────────────────────────────────────────────────

open Fable.ReactNative
open Fable.ReactNative.Props

let private errorText (err: OidcError) =
    match err with
    | DiscoveryError ex -> $"Discovery failed: {ex.Message}"
    | IssuerMismatch (expected, actual) -> $"Issuer mismatch: expected {expected}, got {actual}"
    | InvalidState -> "Invalid state parameter (possible CSRF)"
    | TokenExchangeFailed msg -> $"Token exchange failed: {msg}"
    | InvalidToken msg -> $"Invalid token: {msg}"
    | Expired -> "Session expired"
    | ServerError (err, desc) -> $"Server error: {err} — {desc}"
    | NetworkError ex -> $"Network error: {ex.Message}"

let private viewLoading =
    view [ Styles.sceneBackground ]
        [ text [ Styles.loadingText ] "Initializing..." ]

let private viewError (err: OidcError) =
    view [ Styles.sceneBackground ]
        [ view [ Styles.errorContainer ]
            [ text [ Styles.errorTitle ] "Authentication Error"
              text [ Styles.bodyText ] (errorText err) ] ]

let private viewUnauthenticated dispatch =
    view [ Styles.sceneBackground ]
        [ view [ Styles.card ]
            [ text [ Styles.titleText ] "Elmish OIDC Sample"
              text [ Styles.bodyText ] "Sign in with your Microsoft account to continue."
              text [ Styles.primaryBtnText ] "Sign in with Microsoft"
              |> touchableHighlightWithChild
                    [ Styles.primaryBtnStyle
                      TouchableHighlightProperties.UnderlayColor "#106ebe"
                      OnPress (fun () -> dispatch (OidcMsg LogIn)) ] ] ]

let private viewAuthenticated (session: Session<UserInfo>) dispatch =
    let displayName =
        session.userInfo
        |> Option.map (fun u -> u.name)
        |> Option.defaultValue session.claims.sub

    let email =
        session.userInfo
        |> Option.map (fun u -> u.email)
        |> Option.defaultValue ""

    view [ Styles.sceneBackground ]
        [ view [ Styles.card ]
            [ text [ Styles.titleText ] "Elmish OIDC Sample"
              view [ Styles.userInfoContainer ]
                [ text [ Styles.subtitleText ] $"Welcome, {displayName}!"
                  if email <> "" then
                      text [ Styles.detailText ] email
                  text [ Styles.detailText ] $"Subject: {session.claims.sub}"
                  text [ Styles.detailText ] $"Expires: {session.expiresAt.LocalDateTime}" ]
              text [ Styles.secondaryBtnText ] "Sign out"
              |> touchableHighlightWithChild
                    [ Styles.secondaryBtnStyle
                      TouchableHighlightProperties.UnderlayColor "#ccc"
                      OnPress (fun () -> dispatch (OidcMsg LogOut)) ] ] ]

let view' (model: Model) (dispatch: Msg -> unit) =
    match model.oidc with
    | Initializing -> viewLoading
    | Failed err -> viewError err
    | Ready (_, _, readyState) ->
        match readyState with
        | Authenticated session
        | Renewing session ->
            viewAuthenticated session dispatch
        | ProcessingCallback _
        | ExchangingCode
        | ValidatingToken
        | Redirecting ->
            viewLoading
        | Unauthenticated ->
            viewUnauthenticated dispatch

// ── Bootstrap ───────────────────────────────────────────────────────────

open Elmish.Expo

Program.mkProgram init update view'
|> Program.withSubscription subscribe
|> Program.withConsoleTrace
|> Program.withExpo
|> Program.run
