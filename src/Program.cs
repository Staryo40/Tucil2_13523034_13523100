using System.ComponentModel.DataAnnotations;
using IOHandler;
using Quadtree;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


class Program
{
    const int MAX_SEARCH = 10;
    const float TARGET_RANGE = 0.01f;
    const double MAX_POSSIBLE_VARIANCE = 16256.25;
    const double MAX_POSSIBLE_MAD = 63.75;
    const double MAX_POSSIBLE_MVP = 255;
    const double MAX_POSSIBLE_ENTROPY = 8;
    const double MAX_POSSIBLE_SSIM = 1;

    static void Main()
    {
        #region inputs
        (Rgba32[,] image, long oriFileSize) = InputHandler.GetImage();
        int errorMethod = InputHandler.GetErrorMethod();
        float targetCompression = InputHandler.GetTargetCompression();

        int minimumBlock = 1;
        double threshold = 10;
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
        QuadtreeTree t = null!;
        Rgba32[,] outputArray = null!;

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

            int dimensions = image.GetLength(0) * image.GetLength(1);

            // Binary search
            for (minimumBlock = (int) (dimensions * 0.0001); minimumBlock >= 1; minimumBlock /= 4) {
                double leftBound = 0;
                double rightBound = errorMethod switch {
                    1 => MAX_POSSIBLE_VARIANCE,
                    2 => MAX_POSSIBLE_MAD,
                    3 => MAX_POSSIBLE_MVP,
                    4 => MAX_POSSIBLE_ENTROPY,
                    5 => MAX_POSSIBLE_SSIM,
                    _ => MAX_POSSIBLE_VARIANCE
                };

                threshold = leftBound + (rightBound - leftBound) / 2;
                t = new QuadtreeTree(image, image.GetLength(0), image.GetLength(1), minimumBlock, errorMethod, threshold);

                float tempPercentage;
                bool alwaysDecrease = true;
                int itr = 0;
                while (true)
                {
                    outputArray = t.CreateImage();
                    OutputHandler.SaveImage(imageOutputPath, outputArray);
                    long tempSize = new FileInfo(imageOutputPath).Length;
                    tempPercentage = (1 - (float) tempSize / (float) oriFileSize);

                    if (MathF.Abs(tempPercentage - targetCompression) < TARGET_RANGE || itr >= 10)
                        break;
                    else if (targetCompression > tempPercentage) {
                        leftBound = threshold;
                        alwaysDecrease = false;
                    }
                    else
                        rightBound = threshold;

                    threshold = leftBound + (rightBound - leftBound) / 2;
                    t.updateThreshold(threshold);

                    itr++;
                }

                if (!alwaysDecrease) break;
                if (MathF.Abs(tempPercentage - targetCompression) < TARGET_RANGE) break;
            }
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
        if (targetCompression != 0) {
            Console.WriteLine("Ambang atas error: " + threshold);
            Console.WriteLine("Ukuran blok minimum: " + minimumBlock);
        }
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
