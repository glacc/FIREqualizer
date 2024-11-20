using Glacc.UI;
using Glacc.UI.Elements;
using Glacc.UI.Components;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;

namespace Glacc.FIRTest_Visual
{
    internal class Program
    {
        static AppWindow appWindow = new AppWindow("FIR Equalizer Test", 1024, 768);

        static float[] impulse = Array.Empty<float>();

        static List<Element?> elements = new List<Element?>();

        static FIRFilter? filter;

        static void TestFIRWithGraph(int n, int cutoffLF, int cutoffHF)
        {
            /*
            const int n = 127;
            const int cutoffLF = 0;
            const int cutoffHF = 8;
            */

            float[] freqs = new float[n];
            for (int i = cutoffLF; i < cutoffHF; i++)
            {
                freqs[i] = 1.0f;
                freqs[freqs.Length - 1 - i] = 1.0f;
            }
            freqs[cutoffHF - 1] = freqs[(n - 1) - (cutoffHF - 1)] = 0.5f;
            if (cutoffLF > 0)
                freqs[cutoffLF] = freqs[(n - 1) - cutoffLF] = 0.5f;
            FIRFilter.CreateImpulse(ref freqs, out impulse);

            float[] xCoord = new float[n];
            for (int i = 0; i < n; i++)
                xCoord[i] = i;

            filter = new FIRFilter(ref freqs);

            Graph<float, float> graphFreqs = new Graph<float, float>(16, 16, 512, 192);
            graphFreqs.horz = xCoord;
            graphFreqs.vert = freqs;
            graphFreqs.left = 0;
            graphFreqs.right = n;
            graphFreqs.top = 1f;
            graphFreqs.bottom = 0f;
            graphFreqs.bgColor = Color.White;
            graphFreqs.scly = 0.5f;
            graphFreqs.sclLen = 4;
            graphFreqs.BeginDraw();
            graphFreqs.isStem = true;
            graphFreqs.Plot();
            graphFreqs.EndDraw();

            Graph<float, float> graphImpulse = new Graph<float, float>(16, 256, 512, 192);
            graphImpulse.horz = xCoord;
            graphImpulse.vert = impulse;
            graphImpulse.left = 0;
            graphImpulse.right = n;
            graphImpulse.top = 1f;
            graphImpulse.bottom = 0f;
            graphImpulse.bgColor = Color.White;
            graphImpulse.scly = 0.5f;
            graphImpulse.sclLen = 4;
            graphImpulse.BeginDraw();
            graphImpulse.isStem = true;
            graphImpulse.Plot();
            graphImpulse.isStem = false;
            graphImpulse.Plot();
            graphImpulse.EndDraw();

            Graph<float, float> signalGraph = new Graph<float, float>(16, 496, 1024 - 32, 192);
            float[] xCoordSignal = Array.Empty<float>();

            elements.Add(graphFreqs);
            elements.Add(graphImpulse);
            elements.Add(signalGraph);

            const int signalLen = 256;
            int[] periods = [32, 0];
            int[] wavelens = [8 /*8, 32*/];

            xCoordSignal = new float[signalLen];
            float[] originalSignal = new float[signalLen];
            float[] filteredSignal = new float[signalLen];
            for (int i = 0; i < signalLen; i++)
            {
                float signal = 0.0f;
                for (int j = 0; j < periods.Length; j++)
                {
                    int period = periods[j];
                    if (period == 0)
                        continue;

                    signal += ((i % period) >= (period / 2)) ? 1.0f : -1.0f;
                }
                for (int j = 0; j < wavelens.Length; j++)
                {
                    int wavelen = wavelens[j];
                    if (wavelen == 0)
                        continue;

                    signal += MathF.Sin(2.0f * MathF.PI * ((i % wavelen) / (float)wavelen));
                }
                originalSignal[i] = signal;
                filteredSignal[i] = filter.Next(signal);

                xCoordSignal[i] = i;
            }

            signalGraph.horz = xCoordSignal;
            signalGraph.left = 0;
            signalGraph.right = signalLen;
            signalGraph.top = 1.5f;
            signalGraph.bottom = -1.5f;
            signalGraph.sclx = 16;
            signalGraph.scly = 0.5f;
            signalGraph.bgColor = Color.White;
            signalGraph.BeginDraw();
            signalGraph.lineColor = Color.Red;
            signalGraph.vert = originalSignal;
            signalGraph.Plot();
            signalGraph.lineColor = Color.Blue;
            signalGraph.vert = filteredSignal;
            signalGraph.Plot();
            signalGraph.EndDraw();
        }

