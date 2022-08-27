using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Tesseract;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WindowsFormsApp1
{
    class Operations
    {
        /// <summary>
        /// Ассинхронное распознавание символов.
        /// <para>
        /// <paramref name="img"/> Обрабатываемое изображение.
        /// </para>
        /// </summary>
        /// <param name="img"></param>
        public async static void CharacterRecognition(Bitmap img)
        {
            var results = new List<string>();
            string text = "";
            await (Task.Run(() =>
            {
            using (var e = new TesseractEngine("./AppData/tessdata", "rus"))
            {
                text = e.Process(img).GetText();
            }
                GC.Collect();
                if (!(text.ToLower().Contains('(') || text.ToLower().Contains(')') || text.ToLower().Contains('\\') || text.ToLower().Contains('[') 
                || text.ToLower().Contains(']') || text.ToLower().Contains('^') || text.ToLower().Contains('n') || text.ToLower().Contains('r')))
                {
                    using (StreamReader defFile = new StreamReader("./AppData/Definitions_Words/Definitions.txt"))
                    {
                        foreach (string def in defFile.ReadToEnd().Split(' ', '\n'))
                            if (!string.IsNullOrEmpty(def) | !string.IsNullOrWhiteSpace(def))
                                if (new Regex(def.Substring(0, def.Length - 1)).IsMatch(text))
                                    results.Add(def.Substring(0, def.Length - 1));
                    }
                }    
            }));
            if (!string.IsNullOrEmpty(text) & !string.IsNullOrWhiteSpace(text))
                if (results.Count > 1)
                {
                    try
                    {
                        new Form3(results[0], new Point(Cursor.Position.X + 40, Cursor.Position.Y + 40)).Show();
                        Image cheatPic1 = Image.FromFile("./AppData/Definitions_Images/" + results[0] + ".png");
                        Image cheatPic2 = Image.FromFile("./AppData/Definitions_Images/" + results[1] + ".png");
                        if (cheatPic2.Height + 40 + Cursor.Position.Y > Screen.PrimaryScreen.Bounds.Height)
                            if (cheatPic2.Width + 40 + Cursor.Position.X > Screen.PrimaryScreen.Bounds.Width)
                                new Form3(results[1], new Point(Screen.PrimaryScreen.Bounds.Width - cheatPic2.Width - 40,
                                    Screen.PrimaryScreen.Bounds.Height - 40 - cheatPic2.Height)).Show();
                            else
                                new Form3(results[1], new Point(Cursor.Position.X - 40, Screen.PrimaryScreen.Bounds.Height - 40 - cheatPic2.Height)).Show();
                        else
                            new Form3(results[1], new Point(Cursor.Position.X - 40 - cheatPic2.Width, Cursor.Position.Y - 40 - cheatPic2.Height)).Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка!\n" + ex.ToString());
                    }
                }
                else if (results.Count != 0)
                {
                    try
                    {
                        Image cheatPic1 = Image.FromFile("./AppData/Definitions_Images/" + results[0] + ".png");
                        if (cheatPic1.Height + 40 + Cursor.Position.Y > Screen.PrimaryScreen.Bounds.Height)
                            if (cheatPic1.Width + 40 + Cursor.Position.X > Screen.PrimaryScreen.Bounds.Width)
                                new Form3(results[0], new Point(Screen.PrimaryScreen.Bounds.Width - cheatPic1.Width + 40,
                                    Screen.PrimaryScreen.Bounds.Height + 40 - cheatPic1.Height)).Show();
                            else
                                new Form3(results[0], new Point(Cursor.Position.X + 40, Screen.PrimaryScreen.Bounds.Height + 40 - cheatPic1.Height)).Show();
                        else
                            new Form3(results[0], new Point(Cursor.Position.X + 40, Cursor.Position.Y + 40)).Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка!\n" + ex.ToString());
                    }

                }
    }
        /// <summary>
        ///  Собираем кусочки картинки после их модификации в единое определение.
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="heigth"></param>
        /// <param name="width"></param>
        /// <param name="resName"></param>
        public static void FinalDefinition(List<Bitmap> parts, int heigth, int width, string resName)
        {
           new Task(() => {
                Bitmap result = new Bitmap(width, heigth);
                int c = 0; 
                foreach (Bitmap part in parts)
                {
                    Graphics g = Graphics.FromImage(result);
                    g.DrawImage(part, new Point(0, c));
                    c += part.Height;
                }
                Thread.Sleep(2000);
                parts.ForEach(part => part.Dispose());
                if (File.Exists("./AppData/Definitions_Images/" + resName + ".png"))
                    using (FileStream reWrite = new FileStream("./AppData/Definitions_Images/" + resName + ".png", FileMode.Truncate))
                    using (Bitmap resCopy = (Bitmap)result.Clone())
                        resCopy.Save(reWrite, System.Drawing.Imaging.ImageFormat.Png);
                else
                   using (FileStream reWrite = new FileStream("./AppData/Definitions_Images/" + resName + ".png", FileMode.Create))
                   using (Bitmap resCopy = (Bitmap)result.Clone())
                       resCopy.Save(reWrite, System.Drawing.Imaging.ImageFormat.Png);
           }).Start();
        }
        /// <summary>
        /// Разбираю картинку на отдельные строки.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static List<Bitmap> ImageDivision(Bitmap image)
        {
            Bitmap imgCopy = (Bitmap)image.Clone();
            List<AmountAndType> columns = Binarization(Operations.FindColorComponentOfTheImage(image, image.Width, image.Height), image.Width, image.Height, image);
            List<Bitmap> images = new List<Bitmap>();
            int c = 0;
            for (int i = 1; i < columns.Count; i += 2)
            {
                if (columns.Count % 2 != 0)
                    if (columns[i].type == 0)
                    {
                        c += columns[i].amount;
                        i++;
                        continue;
                    }
                images.Add(imgCopy.Clone(new Rectangle(0, c, imgCopy.Width, +columns[i - 1].amount + columns[i].amount),
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb));
                c += +columns[i - 1].amount + columns[i].amount;
            }
            return images;
        }
        /// <summary>
        /// Метод для нахождения самой распространненной цветовой компоненты изображения.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static byte FindColorComponentOfTheImage(Bitmap image, int w, int h)
        {
            uint white = 0;
            uint green = 0;//компоненты цвета
            uint blue = 0;
            uint red = 0;
            for (int iter1 = 0; iter1 < w; iter1++)     //В первом цикле определяю, каких пикселей больше - красных, зеленых или синих/
                for (int iter2 = 0; iter2 < h; iter2++)
                {
                    Color curColor = image.GetPixel(iter1, iter2);
                    if (curColor.R > 240 & curColor.G > 240 & curColor.B > 240)
                        white++;
                    else if (curColor.R > 128)
                        red++;
                    else if (curColor.G > 128)
                        green++;
                    else if (curColor.B > 128)
                        blue++;
                }
            if (white < green + blue || white < green + red || white < blue + red)
            {
                switch (green > blue)//Нахожу самый распространенный цвет. 
                {
                    case true:
                        {
                            switch (red > green)
                            {
                                case true:
                                    return 1;
                                case false:
                                    return 2;
                            }
                            break;
                        }
                    case false:
                        {
                            switch (red > blue)
                            {
                                case true:
                                    return 1;
                                case false:
                                    return 3;
                            }
                            break;
                        }
                }
            }
            return 0;
        }
        /// <summary>
        /// Метод, производящий бинаризацию картинки по 1 из 3х компонент - R, G или B, а также разделяющий исходный набор пикселей изображения на строки.
        /// </summary>
        /// <param name="component"> Цветовая компонента, по которой будет происходить бинаризцаия.</param>
        /// <param name="width"> Ширина картинки.</param>
        /// <param name="heigth"> Высота картинки</param>
        public static List<AmountAndType> Binarization(byte component, int width,
            int heigth, Bitmap image)
        {
            //Провожу бинаризацию по определенному цвету, используя порогувую функцию. 
            //     { 0, если величина цветовая пикселя < 128   
            // F = {
            //     { 1, иначе
            if (component != 0)
            {
                List<AmountAndType> columns = new List<AmountAndType>();
                var binImg = new Bitmap(image.Width, image.Height);
                bool isBlack; 
                bool previsBlack = false;
                ushort counter = 0;
                for (int j = 0; j < heigth; j++)
                {
                    isBlack = false;
                    for (int i = 0; i < width; i++)
                    {

                        switch (component)
                        {
                            case 1:
                                {
                                    if (image.GetPixel(i, j).R > 128)
                                    {
                                        binImg.SetPixel(i, j, Color.Black);
                                        isBlack = true;
                                    }
                                    else
                                    {
                                        binImg.SetPixel(i, j, Color.White);
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (image.GetPixel(i, j).G > 128)
                                    {
                                        binImg.SetPixel(i, j, Color.Black);
                                        isBlack = true;
                                    }
                                    else
                                    {
                                        binImg.SetPixel(i, j, Color.White);
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    if (image.GetPixel(i, j).B > 128)
                                    {
                                        binImg.SetPixel(i, j, Color.Black);
                                        isBlack = true;
                                    }
                                    else
                                    {
                                        binImg.SetPixel(i, j, Color.White);
                                    }
                                    break;
                                }
                        }
                    }
                    if (j == 0) //Подсчет пустых строк.
                        switch (isBlack)
                        {
                            case true:
                                {
                                    counter++;
                                    previsBlack = true;
                                    break;
                                }
                            case false:
                                {
                                    counter++;
                                    break;
                                }
                        }
                    else if (previsBlack == isBlack) //Если предыдущая строка тоже содержит\не содержит единиц, увеличиваем счетчик.
                    {                            //Иначе - пишем имеющееся количество, ставим счетчик в 1, т.к. новый элемент не совпадает с предыдущими.
                        counter++;               //После чего меняем значение предыдущего элемента.
                    }
                    else
                    {
                        if (previsBlack)
                            columns.Add(new AmountAndType(counter, 1));
                        else
                            columns.Add(new AmountAndType(counter, 0));
                        previsBlack = isBlack;
                        counter = 1;
                    }
                }
                if (previsBlack)
                    columns.Add(new AmountAndType(counter, 1));
                else
                    columns.Add(new AmountAndType(counter, 0));
                AdditionalColumnsCheck(ref columns);
                return columns;
            }
            else
            {
                List<AmountAndType> columns = DivisionByColumns(heigth, width, image);
                AdditionalColumnsCheck(ref columns);
                return columns;
            }
        }
        /// <summary>
        /// Дополнительная разрезка строки - удаление мусора, группировка хвостиков и основных частей букв.
        /// </summary>
        /// <param name="genStrOffset"> Тип отступа.</param>
        private static void SecondColumnsCut(uint genStrOffset, List<AmountAndType> columns)
        {
            int i = 0;
            if (columns[i].type != 0)
                i++;
            while (i < columns.Count)
            {
                if (columns[i].amount <= 0.3 * genStrOffset && i > 0 && i + 1 < columns.Count)
                {
                    if (columns[i - 1].amount <= 0.3 * genStrOffset)
                    {
                        //Прибавляем к следующей последовательности строк,содержащих 1: i, i-1 строки.
                        AmountAndType a = columns[i + 1];
                        a.amount += (ushort)(columns[i].amount + columns[i - 1].amount);
                        columns[i + 1] = a;
                        columns.RemoveAt(i);
                        columns.RemoveAt(i - 1);
                    }
                    else
                        i++;
                }
                else
                    i += 2;
            }
        }
        /// <summary>
        /// Метод, вычисляющий самый распространенный отступ между строк на картинке.
        /// Рассматриваю 3 типа отступов: маленький(до 10пикселей), средний(10-20), большой(20-30).
        /// </summary>
        private static void AdditionalColumnsCheck(ref List<AmountAndType> columns)
        {
            uint sCounter = 0;
            uint nCounter = 0;
            uint bCounter = 0;
            int i = 0;
            if (columns[i].type != 0)
                i++;
            while (i < columns.Count)
            {
                switch (columns[i].amount < 10)
                {
                    case true:
                        {
                            sCounter++;
                            break;
                        }
                    case false:
                        {
                            switch (columns[i].amount < 20)
                            {
                                case true:
                                    {
                                        nCounter++;
                                        break;
                                    }
                                case false:

                                    {
                                        bCounter++;
                                        break;
                                    }
                            }
                            break;
                        }
                }
                i += 2;
            }
            switch (nCounter > bCounter)
            {
                case true:
                    {
                        switch (nCounter > sCounter)
                        {
                            case true:
                                {
                                    SecondColumnsCut(20, columns);
                                    break;
                                }
                            case false:
                                {
                                    SecondColumnsCut(10, columns);
                                    break;
                                }
                        }
                        break;
                    }
                case false:
                    {
                        switch (bCounter > sCounter)
                        {
                            case true:
                                {
                                    SecondColumnsCut(30, columns);
                                    break;
                                }
                            case false:
                                {
                                    SecondColumnsCut(10, columns);
                                    break;
                                }
                        }
                        break;
                    }
            }

        }
        /// <summary>
        /// Метод, разрезающий изображение с текстом на строки, если картинке не нужна бинаризация.
        /// </summary>
        /// <param name="heigth"> Высота картинки.</param>
        /// <param name="width"> Ширина картинки. </param>
        public static List<AmountAndType> DivisionByColumns(int heigth, int width, Bitmap image)
        {
            List<AmountAndType> columns = new List<AmountAndType>();
            bool isBlack = false;
            bool previsBlack = false;
            ushort counter = 0;
            List<int> curPixelString; //Список для подсчета пустых и непустых пикселей в строке.
            List<Color> colors = new List<Color>();
            for (int i = 0; i < heigth; i++)
            {
                isBlack = false;
                curPixelString = new List<int>(new int[] { 0, 0 });
                colors.Clear();
                for (int j = 0; j < width; j++)
                {
                    Color curPixel = image.GetPixel(j, i);
                    colors.Add(curPixel);
                    if (curPixel.R * curPixel.G * curPixel.G != 0)
                    {
                        j++;
                        j--;
                    }
                    if (curPixel.R < 128 || curPixel.G < 128 || curPixel.B < 128)
                        curPixelString[1]++;
                    else
                        curPixelString[0]++;
                }
                isBlack = (double)curPixelString[1] / (curPixelString[0] + curPixelString[1]) > 0.1 ? true : false;// Считается, что строка содержит текст, если кол-во непустых 
                                                                                                                   // пикселей в ней >= 30%.
                if (i == 0) //Подсчет пустых строк.
                    switch (isBlack)
                    {
                        case true:
                            {
                                counter++;
                                previsBlack = true;
                                break;
                            }
                        case false:
                            {
                                counter++;
                                break;
                            }
                    }
                else if (previsBlack == isBlack) //Если предыдущая строка тоже содержит\не содержит единиц, увеличиваем счетчик.
                {                            //Иначе - пишем имеющееся количество, ставим счетчик в 1, т.к. новый элемент не совпадает с предыдущими.
                    counter++;               //После чего меняем значение предыдущего элемента.
                }
                else
                {
                    if (previsBlack)
                        columns.Add(new AmountAndType(counter, 1));
                    else
                        columns.Add(new AmountAndType(counter, 0));
                    previsBlack = isBlack;
                    counter = 1;
                }
            }
            if (previsBlack)
                columns.Add(new AmountAndType(counter, 1));
            else
                columns.Add(new AmountAndType(counter, 0));
            return columns;
        }
    }
}
