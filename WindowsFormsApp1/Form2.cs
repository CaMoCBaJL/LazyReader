using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using gma.System.Windows;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        //Блок переменных для последнего действия.
        List<Bitmap> defPartsResult;
        int heigth;
        string defText;
        //Блок переменных для работы приложения.
        bool status = false;
        List<Bitmap> defParts;
        UserActivityHook actHook = new UserActivityHook();
        Point firstCoords;
        Point secondCoords;
        bool fP = false;
        string def;
        /// <summary>
        /// Работаем! Класс для модификации выбранной области экрана.
        /// </summary>
        /// <param name="img"></param>
        /// <param name="f"></param>
        public Form2(Bitmap img, string def)
        {
            InitializeComponent();
            this.def = def;
            actHook.OnMouseActivity += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (fP)
                    {
                        secondCoords = Cursor.Position;
                        fP = false;
                        actHook.Stop();
                        new Form2(Form1.PrintScreen(firstCoords, secondCoords), def).Show();
                        foreach (object f in Application.OpenForms)
                            if (f.GetType() == typeof(Form2))
                            {
                                Form2 form = (Form2)f;
                                form.Close();
                                break;
                            }
                    }
                    else
                    {
                        firstCoords = e.Location;
                        fP = true;
                    }
                }
            };
            actHook.Stop();
            defParts = new List<Bitmap>(Operations.ImageDivision(img));
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            int c = 0;
            int number = 0;
            foreach (Bitmap part in defParts)
            {
                PictureBox pBox = new PictureBox();
                pBox.Size = part.Size;
                pBox.Name = number.ToString();
                number++;
                pBox.Image = part;
                pBox.Location = new Point(20, 20 + c);
                c += part.Height + 10;
                pBox.MouseDown += (s, a) =>
                {
                    PictureBox box = (PictureBox)s;
                    if (a.Location.X >= box.Image.Width - 10)
                        status = true;
                };
                pBox.MouseUp += (s, a) =>
                {
                    if (status)
                    {
                        PictureBox curBox = (PictureBox)s;
                        Bitmap img = (Bitmap)curBox.Image;
                        Bitmap newImg;
                        if (a.Location.X < defParts[int.Parse(curBox.Name)].Width)
                        {
                            if (a.Location.X > 10)
                            {
                                newImg = defParts[int.Parse(curBox.Name)].Clone(new Rectangle(new Point(0, 0), new Size(a.Location.X, curBox.Height)),
                                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                curBox.Image = newImg;
                                curBox.Size = newImg.Size;
                            }
                            else
                            {
                                curBox.Image = defParts[int.Parse(curBox.Name)];
                                curBox.Size = defParts[int.Parse(curBox.Name)].Size;
                                MessageBox.Show("Нельзя уменьшать изображение.");
                            }
                        }
                        else
                        {
                            curBox.Image = defParts[int.Parse(curBox.Name)];
                            curBox.Size = defParts[int.Parse(curBox.Name)].Size;
                            MessageBox.Show("Нельзя увеличивать изображение.");
                        }
                        status = false;
                    }
                };
                pBox.MouseDoubleClick += (s, a) =>
                {
                    if (MessageBox.Show("Удалить фрагмент?", "Выберите действие", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        PictureBox box = (PictureBox)s;
                        foreach (object elem in Controls)
                            if (elem.GetType() == typeof(PictureBox))
                            {
                                PictureBox box1 = (PictureBox)elem;
                                if (int.Parse(box1.Name) > int.Parse(box.Name))
                                    box1.Name = (int.Parse(box1.Name) - 1).ToString();
                            }
                        Controls.Remove(box);
                        defParts.Remove((Bitmap)box.Image);
                    }
                };
                Controls.Add(pBox);
            }
            this.Width = Controls[0].Width + 370;
            TextBox defInsert = new TextBox();
            defInsert.Location = new Point(Controls[0].Width + 50, 200);
            defInsert.Width = 100;
            if (!string.IsNullOrEmpty(def) || !string.IsNullOrWhiteSpace(def))
                defInsert.Text = def;
            defInsert.KeyDown += (s, a) =>
            {
                if (a.KeyCode == Keys.Enter)
                    if (!string.IsNullOrWhiteSpace(defInsert.Text) & !string.IsNullOrEmpty(defInsert.Text))
                    {
                        FileInfo definitions = new FileInfo("./AppData/Definitions_Words/Definitions.txt");
                        if (!definitions.Exists)
                            using (StreamWriter firstDef = new StreamWriter("./AppData/Definitions_Words/Definitions.txt"))
                                firstDef.WriteLine(defInsert.Text);
                        else
                        {
                            var alphabet = new List<char>();
                            for (int i = 1072; i < 1104; i++)
                            {
                                alphabet.Add((char)i);
                                    //добавляем Ё
                                    if (i == 1077)
                                    alphabet.Add((char)1105);
                            }
                            if (defInsert.Text.ToLower().Except(alphabet).Count() != 0)
                                MessageBox.Show("Уберите лишнее из определения!");
                            else
                            {
                                bool matchRes = false;
                                using (StreamReader words = new StreamReader("./AppData/Definitions_Words/Definitions.txt"))
                                {
                                    string allWords = words.ReadToEnd();
                                    matchRes = new Regex(defInsert.Text).IsMatch(allWords);
                                }
                                if (!matchRes)
                                    using (StreamWriter addWord = new StreamWriter("./AppData/Definitions_Words/Definitions.txt", true))
                                        addWord.WriteLine(defInsert.Text);
                                heigth = 0;
                                defParts.ForEach(bitmap => heigth += bitmap.Height);
                                defPartsResult = new List<Bitmap>();
                                foreach (object elem in Controls)
                                    if (elem.GetType() == typeof(PictureBox))
                                    {
                                        PictureBox box = (PictureBox)elem;
                                        defPartsResult.Add((Bitmap)box.Image);
                                    }
                                foreach (object elem in Controls)
                                    if (elem.GetType() == typeof(PictureBox))
                                        Controls.Remove((PictureBox)elem);
                                defText = defInsert.Text;
                                Operations.FinalDefinition(defPartsResult, heigth, defParts[0].Width, defText);
                                this.Close();
                            }
                        }
                    }
                    else
                        MessageBox.Show("Введите определение!");
            };
            Controls.Add(defInsert);
            Label l = new Label();
            l.Text = "Введите определение без скобок и знаков препинания.";
            l.Width = 300;
            l.Location = new Point(defInsert.Location.X, defInsert.Location.Y - 50);
            Controls.Add(l);
            Button b = new Button();
            b.Text = "Изменить область";
            b.Click += (s, a) => { this.Hide(); MessageBox.Show("Выделите область экрана."); actHook.Start(); };
            b.Location = new Point(l.Location.X, l.Location.Y - 30);
            b.Width = 150;
            b.BackColor = Color.LemonChiffon;
            Controls.Add(b);
        }
    }
}