        class FilterTestStream : SoundStream
        {
            public Mutex mutex = new Mutex();

            uint sampleRate = 44100;
            const uint bufferSamples = 4096;
            int numOfChannels = 2;

            uint bufferSize;

            SoundBuffer? soundBuffer = null;

            short[] inputSamples = Array.Empty<short>();
            int samplePos = 0;

            public FIRFilter?[] filters = Array.Empty<FIRFilter>();

            short[] buffer = Array.Empty<short>();

            public int currPosSample
            {
                get => samplePos / numOfChannels;
            }

            public int lengthSample
            {
                get => inputSamples.Length / numOfChannels;
            }

            public float currPosSec
            {
                get => currPosSample / (float)sampleRate;
            }

            public float lengthSec
            {
                get => lengthSample / (float)sampleRate;
            }

            public float progress
            {
                get => currPosSample / (float)lengthSample;
                set => SeekTo(value);
            }

            protected override bool OnGetData(out short[] samples)
            {
                if (soundBuffer == null)
                {
                    samples = Array.Empty<short>();
                    return false;
                }

                mutex.WaitOne();

                for (int i = 0; i < buffer.Length; i += numOfChannels)
                {
                    for (int j = 0; j < numOfChannels; j++)
                    {
                        short currSample;
                        if (samplePos < inputSamples.Length)
                        {
                            int currIter = (int)filters[j]!.Next(inputSamples[samplePos]);
                            if (currIter > 32767)
                                currIter = 32767;
                            if (currIter < -32768)
                                currIter = -32768;

                            currSample = (short)currIter;

                            samplePos++;
                        }
                        else
                            currSample = 0;

                        buffer[i + j] = currSample;
                    }
                }

                samples = buffer;

                if (samplePos >= inputSamples.Length)
                    samplePos = 0;

                mutex.ReleaseMutex();

                return true;
            }

            protected override void OnSeek(Time timeOffset) { }

            public void SeekTo(float percent)
            {
                Action<float> worker = delegate (float percent)
                {
                    mutex.WaitOne();

                    if (percent < 0f)
                        percent = 0f;
                    if (percent > 1f)
                        percent = 1f;

                    int numOfSamples = inputSamples.Length / numOfChannels;
                    int sampleSeekTo = (int)(numOfSamples * percent);
                    samplePos = sampleSeekTo * numOfChannels;

                    mutex.ReleaseMutex();
                };

                Task.Run(() => worker(percent));
            }

            public void LoadSong(string path)
            {
                try
                {
                    SoundBuffer soundBufferNew = new SoundBuffer(path);
                    soundBuffer = soundBufferNew;

                    if (openedFileLabel != null)
                        openedFileLabel.text = path;

                    InitParameters();
                }
                catch { }
            }

            void InitParameters()
            {
                if (soundBuffer == null)
                    return;

                inputSamples = soundBuffer.Samples;

                sampleRate = soundBuffer.SampleRate;
                numOfChannels = (int)soundBuffer.ChannelCount;

                bufferSize = bufferSamples * (uint)numOfChannels;
                buffer = new short[bufferSize];

                filters = new FIRFilter?[numOfChannels];

                samplePos = 0;

                Initialize();
            }

            void Initialize()
                => Initialize((uint)numOfChannels, sampleRate);

            public FilterTestStream()
            {
                Initialize();
            }
        }

        static float[] bands = [60f, 125f, 250f, 600f, 1000f, 3000f, 6000f, 12000f, 14000f, 16000f];
        static float[] magnitudesInDb = new float[bands.Length];
        static List<Label> magnitudeLabels = new List<Label>();

        const float minDb = -24f;
        const float maxDb = 0f;
        const float defaultPercent = 1f;

        const int taps = 256;
        static float[] equalizerCurve = new float[taps / 2];
        static float[] equalizerFreqs = new float[taps];

        static float[] equalizerCurveXCoord = new float[taps / 2];
        static Graph<float, float> equalizerCurveGraph = new Graph<float, float>(16, 256, 512, 160);

        static float[] filterImpulseXCoord = new float[taps];
        static Graph<float, float> filterImpulseGraph = new Graph<float, float>(16, 256 + 192, 512, 160);

