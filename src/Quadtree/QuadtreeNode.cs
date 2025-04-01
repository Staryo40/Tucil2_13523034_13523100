using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Quadtree{
    class QuadtreeNode
    {
        public Rgba32[,]? nodeImage;
        public (int, int, int, int) boxBorder; // top, left, right, bottom
        private int depth;
        private int width;
        private int height;
        private bool leaf;
        public (QuadtreeNode, QuadtreeNode, QuadtreeNode, QuadtreeNode)? children; // topleft, topright, bottomleft, bottomright

        public QuadtreeNode(){
            nodeImage = null;
            boxBorder = (0, 0, 0, 0);
            depth = 0;
            children = null;
            width = 0;
            height = 0;  
            leaf = false;
        }

        public QuadtreeNode(Rgba32[,] image, (int, int, int, int) bor, int d, (QuadtreeNode, QuadtreeNode, QuadtreeNode, QuadtreeNode)? c = null)
        {
            boxBorder = bor;
            depth = d;
            children = c;

            var (top, left, right, bottom) = boxBorder;
            width = right - left;
            height = bottom - top;  
            leaf = false;

            nodeImage = CropImage(image, top, left, right, bottom);
        }
        public int getDepth(){
            return depth;
        }
        public int getWidth(){
            return width;
        }
        public int getHeight(){
            return height;
        }
        public bool isLeaf(){
            return leaf;
        }
        public void setLeaf(bool l){
            leaf = l;
        }
        public Rgba32[,] CropImage(Rgba32[,] image, int top, int left, int right, int bottom)
        {
            var result = new Rgba32[bottom - top, right - left];

            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    result[y - top, x - left] = image[y, x]; 
                }
            }

            return result;
        }

        public void split(){
            if (nodeImage == null) 
            {
                throw new InvalidOperationException("Node image cannot be null during split.");
            }

            var (top, left, right, bottom) = boxBorder;
            int midHor = left + (right - left) / 2;
            int midVer = top + (bottom - top) / 2;
            
            QuadtreeNode topLeft = new QuadtreeNode(nodeImage, (top, left, midVer, midHor), depth + 1, null);
            QuadtreeNode topRight = new QuadtreeNode(nodeImage, (top, midVer, right, midHor), depth + 1, null);
            QuadtreeNode bottomLeft = new QuadtreeNode(nodeImage, (midHor, left, midVer, bottom), depth + 1, null);
            QuadtreeNode bottomRight = new QuadtreeNode(nodeImage, (midHor, midVer, right, bottom), depth + 1, null);
            children = (topLeft, topRight, bottomLeft, bottomRight);
        }

        public double errorVariance(){
            if (nodeImage == null) 
            {
                throw new InvalidOperationException("Node image cannot be null to count error.");
            }

            double N = width * height;
            var (meanR, meanG, meanB) = colorMean();
            
            double varianceR = 0;
            double varianceG = 0;
            double varianceB = 0;
            
            for (int i = 0; i < height; i++){
                for (int j = 0; j < width; j++){
                    varianceR += (nodeImage[i, j].R - meanR) * (nodeImage[i, j].R - meanR);
                    varianceG += (nodeImage[i, j].G - meanG) * (nodeImage[i, j].G - meanG);
                    varianceB += (nodeImage[i, j].B - meanB) * (nodeImage[i, j].B - meanB);
                }
            }

            varianceR /= N;
            varianceG /= N;
            varianceB /= N;

            double result = (varianceR + varianceG + varianceB) / 3;
            return result;
        }

        public (double, double, double) colorMean(){
            if (nodeImage == null) 
            {
                throw new InvalidOperationException("Node image cannot be null to count mean.");
            }

            double N = width * height;
            double sumR = 0;
            double sumG = 0;
            double sumB = 0;
            for (int i = 0; i < height; i++){
                for (int j = 0; j < width; j++){
                    sumR += nodeImage[i, j].R;
                    sumG += nodeImage[i, j].G;
                    sumB += nodeImage[i, j].B;
                }
            }

            double meanR = sumR / N;
            double meanG = sumG / N;
            double meanB = sumB / N;

            return (meanR, meanG, meanB);
        }
    }
}
