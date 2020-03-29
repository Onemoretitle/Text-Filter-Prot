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
    public struct word
    {
        public int start;
        public int end;
        public string karnel;
        public bool valid;
    }

    public struct text
    {
        public string fileName;
        public List<word> words;
    }

    public partial class Form1 : Form
    {
        public text buffer;        
        private bool hasSave;

        private void saveFile()
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                buffer.fileName = saveFileDialog1.FileName;
                File.WriteAllText(buffer.fileName, richTextBox1.Text);
            }
        }

        private void openFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                buffer.fileName = openFileDialog1.FileName;
                richTextBox1.Clear();
                richTextBox1.Text = File.ReadAllText(buffer.fileName);
                fill();
            }
        }

        private void fill()
        {
            word[] stick = new word[1000];

            var context = new Context();
            context.SetStrategy(new ConcreteStrategyA());
            List<string> result = context.DoSomeBusinessLogic();

            // Клиентский код может знать или не знать о Конкретном Итераторе или классах Коллекций, в зависимости от уровня косвенности, который вы хотите сохранить в своей программе.
            var collection = new WordsCollection();
            int wordStart = 0; 
            int wordEnd = 0;
            int k = 0;
            bool printer = false;
            for (int i = 0; i < richTextBox1.TextLength; i++) 
            {
                if (!Char.IsLetter(richTextBox1.Text[i]) && printer)
                {
                    printer = false;
                    wordEnd = i - 1;
                    stick[k].start = wordStart;
                    stick[k].end = wordEnd;
                    stick[k].valid = true;
                    stick[k].karnel = result[k++];
                    //collection.AddItem(wordStart, wordEnd, result[k++]);
                }
                if (Char.IsLetter(richTextBox1.Text[i]) && !printer) 
                {
                    wordStart = i;
                    printer = true;
                }
            }
            if (printer && result.Count >= k)
            {
                printer = false;
                wordEnd = richTextBox1.TextLength - 1;
                stick[k].start = wordStart;
                stick[k].end = wordEnd;
                stick[k].valid = true;
                stick[k].karnel = result[k];
                //collection.AddItem(wordStart, wordEnd, result[k++]);
            }
            int valids = 0;
            for (int i = 0; i < stick.Length; i++)
            {
                if (stick[i].karnel != null)
                {
                        StreamReader str = new StreamReader("vocabulary.txt", Encoding.Default);
                    while (!str.EndOfStream)
                    {
                        string st = str.ReadLine();
                        if (st.StartsWith(stick[i].karnel))
                        {
                            stick[i].valid = false;
                            valids++;
                            break;// останавливаем цикл
                        }
                    }
                    collection.AddItem(stick[i].start, stick[i].end, stick[i].karnel, stick[i].valid); 
                }
            }
            buffer.words = collection.getItems();
            int capasity = buffer.words.Count;
            foreach (var element in buffer.words) 
            {
                if (!element.valid)
                {
                    richTextBox1.SelectionStart = element.start;
                    richTextBox1.SelectionLength = element.end + 1 - element.start;
                    richTextBox1.SelectionColor = Color.MediumVioletRed;
                }
            }
            label1.Text = "\tКолчиество слов: " + capasity  + "\n\tОшибки: " + valids;
            File.WriteAllText(@"convert/tmp.txt", richTextBox1.Text, Encoding.Default);
            
   
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hasSave = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveFile();
            hasSave = true;
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            openFile();
            hasSave = false;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            //fill();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form ifrm = new Form2();
            ifrm.Left = this.Left; 
            ifrm.Top = this.Top;
            ifrm.ShowDialog();
        }
    }
    //
    // Итератор
    //
    abstract class Iterator : IEnumerator
    {
        object IEnumerator.Current => Current();

        // Возвращает ключ текущего элемента
        public abstract int Key();

        // Возвращает текущий элемент.
        public abstract object Current();

        // Переходит к следующему элементу.
        public abstract bool MoveNext();

        // Перематывает Итератор к первому элементу.
        public abstract void Reset();
    }

    abstract class IteratorAggregate : IEnumerable
    {
        // Возвращает Iterator или другой IteratorAggregate для реализующего объекта.
        public abstract IEnumerator GetEnumerator();
    }

    // Конкретные Итераторы реализуют различные алгоритмы обхода. Эти классы постоянно хранят текущее положение обхода.
    class AlphabeticalOrderIterator : Iterator
    {
        private WordsCollection _collection;

        // Хранит текущее положение обхода. У итератора может быть множество других полей для хранения состояния итерации, особенно когда он должен работать с определённым типом коллекции.
        private int _position = -1;
        private bool _reverse = false;

        public AlphabeticalOrderIterator(WordsCollection collection, bool reverse = false)
        {
            this._collection = collection;
            this._reverse = reverse;

            if (reverse)
            {
                this._position = collection.getItems().Count;
            }
        }

        public override object Current()
        {
            return this._collection.getItems()[_position];
        }

        public override int Key()
        {
            return this._position;
        }

        public override bool MoveNext()
        {
            int updatedPosition = this._position + (this._reverse ? -1 : 1);

            if (updatedPosition >= 0 && updatedPosition < this._collection.getItems().Count)
            {
                this._position = updatedPosition;
                return true;
            }
            else
            {
                return false;
            }
        }
        public override void Reset()
        {
            this._position = this._reverse ? this._collection.getItems().Count - 1 : 0;
        }
    }

    // Конкретные Коллекции предоставляют один или несколько методов для получения новых экземпляров итератора, совместимых с классом коллекции.
    class WordsCollection : IteratorAggregate
    {
        List<word> _collection = new List<word>();
        bool _direction = false;
        public void ReverseDirection()
        {
            _direction = !_direction;
        }
        public List<word> getItems()
        {
            return _collection;
        }
        public void AddItem(int _start, int _end, string _karnel, bool _valid)
        {
            word elem;
            elem.start = _start;
            elem.end = _end;
            elem.valid = _valid;
            elem.karnel = _karnel;
            this._collection.Add(elem);
        }
        public override IEnumerator GetEnumerator()
        {
            return new AlphabeticalOrderIterator(this, _direction);
        }
    }
    //
    // Конец итератора
    //

    //
    // Адаптер
    //

    // Целевой класс объявляет интерфейс, с которым может работать клиентский код.
    public interface ITarget
    {
        string GetRequest();
    }

    // Адаптируемый класс содержит некоторое полезное поведение, но его
    // интерфейс несовместим  с существующим клиентским кодом. Адаптируемый
    // класс нуждается в некоторой доработке,  прежде чем клиентский код сможет
    // его использовать.
    class Adaptee
    {
        public string GetSpecificRequest()
        {
            return "Specific request.";
        }
    }

    // Адаптер делает интерфейс Адаптируемого класса совместимым с целевым
    // интерфейсом.
    class Adapter : ITarget
    {
        private readonly Adaptee _adaptee;

        public Adapter(Adaptee adaptee)
        {
            this._adaptee = adaptee;
        }

        public string GetRequest()
        {
            return $"This is '{this._adaptee.GetSpecificRequest()}'";
        }
    }
    
    //
    // Конец адаптера
    //

    //
    // Стратегия
    //

    // Контекст определяет интерфейс, представляющий интерес для клиентов.
    class Context
    {
        // Контекст хранит ссылку на один из объектов Стратегии. Контекст не
        // знает конкретного класса стратегии. Он должен работать со всеми
        // стратегиями через интерфейс Стратегии.
        private IStrategy _strategy;

        public Context()
        {
        }

        // Обычно Контекст принимает стратегию через конструктор, а также
        // предоставляет сеттер для её изменения во время выполнения.
        public Context(IStrategy strategy)
        {
            this._strategy = strategy;
        }

        // Обычно Контекст позволяет заменить объект Стратегии во время
        // выполнения.
        public void SetStrategy(IStrategy strategy)
        {
            this._strategy = strategy;
        }

        // Вместо того, чтобы самостоятельно реализовывать множественные версии
        // алгоритма, Контекст делегирует некоторую работу объекту Стратегии.
        public List<string> DoSomeBusinessLogic()
        {
            var result = this._strategy.DoAlgorithm();
            //MessageBox.Show("Обработка завершена", "Сообщение", MessageBoxButtons.OK);
            return result;
        }
    }

    // Интерфейс Стратегии объявляет операции, общие для всех поддерживаемых
    // версий некоторого алгоритма.
    //
    // Контекст использует этот интерфейс для вызова алгоритма, определённого
    // Конкретными Стратегиями.
    public interface IStrategy
    {
        List<string> DoAlgorithm();
    }

    // Конкретные Стратегии реализуют алгоритм, следуя базовому интерфейсу
    // Стратегии. Этот интерфейс делает их взаимозаменяемыми в Контексте.
    class ConcreteStrategyA : IStrategy
    {
        public List<string> DoAlgorithm()
        {
           /*string text = File.ReadAllText("convert/tmp.txt");
            var dataFilePath ="full7z-mlteast-ru.lem";
            var stream = File.OpenRead(dataFilePath);
            var lemmatizer = new Lemmatizer(stream);
            var result = lemmatizer.Lemmatize(text);
            string[] list = result.Split(' ');*/
        
            string exampleSentence = File.ReadAllText("convert/tmp.txt", Encoding.Default);
            string[] exampleWords = exampleSentence.Split(new char[] { ' ', ',', '.', ')', '(' }, StringSplitOptions.RemoveEmptyEntries);
            ILemmatizer lmtz = new LemmatizerPrebuiltCompact(LemmaSharp.LanguagePrebuilt.Russian);

            /*Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Example sentence lemmatized");
            Console.WriteLine("        WORD ==> LEMMA");*/
            List<string> list = new List<string>();
            foreach (string word in exampleWords)
                list.Add(LemmatizeOne(lmtz, word));
            return list;
            //Console.ForegroundColor = ConsoleColor.White;
        }

        private static string LemmatizeOne(LemmaSharp.ILemmatizer lmtz, string word)
        {
            string wordLower = word.ToLower();
            string lemma = lmtz.Lemmatize(wordLower);
            return lemma;
            /*Console.ForegroundColor = wordLower == lemma ? ConsoleColor.White : ConsoleColor.Red;
            Console.WriteLine("{0,12} ==> {1}", word, lemma);*/
        }

    }
    //
    // Конец стратегии
    //


    //
    // Команда
    //
    // Интерфейс Команды объявляет метод для выполнения команд.
    public interface ICommand
    {
        void Execute();
    }

    // Некоторые команды способны выполнять простые операции самостоятельно.
    class SimpleCommand : ICommand
    {
        private string _payload = string.Empty;

        public SimpleCommand(string payload)
        {
            this._payload = payload;
        }

        public void Execute()
        {
            Console.WriteLine($"SimpleCommand: See, I can do simple things like printing ({this._payload})");
        }
    }

    // Но есть и команды, которые делегируют более сложные операции другим
    // объектам, называемым «получателями».
    class ComplexCommand : ICommand
    {
        private Receiver _receiver;

        // Данные о контексте, необходимые для запуска методов получателя.
        private string _a;

        private string _b;

        // Сложные команды могут принимать один или несколько объектов-
        // получателей вместе с любыми данными о контексте через конструктор.
        public ComplexCommand(Receiver receiver, string a, string b)
        {
            this._receiver = receiver;
            this._a = a;
            this._b = b;
        }

        // Команды могут делегировать выполнение любым методам получателя.
        public void Execute()
        {
            Console.WriteLine("ComplexCommand: Complex stuff should be done by a receiver object.");
            this._receiver.DoSomething(this._a);
            this._receiver.DoSomethingElse(this._b);
        }
    }

    // Классы Получателей содержат некую важную бизнес-логику. Они умеют
    // выполнять все виды операций, связанных с выполнением запроса. Фактически,
    // любой класс может выступать Получателем.
    class Receiver
    {
        public void DoSomething(string a)
        {
            Console.WriteLine($"Receiver: Working on ({a}.)");
        }

        public void DoSomethingElse(string b)
        {
            Console.WriteLine($"Receiver: Also working on ({b}.)");
        }
    }

    // Отправитель связан с одной или несколькими командами. Он отправляет
    // запрос команде.
    class Invoker
    {
        private ICommand _onStart;

        private ICommand _onFinish;

        // Инициализация команд
        public void SetOnStart(ICommand command)
        {
            this._onStart = command;
        }

        public void SetOnFinish(ICommand command)
        {
            this._onFinish = command;
        }

        // Отправитель не зависит от классов конкретных команд и получателей.
        // Отправитель передаёт запрос получателю косвенно, выполняя команду.
        public void DoSomethingImportant()
        {
            if (this._onStart is ICommand)
            {
                this._onStart.Execute();
            }
            if (this._onFinish is ICommand)
            {
                this._onFinish.Execute();
            }
        }
    }

    class test
    {
        /*static void Main(string[] args)
        {
            // Клиентский код может параметризовать отправителя любыми
            // командами.
            Invoker invoker = new Invoker();
            invoker.SetOnStart(new SimpleCommand("Say Hi!"));
            Receiver receiver = new Receiver();
            invoker.SetOnFinish(new ComplexCommand(receiver, "Send email", "Save report"));

            invoker.DoSomethingImportant();
        }*/
    }
    //
    // Конец команды
    //

    //
    // Пул объектов (одиночка)
    //  
    class Vocabulary
    {
        public string kernel;
        public Vocabulary()
        {

        }
        public void reset()
        {
            kernel = "";
        }
        public void setParamiters(string _kernel)
        {
            kernel = _kernel;
        }
    }
    class Pool
    {
        private List<Vocabulary> vocabularyWords;
        private static Pool instance;
        private Pool() { }
        public static Pool getInstance()
        {
            if (instance == null)
            {
                instance = new Pool();
            }
            return instance;
        }
        public Vocabulary getWord()
        {
            if (vocabularyWords.Count == 0)
            {
                return new Vocabulary();
            }
            else
            {
                Vocabulary resource = vocabularyWords[0];
                vocabularyWords.RemoveAt(0);
                return resource;
            }
        }
        public void returnWord(Vocabulary obj)
        {
            obj.reset();
            vocabularyWords.Add(obj);
        }
    }
    //
    // Конец пула объектов
    //

}

