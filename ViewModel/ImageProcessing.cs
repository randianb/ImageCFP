using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageCFP.ViewModel
{
    public class ImageProcessing
    {
        public ImageProcessing()
        { }
        #region guass smooth 高斯平滑滤波
        public double[] guassSmooth(int width, int height, double[] inputImage, double sigma)
        {
            double[] outImage = new double[width * height];

            double std2 = 2 * sigma * sigma;
            int radius = (int)(Math.Ceiling(3 * sigma));
            int filterWidth = 2 * radius + 1;
            double[] filter = new double[filterWidth];
            double[] tempImage = new double[width * height];
            double sum = 0;
            //产生一维高斯函数
            for (int i = 0; i < filterWidth; i++)
            {
                int xx = (i - radius) * (i - radius);
                filter[i] = Math.Exp(-xx / std2);
                sum += filter[i];
            }
            //归一化
            for (int i = 0; i < filterWidth; i++)
            {
                filter[i] = filter[i] / sum;
            }
            //水平方向滤波
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    if (!double.IsNaN(inputImage[i * width + j]))
                    {
                        double temp = 0;
                        for (int k = -radius; k <= radius; k++)
                        {
                            //循环延拓
                            int rem = (Math.Abs(j + k)) % width;
                            //计算卷积和
                            if (!double.IsNaN(inputImage[i * width + rem]))
                                temp += inputImage[i * width + rem] * filter[k + radius];
                        }
                        tempImage[i * width + j] = temp;
                    }
                    else
                        tempImage[i * width + j] = double.NaN;
                }
            //垂直方向滤波
            for (int j = 0; j < width; j++)
                for (int i = 0; i < height; i++)
                {
                    if (!double.IsNaN(inputImage[i * width + j]))
                    {
                        double temp = 0;
                        for (int k = -radius; k <= radius; k++)
                        {
                            int rem = (Math.Abs(i + k)) % height;
                            if (!double.IsNaN(tempImage[rem * width + j]))
                                temp += tempImage[rem * width + j] * filter[k + radius];
                        }
                        outImage[i * width + j] = temp;
                    }
                    else
                        outImage[i * width + j] = double.NaN;
                }
            return outImage;
        }
        public byte[] guassSmooth(int width, int height, byte[] inputImage, double sigma)
        {
            byte[] outImage = new byte[width * height];

            double std2 = 2 * sigma * sigma;
            int radius = (int)(Math.Ceiling(3 * sigma));
            int filterWidth = 2 * radius + 1;
            double[] filter = new double[filterWidth];
            double[] tempImage = new double[width * height];
            double sum = 0;
            //产生一维高斯函数
            for (int i = 0; i < filterWidth; i++)
            {
                int xx = (i - radius) * (i - radius);
                filter[i] = Math.Exp(-xx / std2);
                sum += filter[i];
            }
            //归一化
            for (int i = 0; i < filterWidth; i++)
            {
                filter[i] = filter[i] / sum;
            }
            //水平方向滤波
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    double temp = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        //循环延拓
                        int rem = (Math.Abs(j + k)) % width;
                        //计算卷积和
                        temp += inputImage[i * width + rem] * filter[k + radius];
                    }
                    tempImage[i * width + j] = temp;
                }
            //垂直方向滤波
            for (int j = 0; j < width; j++)
                for (int i = 0; i < height; i++)
                {
                    double temp = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int rem = (Math.Abs(i + k)) % height;
                        temp += tempImage[rem * width + j] * filter[k + radius];
                    }
                    outImage[i * width + j] = (byte)temp;
                }
            return outImage;
        }
        #endregion

        //一阶微分处理会产生较宽的边缘；二阶微分处理对细节有较强的响应，如细线和孤立点；
        //一阶微分处理一般对灰度阶梯有较强的响应；二阶微分处理对灰度级阶梯变化产生双响应。
        #region LaplasSharp 拉普拉斯锐化（二阶微分的图像增强）
        public double[] LaplasSharp(int width,int height,double[] inputImage,int model= 0,bool hasBadPixel = false)
        { 
            double[] outImage = new double[width * height];
            if (model == 0)//x、y方向微分处理
            {
                double[] templates = { 0, 1, 0, 1, -4, 1, 0, 1, 0 };
                outImage = tempOperation(inputImage, width, height, 1, templates, hasBadPixel);
            }
            else if (model == 1)//扩展（包括对角线）
            {
                double[] templates = { 1, 1, 1, 1, -8, 1, 1, 1, 1 };
                outImage = tempOperation(inputImage, width, height, 1, templates, hasBadPixel);
            }
            else if (model == 2)//其他拉普拉斯变换
            {
                double[] templates = { 0, -1, 0, -1, 4, -1, 0, -1, 0 };
                outImage = tempOperation(inputImage, width, height, 1, templates, hasBadPixel);
            }
            else if(model == 3)//其他拉普拉斯变换
            {
                double[] templates = { -1, -1, -1, -1, 8, -1, -1, -1, -1 };
                outImage = tempOperation(inputImage, width, height, 1, templates, hasBadPixel);
            }
            else if (model == 4)//图像反锐化掩蔽（将图像模糊形式从原始图像中去除）
            {
                double[] templates = { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
                outImage = tempOperation(inputImage, width, height, 1, templates, hasBadPixel);
            }
            else if (model == 5)//图像反锐化掩蔽2（将图像模糊形式从原始图像中去除）
            {
                double[] templates = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
                outImage = tempOperation(inputImage, width, height, 1, templates, hasBadPixel);
            }
            else
            {
                double[] templates = { 0, 1, 0, 1, -4, 1, 0, 1, 0 };
                outImage = tempOperation(inputImage, width, height, 1, templates, hasBadPixel);
            }
            return outImage;
        }
        public byte[] LaplasSharp(int width, int height, byte[] inputImage, int model = 0)
        {
            byte[] outImage = new byte[width * height];
            if (model == 0)
            {
                double[] templates = { 0, 1, 0, 1, -4, 1, 0, 1, 0 };
                outImage = tempOperation(inputImage, width, height, 1, templates);
            }
            else if (model == 1)//扩展
            {
                double[] templates = { 1, 1, 1, 1, -8, 1, 1, 1, 1 };
                outImage = tempOperation(inputImage, width, height, 1, templates);
            }
            else if (model == 2)
            {
                double[] templates = { 0, -1, 0, -1, 4, -1, 0, -1, 0 };
                outImage = tempOperation(inputImage, width, height, 1, templates);
            }
            else if(model == 3)
            {
                double[] templates = { -1, -1, -1, -1, 8, -1, -1, -1, -1 };
                outImage = tempOperation(inputImage, width, height, 1, templates);
            }
             else if (model == 4)//图像反锐化掩蔽（将图像模糊形式从原始图像中去除）
            {
                double[] templates = { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
                outImage = tempOperation(inputImage, width, height, 1, templates);
            }
            else if (model == 5)//图像反锐化掩蔽2（将图像模糊形式从原始图像中去除）
            {
                double[] templates = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
                outImage = tempOperation(inputImage, width, height, 1, templates);
            }
            else
            {
                double[] templates = { 0, 1, 0, 1, -4, 1, 0, 1, 0 };
                outImage = tempOperation(inputImage, width, height, 1, templates);
            }
            return outImage;
        }
        #endregion
        #region Algorithm
        private double[] tempOperation(double[] map,int width,int height,int templen,double[] templates,bool hasBadPixel = false)//模板运算
        {
            double[] outmap = new double[width * height];
            if (hasBadPixel)
            {
                for (int y = templen; y < height - templen; y++)
                    for (int x = templen; x < width - templen; x++)
                    {
                        double sum = 0;
                        int index = 0;
                        int count = 0;
                        for (int i = y - templen; i <= y + templen; i++)
                            for (int j = x - templen; j <= x + templen; j++)
                            {
                                if (!double.IsNaN(map[j + i * width]))
                                {
                                    sum += map[j + i * width] * templates[index];
                                    count++;
                                }
                            }
                        if (count <= 1)
                            outmap[x + y * width] = double.NaN;
                        else
                            outmap[x + y * width] = sum;
                    }
            }
            else
            {
                for (int y = templen; y < height - templen; y++)
                    for (int x = templen; x < width - templen; x++)
                    {
                        double sum = 0;
                        int index = 0;
                        for (int i = y - templen; i <= y + templen; i++)
                            for (int j = x - templen; j <= x + templen; j++)
                            {
                                sum += map[j + i * width] * templates[index];
                            }
                        outmap[x + y * width] = sum;

                    }
            }
            return outmap;
        }
        private byte[] tempOperation(byte[] map, int width, int height, int templen, double[] templates)//模板运算
        {
            byte[] outmap = new byte[width * height];

            for (int y = templen; y < height - templen; y++)
                for (int x = templen; x < width - templen; x++)
                {
                    double sum = 0;
                    int index = 0;
                    for (int i = y - templen; i <= y + templen; i++)
                        for (int j = x - templen; j <= x + templen; j++)
                        {
                            sum += map[j + i * width] * templates[index];
                        }
                    outmap[x + y * width] = (byte)sum;
                }
            return outmap;
        }
        #endregion
    }
}
