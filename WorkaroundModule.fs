namespace MapTiles

open SixLabors
open SixLabors.ImageSharp.Processing

module WorkaroundModule =
    open SixLabors.ImageSharp.Drawing.Processing

    let DoFill (col : ImageSharp.Color) (rect : ImageSharp.RectangleF) (y : IImageProcessingContext) =
        y.Fill(
                col,
                rect
        ) |> ignore
        ()

    let DoPolygon (col : ImageSharp.Color) (points : ImageSharp.PointF[]) (y : IImageProcessingContext) =
        y.FillPolygon(
                col,
                points
        ) |> ignore
        ()
