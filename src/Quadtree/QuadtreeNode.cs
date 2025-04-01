using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Quadtree{
    class QuadtreeNode
    {
        private QuadtreeTree tree;
        public (int, int) TopLeft { get; private set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsLeaf;
        public (QuadtreeNode, QuadtreeNode, QuadtreeNode, QuadtreeNode)? Children; // topleft, topright, bottomleft, bottomright

        public QuadtreeNode(QuadtreeTree t){
            tree = t;
            Depth = 0;
            Children = null;
            Width = 0;
            Height = 0;  
            IsLeaf = false;
        }

        public QuadtreeNode(QuadtreeTree t, (int, int, int, int) bor, int d, (QuadtreeNode, QuadtreeNode, QuadtreeNode, QuadtreeNode)? c = null)
        {
            tree = t;
            Depth = d;
            Children = c;

            var (top, left, right, bottom) = bor;
            TopLeft = (left, top);
            Width = right - left;
            Height = bottom - top;  
            IsLeaf = false;
        }

        public (int, int, int, int) GetBorders()
        {
            var (top, left) = TopLeft;
            var (right, bottom) = (left + Width, top + Height);
            return (top, left, right, bottom);
        }

        public void split(){
            var (top, left, right, bottom) = GetBorders();
            int midHor = left + (right - left) / 2;
            int midVer = top + (bottom - top) / 2;
            
            QuadtreeNode topLeft = new QuadtreeNode(tree, (top, left, midVer, midHor), Depth + 1, null);
            QuadtreeNode topRight = new QuadtreeNode(tree, (top, midVer, right, midHor), Depth + 1, null);
            QuadtreeNode bottomLeft = new QuadtreeNode(tree, (midHor, left, midVer, bottom), Depth + 1, null);
            QuadtreeNode bottomRight = new QuadtreeNode(tree, (midHor, midVer, right, bottom), Depth + 1, null);
            Children = (topLeft, topRight, bottomLeft, bottomRight);
        }

        public double errorVariance(){
            double N = Width * Height;
            var (meanR, meanG, meanB) = colorMean();
            
            double varianceR = 0;
            double varianceG = 0;
            double varianceB = 0;
            
            for (int i = 0; i < Height; i++){
                for (int j = 0; j < Width; j++){
                    varianceR += (tree.GetPixel(i, j, TopLeft).R - meanR) * (tree.GetPixel(i, j, TopLeft).R - meanR);
                    varianceG += (tree.GetPixel(i, j, TopLeft).G - meanG) * (tree.GetPixel(i, j, TopLeft).G - meanG);
                    varianceB += (tree.GetPixel(i, j, TopLeft).B - meanB) * (tree.GetPixel(i, j, TopLeft).B - meanB);
                }
            }

            varianceR /= N;
            varianceG /= N;
            varianceB /= N;

            double result = (varianceR + varianceG + varianceB) / 3;
            return result;
        }

        public (double, double, double) colorMean(){
            double N = Width * Height;
            double sumR = 0;
            double sumG = 0;
            double sumB = 0;
            for (int i = 0; i < Height; i++){
                for (int j = 0; j < Width; j++){
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
