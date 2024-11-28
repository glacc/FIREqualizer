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

        #region TestGraphAndFilter

        static void TestFIRWithGraph(int n, int cutoffLF, int cutoffHF, int signalLength)
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

            FIRFilter filter = new FIRFilter(ref freqs);

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

            int[] periods = [64, 0];
            int[] wavelens = [0 /*8*/ /*8, 32*/];

            xCoordSignal = new float[signalLength];
            float[] originalSignal = new float[signalLength];
            float[] filteredSignal = new float[signalLength];
            for (int i = 0; i < signalLength; i++)
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
            signalGraph.right = signalLength;
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

        #endregion

        #region AudioStreamAndEqualizer

        class FilteredStream : SoundStream
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

            public bool updateFlag = false;

            public bool multithreaded = true;

            short[] buffer = Array.Empty<short>();

            List<Task> tasks = new List<Task>();

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

                if (multithreaded)
                {
                    mutex.WaitOne();

                    int numOfCores = Environment.ProcessorCount;

                    SemaphoreSlim semaphore = new SemaphoreSlim(numOfCores, numOfCores);

                    Action<int> worker = delegate (int channel)
                    {
                        int posInput = samplePos + channel;

                        semaphore.Wait();

                        FIRFilter? filter = filters[channel];
                        if (filter == null)
                            return;

                        for (int i = channel; i < bufferSize; i += numOfChannels)
                        {
                            int currIter = (int)filter.Next(inputSamples[posInput]);
                            if (currIter > short.MaxValue)
                                currIter = short.MaxValue;
                            if (currIter < short.MinValue)
                                currIter = short.MinValue;

                            buffer[i] = (short)currIter;

                            posInput += numOfChannels;
                            if (posInput >= inputSamples.Length)
                                break;
                        }

                        semaphore.Release();
                    };

                    tasks.Clear();
                    for (int i = 0; i < numOfChannels; i++)
                    {
                        int numOfChannel = i;

                        Task newTask = Task.Run(() => worker(numOfChannel));

                        tasks.Add(newTask);
                    }
                    Task.WaitAll(tasks.ToArray());

                    foreach (Task task in tasks)
                        task.Dispose();

                    semaphore.Dispose();

                    samplePos += (int)bufferSize;

                    updateFlag = true;

                    mutex.ReleaseMutex();
                }
                else
                {
                    for (int i = 0; i < buffer.Length; i += numOfChannels)
                    {
                        mutex.WaitOne();

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

                        updateFlag = true;

                        mutex.ReleaseMutex();
                    }
                }

                samples = buffer;

                if (samplePos >= inputSamples.Length)
                    samplePos = 0;

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

                updateFlag = false;

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

            public FilteredStream()
            {
                Initialize();
            }
        }

        static FilteredStream audioStream = new FilteredStream();

        static float[] bands = [60f, 125f, 250f, 500f, 1000f, 2000f, 4000f, 8000f, 12000f, 16000f];
        static float[] magnitudesInDb = new float[bands.Length];
        static List<Label> magnitudeLabels = new List<Label>();

        const float minDb = -24f;
        const float maxDb = 0f;
        const float defaultPercent = 1f;

        static int taps = 256;
        const int minTaps = 8;
        const int maxTaps = 32768;
        static float[] equalizerCurve = Array.Empty<float>();
        static float[] equalizerFreqs = Array.Empty<float>();

        static bool normalize = false;
        static float normalizeMultiplier = 1f;

        static float[] equalizerCurveXCoord = Array.Empty<float>();
        static Graph<float, float> equalizerCurveGraph = new Graph<float, float>(0, 256, 512, 160);

        static float[] filterImpulseXCoord = Array.Empty<float>();
        static Graph<float, float> filterImpulseGraph = new Graph<float, float>(0, 256 + 192, 512, 160);

        static InputBox? ipbTaps;

        static void OnThreadToggleClicked(object? sender, EventArgs e)
        {
            Button? button = sender as Button;
            if (button == null)
                return;

            audioStream.multithreaded = !audioStream.multithreaded;

            button.text = audioStream.multithreaded ? "Multithreaded" : "Singlethread";
        }

        static void OnEqualizerMove(object? sender, EventArgs e)
        {
            ScrollBar? scrollBar = (sender as ScrollBar) ?? null;
            if (scrollBar == null)
                return;

            int numOfBand = int.Parse(scrollBar.customData);

            float magnitudeDb = minDb + ((maxDb - minDb) * (1f - scrollBar.scrollPercent));

            magnitudesInDb[numOfBand] = magnitudeDb;
            magnitudeLabels[numOfBand].text = $"{magnitudeDb:+0.0;-0.0} dB";

            RecalcEqualizerCurve();

            RedrawFreqGraph();
        }

        static void OnEqualizerAdjustEnds(object? sender, EventArgs e)
            => ApplyEqualizerCurve();

        static void OnNewTapsLostFocus(object? sender, EventArgs e)
        {
            InputBox? inputBox = sender as InputBox;
            if (inputBox == null)
                return;

            inputBox.text = $"{taps}";
        }

        static void OnNewTapsEntered(object? sender, EventArgs e)
        {
            InputBox? inputBox = sender as InputBox;
            if (inputBox == null)
                return;

            int newTaps = -1;
            int.TryParse(inputBox.text, out newTaps);
            if (newTaps <= minTaps)
                newTaps = minTaps;
            if (newTaps >= maxTaps)
                newTaps = maxTaps;

            if (newTaps == taps)
            {
                inputBox.text = $"{taps}";
                return;
            }

            taps = newTaps;

            InitOrUpdateTaps();

            RecalcEqualizerCurve();

            RedrawFreqGraph();

            ApplyEqualizerCurve();
        }

        static void InitOrUpdateTaps()
        {
            equalizerCurve = new float[taps / 2];
            equalizerFreqs = new float[taps];
            equalizerCurveXCoord = new float[taps / 2];

            filterImpulseXCoord = new float[taps];

            for (int i = 0; i < taps / 2; i++)
                equalizerCurveXCoord[i] = i;

            for (int i = 0; i < taps; i++)
                filterImpulseXCoord[i] = i;

            equalizerCurveGraph.right = taps / 2;
            filterImpulseGraph.right = taps;
            equalizerCurveGraph.horz = equalizerCurveXCoord;
            filterImpulseGraph.horz = filterImpulseXCoord;
        }

        static void InitFreqGraph()
        {
            // Equalizer Curve
            equalizerCurveGraph.left = 0f;
            equalizerCurveGraph.right = taps / 2;
            equalizerCurveGraph.top = 1f;
            equalizerCurveGraph.bottom = 0f;

            equalizerCurveGraph.sclx = 2f;
            equalizerCurveGraph.scly = 0.1f;
            equalizerCurveGraph.sclLen = 4f;

            equalizerCurveGraph.horz = equalizerCurveXCoord;

            elements.Add(equalizerCurveGraph);

            // Filter Impulse
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
            InitFreqGraph();

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
                scrollBarEqualizerBand.onScrollEnds += OnEqualizerAdjustEnds;
                scrollBarEqualizerBand.scrollerSizePixels = scrollBarWidth;
                scrollBarEqualizerBand.scrollPercent = 1f - defaultPercent;
                scrollBarEqualizerBand.scrollBgSpeedMultiplier = 20f;

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

            px += 32;

            Button btnMultithread = new Button("Multithreaded", px, py + 24 + 16, 160, 24);
            btnMultithread.onClick += OnThreadToggleClicked;
            elements.Add(btnMultithread);

            Label labelTaps = new Label($"FIR taps", px, py + 12, 16);
            labelTaps.textAlign = TextAlign.Left;
            elements.Add(labelTaps);
            px += 72;

            ipbTaps = new InputBox(px, py, 240, 24, $"{taps}");
            ipbTaps.lostFocusAfterEnter = true;
            ipbTaps.onEnterPressed += OnNewTapsEntered;
            ipbTaps.onLostFocus += OnNewTapsLostFocus;
            elements.Add(ipbTaps);

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

                filter = new FIRFilter(ref equalizerFreqs);
                audioStream.filters[i] = filter;

                lastFilter = filter;
            }

            audioStream.mutex.ReleaseMutex();

            if (lastFilter == null)
                return;

            float maxVal = float.MinValue;
            float minVal = float.MaxValue;
            float[] impulse = new float[lastFilter.filterLength];
            for (int i = 0; i < lastFilter.filterLength; i++)
            {
                float val = lastFilter.Impulse(i);

                if (val > maxVal)
                    maxVal = val;
                if (val < minVal)
                    minVal = val;

                impulse[i] = val;
            }

            normalizeMultiplier = 1f / (maxVal - minVal);

            filterImpulseGraph.top = maxVal;
            filterImpulseGraph.bottom = minVal;
            filterImpulseGraph.BeginDraw();

            filterImpulseGraph.vert = impulse;
            filterImpulseGraph.Plot();

            filterImpulseGraph.EndDraw();
        }

        #endregion

        #region FileSelector

        static FileSelector? fileSelector;
        static Button? openFileBtn;
        static Label? openedFileLabel;

        static void OnOpenClicked(object? sender, EventArgs e)
        {
            if (fileSelector == null)
                return;

            fileSelector.visable = true;

            Button? btn = sender as Button;
            if (btn == null)
                return;

            btn.enabled = false;
        }

        static void OnFileSelectorCancel(object? sender, EventArgs e)
        {
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

            openFileBtn = new Button("Open...", appWindow.width - 128 - 8, 8, 128, 24);
            openFileBtn.onClick += OnOpenClicked;
            elements.Add(openFileBtn);

            fileSelector = new FileSelector(null, appWindow.width - 512, 0, 512, 512);
            fileSelector.SetBgColor(0xEFEFEFFF);
            fileSelector.onCancel += OnFileSelectorCancel;
            fileSelector.onFileSelect += OnFileSelectorSelect;
            fileSelector.visable = false;
            elements.Add(fileSelector);
        }

        #endregion

        #region PlaybackControl

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

        #endregion

        #region FilterSignalGraph

        static bool fullView = false;

        static Graph<float, float> filterInputGraph = new Graph<float, float>(512, 256, 512, 160);
        static Graph<float, float> filterOutputGraph = new Graph<float, float>(512, 256 + 192, 512, 160);
        static Button? btnToggleFull;
        static Button? btnToggleNormalize;

        static void OnToggleFullClicked(object? sender, EventArgs e)
        {
            Button? button = sender as Button;
            if (button == null)
                return;

            fullView = !fullView;
            button.text = fullView ? "Full" : "Half";
        }

        static void OnToggleNormalizeClicked(object? sender, EventArgs e)
        {
            Button? button = sender as Button;
            if (button == null)
                return;

            normalize = !normalize;
            button.text = normalize ? "Normalize On" : "Normalize Off";
        }

        static void InitFilterSignalGraph()
        {
            filterInputGraph.left = filterOutputGraph.left = 0;
            filterInputGraph.right = filterOutputGraph.right = taps;
            filterInputGraph.top = filterOutputGraph.top = short.MaxValue;
            filterInputGraph.bottom = filterOutputGraph.bottom = short.MinValue;

            filterInputGraph.sclx = filterOutputGraph.sclx = 2f;
            filterInputGraph.scly = filterOutputGraph.scly = 32768 / 10f;
            filterInputGraph.sclLen = filterOutputGraph.sclLen = 4f;

            filterInputGraph.lineColor = Color.Red;
            filterOutputGraph.lineColor = Color.Blue;

            elements.Add(filterInputGraph);
            elements.Add(filterOutputGraph);

            btnToggleFull = new Button("Half", appWindow.width - 128 - 16, 256 + 192 + 160 + 16, 128, 24);
            btnToggleFull.onClick += OnToggleFullClicked;
            elements.Add(btnToggleFull);

            btnToggleNormalize = new Button("Normalize Off", appWindow.width - 128 - 16 - 128 - 16, 256 + 192 + 160 + 16, 128, 24);
            btnToggleNormalize.onClick += OnToggleNormalizeClicked;
            elements.Add(btnToggleNormalize);
        }

        static void UpdateFilterSignalGraph()
        {
            if (!audioStream.updateFlag)
                return;

            audioStream.mutex.WaitOne();

            audioStream.updateFlag = false;

            int len = fullView ? taps : (taps / 2);

            float[] filterInputSignalAvg = new float[len];
            float[] filterOutputSignalAvg = new float[len];

            for (int i = 0; i < len; i++)
                filterInputSignalAvg[i] = filterOutputSignalAvg[i] = 0f;

            int filterCount = 0;
            for (int i = 0; i < audioStream.filters.Length; i++)
            {
                FIRFilter? filter = audioStream.filters[i];
                if (filter == null)
                    continue;

                for (int j = 0; j < len; j++)
                {
                    filterInputSignalAvg[j] += filter.InputSignal(j);
                    filterOutputSignalAvg[j] += filter.OutputSignal(j);
                }

                filterCount++;
            }

            audioStream.mutex.ReleaseMutex();

            float multiplier = normalize ? normalizeMultiplier : 1f;
            for (int i = 0; i < len; i++)
            {
                filterInputSignalAvg[i] /= filterCount;
                filterOutputSignalAvg[i] /= filterCount / multiplier;
            }

            filterInputGraph.right = filterOutputGraph.right = len - 1;

            float[] filterSignalXCoord = new float[len];
            for (int i = 0; i < len; i++)
                filterSignalXCoord[i] = i;
            filterInputGraph.horz = filterOutputGraph.horz = filterSignalXCoord;

            filterInputGraph.BeginDraw();
            filterOutputGraph.BeginDraw();

            filterInputGraph.vert = filterInputSignalAvg;
            filterOutputGraph.vert = filterOutputSignalAvg;
            filterInputGraph.Plot();
            filterOutputGraph.Plot();

            filterInputGraph.EndDraw();
            filterOutputGraph.EndDraw();
        }

        #endregion

        static void UserInit(object? sender, EventArgs e)
        {
            /*
            float[] freqs;
            FreqResponseBandpass(44100, 20, 22050, taps, out freqs);

            for (int i = 0; i < testStream.filters.Length; i++)
                testStream.filters[i] = new FIRFilter(ref freqs);
            */

            InitOrUpdateTaps();

            InitEqualizer();
            RecalcEqualizerCurve();
            ApplyEqualizerCurve();

            RedrawFreqGraph();

            audioStream.Play();

            // TestFIRWithGraph(128, 0, 16, 512);

            InitFilterSignalGraph();

            InitProgressBar();

            AddFileSelectorBtns();
        }

        static void UserUpdate(object? sender, EventArgs e)
        {
            UpdateFilterSignalGraph();
            UpdateProgressBar();

            Utils.UpdateElements(elements);
        }

        static void UserDraw(object? sender, EventArgs e)
        {
            Utils.DrawElements(elements, appWindow.renderTexture);
        }

        static void UserClose(object? sender, EventArgs e)
        {
            audioStream.Stop();

            audioStream.mutex.WaitOne();
            audioStream.mutex.ReleaseMutex();

            audioStream.Dispose();
        }

        static void Main(string[] args)
        {
            appWindow.userInit += UserInit;
            appWindow.userUpdate += UserUpdate;
            appWindow.userDraw += UserDraw;
            appWindow.afterClosing += UserClose;

            appWindow.Run();
        }
    }
}
