using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Quadtree{
    class QuadtreeTree
    {
        public Rgba32[,] Image { get; private set; }
        private QuadtreeNode root;
        public QuadtreeTree(Rgba32[,] i, int width, int height){
            Image = i;
            root = new QuadtreeNode(this, (0, 0), 0, width, height, null);
            buildTree(root, 6, 0.001);
        }

        public Rgba32 GetPixel(int x, int y) => Image[x, y];
        public Rgba32 GetPixel(int x, int y, (int top, int left) offset) 
        {
            int newX = x + offset.left;
            int newY = y + offset.top;
            
            if (newX < 0 || newX >= Image.GetLength(0) || newY < 0 || newY >= Image.GetLength(1))
            {
                throw new IndexOutOfRangeException($"Attempted to access ({newX}, {newY}), but image is {Image.GetLength(0)}x{Image.GetLength(1)}");
            }

            return Image[newX, newY];
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
            float m = 1;  // scaling
            int dx = 0, dy = 0; // padding

            int imageWidth = (int)(root.Width * m + dx);
            int imageHeight = (int)(root.Height * m + dy);
            Console.WriteLine($"Image Dimensions: {imageWidth}x{imageHeight}");

            var image = new Image<Rgba32>(imageWidth, imageHeight);
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    image[x, y] = Color.Black;
                }
            }

            var leafNodes = GetLeafNodesAtDepth(depth);

            foreach (var node in leafNodes)
            {
                var (t, l, r, b) = node.GetBorders();
                var box = new Rectangle(
                    (int)(l * m + dx),
                    (int)(t * m + dy),
                    (int)((r - l) * m - 1),
                    (int)((b - t) * m - 1)
                );
                
                var (meanR, meanG, meanB) = node.colorMean();

                Rgba32 avgColor = new Rgba32((byte) meanR, (byte) meanG, (byte) meanB, 255);
                image.Mutate(ctx => ctx.Fill(avgColor, box));  
            }

            return image;
        }

        private List<QuadtreeNode> GetLeafNodesAtDepth(int depth)
        {
            List<QuadtreeNode> leafNodes = new List<QuadtreeNode>();
            CollectLeafNodesAtDepth(root, depth, leafNodes);
            return leafNodes;
        }

        // Recursively collect leaf nodes at the specified depth
        private void CollectLeafNodesAtDepth(QuadtreeNode node, int depth, List<QuadtreeNode> leafNodes)
        {
            if (node.IsLeaf || node.Depth == depth)
            {
                leafNodes.Add(node);
            }
            else if (node.Children != null)
            {
                (QuadtreeNode topLeft, QuadtreeNode topRight, QuadtreeNode bottomLeft, QuadtreeNode bottomRight) = node.Children.Value;
                CollectLeafNodesAtDepth(topLeft, depth, leafNodes);
                CollectLeafNodesAtDepth(topRight, depth, leafNodes);
                CollectLeafNodesAtDepth(bottomLeft, depth, leafNodes);
                CollectLeafNodesAtDepth(bottomRight, depth, leafNodes);
            }
        }
    }
}