        static void OnEqualizerMove(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            ScrollBar? scrollBar = sender as ScrollBar;
            if (scrollBar == null)
                return;

            int numOfBand = int.Parse(scrollBar.customData);

            float magnitudeDb = minDb + ((maxDb - minDb) * (1f - scrollBar.scrollPercent));

            magnitudesInDb[numOfBand] = magnitudeDb;
            magnitudeLabels[numOfBand].text = $"{magnitudeDb:+0.0;-0.0} dB";

            RecalcEqualizerCurve();

            RedrawFreqGraph();
        }

        static void OnEqualizerAdjEnds(object? sender, EventArgs e)
        {
            ApplyEqualizerCurve();
        }

        static void InitFreqGraph()
        {
            // Equalizer Curve
            for (int i = 0; i < taps / 2; i++)
                equalizerCurveXCoord[i] = i;

            equalizerCurveGraph.left = 0f;
            equalizerCurveGraph.right = taps / 2;
            equalizerCurveGraph.top = 1f;
            equalizerCurveGraph.bottom = 0f;

            equalizerCurveGraph.scly = 0.1f;

            equalizerCurveGraph.horz = equalizerCurveXCoord;

            elements.Add(equalizerCurveGraph);

            // Filter Impulse
            for (int i = 0; i < taps; i++)
                filterImpulseXCoord[i] = i;

            filterImpulseGraph.left = 0f;
            filterImpulseGraph.right = taps;
            filterImpulseGraph.top = 1f;
            filterImpulseGraph.bottom = 0f;

            filterImpulseGraph.sclx = 2f;
            filterImpulseGraph.scly = 0.1f;
            filterImpulseGraph.sclLen = 4f;

            filterImpulseGraph.horz = filterImpulseXCoord;

            elements.Add(filterImpulseGraph);
        }

        static void RedrawFreqGraph()
        {
            equalizerCurveGraph.BeginDraw();

            equalizerCurveGraph.vert = equalizerCurve;
            equalizerCurveGraph.Plot();

            equalizerCurveGraph.EndDraw();
        }

        static void InitEqualizer()
        {
            int px = 16;
            int py = 16;
            int pInc = 32;
            int scrollBarWidth = 16;
            int scrollBarLength = 160;
            for (int i = 0; i < bands.Length; i++)
            {
                // ScrollBar
                ScrollBar scrollBarEqualizerBand = new ScrollBar(px, py, scrollBarWidth, scrollBarLength);
                scrollBarEqualizerBand.customData = $"{i}";
                scrollBarEqualizerBand.onMove += OnEqualizerMove;
                scrollBarEqualizerBand.onScrollEnds += OnEqualizerAdjEnds;
                scrollBarEqualizerBand.scrollerSizePixels = scrollBarWidth;
                scrollBarEqualizerBand.scrollPercent = 1f - defaultPercent;

                // Label
                int labelx = px + (scrollBarWidth / 2);
                int labely = py + scrollBarLength;

                float bandFreq = bands[i];
                string freqStr;
                if (bandFreq >= 1000f)
                    freqStr = $"{MathF.Round(bandFreq / 1000f)} kHz";
                else
                    freqStr = $"{MathF.Round(bandFreq)} Hz";
                Label bandLabel = new Label(freqStr, labelx, labely + 4, 8);
                bandLabel.textAlign = TextAlign.Top;

                Label magnitudeLabel = new Label("0.0 dB", labelx, labely + 16, 8);
                magnitudeLabel.textAlign = TextAlign.Top;

                // Add elements
                elements.Add(scrollBarEqualizerBand);
                elements.Add(bandLabel);
                magnitudeLabels.Add(magnitudeLabel);

                px += pInc;
            }

            foreach (Label label in magnitudeLabels)
                elements.Add(label);

            RecalcEqualizerCurve();
        }

