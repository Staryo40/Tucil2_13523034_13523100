using IOHandler;
using Quadtree;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


class Program
{
    static void Main()
    {
        // #region inputs
        (Rgba32[,] image, long oriFileSize) = InputHandler.GetImage();
        Console.WriteLine($"Image Dimensions in main: {image.GetLength(0)}x{image.GetLength(1)}");
        QuadtreeTree t = new QuadtreeTree(image, image.GetLength(0), image.GetLength(1));
        Image<Rgba32> output = t.CreateImageFromDepth(10);
        if (output == null)
        {
            throw new Exception("Error: Image creation failed, output is null.");
        }

        using (var stream = new MemoryStream())
        {
            output.SaveAsJpeg(stream);
            File.WriteAllBytes("output.jpg", stream.ToArray());
            Console.WriteLine($"MemoryStream Size: {stream.Length} bytes");
        }


        // int errorMethod = InputHandler.GetErrorMethod();

        // float treshold = InputHandler.GetTreshold();

        // int minimumBlock = InputHandler.GetMinimumBlock();

        // float targetCompression = InputHandler.GetTargetCompression();

        // string imageOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gambar hasil kompresi (.png): ", ".png");

        // string gifOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gif hasil (.gif): ", ".gif");
        // #endregion

        // #region processing
        // long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        // Rgba32[,] resultImage = image;

        // long endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        // #endregion

        // #region outputs
        // Console.WriteLine("Waktu eksekusi: " + (endTime - startTime) + " ms");

        // OutputHandler.SaveImage(imageOutputPath, resultImage);

        // Console.WriteLine("Ukuran file gambar sebelum kompresi: " + oriFileSize + " bytes");

        // long compFileSize = new FileInfo(imageOutputPath).Length;
        // Console.WriteLine("Ukuran file gambar setelah kompresi: " + compFileSize + " bytes");

        // float compPercentage = (float) compFileSize / (float) oriFileSize * 100f;
        // Console.WriteLine("Persentase kompresi: " + compPercentage + "%");


        // #endregion
    }
}
