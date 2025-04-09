using System.ComponentModel.DataAnnotations;
using IOHandler;
using Quadtree;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


class Program
{
    static void Main()
    {
        #region inputs
        // Console.WriteLine("Image Dimension: " + image.GetLength(0) + "x" + image.GetLength(1));
        // Console.WriteLine("Image Area: " + (image.GetLength(0) * image.GetLength(1)));

        (Rgba32[,] image, long oriFileSize) = InputHandler.GetImage();

        Console.Clear();
        InputHandler.ShowInputStatus();

        // Start looping progress bar animation
        CancellationTokenSource cts = new CancellationTokenSource();
        var loadingTask = Task.Run(() => InputHandler.ShowLoopingProgressBar("Memproses input", cts.Token));

        QuadtreeArray minMax = new QuadtreeArray(image, oriFileSize, 2);
        minMax.CreateMinMaxImages();

        // Stop animation
        cts.Cancel();
        loadingTask.Wait();

        // Set thresholds
        InputHandler.MinTargetThreshold = minMax.CompressionRates[0];
        InputHandler.MaxTargetThreshold = minMax.CompressionRates[1];

        Console.WriteLine(); 

        int errorMethod = InputHandler.GetErrorMethod();
        double threshold = InputHandler.GetThreshold();
        int minimumBlock = InputHandler.GetMinimumBlock();
        float targetCompression = InputHandler.GetTargetCompression();
        string imageOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gambar hasil kompresi: ", InputHandler.ImageExtensionType);
        string gifOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gif hasil (.gif): ", ".gif");
        
        Console.Clear();
        InputHandler.ShowInputStatus();

        #endregion

        #region processing
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("   PROCESSING");
        Console.ResetColor();

        // GIF Processing
        // QuadtreeArray ta = new QuadtreeArray(image, oriFileSize, 2);
        // ta.CreateGIFImages();
        
        // long gifExecutionTime = 0;
        // for (int i = 0; i < ta.ExecutionTimes.Count; i++){
        //     gifExecutionTime += ta.ExecutionTimes[i];
        // }

        // Image Processing
        long startTimeImage = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        QuadtreeTree t = new QuadtreeTree(image, image.GetLength(0), image.GetLength(1), minimumBlock, errorMethod, threshold);
        Rgba32[,] outputArray = t.CreateImageAtDepth(11);

        long endTimeImage = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        #endregion

        #region outputs
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("   OUTPUT");
        Console.ResetColor();

        Console.WriteLine("Waktu eksekusi: " + (endTimeImage - startTimeImage) + " ms");
        // string imageOutputPath = @"C:\Users\Aryo\PersonalMade\ITB Kuliah Semesteran\Semester 4\Strategi Algoritma\Tucil-Tubes 2025\Tucil2_13523034_13523100\src\output.jpg";
        // string gifOutputPath = @"C:\Users\Aryo\PersonalMade\ITB Kuliah Semesteran\Semester 4\Strategi Algoritma\Tucil-Tubes 2025\Tucil2_13523034_13523100\src\output.gif";

        OutputHandler.SaveImage(imageOutputPath, outputArray);
        // OutputHandler.SaveGIF(gifOutputPath, ta.Buffer);

        Console.WriteLine("Kedalaman Pohon: " + t.maxDepth);
        Console.WriteLine("Jumlah Simpul: " + t.nodeCount);
        Console.WriteLine("Jumlah Daun: " + t.leafCount);

        Console.WriteLine("Ukuran file gambar sebelum kompresi: " + oriFileSize + " bytes");

        long compFileSize = new FileInfo(imageOutputPath).Length;
        Console.WriteLine("Ukuran file gambar setelah kompresi: " + compFileSize + " bytes");

        float compPercentage = (1 - (float) compFileSize / (float) oriFileSize) * 100f;
        Console.WriteLine("Persentase kompresi: " + compPercentage + "%");

        // Console.WriteLine("Waktu eksekusi buat GIF: " + gifExecutionTime + " ms");
        // for (int i = 0; i < ta.CompressionRates.Count; i++)
        // {
        //     Console.WriteLine($"compression {i} = {ta.CompressionRates[i]}");
        // }

        #endregion
    }
}
