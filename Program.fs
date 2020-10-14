open Argu
open ImgToSvg

type CliArguments =
  | Input of path:string
  | Output of path:string
  | FableReact
  | PixelSize of pixels:int
  | IgnoreColor of rgba:string
  
  
  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Input _ -> "Input bitmap image"
      | Output _ -> "Output SVG file"
      | FableReact -> "Output using Fable React SVG"
      | PixelSize _ -> "The size of the rectangles used to represent pixels, defaults to 2"
      | IgnoreColor _ -> "A comma delimited rgb / rgba list (e.g. 200,0,10 or 200,0,10,200) - this colour will not be translated into pixels"

open Implementation

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<CliArguments>(programName = "img2svg.exe")
    let arguments = parser.Parse argv
    let pixelSize = arguments.GetResult(PixelSize, defaultValue = 2)
    let format = if arguments.Contains FableReact then FableReactSvg else Svg
    let ignoreColor = arguments.TryGetResult(IgnoreColor)
    
    match arguments.TryGetResult Input, arguments.TryGetResult Output with
    | Some input, Some output ->
      match convert input pixelSize format ignoreColor with
      | Ok content ->
        System.IO.File.WriteAllText (output,content)
        printf "Image converted"
        0
      | Error msg ->
        printf "%s" msg
        -1
    | _ ->
      printf "%s" (parser.PrintUsage ())
      -2
