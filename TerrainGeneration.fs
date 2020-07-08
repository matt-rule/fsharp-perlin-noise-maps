namespace MapTiles

open System

module TerrainGeneration =

    // let getNeighbourPoints x y =
    //     [Point(-1,0); Point(1,0); Point(0,-1); Point(0,1);
    //         Point(-1,-1); Point(1,-1); Point(1,-1); Point(1,1)]
    //     |> List.map (fun p -> Point(p.X + x, p.Y + y))
    //     |> List.where (fun p -> p.X >= 0 && p.X < mapWidth && p.Y >= 0 && p.Y < mapWidth)

    // let getWaterCount x y =
    //     getNeighbourPoints x y
    //     |> List.where
    //         (fun p ->
    //             (Array2D.get terrainStructArray p.X p.Y).elevation < 60)
    //     |> List.length

    let Run mapWidth masterSeed zoom =
        
        let seedGenerator = Random(masterSeed)

        let noiseArrayElevation = PerlinNoise.Generate (seedGenerator.Next 65536) mapWidth mapWidth (zoom * 2.0)
        let noiseArrayRainfall = PerlinNoise.Generate (seedGenerator.Next 65536) mapWidth mapWidth zoom
        let noiseArrayTemperature = PerlinNoise.Generate (seedGenerator.Next 65536) mapWidth mapWidth (zoom * 2.0)

        let terrainStructArray = Array2D.create mapWidth mapWidth {rainfall=0; temperature=0; elevation=0}

        for x in [0..mapWidth-1] do
            for y in [0..mapWidth-1] do
                do Array2D.set terrainStructArray x y
                    {
                        Array2D.get terrainStructArray x y with
                            elevation = (Array2D.get noiseArrayElevation x y);
                            rainfall = (Array2D.get noiseArrayRainfall x y);
                            temperature = (Array2D.get noiseArrayTemperature x y)
                    }

        terrainStructArray

