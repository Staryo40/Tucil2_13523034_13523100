using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Quadtree{
    class QuadtreeNode
    {
        private QuadtreeTree tree;
        public (int, int) TopLeft { get; private set; } // top, left
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsLeaf;

        public (QuadtreeNode, QuadtreeNode, QuadtreeNode, QuadtreeNode)? Children; // topleft, topright, bottomleft, bottomright

        public QuadtreeNode(QuadtreeTree t){
            // Default constructor
            tree = t;
            Depth = 0;
            Children = null;
            Width = 0;
            Height = 0;  
            IsLeaf = false;
        }
        public QuadtreeNode(QuadtreeTree t, (int, int) tl, int d, int w, int h, (QuadtreeNode, QuadtreeNode, QuadtreeNode, QuadtreeNode)? c = null)
        {
            // User defined constructor
            tree = t;
            Depth = d;
            Children = c;

            var (top, left) = tl;
            TopLeft = (top, left);
            Width = w;
            Height = h;  
            IsLeaf = false;
        }

        public (int, int, int, int) GetBorders()
        {
            var (top, left) = TopLeft;
            var (right, bottom) = (left + Width, top + Height);
            return (top, left, right, bottom);
        }

        public void split(){
            // Procedure to split the node, giving four children to the node if conditions are met

            // Check if still above error threshold
            double errorValue = tree.thresholdMethod switch
            {
                1 => this.errorVariance(),
                2 => this.errorMAD(),
                3 => this.errorMaxPixDiff(),
                4 => this.errorEntropy(),
                5 => this.errorSSIM(),
                _ => this.errorVariance()
            };

            if (errorValue <= tree.errorThreshold)
            {
                this.IsLeaf = true;
                tree.leafNodes.Add(this);
                tree.leafCount += 1;
                return;
            }
            
            // Counting borders, width, and height of new children
            var (top, left, right, bottom) = GetBorders();
            int midHor = top + (bottom - top)/ 2;
            int midVer = left + (right - left) / 2;

            int widthLeft = (right - left) / 2;  
            int widthRight = (right - left) - widthLeft;  

            int heightTop = (bottom - top) / 2;  
            int heightBottom = (bottom - top) - heightTop;  

            // Check if still above minimum block size
            if (widthLeft * heightTop < tree.minimumBlock || widthLeft == 0 || widthRight == 0 || heightTop == 0 || heightBottom == 0){
                this.IsLeaf = true;
                tree.leafNodes.Add(this);
                tree.leafCount += 1;
                return;
            }
            
            QuadtreeNode topLeft = new QuadtreeNode(tree, (top, left), Depth + 1, widthLeft, heightTop, null);
            QuadtreeNode topRight = new QuadtreeNode(tree, (top, midVer), Depth + 1, widthRight, heightTop, null);
            QuadtreeNode bottomLeft = new QuadtreeNode(tree, (midHor, left), Depth + 1, widthLeft, heightBottom, null);
            QuadtreeNode bottomRight = new QuadtreeNode(tree, (midHor, midVer), Depth + 1, widthRight, heightBottom, null);

            tree.nodeCount += 4;

            if (Depth + 1 > tree.maxDepth){
                tree.maxDepth = Depth + 1;
            }

            Children = (topLeft, topRight, bottomLeft, bottomRight);
        }

        public double errorVariance(){
            // Error measurement using Variance of the three color channels
            double N = Width * Height;
            var (meanR, meanG, meanB) = colorMean();
            
            double varianceR = 0;
            double varianceG = 0;
            double varianceB = 0;
            
            for (int i = 0; i < Width; i++){
                for (int j = 0; j < Height; j++){
                    varianceR += (tree.GetPixel(i, j, TopLeft).R - meanR) * (tree.GetPixel(i, j, TopLeft).R - meanR);
                    varianceG += (tree.GetPixel(i, j, TopLeft).G - meanG) * (tree.GetPixel(i, j, TopLeft).G - meanG);
                    varianceB += (tree.GetPixel(i, j, TopLeft).B - meanB) * (tree.GetPixel(i, j, TopLeft).B - meanB);
                }
            }

            varianceR = varianceR / N;
            varianceG = varianceG / N;
            varianceB = varianceB / N;

            double result = (varianceR + varianceG + varianceB) / 3;
            return result;
        }

        public double errorMAD(){
            // Error measurement using Mean Absolute Deviation of the three color channels
            double N = Width * Height;
            var (meanR, meanG, meanB) = colorMean();
            
            double madR = 0;
            double madG = 0;
            double madB = 0;
            
            for (int i = 0; i < Width; i++){
                for (int j = 0; j < Height; j++){
                    madR += Math.Abs(tree.GetPixel(i, j, TopLeft).R - meanR);
                    madG += Math.Abs(tree.GetPixel(i, j, TopLeft).G - meanG);
                    madB += Math.Abs(tree.GetPixel(i, j, TopLeft).B - meanB);
                }
            }

            madR = madR / N;
            madG = madG / N;
            madB = madB / N;

            double result = (madR + madG + madB) / 3;
            return result;
        }

        public double errorMaxPixDiff(){
            // Error measurement using Max Pixel Difference of the three color channels
            double maxR = tree.GetPixel(0, 0, TopLeft).R;
            double maxG = tree.GetPixel(0, 0, TopLeft).G;
            double maxB = tree.GetPixel(0, 0, TopLeft).B;

            double minR = tree.GetPixel(0, 0, TopLeft).R;
            double minG = tree.GetPixel(0, 0, TopLeft).G;
            double minB = tree.GetPixel(0, 0, TopLeft).B;
            
            for (int i = 0; i < Width; i++){
                for (int j = 0; j < Height; j++){
                    var pixel = tree.GetPixel(i, j, TopLeft);

                    if (maxR < pixel.R) maxR = pixel.R;
                    if (maxG < pixel.G) maxG = pixel.G;
                    if (maxB < pixel.B) maxB = pixel.B;

                    if (minR > pixel.R) minR = pixel.R;
                    if (minG > pixel.G) minG = pixel.G;
                    if (minB > pixel.B) minB = pixel.B;
                }
            }

            double diffR = maxR - minR;
            double diffG = maxG - minG;
            double diffB = maxB - minB;

            double result = (diffR + diffG + diffB) / 3;
            return result; 
        }

        public double errorEntropy(){
            // Error measurement using entropy of the three color channels
            double N = Width * Height;

            int[] redCount = new int[256];
            int[] greenCount = new int[256];
            int[] blueCount = new int[256];

            for (int i = 0; i < Width; i++){
                for (int j = 0; j < Height; j++){
                    redCount[tree.GetPixel(i, j, TopLeft).R] += 1;
                    greenCount[tree.GetPixel(i, j, TopLeft).G] += 1;
                    blueCount[tree.GetPixel(i, j, TopLeft).B] += 1;
                }
            }

            double redEntropy = 0;
            double greenEntropy = 0;
            double blueEntropy = 0;

            for (int i = 0; i < 256; i++){
                if (redCount[i] != 0){
                    double probI = redCount[i] / N;
                    redEntropy += (probI) * (Math.Log(probI) / Math.Log(2));
                }
                if (greenCount[i] != 0){
                    double probI = greenCount[i] / N;
                    greenEntropy += (probI) * (Math.Log(probI) / Math.Log(2));
                }
                if (blueCount[i] != 0){
                    double probI = blueCount[i] / N;
                    blueEntropy += (probI) * (Math.Log(probI) / Math.Log(2));
                }
            }

            redEntropy = -redEntropy;
            greenEntropy = -greenEntropy;
            blueEntropy = -blueEntropy;

            double result = (redEntropy + greenEntropy + blueEntropy) / 3;
            return result;
        }

        public double errorSSIM(){
            // Error measurement using ...
            const double C2 = 58.5225;
            const double WR = 0.2989;
            const double WG = 0.5870;
            const double WB = 0.1140;


            double N = Width * Height;
            var (meanR, meanG, meanB) = colorMean();
            
            double varianceR = 0;
            double varianceG = 0;
            double varianceB = 0;
            
            for (int i = 0; i < Width; i++){
                for (int j = 0; j < Height; j++){
                    varianceR += (tree.GetPixel(i, j, TopLeft).R - meanR) * (tree.GetPixel(i, j, TopLeft).R - meanR);
                    varianceG += (tree.GetPixel(i, j, TopLeft).G - meanG) * (tree.GetPixel(i, j, TopLeft).G - meanG);
                    varianceB += (tree.GetPixel(i, j, TopLeft).B - meanB) * (tree.GetPixel(i, j, TopLeft).B - meanB);
                }
            }
            varianceR = varianceR / N;
            varianceG = varianceG / N;
            varianceB = varianceB / N;

            double ssimR = (C2 / (C2 + varianceR));
            double ssimG = (C2 / (C2 + varianceG));
            double ssimB = (C2 / (C2 + varianceB));

            double result = WR * ssimR + WG * ssimG + WB * ssimB;
            return result;
        }

        public (double, double, double) colorMean(){
            // Function to return the mean of each color channel in a node
            double N = Width * Height;
            double sumR = 0;
            double sumG = 0;
            double sumB = 0;

            // Console.WriteLine($"Image Dimensions in mean: {Width}x{Height}, TopLeft: {TopLeft.Item2}x{TopLeft.Item1}, Max iteration: {TopLeft.Item2 + Width}x{TopLeft.Item1 + Height}");
            for (int i = 0; i < Width; i++){
                for (int j = 0; j < Height; j++){
                    sumR += tree.GetPixel(i, j, TopLeft).R;
                    sumG += tree.GetPixel(i, j, TopLeft).G;
                    sumB += tree.GetPixel(i, j, TopLeft).B;
                }
            }

            double meanR = sumR / N;
            double meanG = sumG / N;
            double meanB = sumB / N;

            return (meanR, meanG, meanB);
        }

    }
}
