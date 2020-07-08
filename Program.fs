namespace MapTiles

open SixLabors
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing

open System
open System.Drawing
open System.Linq
open PerlinNoise

type Terrain = Grass | Water | DeepWater | Coast | Shrub | Forest | Desert
type Config = Standard | BigTerrain | ZoomedTerrain

module Main =

    let DEFAULT_SEED = 2882
    let CANVAS_WIDTH = 2048
    let CANVAS_HEIGHT = 1024

    let OUTPUT_FILENAME = "output.png"

    let GetConfigHardCodedValues config =
        let mapWidth, tileWidth, zoom =
            match config with
                | Config.Standard -> 100, 10, 0.2
                | Config.BigTerrain -> 500, 2, 0.5
                | Config.ZoomedTerrain -> 200, 5, 0.5
        (mapWidth, tileWidth, zoom)

    let GetSeed (argv : string array) =
        if (argv.Count() = 0)
        then
            DEFAULT_SEED
        else
            let mutable parsedInt = 0
            let success = Int32.TryParse(argv.[0], &parsedInt)
            if success
            then
                parsedInt
            else
                DEFAULT_SEED

    [<EntryPoint>]
    let main argv =

        let (mapWidth, tileWidth, zoom) =
            GetConfigHardCodedValues Config.ZoomedTerrain

        let tileHeight = tileWidth / 2

        let masterSeed = GetSeed argv

        let temperatureOffset = 0.0

        let terrainStructArray =
            TerrainGeneration.Run
                mapWidth
                masterSeed
                zoom

        use image =
            Painting.PaintWorld
                terrainStructArray
                mapWidth
                tileWidth
                tileHeight
                temperatureOffset
                CANVAS_WIDTH
                CANVAS_HEIGHT

        image.Save(OUTPUT_FILENAME)
            
        0
