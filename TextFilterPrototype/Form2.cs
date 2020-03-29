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
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LemmaSharp;

namespace TextFilterPrototype
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            
            if (File.Exists("vocabulary.txt"))
            {
                textBox1.Clear();
                textBox1.Text = File.ReadAllText("vocabulary.txt", Encoding.Default);
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            DoAlgorithm();
            File.WriteAllText("vocabulary.txt", textBox1.Text, Encoding.Default);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Это окно закроется","Сообщение", MessageBoxButtons.OK);
            //File.WriteAllText("vocabulary.txt", textBox1.Text, Encoding.Default);
            this.Close();
        }

        public void DoAlgorithm()
        {
            //string exampleSentence = File.ReadAllText("vocabulary.txt", Encoding.Default);
            string[] exampleWords = textBox1.Text.Split(new char[] { ' ', ',', '.', ')', '(', '\n', ' ', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            ILemmatizer lmtz = new LemmatizerPrebuiltCompact(LemmaSharp.LanguagePrebuilt.Russian);
            textBox1.Clear();
            string value = "";
            foreach (string word in exampleWords)
            {
                value += LemmatizeOne(lmtz, word);
            }
            textBox1.Text = value;
        }

        private static string LemmatizeOne(LemmaSharp.ILemmatizer lmtz, string word)
        {
            string wordLower = word.ToLower();
            string lemma = lmtz.Lemmatize(wordLower) + "\n\r";
            return lemma;
        }
    }

}
