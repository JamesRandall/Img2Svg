module ImgToSvg.Implementation
open System.Text
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

let private loadImage (filename:string) =
  try
    let image:Image<Rgba32> = Image<Rgba32>.Load filename
    match image with
    | null -> Error "Unable to load image"
    | _ ->
      Ok image // image
  with
    ex -> Error (sprintf "Unable to load image: %s" ex.Message)

type OutputFormat =
  | Svg
  | FableReactSvg

module Formatting =
  type Color =
    { Red: byte
      Green: byte
      Blue: byte
      Alpha: byte
    }
    member c.Rgba = sprintf "rgba(%d,%d,%d,%d)" c.Red c.Green c.Blue c.Alpha
    static member FromCommaDelimited (csvColors:string) =
      let error = Error "Invalid colour format - must be r,g,b or r,g,b,a"
      try
        let components = csvColors.Split(',') |> Array.map(fun c -> c |> byte)
        match components.Length with
        | 3 -> Ok { Red = components.[0] ; Green = components.[1] ; Blue = components.[2] ; Alpha = 255uy }
        | 4 -> Ok { Red = components.[0] ; Green = components.[1] ; Blue = components.[2] ; Alpha = components.[3] }
        | _ -> error
      with
        _ -> error
                 
  type IOutputFormatter =
    abstract member Header: string
    // x,y,color -> string
    abstract member Pixel: (int * int * Color) -> string
    abstract member Footer: string
    
  type SvgFormatter =
    { Width: int
      Height: int
      PixelSize: int
    }
    interface IOutputFormatter with
      member f.Header =
        let outputWidth = f.Width * f.PixelSize
        let outputHeight = f.Height * f.PixelSize
        sprintf
          "<svg width=\"%d\" height=\"%d\" viewBox=\"0 0 %d %d\">"
          outputWidth
          outputHeight
          outputWidth
          outputHeight
      member f.Pixel ((x,y,color)) =
        sprintf
          "  <rect x=\"%d\" y=\"%d\" width=\"%d\" height=\"%d\" fill=\"%s\" />"
          (x*f.PixelSize) (y*f.PixelSize) f.PixelSize f.PixelSize color.Rgba
      member f.Footer = "</svg>"
  
  type FableReactFormatter =
    { Width: int
      Height: int
      PixelSize: int
    }
    interface IOutputFormatter with
      member f.Header =
        let outputWidth = f.Width * f.PixelSize
        let outputHeight = f.Height * f.PixelSize
        sprintf
          "svg [SVGAttr.Width %d ; SVGAttr.Height %d ; SVGAttr.ViewBox \"0 0 %d %d\"] [|"
          outputWidth
          outputHeight
          outputWidth
          outputHeight
      member f.Pixel ((x,y,color)) =
        sprintf
          "  rect [SVGAttr.X \"%d\" ; SVGAttr.Y \"%d\" ; SVGAttr.Width \"%d\" ; SVGAttr.Height \"%d\" ; SVGAttr.Fill \"%s\"][]"
          (x*f.PixelSize) (y*f.PixelSize) f.PixelSize f.PixelSize color.Rgba
      member f.Footer = "|]"
  
  let create format width height pixelSize =
    match format with
    | Svg ->
      let svgFormatter:SvgFormatter = { Width = width ; Height = height ; PixelSize = pixelSize  }
      svgFormatter :> IOutputFormatter
    | FableReactSvg ->
      let fableReactFormatter:FableReactFormatter = { Width = width ; Height = height ; PixelSize = pixelSize  }
      fableReactFormatter :> IOutputFormatter
    
let extractColors optionalIgnoreColor (image:Image<Rgba32>) =
  seq { for y=0 to image.Height-1 do for x=0 to image.Width-1 do (x,y) }
  |> Seq.map(fun (x,y) ->
    let pixel = image.[x,y]
    let color:Formatting.Color = { Red = pixel.R ; Green = pixel.G ; Blue = pixel.B ; Alpha = pixel.A }
    x,y,color
  )
  |> Seq.filter (fun (_,_,c) -> match optionalIgnoreColor with | Some ignoreColor -> ignoreColor <> c | None -> true)
 
let convert inputFilename pixelSize outputFormat optionalIgnoreCommaDelimitedColor =
  let imageResult = loadImage inputFilename
  match imageResult with
  | Ok image ->
    let formatter = Formatting.create outputFormat image.Width image.Height pixelSize
    let ignoreColorResult =
      match optionalIgnoreCommaDelimitedColor with
      | Some cdc ->
        Formatting.Color.FromCommaDelimited cdc |> Result.bind(fun c -> c |> Some |> Ok)
      | None -> Ok None     
    match ignoreColorResult with
    | Error msg -> Error msg
    | Ok ignoreColor ->        
      image
      |> extractColors (ignoreColor)
      |> Seq.fold (fun (builder:StringBuilder) xyColor ->
          builder.AppendLine(formatter.Pixel xyColor)
        ) (StringBuilder(formatter.Header))
      |> fun sb -> sb.AppendLine(formatter.Footer) |> ignore ; sb.ToString()
      |> Ok
  | Error message -> Error message