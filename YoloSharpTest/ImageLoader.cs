using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSharpTest
{
    public class ImageLoader
    {
        enum ExifOrientation : ushort
        {
            TopLeft = 1,
            TopRight = 2,
            BottomRight = 3,
            BottomLeft = 4,
            LeftTop = 5,
            RightTop = 6,
            RightBottom = 7,
            LeftBottom = 8
        }
        /// <summary>
        /// 根据图像的Exif信息进行旋转，并将该文件读取为32位ARGB图像。
        /// 算法 <a href="http://blog.shibayan.jp/entry/20140428/1398688687">http://blog.shibayan.jp/entry/20140428/1398688687</a>参照 
        /// </summary>
        /// <param name="filename">要读取的位图</param>
        /// <returns>反映旋转信息的位图</returns>
        public static Bitmap Load(string filename)
        {
            Bitmap result;
            using (Bitmap bitmap = new Bitmap(filename))
            {
                // 0x0112 = Orientation 标记ID持有
                var property = bitmap.PropertyItems.FirstOrDefault(p => p.Id == 0x0112);

                if (property != null)
                {
                    var rotation = RotateFlipType.RotateNoneFlipNone;

                    var orientation = (ExifOrientation)BitConverter.ToUInt16(property.Value, 0);

                    // 根据Exif信息旋转图像
                    switch (orientation)
                    {
                        case ExifOrientation.TopLeft:
                            break;
                        case ExifOrientation.TopRight:
                            rotation = RotateFlipType.RotateNoneFlipX;
                            break;
                        case ExifOrientation.BottomRight:
                            rotation = RotateFlipType.Rotate180FlipNone;
                            break;
                        case ExifOrientation.BottomLeft:
                            rotation = RotateFlipType.RotateNoneFlipY;
                            break;
                        case ExifOrientation.LeftTop:
                            rotation = RotateFlipType.Rotate270FlipY;
                            break;
                        case ExifOrientation.RightTop:
                            rotation = RotateFlipType.Rotate90FlipNone;
                            break;
                        case ExifOrientation.RightBottom:
                            rotation = RotateFlipType.Rotate90FlipY;
                            break;
                        case ExifOrientation.LeftBottom:
                            rotation = RotateFlipType.Rotate270FlipNone;
                            break;
                    }

                    bitmap.RotateFlip(rotation);

                    property.Value = BitConverter.GetBytes((ushort)ExifOrientation.TopLeft);
                    bitmap.SetPropertyItem(property);
                }
                // Covert to 32bit ARGB 
                result = new Bitmap(bitmap.Width, bitmap.Height);
                using(Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(bitmap,
                        new Rectangle(0, 0, result.Width, result.Height),
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        GraphicsUnit.Pixel);
                }
            }
            return result;
        }

        /// <summary>
        /// 在图像周围添加边距（黑色背景），以便图像具有特定的高宽比
        /// </summary>
        /// <param name="bmp">操作目标位图</param>
        /// <param name="aspectRatio">长宽比(width/height)</param>
        /// <returns>添加了边距的位图</returns>
        public static Bitmap AddBorder(Bitmap bmp, float aspectRatio)
        {
            if(aspectRatio <= 0f)
            {
                return new Bitmap(bmp);
            }
            float a = 1f * bmp.Width / bmp.Height;
            if(a < aspectRatio) // 由于原始图像长于指定的宽高比，因此在左侧和右侧添加边距
            {
                Bitmap output = new Bitmap((int)(bmp.Height * aspectRatio), bmp.Height);
                using(Graphics g = Graphics.FromImage(output))
                {
                    g.Clear(Color.Black);

                    int s = (output.Width - bmp.Width) / 2;
                    Rectangle dest = new Rectangle(s, 0, bmp.Width, bmp.Height);
                    Rectangle src = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    g.DrawImage(bmp, dest, src, GraphicsUnit.Pixel);
                }
                return output;
            }
            else // 由于原始图像长于指定的宽高比，因此请上下添加边距
            {
                Bitmap output = new Bitmap(bmp.Width, (int)(bmp.Width / aspectRatio));
                using (Graphics g = Graphics.FromImage(output))
                {
                    g.Clear(Color.Black);

                    int s = (output.Height - bmp.Height) / 2;
                    Rectangle dest = new Rectangle(0, s, bmp.Width, bmp.Height);
                    Rectangle src = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    g.DrawImage(bmp, dest, src, GraphicsUnit.Pixel);
                }
                return output;
            }
        }
    }
}
