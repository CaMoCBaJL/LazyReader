using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace BL
{
    public class PrintScreenBL
    {
        public static Bitmap PrintScreen(Point fP, Point sP)
        {
            if (Math.Abs(fP.X - sP.X) < 5 || Math.Abs(fP.Y - sP.Y) < 5)
            {
                return null;
            }
            else
            {
                Bitmap printscreen = new Bitmap(new MemoryStream());
                Graphics graphics = Graphics.FromImage(printscreen as Image);
                graphics.CopyFromScreen(fP, sP, new Size(Math.Abs(fP.X - sP.X), Math.Abs(fP.Y - sP.Y)));
                return printscreen;
            }
        }

        /// <summary>
        /// Эмуляция нажатия кнопки Prt Scr. Скриншот помещается по адресу "D:/1.jpeg", после чего область около курсора обрезается до прямоугольника. 
        /// </summary>
        /// <param name="mcords"> 
        /// Координаты курсора относительно экрана. 
        /// </param>
        public void PrintScreen(Point cursorPos, Size screenSize)
        {
            Bitmap printscreen = new Bitmap(new MemoryStream());
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            Rectangle screenRegion = CalculateScreenRegion(cursorPos, screenSize);
            graphics.CopyFromScreen(screenRegion.Location.X,
                screenRegion.Location.Y,
                screenRegion.Location.X + screenRegion.Width,
                screenRegion.Location.Y + screenRegion.Height,
                printscreen.Size);
            //проверяю выход за границы экрана области отреза.
            new ImageProcessingBL().CharacterRecognition(printscreen);
        }

        public Rectangle CalculateScreenRegion(Point cursorPos, Size screenSize)
        {
            if (cursorPos.X < 150 || cursorPos.Y < 50)
                switch (cursorPos.X < 150)
                {
                    case true:
                        {
                            switch (cursorPos.Y < 50)
                            {
                                case true:
                                    {
                                        return new Rectangle(0, 0, 300, 100);
                                    }
                                case false:
                                    {
                                        return new Rectangle(0, cursorPos.Y - 50, 300, 100);
                                    }
                            }
                        }
                    case false:
                        {
                            switch (cursorPos.Y < 50)
                            {
                                case true:
                                    {
                                        return new Rectangle(cursorPos.X - 150, 0, 300, 100);
                                    }
                                case false:
                                    {
                                        return new Rectangle(cursorPos.X - 150, cursorPos.Y - 50, 300, 100);
                                    }
                            }
                        }
                }
            else if (cursorPos.X > screenSize.Width - 150 || cursorPos.Y > screenSize.Height - 50)
                switch (cursorPos.X + 150 > screenSize.Width)
                {
                    case true:
                        {
                            switch (cursorPos.Y + 50 > screenSize.Height)
                            {
                                case true:
                                    {
                                        return new Rectangle(cursorPos.X - 300, cursorPos.Y - 100, 300, 100);
                                    }
                                case false:
                                    {
                                        return new Rectangle(cursorPos.X - 300, cursorPos.Y - 50, 300, 100);
                                    }
                            }
                        }
                    case false:
                        {
                            switch (cursorPos.Y + 50 > screenSize.Height)
                            {
                                case true:
                                    {
                                        return new Rectangle(cursorPos.X - 150, cursorPos.Y - 100, 300, 100);
                                    }
                                case false:
                                    {
                                        return new Rectangle(cursorPos.X - 150, cursorPos.Y - 50, 300, 100);
                                    }
                            }
                        }
                }
            else
                return new Rectangle(cursorPos.X - 150, cursorPos.Y - 50, 300, 100);
        }
    }
}
