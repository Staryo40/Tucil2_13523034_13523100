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

        double minimumBlock = 200;
        double threshold = 5;

        // int errorMethod = InputHandler.GetErrorMethod();

        // float treshold = InputHandler.GetTreshold();

        // int minimumBlock = InputHandler.GetMinimumBlock();

        // float targetCompression = InputHandler.GetTargetCompression();

        // string imageOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gambar hasil kompresi (.png): ", ".png");

        // string gifOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gif hasil (.gif): ", ".gif");
        // #endregion

        #region processing
        long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        QuadtreeTree t = new QuadtreeTree(image, image.GetLength(0), image.GetLength(1), minimumBlock, 1, threshold);

        Rgba32[,] outputArray = t.CreateImageFromDepth(10);
        if (outputArray == null)
        {
            throw new Exception("Error: Image creation failed, output is null.");
        }

        long endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        #endregion

        #region outputs
        Console.WriteLine("Waktu eksekusi: " + (endTime - startTime) + " ms");
        string imageOutputPath = @"C:\Users\Aryo\PersonalMade\ITB Kuliah Semesteran\Semester 4\Strategi Algoritma\Tucil-Tubes 2025\Tucil2_13523034_13523100\src\output.png";
        
        OutputHandler.SaveImage(imageOutputPath, outputArray);

        Console.WriteLine("Ukuran file gambar sebelum kompresi: " + oriFileSize + " bytes");

        long compFileSize = new FileInfo(imageOutputPath).Length;
        Console.WriteLine("Ukuran file gambar setelah kompresi: " + compFileSize + " bytes");

        float compPercentage = (float) compFileSize / (float) oriFileSize * 100f;
        Console.WriteLine("Persentase kompresi: " + compPercentage + "%");

        #endregion
    }
}
