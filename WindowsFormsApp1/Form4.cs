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
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form4 : Form
    {
        List<string> definitions;
        public Form4()
        {
            InitializeComponent();
            definitions = new List<string>();
            if (!File.Exists("./AppData/Definitions_Words/Definitions.txt"))
            {
                File.Create("./AppData/Definitions_Words/Definitions.txt");
                Thread.Sleep(1000);
            }
            using (StreamReader reader = new StreamReader("./AppData/Definitions_Words/Definitions.txt"))
            {
                definitions.AddRange(reader.ReadToEnd().Split());
            }
            definitions.RemoveAll(str => str == string.Empty || str == null);
            int c = 0;
            foreach (string def in definitions)
            {
                Label lDef = new Label();
                lDef.Text = def;
                lDef.Size = new Size(100, 20);
                lDef.Location = new Point(20, 20 + c);
                c += 30;
                lDef.DoubleClick += (s, a) =>
                { 
                    using (Bitmap img = new Bitmap("./AppData/Definitions_Images/" + def + ".png")) 
                        new Form2(img, def).Show(); 
                    this.Close();
                };
                Label defDel = new Label();
                defDel.Text = "X";
                defDel.ForeColor = Color.Red;
                defDel.Location = new Point(lDef.Width + 40, lDef.Location.Y);
                defDel.Click += (s, a) =>
                {
                    if (MessageBox.Show("Удалить определениe?", "Удаление", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        definitions.Remove(def);
                        StringBuilder result = new StringBuilder();
                        definitions.ForEach(elem => result.Append(elem+ "\n"));
                        using (StreamWriter writer = new StreamWriter("./AppData/Definitions_Words/Definitions.txt"))
                            writer.Write(result.ToString());
                        FileInfo delPic = new FileInfo("./Definitions_Images/" + def + ".png");
                        delPic.Delete();
                        this.Close();
                        new Form4().Show();
                    }
                };
                Controls.Add(lDef);
                Controls.Add(defDel);
            }
            
        }

    }
}
