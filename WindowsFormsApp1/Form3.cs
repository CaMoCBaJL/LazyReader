using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        Point loc;
        /// <summary>
        /// Вывод определения.
        /// </summary>
        /// <param name="definition"> Само определение.</param>
        /// <param name="location"> Координаты курсора.</param>
        public Form3(string definition, Point location)
        {
            InitializeComponent();
            Image img = Image.FromFile("./AppData/Definitions_Images/" + definition + ".png");
            pictureBox1.Size = img.Size;
            pictureBox1.Image = img;
            pictureBox1.Location = new Point(0, 0);
            loc = location;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            this.Size = pictureBox1.Size;
            this.DesktopLocation = loc;
            loc = Cursor.Position;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Cursor.Position != loc)
            {
                this.Close();
                GC.Collect();
            }
        }
    }
}
