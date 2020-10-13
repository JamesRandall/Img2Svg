# Img2Svg

Converts a bitmap image to pixelised SVG (with the option to use the F# Fable React format). What do I mean by pixelised - well it scans the image looking at each pixel and converts that to a rectangle of a given size so you end up with output made up of lots of little squares that, unless scaled, looks like the original image.

Weird right... may not be of much use to people but I wanted to convert some vector art into independent pixels that I could move around using JavaScript. Tested on limited use cases so far.

If its useful to you great. Comes with no promise of support or acceptance of PR or anything. I may or may not look at an issue. Take it or leave it.

Compile the solution in Rider, VS Code, Visual Studio etc. and you'll get an exe dropped out called Img2Svg. Run it and it will give you the parameters:

    USAGE: img2svg.exe [--help] [--input <string>] [--output <string>] [--fablereact] [--pixelsize <int>]

    OPTIONS:

        --input <string>      Input bitmap image
        --output <string>     Output SVG file
        --fablereact          Output using Fable React SVG
        --pixelsize <int>     The size of the rectangles used to represent pixels, defaults to 2
        --help                display this list of options.

## Credits

Massive thanks to SixLabors for the [ImageSharp](https://github.com/SixLabors/ImageSharp) library that I use to do the image processing.