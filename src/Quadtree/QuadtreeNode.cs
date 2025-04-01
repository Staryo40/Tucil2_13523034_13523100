using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Quadtree{
    class QuadtreeNode
    {
        public Rgba32[,]? NodeImage;
        public (int, int, int, int) BoxBorder; // top, left, right, bottom
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsLeaf;
        public (QuadtreeNode, QuadtreeNode, QuadtreeNode, QuadtreeNode)? Children; // topleft, topright, bottomleft, bottomright

        public QuadtreeNode(){
            NodeImage = null;
            BoxBorder = (0, 0, 0, 0);
            Depth = 0;
            Children = null;
            Width = 0;
            Height = 0;  
            IsLeaf = false;
        }

        public QuadtreeNode(Rgba32[,] image, (int, int, int, int) bor, int d, (QuadtreeNode, QuadtreeNode, QuadtreeNode, QuadtreeNode)? c = null)
        {
            BoxBorder = bor;
            Depth = d;
            Children = c;

            var (top, left, right, bottom) = BoxBorder;
            Width = right - left;
            Height = bottom - top;  
            IsLeaf = false;

            NodeImage = CropImage(image, top, left, right, bottom);
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
            if (NodeImage == null) 
            {
                throw new InvalidOperationException("Node image cannot be null during split.");
            }

            var (top, left, right, bottom) = BoxBorder;
            int midHor = left + (right - left) / 2;
            int midVer = top + (bottom - top) / 2;
            
            QuadtreeNode topLeft = new QuadtreeNode(NodeImage, (top, left, midVer, midHor), Depth + 1, null);
            QuadtreeNode topRight = new QuadtreeNode(NodeImage, (top, midVer, right, midHor), Depth + 1, null);
            QuadtreeNode bottomLeft = new QuadtreeNode(NodeImage, (midHor, left, midVer, bottom), Depth + 1, null);
            QuadtreeNode bottomRight = new QuadtreeNode(NodeImage, (midHor, midVer, right, bottom), Depth + 1, null);
            Children = (topLeft, topRight, bottomLeft, bottomRight);
        }

        public double errorVariance(){
            if (NodeImage == null) 
            {
                throw new InvalidOperationException("Node image cannot be null to count error.");
            }

            double N = Width * Height;
            var (meanR, meanG, meanB) = colorMean();
            
            double varianceR = 0;
            double varianceG = 0;
            double varianceB = 0;
            
            for (int i = 0; i < Height; i++){
                for (int j = 0; j < Width; j++){
                    varianceR += (NodeImage[i, j].R - meanR) * (NodeImage[i, j].R - meanR);
                    varianceG += (NodeImage[i, j].G - meanG) * (NodeImage[i, j].G - meanG);
                    varianceB += (NodeImage[i, j].B - meanB) * (NodeImage[i, j].B - meanB);
                }
            }

            varianceR /= N;
            varianceG /= N;
            varianceB /= N;

            double result = (varianceR + varianceG + varianceB) / 3;
            return result;
        }

        public (double, double, double) colorMean(){
            if (NodeImage == null) 
            {
                throw new InvalidOperationException("Node image cannot be null to count mean.");
            }

            double N = Width * Height;
            double sumR = 0;
            double sumG = 0;
            double sumB = 0;
            for (int i = 0; i < Height; i++){
                for (int j = 0; j < Width; j++){
                    sumR += NodeImage[i, j].R;
                    sumG += NodeImage[i, j].G;
                    sumB += NodeImage[i, j].B;
                }
            }

            double meanR = sumR / N;
            double meanG = sumG / N;
            double meanB = sumB / N;

            return (meanR, meanG, meanB);
        }
    }
}
