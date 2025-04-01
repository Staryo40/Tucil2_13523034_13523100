using IOHandler;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


class Program
{
    static void Main()
    {
        #region inputs
        Rgba32[,] image = InputHandler.GetImage();

        int errorMethod = InputHandler.GetErrorMethod();

        float treshold = InputHandler.GetTreshold();

        int minimumBlock = InputHandler.GetMinimumBlock();

        float targetCompression = InputHandler.GetTargetCompression();

        string imageOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gambar hasil kompresi (.png): ", ".png");

        string gifOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gif hasil (.gif): ", ".gif");
        #endregion

        #region processing
        long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        Rgba32[,] resultImage = image;

        long endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        #endregion

        #region outputs
        OutputHandler.SaveImage(imageOutputPath, resultImage);

        Console.WriteLine("Waktu eksekusi: " + (endTime - startTime) + " ms");
        #endregion
    }
}