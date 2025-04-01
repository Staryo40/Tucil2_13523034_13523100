using System;
using System.IO;
using Quadtree;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

class Program
{
    static void Main()
    {
        string imagePath = "CasPrice.png"; // Path to your .png file
        string outputImagePath = "ProcessedImage.png"; 
        
        try
        {
            // Load the image
            using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
            {
                int width = image.Width;
                int height = image.Height;

                Rgba32[,] colorMatrix = new Rgba32[width, height];

                Console.WriteLine($"Width: {width}");
                Console.WriteLine($"Height: {height}");

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        colorMatrix[x, y] = image[x, y]; 
                    }
                }
                QuadtreeTree quadtree = new QuadtreeTree(colorMatrix, width, height);
                Console.WriteLine("Hello 1");
                Image<Rgba32> processedImage = quadtree.CreateImageFromDepth(5); // Change depth as needed
                Console.WriteLine("Hello");
                // Save the processed image
                processedImage.Save(outputImagePath);
                Console.WriteLine($"Processed image saved to: {outputImagePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