        static void RecalcEqualizerCurve()
        {
            int maxFreqHz = (int)audioStream.SampleRate / 2;

            int maxFreq = taps / 2;
            int currBandNum = 0;
            for (int i = 0; i < taps / 2; i++)
            {
                float currFreqHz = maxFreqHz * i / (float)maxFreq;
                while (currBandNum < bands.Length - 1)
                {
                    if (bands[currBandNum + 1] < currFreqHz)
                        currBandNum++;
                    else
                        break;
                }

                float currMagnitudeDb;
                float currBandHz = bands[currBandNum];
                float currBandMagnitudeDb = magnitudesInDb[currBandNum];
                if (currBandNum < bands.Length - 1)
                {
                    if (currFreqHz > currBandHz)
                    {
                        float nextBandHz = bands[currBandNum + 1];
                        float nextBandMagnitudeDb = magnitudesInDb[currBandNum + 1];

                        float percent = (currFreqHz - currBandHz) / (nextBandHz - currBandHz);

                        currMagnitudeDb = currBandMagnitudeDb + ((nextBandMagnitudeDb - currBandMagnitudeDb) * MathF.Sin((MathF.PI / 2f) * percent));
                    }
                    else
                        currMagnitudeDb = currBandMagnitudeDb;
                }
                else
                    currMagnitudeDb = currBandMagnitudeDb;
                float currMagnitudeLinear = MathF.Pow(10, currMagnitudeDb / 20f);

                equalizerCurve[i] = currMagnitudeLinear;
            }
        }

        static void ApplyEqualizerCurve()
        {
            audioStream.mutex.WaitOne();

            for (int i = 0; i < taps / 2; i++)
            {
                equalizerFreqs[i] = equalizerCurve[i];
                equalizerFreqs[taps - 1 - i] = equalizerCurve[i];
            }

            FIRFilter? lastFilter = null;
            for (int i = 0; i < audioStream.filters.Length; i++)
            {
                FIRFilter filter;

                if (audioStream.filters[i] == null)
                {
                    filter = new FIRFilter(ref equalizerFreqs);

                    audioStream.filters[i] = filter;
                }
                else
                {
                    audioStream.filters[i]!.UpdateFreqs(ref equalizerFreqs);

                    filter = audioStream.filters[i]!;
                }

                lastFilter = filter;

                /*
                FIRFilter? filter = testStream.filters[i];
                if (filter == null)
                    continue;

                filter.UpdateFreqs(ref equalizerFreqs);

                lastFilter = filter;
                */
            }

            audioStream.mutex.ReleaseMutex();

            if (lastFilter == null)
                return;

            float maxVal = float.MinValue;
            float minVal = float.MaxValue;
            float[] impulse = new float[lastFilter.impulseLength];
            for (int i = 0; i < lastFilter.impulseLength; i++)
            {
                float val = lastFilter[FIRFilter.DataType.Impulse, i];

                if (val > maxVal)
                    maxVal = val;
                if (val < minVal)
                    minVal = val;

                impulse[i] = val;
            }

            filterImpulseGraph.top = maxVal;
            filterImpulseGraph.bottom = minVal;
            filterImpulseGraph.BeginDraw();

            filterImpulseGraph.vert = impulse;
            filterImpulseGraph.Plot();

            filterImpulseGraph.EndDraw();
        }

        static FilterTestStream audioStream = new FilterTestStream();

        static void FreqResponseBandpass(int sampleRateHz, int minHz, int maxHz, int impulseLength, out float[] freqs)
        {
            int minFreqHz = minHz;
            int maxFreqHz = maxHz;

            freqs = new float[impulseLength];

            int maxFreq = impulseLength / 2;
            int maxPassFreq = maxFreq * maxFreqHz / (sampleRateHz / 2);
            int minPassFreq = maxFreq * minFreqHz / (sampleRateHz / 2);

            int i = minPassFreq;
            while (i < maxPassFreq)
            {
                freqs[i] = 1.0f;
                freqs[impulseLength - 1 - i] = 1.0f;

                i++;
            }
        }

        static FileSelector? fileSelector;
        static Button? openFileBtn;
        static Label? openedFileLabel;

        static void OnOpenClicked(object? sender, EventArgs e)
        {
            if (fileSelector == null)
                return;

            fileSelector.visable = true;

            if (sender == null)
                return;

            Button? btn = sender as Button;
            if (btn == null)
                return;

            btn.enabled = false;
        }

        static void OnFileSelectorCancel(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            FileSelector? fileSelector = sender as FileSelector;
            if (fileSelector == null)
                return;

            fileSelector.visable = false;

            if (openFileBtn == null)
                return;

            openFileBtn.enabled = true;
        }

        static void OnFileSelectorSelect(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            FileSelector? fileSelector = sender as FileSelector;
            if (fileSelector == null)
                return;

            fileSelector.visable = false;

            if (openFileBtn == null)
                return;

            openFileBtn.enabled = true;

            audioStream.Stop();

            audioStream.mutex.WaitOne();

            audioStream.LoadSong(fileSelector.lastSelectedFilePath);
            ApplyEqualizerCurve();

            audioStream.mutex.ReleaseMutex();

            audioStream.Play();
        }

