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
        (Rgba32[,] image, long oriFileSize) = InputHandler.GetImage();
        int errorMethod = InputHandler.GetErrorMethod();
        float targetCompression = InputHandler.GetTargetCompression();

        int minimumBlock = 1;
        double threshold = 0;
        if (targetCompression == 0){
            threshold = InputHandler.GetThreshold();
            minimumBlock = InputHandler.GetMinimumBlock();
        }
        string imageOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gambar hasil kompresi: ", InputHandler.ImageExtensionType);
        string gifOutputPath = InputHandler.GetOutputPath("Masukkan alamat absolut gif hasil (.gif): ", ".gif");
        
        Console.Clear();
        InputHandler.ShowInputStatus();

        #endregion

        #region processing
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("   PROCESSING");
        Console.ResetColor();

        long startTimeImage;
        long endTimeImage;
        QuadtreeTree t;
        Rgba32[,] outputArray;

        if (targetCompression == 0){ // NO compression target
            // Image processing
            startTimeImage = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            t = new QuadtreeTree(image, image.GetLength(0), image.GetLength(1), minimumBlock, errorMethod, threshold);
            outputArray = t.CreateImage();

            endTimeImage = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        } else { // Compression target
            // Image Processing
            Console.WriteLine("Pemrosesan image akan lebih lama dengan fitur target compression");

            startTimeImage = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            // Binary Search get error 
            t = new QuadtreeTree(image, image.GetLength(0), image.GetLength(1), 1, errorMethod, threshold); // change threshold with the one got from binary search
            outputArray = t.CreateImage();

            endTimeImage = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
        // GIF processing
        QuadtreeArray ta = new QuadtreeArray(image, oriFileSize, InputHandler.getImageExtensionNum());
        ta.CreateGIFImages(t);
        
        long gifExecutionTime = 0;
        for (int i = 0; i < ta.ExecutionTimes.Count; i++){
            gifExecutionTime += ta.ExecutionTimes[i];
        }
        Console.WriteLine();

        #endregion

        #region outputs
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("   OUTPUT");
        Console.ResetColor();
        
        OutputHandler.SaveImage(imageOutputPath, outputArray);
        OutputHandler.SaveGIF(gifOutputPath, ta.Buffer);

        Console.WriteLine("Waktu eksekusi gambar: " + (endTimeImage - startTimeImage) + " ms");
        Console.WriteLine("Waktu eksekusi GIF: " + (gifExecutionTime) + " ms");
        Console.WriteLine("Kedalaman Pohon: " + t.maxDepth);
        Console.WriteLine("Jumlah Simpul: " + t.nodeCount);
        Console.WriteLine("Jumlah Daun: " + t.leafCount);

        Console.WriteLine("Ukuran file gambar sebelum kompresi: " + oriFileSize + " bytes");

        long compFileSize = new FileInfo(imageOutputPath).Length;
        Console.WriteLine("Ukuran file gambar setelah kompresi: " + compFileSize + " bytes");

        float compPercentage = (1 - (float) compFileSize / (float) oriFileSize) * 100f;
        if (targetCompression != 0){
            Console.WriteLine("Target kompresi: " + targetCompression + "%");
        }
        Console.WriteLine("Persentase kompresi: " + compPercentage + "%");

        #endregion
    }
}
