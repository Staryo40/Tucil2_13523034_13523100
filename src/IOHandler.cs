using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace IOHandler
{
    public static class InputHandler
    {
        public static Rgba32[,] GetImage()
        {
            string? absolutePath;
            while (true)
            {
                Console.Write("Masukkan alamat absolut gambar yang akan dikompresi: ");
                absolutePath = Console.ReadLine()?.Trim();

                if (absolutePath == null)
                {
                    Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    continue;
                }

                try
                {
                    if (!File.Exists(absolutePath))
                    {
                        Console.WriteLine("Gambar tidak ditemukan!");
                    }

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
                        return pixelMatrix;
                    }
                }
                catch
                {
                    Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    continue;
                }
            }
        }

        public static int GetErrorMethod()
        {
            int value;
            while (true)
            {
                Console.WriteLine("1. Variance");
                Console.WriteLine("2. Mean Absolute Deviation (MAD)");
                Console.WriteLine("3. Max Pixel Difference");
                Console.WriteLine("4. Entropy");
                Console.WriteLine("5. Structural Similarity Index (SSIM)");

                Console.Write("Pilih metode pengukuran error yang diinginkan [1-5]: ");
                string? input = Console.ReadLine()?.Trim();
                
                if (input == null)
                {
                    Console.WriteLine("Input tidak valid, harap masukkan angka yang valid.");
                    continue;
                }

                try
                {
                    value = Convert.ToInt32(input);

                    if (value < 1 || value > 5)
                    {
                        Console.WriteLine("Input tidak valid, harap masukkan angka bulat 1-5.");
                        continue;
                    }

                    return value;
                }
                catch
                {
                    Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    continue;
                }
            }
        }

        public static float GetTreshold()
        {
            float value;

            while (true)
            {
                Console.Write("Masukkan ambang atas: ");
                string? input = Console.ReadLine()?.Trim();
                
                if (input == null)
                {
                    Console.WriteLine("Input tidak valid, harap masukkan angka yang valid.");
                    continue;
                }

                try
                {
                    value = Convert.ToSingle(input);

                    if (value < 0)
                    {
                        Console.WriteLine("Input tidak valid, harap masukkan angka >= 0.");
                        continue;
                    }

                    return value;
                }
                catch
                {
                    Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    continue;
                }
            }
        }
    
        public static int GetMinimumBlock()
        {
            int value;

            while (true)
            {
                Console.Write("Masukkan ukuran blok minimum: ");
                string? input = Console.ReadLine()?.Trim();
                
                if (input == null)
                {
                    Console.WriteLine("Input tidak valid, harap masukkan angka yang valid.");
                    continue;
                }

                try
                {
                    value = Convert.ToInt32(input);

                    return value;
                }
                catch
                {
                    Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    continue;
                }
            }
        }

        public static float GetTargetCompression()
        {
            float value;

            while (true)
            {
                Console.Write("Masukkan target persentase kompresi (0 jika tidak ada target) [0-1]: ");
                string? input = Console.ReadLine()?.Trim();
                
                if (input == null)
                {
                    Console.WriteLine("Input tidak valid, harap masukkan angka yang valid.");
                    continue;
                }

                try
                {
                    value = Convert.ToSingle(input);

                    if (value < 0 || value > 1)
                    {
                        Console.WriteLine("Input tidak valid, harap masukkan angka 0-1.");
                        continue;
                    }

                    return value;
                }
                catch
                {
                    Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    continue;
                }
            }
        }

        public static string GetOutputPath(string message, string extension)
        {
            string? absolutePath;
            while (true)
            {
                Console.Write(message);
                absolutePath = Console.ReadLine();

                try
                {
                    if (absolutePath == null)
                    {
                        Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                        continue;
                    }

                    string? directory = Path.GetDirectoryName(absolutePath);

                    if (directory == null || !Directory.Exists(directory))
                    {
                        Console.WriteLine("Direktori tidak ditemukan!");
                        continue;
                    }

                    if (!absolutePath.EndsWith(extension))
                    {
                        Console.WriteLine("Ekstensi file tidak sesuai!");
                        continue;
                    }

                    if (File.Exists(absolutePath))
                    {
                        Console.Write("File sudah ada! Apakah ingin menggantinya (y/n)? ");
                        string? response = Console.ReadLine()?.Trim().ToLower();

                        if (response != "y")
                        {
                            continue;
                        }
                    }

                    return absolutePath;
                }
                catch
                {
                    Console.WriteLine("Input tidak valid, harap masukkan alamat yang valid.");
                    continue;
                }
            }
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
    }
}