        static void AddFileSelectorBtns()
        {
            openedFileLabel = new Label("", 16, appWindow.height - (16 * 3) - 8, 16);
            openedFileLabel.textAlign = TextAlign.Left;
            elements.Add(openedFileLabel);

            openFileBtn = new Button("Open", appWindow.width - 80 - 8, 8, 80, 24);
            openFileBtn.onClick += OnOpenClicked;
            elements.Add(openFileBtn);

            fileSelector = new FileSelector(null, appWindow.width - 512, 0, 512, 512);
            fileSelector.SetBgColor(0xEFEFEFFF);
            fileSelector.onCancel += OnFileSelectorCancel;
            fileSelector.onFileSelect += OnFileSelectorSelect;
            fileSelector.visable = false;
            elements.Add(fileSelector);
        }

        static Label? progressText;
        static ScrollBar? progressBar;
        static Button? btnPlay;
        static Button? btnStop;

        static void OnProgressBarSeek(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            ScrollBar? scrollBar = sender as ScrollBar;
            if (scrollBar == null)
                return;

            audioStream.SeekTo(scrollBar.scrollPercent);
        }

        static void OnPlayClicked(object? sender, EventArgs e)
        {
            audioStream.Play();
        }

        static void OnStopClicked(object? sender, EventArgs e)
        {
            audioStream.Stop();
        }

        static void InitProgressBar()
        {
            const int spacing = 16;
            int py = appWindow.height - spacing - 16;
            int px = spacing;

            progressBar = new ScrollBar(px, py, 16, 512, ScrollBarDirection.Horizontal);
            progressBar.scrollerSizePixels = progressBar.width;
            progressBar.scrollBgSpeedMultiplier = 5f;
            progressBar.onScrollEnds = OnProgressBarSeek;
            elements.Add(progressBar);
            px += 512 + spacing;

            progressText = new Label("", px, py + 8, 16);
            progressText.textAlign = TextAlign.Left;
            elements.Add(progressText);
            px += 128 + spacing;

            btnPlay = new Button("Play", px, py, 64, 16);
            btnPlay.fontSize = 12;
            btnPlay.onClick += OnPlayClicked;
            elements.Add(btnPlay);
            px += 64 + spacing;

            btnStop = new Button("Stop", px, py, 64, 16);
            btnStop.fontSize = 12;
            btnStop.onClick += OnStopClicked;
            elements.Add(btnStop);
        }

        static void UpdateProgressBar()
        {
            if (progressText == null || progressBar == null)
                return;

            TimeSpan lengthSeconds = TimeSpan.FromSeconds(audioStream.lengthSec);
            TimeSpan currPosSeconds = TimeSpan.FromSeconds(audioStream.currPosSec);
            progressText.text =
                $"{(int)currPosSeconds.TotalMinutes:00}:{(int)currPosSeconds.Seconds:00} / " +
                $"{(int)lengthSeconds.TotalMinutes:00}:{(int)lengthSeconds.Seconds:00}";

            if (!progressBar.isDragging)
                progressBar.scrollPercent = audioStream.progress;
        }

        static void UserInit(object? sender, EventArgs e)
        {
            /*
            float[] freqs;
            FreqResponseBandpass(44100, 20, 22050, taps, out freqs);

            for (int i = 0; i < testStream.filters.Length; i++)
                testStream.filters[i] = new FIRFilter(ref freqs);
            */

            InitFreqGraph();

            InitEqualizer();
            RecalcEqualizerCurve();
            ApplyEqualizerCurve();

            RedrawFreqGraph();

            audioStream.Play();

            // TestFIRWithGraph(127, 0, 8);

            InitProgressBar();

            AddFileSelectorBtns();
        }

        static void UserUpdate(object? sender, EventArgs e)
        {
            UpdateProgressBar();

            Utils.UpdateElements(elements);
        }

        static void UserDraw(object? sender, EventArgs e)
        {
            Utils.DrawElements(elements, appWindow.renderWindow);
        }

        static void Main(string[] args)
        {
            appWindow.userInit += UserInit;
            appWindow.userUpdate += UserUpdate;
            appWindow.userDraw += UserDraw;

            appWindow.Run();
        }
    }
}
