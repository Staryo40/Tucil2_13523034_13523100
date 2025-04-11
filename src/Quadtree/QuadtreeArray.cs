using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Quadtree{
    class QuadtreeArray{
        public Rgba32[,] OriginalImage { get; private set; } 
        public long OriginalSize { get; private set; }
        public int Extension {get; private set; }   // 1. PNG
                                                    // 2. JPG
        public List<Rgba32[,]> Buffer { get; private set; }
        public List<long> ExecutionTimes { get; private set; }
        public List<float> CompressionRates { get; private set; }
        public QuadtreeArray(Rgba32[,] i, long size, int extension){
            this.OriginalImage = i;
            this.OriginalSize = size;
            this.Extension = extension;
            this.Buffer = new List<Rgba32[,]>();
            this.ExecutionTimes = new List<long>();
            this.CompressionRates = new List<float>();
        }

        public void CreateGIFImages(QuadtreeTree t){
            for (int i = 0; i <= t.maxDepth; i++){
                long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                Rgba32[,] outputArray = t.CreateImageAtDepth(i);
                
                long endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                ExecutionTimes.Add(endTime-startTime);
                Buffer.Add(outputArray);

                int total = t.maxDepth + 1;
                int current = i + 1;

                // Generate the bar
                string bar = new string('■', current);
                Console.Write($"\rCurrently completed GIF images: [{bar.PadRight(total)}] {current}/{total}");
            }
            Console.Write("\r" + new string(' ', 100) + "\r");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("GIF processing finished!");
            Console.ResetColor();
            Console.WriteLine("");
        }

        public void CreateMinMaxImages(){
            int imageWidth = OriginalImage.GetLength(0);
            int imageHeight = OriginalImage.GetLength(1);
            int minimalBlock = (int)((imageWidth / Math.Pow(2, 11)) * (imageHeight / Math.Pow(2, 11)));

            QuadtreeTree t = new QuadtreeTree(OriginalImage, imageWidth, imageHeight, minimalBlock, 1, 0);
            Rgba32[,] outputArrayZero = t.CreateImageAtDepth(0);
            Rgba32[,] outputArrayEleven = t.CreateImageAtDepth(11);

            float compressionMax = 1 - ((float) GetExpectedFileSize(outputArrayZero, Extension) / (float) OriginalSize);
            float compressionMin = 1 - ((float) GetExpectedFileSize(outputArrayEleven, Extension) / (float) OriginalSize);
            Buffer.Add(outputArrayZero);
            Buffer.Add(outputArrayEleven);
            CompressionRates.Add(compressionMin);
            CompressionRates.Add(compressionMax);
            
            Console.WriteLine("");
        }
        public void CreateMinImage(){
            int imageWidth = OriginalImage.GetLength(0);
            int imageHeight = OriginalImage.GetLength(1);

            QuadtreeTree t = new QuadtreeTree(OriginalImage, imageWidth, imageHeight, 0, 1, 0);
            Rgba32[,] outputArrayZero = t.CreateImageAtDepth(0);

            float compressionMax = 1 - ((float) GetExpectedFileSize(outputArrayZero, Extension) / (float) OriginalSize);
            Buffer.Add(outputArrayZero);
            CompressionRates.Add(compressionMax);
            
            Console.WriteLine("");
        }
        public static long GetExpectedFileSize(Rgba32[,] pixelMatrix, int extension)
        {
            using var image = MatrixToImage(pixelMatrix);
            using var ms = new MemoryStream();

            switch (extension)
            {
                case 1: // PNG
                    image.Save(ms, new PngEncoder());
                    break;
                case 2: // JPG
                    image.Save(ms, new JpegEncoder()); 
                    break;
                default:
                    throw new ArgumentException("Unsupported extension value.");
            }

            return ms.Length;
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