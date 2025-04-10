using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Quadtree{
    class QuadtreeTree
    {
        public Rgba32[,] Image { get; private set; }
        private QuadtreeNode root;
        public int minimumBlock { get; private set; }
        public int thresholdMethod { get; private set; }    // 1 = Variance
                                                            // 2 = Mean Absolute Deviation (MAD)
                                                            // 3 = Max Pixel Difference
                                                            // 4 = Entropy
                                                            // 5 = Structural Similarity Index (SSIM)
        public double errorThreshold { get; private set; }
        public int nodeCount { get; set; }
        public int leafCount { get; set; }
        public int maxDepth { get; set; }
        public List<QuadtreeNode> leafNodes { get; set; }
        
        public QuadtreeTree(Rgba32[,] i, int width, int height, int mb, int tm, double t){
            // User-defined Constructor
            this.Image = i;
            this.root = new QuadtreeNode(this, (0, 0), 0, width, height, null);
            this.minimumBlock = mb;
            this.thresholdMethod = tm;
            this.errorThreshold = t;
            this.nodeCount = 1;
            this.leafCount = 0;
            this.maxDepth = 0;
            this.leafNodes = new List<QuadtreeNode>();

            buildTree(root);
        }

        // Helper functions to get certain pixels from the original image
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
            // Procedure to build Quadtree with the parameters given for image compression
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

        public void updateThreshold(double t){
            double prevThreshold = errorThreshold;
            errorThreshold = t;
            updateTree(root, prevThreshold);
        }

        public void updateTree(QuadtreeNode r, double prevThreshold)
        {
            // Procedure to update Quadtree with the parameters given for image compression
            if (this.errorThreshold > prevThreshold)
            {
                if (r.IsLeaf || r.Children == null)
                {
                    r.IsLeaf = false;
                    
                    buildTree(r);
                }
                else
                {
                    (QuadtreeNode topLeft, QuadtreeNode topRight, QuadtreeNode bottomLeft, QuadtreeNode bottomRight) = r.Children.Value;
                    updateTree(topLeft, prevThreshold);
                    updateTree(topRight, prevThreshold);
                    updateTree(bottomLeft, prevThreshold);
                    updateTree(bottomRight, prevThreshold);
                }
            }
            else
            {
                if (r.IsLeaf || r.Children == null)
                {
                    return;
                }
                else
                {
                    r.merge();

                    if (r.IsLeaf || r.Children == null){
                        return;
                    }

                    (QuadtreeNode topLeft, QuadtreeNode topRight, QuadtreeNode bottomLeft, QuadtreeNode bottomRight) = r.Children.Value;
                    updateTree(topLeft, prevThreshold);
                    updateTree(topRight, prevThreshold);
                    updateTree(bottomLeft, prevThreshold);
                    updateTree(bottomRight, prevThreshold);
                }
            }
        }

        public Rgba32[,] CreateImage()
        {
            // Function that creates the compressed image based on the Quadtree that is built
            float m = 1;  // scaling
            int dx = 0, dy = 0; // padding

            int imageWidth = (int)(root.Width * m + dx);
            int imageHeight = (int)(root.Height * m + dy);

            var image = new Rgba32[imageWidth, imageHeight];

            // var leafNodes2 = GetLeafNodesAtDepth(this.maxDepth);

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

        public Rgba32[,] CreateImageAtDepth(int n)
        {
            // Function that creates the compressed image based on the Quadtree that is built
            float m = 1;  // scaling
            int dx = 0, dy = 0; // padding

            int imageWidth = (int)(root.Width * m + dx);
            int imageHeight = (int)(root.Height * m + dy);

            var image = new Rgba32[imageWidth, imageHeight];

            var leafNodesDepth = GetLeafNodesAtDepth(n);

            foreach (var node in leafNodesDepth)
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
            // MIGHT BECOME LEGACY
            // Function that returns the leaves of the tree up to a certain depth 
            List<QuadtreeNode> leafNodes = new List<QuadtreeNode>();
            CollectLeafNodesAtDepth(root, depth, leafNodes);
            return leafNodes;
        }

        private void CollectLeafNodesAtDepth(QuadtreeNode node, int depth, List<QuadtreeNode> leafNodes)
        {
            // Procedure to collect leaf nodes up to the specified depth
            if (node.Depth == depth || node.IsLeaf)
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
