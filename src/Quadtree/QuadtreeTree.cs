using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Quadtree{
    class QuadtreeTree
    {
        private Rgba32[,] image;
        private QuadtreeNode root;
        public QuadtreeTree(Rgba32[,] i, int width, int height){
            image = i;
            Console.WriteLine("ran 0");
            root = new QuadtreeNode(image, (0, 0, width, height), 0, null);
            Console.WriteLine("ran 1");
            buildTree(root, 10, 0.5);
            Console.WriteLine("ran 2");
        }

        public void buildTree(QuadtreeNode r, int maxDepth, double errorThreshold){
            if (r.Depth >= maxDepth || r.errorVariance() <= errorThreshold){
                 if (r.Depth > maxDepth){
                    maxDepth = r.Depth;
                 }
                    
                r.IsLeaf = true;
                return;
            }
            
            r.split();

            (QuadtreeNode topLeft, QuadtreeNode topRight, QuadtreeNode bottomLeft, QuadtreeNode bottomRight) = r.Children.Value;
            buildTree(topLeft, maxDepth, errorThreshold);
            buildTree(topRight, maxDepth, errorThreshold);
            buildTree(bottomLeft, maxDepth, errorThreshold);
            buildTree(bottomRight, maxDepth, errorThreshold);
        }

        public Image<Rgba32> CreateImageFromDepth(int depth)
        {
            float m = 1; // This constant should be defined (like in Python)
            int dx = 0, dy = 0;  // Padding for each image section
            int imageWidth = (int)(root.Width * m + dx);
            int imageHeight = (int)(root.Height * m + dy);

            var image = new Image<Rgba32>(imageWidth, imageHeight);
            image.Mutate(ctx => ctx.Fill(Color.Black));  // Background color (black)

            var leafNodes = GetLeafNodesAtDepth(depth);

            foreach (var node in leafNodes)
            {
                var (l, t, r, b) = node.BoxBorder;
                var box = new Rectangle(
                    (int)(l * m + dx),
                    (int)(t * m + dy),
                    (int)((r - l) * m - 1),
                    (int)((b - t) * m - 1)
                );

                if (node.NodeImage != null)
                {
                    Rgba32 avgColor = ComputeAverageColor(node.NodeImage);
                    image.Mutate(ctx => ctx.Fill(avgColor, box));  // Fill with computed color
                }
            }

            return image;
        }

        private List<QuadtreeNode> GetLeafNodesAtDepth(int depth)
        {
            List<QuadtreeNode> leafNodes = new List<QuadtreeNode>();
            // Recursively traverse the tree and collect leaf nodes
            CollectLeafNodesAtDepth(root, depth, leafNodes);
            return leafNodes;
        }

        // Recursively collect leaf nodes at the specified depth
        private void CollectLeafNodesAtDepth(QuadtreeNode node, int depth, List<QuadtreeNode> leafNodes)
        {
            // If it's a leaf or has reached the given depth, add it to the list
            if (node.IsLeaf || node.Depth == depth)
            {
                leafNodes.Add(node);
            }
            else if (node.Children != null)
            {
                // Recursively process each child
                (QuadtreeNode topLeft, QuadtreeNode topRight, QuadtreeNode bottomLeft, QuadtreeNode bottomRight) = node.Children.Value;
                CollectLeafNodesAtDepth(topLeft, depth, leafNodes);
                CollectLeafNodesAtDepth(topRight, depth, leafNodes);
                CollectLeafNodesAtDepth(bottomLeft, depth, leafNodes);
                CollectLeafNodesAtDepth(bottomRight, depth, leafNodes);
            }
        }


        private Rgba32 ComputeAverageColor(Rgba32[,] image)
        {
            long sumR = 0, sumG = 0, sumB = 0;
            int height = image.GetLength(0);
            int width = image.GetLength(1);
            int pixelCount = width * height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = image[y, x];
                    sumR += pixel.R;
                    sumG += pixel.G;
                    sumB += pixel.B;
                }
            }

            return new Rgba32(
                (byte)(sumR / pixelCount),
                (byte)(sumG / pixelCount),
                (byte)(sumB / pixelCount)
            );
        }


    }
}
