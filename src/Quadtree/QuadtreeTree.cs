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
        public double minimumBlock { get; private set; }
        public int thresholdMethod { get; private set; }    // 1 = Variance
                                                            // 2 = Mean Absolute Deviation (MAD)
                                                            // 3 = Max Pixel Difference
                                                            // 4 = Entropy
                                                            // 5 = Structural Similarity Index (SSIM)
        public double errorThreshold { get; private set; }
        public int nodeCount { get; set; }
        public int leafCount { get; set; }
        public int maxDepth { get; set; }
        public QuadtreeTree(Rgba32[,] i, int width, int height, double mb, int tm, double t){
            this.Image = i;
            this.root = new QuadtreeNode(this, (0, 0), 0, width, height, null);
            this.minimumBlock = mb;
            this.thresholdMethod = tm;
            this.errorThreshold = t;
            this.nodeCount = 1;
            this.leafCount = 0;
            this.maxDepth = 0;
            buildTree(root); // Compression parameters
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

        public void buildTree(QuadtreeNode r){
            r.split();

            if (r.IsLeaf || r.Children == null){
                return;
            }

            (QuadtreeNode topLeft, QuadtreeNode topRight, QuadtreeNode bottomLeft, QuadtreeNode bottomRight) = r.Children.Value;
            buildTree(topLeft);
            buildTree(topRight);
            buildTree(bottomLeft);
            buildTree(bottomRight);
        }

        public Rgba32[,] CreateImage()
        {
            float m = 1;  // scaling
            int dx = 0, dy = 0; // padding

            int imageWidth = (int)(root.Width * m + dx);
            int imageHeight = (int)(root.Height * m + dy);

            var image = new Rgba32[imageWidth, imageHeight];

            var leafNodes = GetLeafNodesAtDepth(this.maxDepth);

            foreach (var node in leafNodes)
            {
                var (t, l, r, b) = node.GetBorders();
                var(meanR, meanG, meanB) = node.colorMean();
                Rgba32 avgColor = new Rgba32((byte)meanR, (byte)meanG, (byte)meanB, 255);

                for (int y = t; y < b; y++)
                {
                    for (int x = l; x < r; x++)
                    {
                        if (x >= 0 && x < imageWidth && y >= 0 && y < imageHeight)
                        {
                            image[x, y] = avgColor;
                        }
                    }
                }
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
