namespace MapTiles

open SixLabors
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing

open System
open System.Drawing
open System.Linq
open PerlinNoise

module Painting =
    let rainfallGradient =
        [
            (55.0, (150.0, 150.0, 60.0))
            (145.0, (42.0, 120.0, 0.0))
            (205.0, (10.0, 63.0, 0.0))
        ]

    let Interp (x : float) (y : float) (n : float) =
        x * (1.0 - n) + y * n

    let rec RgbFromGradient (gradient : (float * (float * float * float)) list) pos index =
        let position1, (red1, green1, blue1) = gradient.[index]

        if pos < position1 || index = gradient.Length - 1
        then
            (red1, green1, blue1)
        else
            let position2, (red2, green2, blue2) = gradient.[index + 1]
            
            if pos < position2
            then
                let interpPos = ((pos - position1) / (position2 - position1))
            
                ((Interp red1 red2 interpPos), (Interp green1 green2 interpPos), (Interp blue1 blue2 interpPos))
            else
                RgbFromGradient gradient pos (index + 1)

    let CalculateForPolygon terrainStructArray x y (origin : Point) temperatureOffset tileWidth tileHeight =
        let geometry =
            [| Point(0,0); Point(-tileWidth,-tileHeight); Point(0,-tileWidth); Point(tileWidth,-tileHeight) |]
            |> Array.map (fun (p : Point) -> Point(x * tileWidth + (y * -tileWidth) + p.X + origin.X, (x + y) * -tileHeight + p.Y + origin.Y))
        
        let baseElevation = float (Array2D.get terrainStructArray x y).elevation
        let baseRainfall = float (Array2D.get terrainStructArray x y).rainfall
        let baseTemperature = float (Array2D.get terrainStructArray x y).temperature

        let isWater = baseElevation < 60.0
        let isSnow = (baseElevation - (baseTemperature + temperatureOffset)) / 2.0 > 50.0
        let elevationFoliageEffect = 255.0 - abs(baseElevation - 70.0)
        let foliageLevel = baseRainfall * 0.25 + elevationFoliageEffect * 0.75

        let red, green, blue =
            if isWater
            then
                let n = baseElevation / 60.0
                0.0, Interp 40.0 101.0 n, Interp 100.0 182.0 n
            else if isSnow
            then
                170.0, 220.0, 255.0
            else
                RgbFromGradient rainfallGradient foliageLevel 0

        let polygonToUse = [|
            ImageSharp.PointF(geometry.[0].X |> float32, geometry.[0].Y |> float32);
            ImageSharp.PointF(geometry.[1].X |> float32, geometry.[1].Y |> float32);
            ImageSharp.PointF(geometry.[2].X |> float32, geometry.[2].Y |> float32);
            ImageSharp.PointF(geometry.[3].X |> float32, geometry.[3].Y |> float32)
        |]

        let colourToUse = ImageSharp.Color(Rgba32(byte red, byte green, byte blue))

        (polygonToUse, colourToUse)

    let PaintWorld terrainStructArray mapWidth tileWidth tileHeight temperatureOffset canvasWidth canvasHeight =
        let image = new Image<Rgba32>(canvasWidth, canvasHeight)

        do image.Mutate(WorkaroundModule.DoFill ImageSharp.Color.Gray (ImageSharp.RectangleF(0.0f, 0.0f, (float32 canvasWidth), (float32 canvasHeight))))

        let panelCentre = Point (canvasWidth / 2, canvasHeight / 2);
        let origin = Point (panelCentre.X, panelCentre.Y + canvasHeight / 6 * 3)

        for x in [0..mapWidth-1] do
            for y in [0..mapWidth-1] do
                let (colourToUse, polygonToUse) =
                    CalculateForPolygon
                        terrainStructArray
                        x
                        y
                        origin
                        temperatureOffset
                        tileWidth
                        tileHeight
                        
                do image.Mutate(WorkaroundModule.DoPolygon polygonToUse colourToUse)

        image
