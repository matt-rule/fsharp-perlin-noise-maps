// This code is a lightly modified version of the source found at http://fssnip.net/9i
// Credit for this Perlin Noise function goes to the original author.

module PerlinNoise
    open System
    open System.Drawing

    let NoiseFunction width height persistence octaves zoom seed bias amplitudeBias handler =
        let noise x y =
            let n = x + y * 57;
            let n = (n <<< 13) ^^^ n
            1.0 - (float ((n * (n * n * 15731 + 789221) + 1376312589) &&& 0x7fffffff)) / 1073741824.0
        let interpolate x y a =
            let f = (1.0 - (cos (a * Math.PI))) * 0.5
            x * (1.0 - f) + y * f
        let smoothedNoise x y =
            let xi = int x |> float
            let yi = int y |> float
            let xfrac = (x - xi) |> float
            let yfrac = (y - yi) |> float
            let xi = xi |> int
            let yi = yi |> int
            let v1 = noise xi yi
            let v2 = noise (xi + 1) yi
            let v3 = noise xi (yi + 1)
            let v4 = noise (xi + 1) (yi + 1)
            let s = interpolate v1 v2 xfrac
            let t = interpolate v3 v4 xfrac
            interpolate s t yfrac
        let pixel x y =
            seq { 0 .. octaves - 1 }
            |> Seq.sumBy (fun octave -> let octave = float octave
                                        let frequency = 0.0098976543 * (octave ** 2.0)
                                        let amplitude = (persistence ** octave) * 0.917183 * amplitudeBias
                                        smoothedNoise ((float (x + seed)) * frequency / zoom) ((float (y + seed)) * frequency / zoom) |> (*) amplitude)
        for x in 0 .. width - 1 do
            for y in 0 .. height - 1 do
                pixel x y |> (+) bias |> handler x y

    let Generate seed width height zoom =
        let noiseArray = Array2D.create width height 0
        let prng = Random(seed)
        (fun x y v ->
            let c = (v + 0.5) * 255.0 |> int |> min 255 |> max 0
            Array2D.set noiseArray x y c)
        |> NoiseFunction width height 0.5 8 zoom (prng.Next() % (Int32.MaxValue / 4)) 0.1368 0.96
        noiseArray