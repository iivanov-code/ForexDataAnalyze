using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.Utils;

namespace ForexVisualization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Graphics.SizeChanged += Graphics_SizeChanged;
        }

        private void Graphics_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Rebuild();
        }

        private PeriodList Prices;
        private bool showSma8, showSma21, showGraphics, showEma12;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            showSma8 = true;
            showSma21 = true;
            showGraphics = true;
            showEma12 = true;
            Rebuild();
        }

        private void Rebuild()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "EUR_USD Historical Data.csv";
            Prices = GetPricesFromFile(path);
            Graphics.Children.Clear();
            MacdGraphics.Children.Clear();
            BuildSomething(Prices);
            BuildMACD(Prices);
        }

        private void BuildSomething(PeriodList list)
        {
            float maxValue, minValue;
            double widthPixels = Graphics.ActualWidth / list.Count;
            maxValue = Prices.Select(x => x.MaxPrice).Max();
            minValue = Prices.Select(x => x.MinPrice).Min();
            if (showGraphics)
            {
                BuildGraphic(list.Reverse().Select(x => x.AvgPrice).ToList(), Brushes.Blue, maxValue, minValue, widthPixels);
            }

            if (showSma8)
            {
                List<float> sma8 = new List<float>();

                for (int i = 8; i < list.Count; i++)
                {
                    sma8.Add(ForexUtils.CalculateSMA(list.Reverse().Skip(i - 8).Take(8).ToList(), MAIPeriod.MAI8));
                }
                widthPixels = Graphics.ActualWidth / sma8.Count;
                BuildGraphic(sma8, Brushes.Red, maxValue, minValue, widthPixels);
            }

            if (showSma21)
            {
                List<float> sma21 = new List<float>();

                for (int i = 21; i < list.Count; i++)
                {
                    sma21.Add(ForexUtils.CalculateSMA(list.Reverse().Skip(i - 21).Take(21).ToList(), MAIPeriod.MAI21));
                }

                widthPixels = Graphics.ActualWidth / sma21.Count;
                BuildGraphic(sma21, Brushes.Green, maxValue, minValue, widthPixels);
            }

            if (showEma12)
            {
                List<float> ema = new List<float>();
                float prevEma = 0;
                for (int i = 12; i < list.Count; i++)
                {
                    prevEma = ForexUtils.CalculateEMA(list.Reverse().Skip(i - 12).Take(12).ToList(), MAIPeriod.MAI12, prevEma);
                    ema.Add(prevEma);
                }

                widthPixels = Graphics.ActualWidth / ema.Count;
                BuildGraphic(ema, Brushes.Orange, maxValue, minValue, widthPixels);
            }
        }

        private void BuildGraphic(List<float> list, Brush lineColor, float maxValue, float minValue, double widthPixels, double startXPixel = 10)
        {
            if (list.Count == 0)
                return;

            double heightPixels = Graphics.ActualHeight / (maxValue - minValue);

            double startYPixel = Graphics.ActualHeight - ((maxValue - list[0]) * heightPixels);

            for (int i = 1; i < list.Count; i++)
            {
                Line line = new Line
                {
                    Stroke = lineColor,
                    StrokeThickness = 2,
                    X1 = startXPixel,
                    Y1 = startYPixel
                };

                startXPixel += widthPixels;

                line.X2 = startXPixel;
                line.Y2 = startYPixel + ((list[i] - list[i - 1]) * heightPixels);

                startYPixel = line.Y2;
                Graphics.Children.Add(line);
            }
        }

        private void BuildMACD(PeriodList list)
        {
            List<Tuple<float, float, float>> macdList = new List<Tuple<float, float, float>>();
            StockStack<float> macdLineList = new StockStack<float>(1440);
            float prevEMA9 = 0, prevEMA12 = 0, prevEMA26 = 0, histogram = 0, macd = 0;

            for (int i = 26; i < list.Count; i++)
            {
                macd = ForexUtils.CalculateMACD(list.Reverse().Skip(i - 26).Take(26).ToList(), macdLineList, out histogram, ref prevEMA9, ref prevEMA12, ref prevEMA26);
                macdLineList.Add(macd);
                macdList.Add(new Tuple<float, float, float>(macd, histogram, prevEMA9));
            }

            double macdPixels = MacdGraphics.ActualHeight / (macdList.Select(x => x.Item1).Max() - macdList.Select(x => x.Item1).Min());
            double histogramPixels = MacdGraphics.ActualHeight / (macdList.Select(x => x.Item2).Max() - macdList.Select(x => x.Item2).Min());
            double signalLinePixels = MacdGraphics.ActualHeight / (macdList.Select(x => x.Item3).Max() - macdList.Select(x => x.Item3).Min());
            double middleLine = MacdGraphics.ActualHeight / 2;
            double width = MacdGraphics.ActualWidth / macdList.Count;

            double startXPixel = 0;
            double macdStartYPixel = MacdGraphics.ActualHeight - ((macdList.Select(x => x.Item1).Max() - macdList[0].Item1) * macdPixels); ;
            double signalLineStartYPixel = MacdGraphics.ActualHeight - ((macdList.Select(x => x.Item3).Max() - macdList[0].Item3) * signalLinePixels);
            MacdGraphics.Children.Add(new Line
            {
                X1 = startXPixel,
                Y1 = MacdGraphics.ActualHeight / 2,
                X2 = MacdGraphics.ActualWidth,
                Y2 = MacdGraphics.ActualHeight / 2,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            });

            for (int i = 1; i < macdList.Count; i++)
            {
                var item = macdList[i];
                var macdLine = new Line
                {
                    X1 = startXPixel,
                    Y1 = macdStartYPixel,
                    StrokeThickness = 1,
                    Stroke = Brushes.Blue
                };

                var histogramLine = new Line
                {
                    X1 = startXPixel,
                    Y1 = middleLine,
                    StrokeThickness = 1,
                };

                var signalLine = new Line
                {
                    X1 = startXPixel,
                    Y1 = signalLineStartYPixel,
                    StrokeThickness = 1,
                    Stroke = Brushes.Black
                };

                startXPixel += width;
                macdLine.X2 = startXPixel;
                histogramLine.X2 = startXPixel;
                signalLine.X2 = startXPixel;

                macdLine.Y2 = macdStartYPixel + ((macdList[i].Item1 - macdList[i - 1].Item1) * macdPixels);
                macdStartYPixel = macdLine.Y2;

                histogramLine.Y2 = middleLine + ((macdList.Select(x => x.Item2).Average() - macdList[i].Item2) * histogramPixels);

                signalLine.Y2 = signalLineStartYPixel + ((macdList[i].Item3 - macdList[i - 1].Item3) * signalLinePixels);
                signalLineStartYPixel = signalLine.Y2;

                if (histogramLine.Y2 > middleLine)
                {
                    histogramLine.Stroke = Brushes.Red;
                }
                else
                {
                    histogramLine.Stroke = Brushes.Green;
                }
                MacdGraphics.Children.Add(histogramLine);
                MacdGraphics.Children.Add(macdLine);
                MacdGraphics.Children.Add(signalLine);
            }
        }

        private static PeriodList GetPricesFromFile(string path)
        {
            IEnumerable<string> allLines = File.ReadLines(path);

            PeriodList data = new PeriodList(ForexTools.Enums.PeriodTypeEnum.Minute);

            foreach (var line in allLines.Skip(1))
            {
                string[] arr = line.Replace("\"", "").Replace("/", "").Split(',');

                data.Add(float.Parse(arr[1]));
                data.Add(float.Parse(arr[2]));
                data.Add(float.Parse(arr[3]));
                data.Add(float.Parse(arr[4]));
                data.EndPeriod();
            }
            return data;
        }

        private void BtnSma8_Click(object sender, RoutedEventArgs e)
        {
            showSma8 = !showSma8;
            Rebuild();
        }

        private void BtnSma21_Click(object sender, RoutedEventArgs e)
        {
            showSma21 = !showSma21;
            Rebuild();
        }

        private void BtnEma12_Click(object sender, RoutedEventArgs e)
        {
            showEma12 = !showEma12;
            Rebuild();
        }

        private void BtnGraphics_Click(object sender, RoutedEventArgs e)
        {
            showGraphics = !showGraphics;
            Rebuild();
        }
    }
}
