// Uses a deterministic crowding genetic algorithm to potentially generate images
// http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.30.8270&rep=rep1&type=pdf

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeneticTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public int SIZE = 16;
        public int PIXELS;
        public int ZOOM = 8;
        public int COUNT = 16;

        public float[][] images;

        public float[] child1, child2;

        public Random rnd;

        public int[] pairs;
        public int pos;

        public bool visU;
        public bool visD;

        public bool pair1;

        public MainWindow() {
            InitializeComponent();

            rnd = new Random();

            PIXELS = 3 * SIZE * SIZE;

            pairs = new int[COUNT];
            images = new float[COUNT][];
            for(int i = 0; i < COUNT; ++i) {
                pairs[i] = i;
                images[i] = new float[PIXELS];
                for(int z = 0; z < 3; ++z)
                    for(int y = 0; y < SIZE; ++y)
                        for(int x = 0; x < SIZE; ++x) {
                            int j = 3 * (y * SIZE + x) + z;
                            images[i][j] = (float)(2.0 * rnd.NextDouble() - 1.0) * 5.0f / (y + 5) / (x + 5);
                        }
            }

            pos = COUNT;
            pair1 = false;

            visU = visD = false;

            Next();
        }

        public void Next() {
            if(pair1) {
                if(visD)
                    images[pairs[pos]] = child1;

                imgU.Source = Render(images[pairs[pos + 1]], ZOOM);
                imgD.Source = Render(child2, ZOOM);

                pos += 2;
                pair1 = false;
            } else {
                if(visD)
                    images[pairs[pos - 1]] = child2;

                if(pos >= COUNT) {
                    for(int i = COUNT - 1; i >= 0; --i) {
                        int n = rnd.Next(i + 1);
                        int temp = pairs[i];
                        pairs[i] = pairs[n];
                        pairs[n] = temp;
                    }

                    pos = 0;
                }

                int crossR = rnd.Next(1, 2 * SIZE - 2);
                int crossG = rnd.Next(1, 2 * SIZE - 2);
                int crossB = rnd.Next(1, 2 * SIZE - 2);

                child1 = new float[PIXELS];
                child2 = new float[PIXELS];
                for(int y = 0; y < SIZE; ++y)
                    for(int x = 0; x < SIZE; ++x) {
                        int i = 3 * (y * SIZE + x);

                        if(x + y < crossR) {
                            child1[i] = Mutate(images[pairs[pos]][i]);
                            child2[i] = Mutate(images[pairs[pos + 1]][i]);
                        } else {
                            child2[i] = Mutate(images[pairs[pos]][i]);
                            child1[i] = Mutate(images[pairs[pos + 1]][i]);
                        }
                        if(x + y < crossG) {
                            child1[i + 1] = Mutate(images[pairs[pos]][i + 1]);
                            child2[i + 1] = Mutate(images[pairs[pos + 1]][i + 1]);
                        } else {
                            child2[i + 1] = Mutate(images[pairs[pos]][i + 1]);
                            child1[i + 1] = Mutate(images[pairs[pos + 1]][i + 1]);
                        }
                        if(x + y < crossB) {
                            child1[i + 2] = Mutate(images[pairs[pos]][i + 2]);
                            child2[i + 2] = Mutate(images[pairs[pos + 1]][i + 2]);
                        } else {
                            child2[i + 2] = Mutate(images[pairs[pos]][i + 2]);
                            child1[i + 2] = Mutate(images[pairs[pos + 1]][i + 2]);
                        }
                    }

                if(Similarity(images[pairs[pos]], child1) + Similarity(images[pairs[pos + 1]], child2) < Similarity(images[pairs[pos]], child2) + Similarity(images[pairs[pos + 1]], child1)) {
                    float[] temp = child1;
                    child1 = child2;
                    child2 = temp;
                }

                imgU.Source = Render(images[pairs[pos]], ZOOM);
                imgD.Source = Render(child1, ZOOM);

                pair1 = true;
            }

            next.IsEnabled = false;
            visU = visD = false;
            selectU.Visibility = System.Windows.Visibility.Hidden;
            selectD.Visibility = System.Windows.Visibility.Hidden;
        }

        private float Similarity(float[] p, float[] c) {
            float diff = 0.0f;

            for(int i = 0; i < PIXELS; ++i)
                diff += Math.Sign(p[i]) * Math.Sign(c[i]);

            return diff;
        }

        private WriteableBitmap Render(float[] p, int zoom) {
            WriteableBitmap bmp = new WriteableBitmap(SIZE * zoom, SIZE * zoom, 96.0, 96.0, PixelFormats.Rgb24, null);

            byte[] pixels = new byte[PIXELS * zoom * zoom];
            for(int z = 0; z < 3; ++z)
                for(int y = 0; y < SIZE; ++y)
                    for(int x = 0; x < SIZE; ++x) {
                        int i;
                        double total = 0.0;

                        for(int dy = 0; dy < SIZE; ++dy)
                            for(int dx = 0; dx < SIZE; ++dx) {
                                i = 3 * (dy * SIZE + dx) + z;
                                total += p[i] * Math.Cos(Math.PI * (x + 0.5) * dx / 16) * Math.Cos(Math.PI * (y + 0.5) * dy / 16);
                            }

                        i = 3 * (y * SIZE + x) + z;
                        byte c = (byte)(255.0 * Math.Min(1.0, Math.Max(0.0, (total + 1.0) / 2.0)));

                        for(int v = 0; v < zoom; ++v)
                            for(int u = 0; u < zoom; ++u) {
                                int j = 3 * (((y * zoom + v) * SIZE + x) * zoom + u) + z;
                                pixels[j] = c;
                            }

                    }

            bmp.WritePixels(new Int32Rect(0, 0, SIZE * zoom, SIZE * zoom), pixels, 3 * SIZE * zoom, 0);
            return bmp;
        }

        private float Mutate(float p) {
            return (float)(p + (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 1.5) * 0.03);
        }

        private void next_Click(object sender, RoutedEventArgs e) {
            Next();
        }

        private void imgU_MouseDown(object sender, MouseButtonEventArgs e) {
            if(!visU) {
                selectU.Visibility = System.Windows.Visibility.Visible;
                selectD.Visibility = System.Windows.Visibility.Hidden;
                visU = true;
                visD = false;
            } else {
                selectU.Visibility = System.Windows.Visibility.Hidden;
                visU = false;
            }

            UpdateButton();
        }

        private void imgD_MouseDown(object sender, MouseButtonEventArgs e) {
            if(!visD) {
                selectD.Visibility = System.Windows.Visibility.Visible;
                selectU.Visibility = System.Windows.Visibility.Hidden;
                visD = true;
                visU = false;
            } else {
                selectD.Visibility = System.Windows.Visibility.Hidden;
                visD = false;
            }

            UpdateButton();
        }

        private void UpdateButton() {
            if(visU != visD) {
                next.IsEnabled = true;
                exportMenu.IsEnabled = true;
            } else {
                next.IsEnabled = false;
                exportMenu.IsEnabled = false;
            }
        }

        private void UpdateSizes() {
            row1.Height = new GridLength(SIZE * ZOOM + 16);
            row2.Height = new GridLength(SIZE * ZOOM + 16);
            col1.Width = new GridLength(SIZE * ZOOM + 16);
        }

        private void New_Click(object sender, RoutedEventArgs e) {
            NewDialog dialog = new NewDialog();

            dialog.imageSize.Text = SIZE.ToString();
            dialog.imageZoom.Text = ZOOM.ToString();
            dialog.poolSize.Text = COUNT.ToString();

            dialog.ShowDialog();

            if(dialog.result == true) {
                SIZE = 16;
                int.TryParse(dialog.imageSize.Text, out SIZE);
                SIZE = Math.Max(SIZE, 1);

                PIXELS = 3 * SIZE * SIZE;

                ZOOM = 8;
                int.TryParse(dialog.imageZoom.Text, out ZOOM);
                ZOOM = Math.Max(ZOOM, 1);

                COUNT = 16;
                int.TryParse(dialog.poolSize.Text, out COUNT);
                COUNT = Math.Max(COUNT, 2);

                UpdateSizes();

                pairs = new int[COUNT];
                images = new float[COUNT][];
                for(int i = 0; i < COUNT; ++i) {
                    pairs[i] = i;

                    images[i] = new float[PIXELS];
                    for(int z = 0; z < 3; ++z)
                        for(int y = 0; y < SIZE; ++y)
                            for(int x = 0; x < SIZE; ++x) {
                                int j = 3 * (y * SIZE + x) + z;
                                images[i][j] = (float)(2.0 * rnd.NextDouble() - 1.0) * 11.0f / (y + 5) / (x + 5);
                            }
                }

                pos = COUNT;
                pair1 = false;

                visU = visD = false;
                exportMenu.IsEnabled = false;

                Next();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "Pool Files (*.pool)|*.pool|All Files (*.*)|*.*";
            dialog.DefaultExt = ".pool";
            dialog.RestoreDirectory = true;

            if(dialog.ShowDialog() == true) {
                string filename = dialog.FileName;

                using(BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Create))) {
                    bw.Write(SIZE);
                    bw.Write(ZOOM);
                    bw.Write(COUNT);

                    for(int i = 0; i < COUNT; ++i) {
                        for(int j = 0; j < PIXELS; ++j) {
                            bw.Write(images[i][j]);
                        }
                    }
                }
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Pool Files (*.pool)|*.pool|All Files (*.*)|*.*";
            dialog.DefaultExt = ".pool";
            dialog.RestoreDirectory = true;

            if(dialog.ShowDialog() == true) {
                string filename = dialog.FileName;

                using(BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open))) {
                    SIZE = br.ReadInt32();
                    PIXELS = 3 * SIZE * SIZE;
                    ZOOM = br.ReadInt32();
                    COUNT = br.ReadInt32();

                    UpdateSizes();

                    pairs = new int[COUNT];
                    images = new float[COUNT][];
                    for(int i = 0; i < COUNT; ++i) {
                        pairs[i] = i;

                        images[i] = new float[PIXELS];
                        for(int j = 0; j < PIXELS; ++j) {
                            images[i][j] = br.ReadSingle();
                        }
                    }
                }

                pos = COUNT;
                pair1 = false;

                visU = visD = false;

                Next();
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e) {
            float[] toWrite = null;

            if(pair1) {
                if(visU) {
                    toWrite = images[pairs[pos]];
                } else if(visD) {
                    toWrite = child1;
                } else {
                    return;
                }
            } else {
                if(visU) {
                    toWrite = images[pairs[pos - 1]];
                } else if(visD) {
                    toWrite = child2;
                } else {
                    return;
                }
            }

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*";
            dialog.DefaultExt = ".png";
            dialog.RestoreDirectory = true;

            if(dialog.ShowDialog() == true) {
                string filename = dialog.FileName;

                using(FileStream file = new FileStream(filename, FileMode.Create)) {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(Render(toWrite, 1)));
                    encoder.Save(file);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

    }
}
