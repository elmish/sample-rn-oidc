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

// Ensure pending auth sessions are completed when the app is activated by a redirect
do emitJsStatement () """require('expo-web-browser').maybeCompleteAuthSession()"""

// Polyfill globalThis.crypto with react-native-quick-crypto (full Web Crypto API including subtle)
do emitJsStatement () """require('react-native-quick-crypto').install()"""

[<Literal>]
let ClientId = "9650ad2d-c196-42a6-a50b-eca54ab1f8d7"

[<Literal>]
let TenantId = "d0d90d2f-858c-4b0a-9b0c-d9444b08555e"

let private redirectUri : string =
    // Expo's AuthSession redirect — uses scheme from app.json + path
    emitJsExpr () "(() => { try { return require('expo-auth-session').makeRedirectUri({ path: 'callback' }); } catch(e) { return 'sample-rn-oidc://callback'; } })()"

do JS.console.log("Redirect URI:", redirectUri)

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

let private storage = ReactNative.memoryStorage ()
let private nav = ReactNative.Navigation.authSession oidcOptions.redirectUri
let private api = Api.create nav storage oidcOptions

// ── Elmish ──────────────────────────────────────────────────────────────

type Model =
    { oidc: Model<UserInfo> }

type Msg =
    | OidcMsg of Msg<UserInfo>

let init () =
    let oidcModel, oidcCmd = api.init
    { oidc = oidcModel }, Cmd.map OidcMsg oidcCmd

let private logMsg (m: Elmish.OIDC.Types.Msg<UserInfo>) =
    match m with
    | Elmish.OIDC.Types.Msg.DiscoveryLoaded _ -> "DiscoveryLoaded"
    | Elmish.OIDC.Types.Msg.DiscoveryFailed ex -> $"DiscoveryFailed: {ex.Message}"
    | Elmish.OIDC.Types.Msg.JwksLoaded _ -> "JwksLoaded"
    | Elmish.OIDC.Types.Msg.JwksFailed ex -> $"JwksFailed: {ex.Message}"
    | Elmish.OIDC.Types.Msg.AuthCallback (code, state) -> $"AuthCallback code={code.[..6]}... state={state.[..6]}..."
    | Elmish.OIDC.Types.Msg.TokenReceived _ -> "TokenReceived"
    | Elmish.OIDC.Types.Msg.TokenValidated _ -> "TokenValidated"
    | Elmish.OIDC.Types.Msg.ValidationFailed err -> $"ValidationFailed: {err}"
    | Elmish.OIDC.Types.Msg.UserInfo _ -> "UserInfo"
    | Elmish.OIDC.Types.Msg.UserInfoFailed ex -> $"UserInfoFailed: {ex.Message}"
    | Elmish.OIDC.Types.Msg.SilentRenewResult r -> $"SilentRenewResult: {r}"
    | Elmish.OIDC.Types.Msg.LogIn -> "LogIn"
    | Elmish.OIDC.Types.Msg.LogOut -> "LogOut"
    | Elmish.OIDC.Types.Msg.LoggedOut -> "LoggedOut"
    | Elmish.OIDC.Types.Msg.SessionRestored _ -> "SessionRestored"
    | Elmish.OIDC.Types.Msg.NoSession -> "NoSession"
    | Elmish.OIDC.Types.Msg.Tick -> "Tick"

let update (msg: Msg) (model: Model) =
    match msg with
    | OidcMsg m ->
        JS.console.log("OIDC msg:", logMsg m)
        let m', c = api.update getUserInfo m model.oidc
        JS.console.log("OIDC model:", m')
        { model with oidc = m' }, Cmd.map OidcMsg c

let subscribe (model: Model) =
    api.subscribe model.oidc |> Sub.map "oidc" OidcMsg

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
