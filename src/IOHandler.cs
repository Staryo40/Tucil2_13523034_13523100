using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace IOHandler
{
    public static class InputHandler
    {
        public static string InputError = "";
        public static string InputAddress = "";
        public static string ErrorThresholdMethod = "";
        public static string ImageExtensionType = ""; 
        public static double ErrorThreshold = -1;
        public static int MinimumBlock = -1;
        public static float CompressionRateTarget = -1;
        public static string ImageOutputAddress = "";
        public static string GIFOutputAddress = "";

        public static (Rgba32[,], long) GetImage()
        {
            string? absolutePath;
            while (true)
            {
                Console.Clear();
                ShowInputStatus();
                if (InputError != ""){
                    ShowInputError();
                }

                Console.Write("Masukkan alamat absolut gambar yang akan dikompresi: ");
                absolutePath = Console.ReadLine()?.Trim();

                if (absolutePath == null)
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    InputError = "Input tidak valid, harap masukkan alamat yang valid.";
                    continue;
                }

                try
                {
                    if (!File.Exists(absolutePath))
                    {
                        // Console.WriteLine("Gambar tidak ditemukan!");
                        InputError = "Gambar tidak ditemukan!";
                        continue;
                    }

                    string extension = Path.GetExtension(absolutePath).ToLower();
                    if (extension != ".png" && extension == ".jpg" && extension == ".jpeg")
                    {
                        // Console.WriteLine("Format file tidak didukung! Harap gunakan .png, .jpg, atau .jpeg.");
                        InputError = "Format file tidak didukung! Harap gunakan .png, .jpg, atau .jpeg.";
                        continue;
                    }
                    ImageExtensionType = extension;

                    using (Image<Rgba32> image = Image.Load<Rgba32>(absolutePath))
                    {
                        int width = image.Width;
                        int height = image.Height;
                        Rgba32[,] pixelMatrix = new Rgba32[width, height];

                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                pixelMatrix[x, y] = image[x,y];
                            }
                        }

                        InputError = "";
                        InputAddress = absolutePath;

                        return (pixelMatrix, new FileInfo(absolutePath).Length);
                    }
                }
                catch
                {
                    Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    InputError = "Input tidak valid, harap masukkan alamat yang valid.";
                    continue;
                }
            }
        }

        public static int GetErrorMethod()
        {
            int value;
            while (true)
            {
                Console.Clear();
                ShowInputStatus();

                Console.WriteLine("1. Variance");
                Console.WriteLine("2. Mean Absolute Deviation (MAD)");
                Console.WriteLine("3. Max Pixel Difference");
                Console.WriteLine("4. Entropy");
                Console.WriteLine("5. Structural Similarity Index (SSIM)");
                Console.WriteLine("");

                if (InputError != ""){
                    ShowInputError();
                }
                Console.Write("Pilih metode pengukuran error yang diinginkan [1-5]: ");
                string? input = Console.ReadLine()?.Trim();
                
                if (input == null)
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan angka yang valid.");
                    InputError = "Input tidak valid, harap masukkan angka yang valid.";
                    continue;
                }

                try
                {
                    value = Convert.ToInt32(input);

                    switch (value)
                    {
                        case 1: ErrorThresholdMethod = "Variance"; break;
                        case 2: ErrorThresholdMethod = "Mean Absolute Deviation (MAD)"; break;
                        case 3: ErrorThresholdMethod = "Max Pixel Difference"; break;
                        case 4: ErrorThresholdMethod = "Entropy"; break;
                        case 5: ErrorThresholdMethod = "Structural Similarity Index (SSIM)"; break;
                        default:
                            InputError = "Input tidak valid, harap masukkan angka bulat 1-5.";
                            continue;
                    }

                    InputError = "";
                    return value;
                }
                catch
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    InputError = "Input tidak valid, harap masukkan alamat yang valid.";
                    continue;
                }
            }
        }

        public static double GetThreshold()
        {
            double value;

            while (true)
            {
                Console.Clear();
                ShowInputStatus();
                if (InputError != ""){
                    ShowInputError();
                }

                Console.Write("Masukkan ambang atas error: ");
                string? input = Console.ReadLine()?.Trim();
                
                if (input == null)
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan angka yang valid.");
                    InputError = "Input tidak valid, harap masukkan angka yang valid.";
                    continue;
                }

                try
                {
                    value = Convert.ToDouble(input);

                    if (value < 0)
                    {
                        // Console.WriteLine("Input tidak valid, harap masukkan angka >= 0.");
                        InputError = "Input tidak valid, harap masukkan angka >= 0.";
                        continue;
                    }

                    ErrorThreshold = value;
                    InputError = "";
                    
                    return value;
                }
                catch
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    InputError = "Input tidak valid, harap masukkan alamat yang valid.";
                    continue;
                }
            }
        }
    
        public static int GetMinimumBlock()
        {
            int value;

            while (true)
            {
                Console.Clear();
                ShowInputStatus();
                if (InputError != ""){
                    ShowInputError();
                }

                Console.Write("Masukkan ukuran blok minimum: ");
                string? input = Console.ReadLine()?.Trim();
                
                if (input == null)
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan angka yang valid.");
                    InputError = "Input tidak valid, harap masukkan angka yang valid.";
                    continue;
                }

                try
                {
                    value = Convert.ToInt32(input);

                    MinimumBlock = value;
                    InputError = "";

                    return value;
                }
                catch
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    InputError = "Input tidak valid, harap masukkan alamat yang valid.";
                    continue;
                }
            }
        }

        public static float GetTargetCompression()
        {
            float value;

            while (true)
            {
                Console.Clear();
                ShowInputStatus();
                if (InputError != ""){
                    ShowInputError();
                }

                Console.Write("Masukkan target persentase kompresi (0 jika tidak ada target) [0-1]: ");
                string? input = Console.ReadLine()?.Trim();
                
                if (input == null)
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan angka yang valid.");
                    InputError = "Input tidak valid, harap masukkan angka yang valid.";
                    continue;
                }

                try
                {
                    value = Convert.ToSingle(input);

                    if (value < 0 || value > 1)
                    {
                        // Console.WriteLine("Input tidak valid, harap masukkan angka 0-1.");
                        InputError = "Input tidak valid, harap masukkan angka 0-1.";
                        continue;
                    }

                    InputError = "";
                    CompressionRateTarget = value;

                    return value;
                }
                catch
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    InputError = "Input tidak valid, harap masukkan alamat yang valid.";
                    continue;
                }
            }
        }

        public static string GetOutputPath(string message, string extension)
        {
            string? absolutePath;
            while (true)
            {
                Console.Clear();
                ShowInputStatus();
                if (InputError != ""){
                    ShowInputError();
                }

                Console.Write(message);
                absolutePath = Console.ReadLine();

                try
                {
                    if (absolutePath == null)
                    {
                        // Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                        InputError = "Input tidak valid, harap masukkan alamat yang valid.";
                        continue;
                    }

                    if (!Path.HasExtension(absolutePath))
                    {
                        absolutePath += extension;
                    }
                    else if (!absolutePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        // Console.WriteLine("Ekstensi file tidak sesuai!");
                        InputError = "Ekstensi file tidak sesuai!";
                        continue;
                    }

                    string? directory = Path.GetDirectoryName(absolutePath);

                    if (directory == null || !Directory.Exists(directory))
                    {
                        // Console.WriteLine("Direktori tidak ditemukan!");
                        InputError = "Direktori tidak ditemukan!";
                        continue;
                    }

                    if (File.Exists(absolutePath))
                    {
                        Console.Write("File sudah ada! Apakah ingin menggantinya (y/n)? ");
                        string? response = Console.ReadLine()?.Trim().ToLower();

                        if (response == "y")
                        {
                            continue;
                        }
                    }

                    InputError = "";
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png"){
                        ImageOutputAddress = absolutePath;
                    } else if (extension == ".gif"){
                        GIFOutputAddress = absolutePath;
                    }

                    return absolutePath;
                }
                catch
                {
                    // Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    InputError = "Input tidak valid, harap masukkan alamat yang valid.";
                    continue;
                }
            }
        }

        public static void ShowInputStatus()
        {
            // string header = " STATUS INPUT ";
            // int sidePadding = (Console.WindowWidth - header.Length) / 4;
            // string line = new string('=', sidePadding) + header + new string('=', sidePadding);
            // Console.WriteLine(line);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("   STATUS INPUT");
            Console.ResetColor();

            Console.WriteLine($"1. Alamat gambar     : {(string.IsNullOrEmpty(InputAddress) ? "[belum diisi]" : InputAddress)}");
            Console.WriteLine($"2. Metode error      : {(string.IsNullOrEmpty(ErrorThresholdMethod) ? "[belum diisi]" : ErrorThresholdMethod)}");
            Console.WriteLine($"3. Threshold error   : {(ErrorThreshold < 0 ? "[belum diisi]" : ErrorThreshold.ToString())}");
            Console.WriteLine($"4. Minimum block     : {(MinimumBlock < 0 ? "[belum diisi]" : MinimumBlock.ToString())}");
            Console.WriteLine($"5. Target kompresi   : {(CompressionRateTarget < 0 ? "[belum diisi]" : CompressionRateTarget.ToString())}");
            Console.WriteLine($"6. Output image path : {(string.IsNullOrEmpty(ImageOutputAddress) ? "[belum diisi]" : ImageOutputAddress)}");
            Console.WriteLine($"7. Output GIF path   : {(string.IsNullOrEmpty(GIFOutputAddress) ? "[belum diisi]" : GIFOutputAddress)}");
            Console.WriteLine();
        }


        public static void ShowInputError()
        {
            Console.WriteLine(InputError);
        }
    }

    public static class OutputHandler
    {
        public static void SaveImage(string outputPath, Rgba32[,] pixelMatrix)
        {
            using (Image<Rgba32> image = new Image<Rgba32>(pixelMatrix.GetLength(0), pixelMatrix.GetLength(1)))
            {
                for (int x = 0; x < pixelMatrix.GetLength(0); x++)
                {
                    for (int y = 0; y < pixelMatrix.GetLength(1); y++)
                    {
                        image[x, y] = pixelMatrix[x, y];
                    }
                }

                image.Save(outputPath);
            }
        }

        public static void SaveGIF(string outputPath, List<Rgba32[,]> frameMatrices, int frameDelay = 50, int repeatCount = 0)
        {
            if (frameMatrices == null || frameMatrices.Count == 0)
                throw new ArgumentException("Frame list is empty.");

            using var gif = MatrixToImage(frameMatrices[0]);

            for (int i = 1; i < frameMatrices.Count; i++)
            {
                using var frameImage = MatrixToImage(frameMatrices[i]);
                gif.Frames.AddFrame(frameImage.Frames.RootFrame);
            }

            foreach (var frame in gif.Frames)
            {
                frame.Metadata.GetGifMetadata().FrameDelay = frameDelay; 
            }

            gif.Metadata.GetGifMetadata().RepeatCount = (ushort)repeatCount; 
            gif.Save(outputPath);
        }

        private static Image<Rgba32> MatrixToImage(Rgba32[,] matrix)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            var image = new Image<Rgba32>(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    image[x, y] = matrix[x, y];
                }
            }

            return image;
        }

    }
}