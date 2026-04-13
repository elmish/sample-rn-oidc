module internal Styles

open Fable.ReactNative
open Fable.ReactNative.Props

let [<Literal>] brandPrimary = "#0078d4"
let [<Literal>] brandSecondary = "#e1e1e1"
let [<Literal>] dangerColor = "#d32f2f"
let [<Literal>] textColor = "#333333"
let [<Literal>] textLight = "#888888"
let [<Literal>] textWhite = "#FFFFFF"
let [<Literal>] backgroundColor = "#f5f5f5"
let [<Literal>] cardBackground = "#FFFFFF"

let [<Literal>] fontSizeBase = 15.
let [<Literal>] titleFontSize = 22.
let [<Literal>] subtitleFontSize = 17.
let [<Literal>] detailFontSize = 13.
let [<Literal>] borderRadius = 8.

let sceneBackground<'a> =
    ViewProperties.Style [
        Flex 1.
        BackgroundColor backgroundColor
        JustifyContent JustifyContent.Center
        AlignItems ItemAlignment.Center
        Padding (dip 20.)
    ]

let card<'a> =
    ViewProperties.Style [
        BackgroundColor cardBackground
        BorderRadius borderRadius
        Padding (dip 24.)
        Width (pct 100.)
        MaxWidth (dip 400.)
        ShadowColor "#000"
        ShadowOpacity 0.1
        ShadowRadius 4.
        Elevation 3.
    ]

let titleText<'a> =
    TextProperties.Style [
        TextStyle.Color textColor
        FontSize titleFontSize
        FontWeight FontWeight.Bold
        TextAlign TextAlignment.Center
        MarginBottom (dip 12.)
    ]

let subtitleText<'a> =
    TextProperties.Style [
        TextStyle.Color textColor
        FontSize subtitleFontSize
        FontWeight FontWeight.Bold
        MarginBottom (dip 8.)
    ]

let bodyText<'a> =
    TextProperties.Style [
        TextStyle.Color textLight
        FontSize fontSizeBase
        TextAlign TextAlignment.Center
        MarginBottom (dip 16.)
    ]

let detailText<'a> =
    TextProperties.Style [
        TextStyle.Color textLight
        FontSize detailFontSize
        MarginBottom (dip 4.)
    ]

let userInfoContainer<'a> =
    ViewProperties.Style [
        MarginTop (dip 16.)
        MarginBottom (dip 20.)
    ]

let primaryBtnStyle<'a> =
    TouchableHighlightProperties.Style [
        BackgroundColor brandPrimary
        BorderRadius borderRadius
        Padding (dip 14.)
        AlignItems ItemAlignment.Center
    ]

let secondaryBtnStyle<'a> =
    TouchableHighlightProperties.Style [
        BackgroundColor brandSecondary
        BorderRadius borderRadius
        Padding (dip 14.)
        AlignItems ItemAlignment.Center
    ]

let primaryBtnText<'a> =
    TextProperties.Style [
        TextStyle.Color textWhite
        FontSize fontSizeBase
        FontWeight FontWeight.Bold
    ]

let secondaryBtnText<'a> =
    TextProperties.Style [
        TextStyle.Color textColor
        FontSize fontSizeBase
        FontWeight FontWeight.Bold
    ]

let loadingText<'a> =
    TextProperties.Style [
        TextStyle.Color textLight
        FontSize subtitleFontSize
        TextAlign TextAlignment.Center
    ]

let errorContainer<'a> =
    ViewProperties.Style [
        BackgroundColor cardBackground
        BorderRadius borderRadius
        Padding (dip 24.)
        Width (pct 100.)
        MaxWidth (dip 400.)
        BorderLeftWidth 4.
        BorderLeftColor dangerColor
    ]

let errorTitle<'a> =
    TextProperties.Style [
        TextStyle.Color dangerColor
        FontSize subtitleFontSize
        FontWeight FontWeight.Bold
        MarginBottom (dip 8.)
    ]
