using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Glacc
{
    public unsafe class FIRFilter : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        unsafe struct FIR_Filter
        {
            public double* freqs;
            public double* impulse;
            public int impulseLength;

            // Queue based FIR filter.
            public double* originalSignal;
            public double* outputSignal;
            public int signalLength;

            public int pos;
        }

        #region C Methods

        [DllImport("FIR.dll", EntryPoint = "Extern_FIR_CreateImpulse", CallingConvention = CallingConvention.Cdecl)]
        extern static void CreateImpulse_Internal(double[] freqs, [Out] double[] impulse, int count);

        [DllImport("FIR.dll", EntryPoint = "Extern_FIR_CreateFilterByFreqs", CallingConvention = CallingConvention.Cdecl)]
        extern static bool CreateFilterByFreqs_Internal(ref FIR_Filter filter, double[] freqs, int count);

        [DllImport("FIR.dll", EntryPoint = "Extern_FIR_Next", CallingConvention = CallingConvention.Cdecl)]
        extern static double Next_Internal(ref FIR_Filter filter, double input);

        [DllImport("FIR.dll", EntryPoint = "Extern_FIR_DestroyFilter", CallingConvention = CallingConvention.Cdecl)]
        extern static void DestroyFilter(ref FIR_Filter filter);

        #endregion

        FIR_Filter filter = new FIR_Filter();

        public int signalLength
        {
            get => filter.signalLength;
        }

        public int freqLength
        {
            get => filter.impulseLength;
        }

        public int impulseLength
        {
            get => filter.impulseLength;
        }

        public enum DataType
        {
            Freqs,
            Impulse,
            OriginalSignal,
            OutputSignal
        };

        int WrapToQueue(int len, int pos, int offset)
        {
            int offsetNew = pos + offset;
            while (offsetNew >= len)
                offsetNew -= len;
            while (offsetNew < 0)
                offsetNew += len;

            return offsetNew;
        }

        public double this[DataType i, int j]
        {
            get
            {
                int offset;
                switch (i)
                {
                    case DataType.Freqs:
                        if (j >= 0 && j < freqLength)
                            return filter.freqs[j];
                        break;
                    case DataType.Impulse:
                        if (j >= 0 && j < impulseLength)
                            return filter.impulse[j];
                        break;
                    case DataType.OriginalSignal:
                        offset = WrapToQueue(signalLength, filter.pos, j);
                        return filter.originalSignal[offset];
                    case DataType.OutputSignal:
                        offset = WrapToQueue(signalLength, filter.pos, j);
                        return filter.outputSignal[offset];
                }

                return 0.0;
            }
        }

        public static void CreateImpulse(ref double[] freqs, out double[] impulse)
        {
            int count = freqs.Length;
            impulse = new double[count];
            CreateImpulse_Internal(freqs, impulse, count);
        }

        public double Next(double input)
            => Next_Internal(ref filter, input);

        public FIRFilter(ref double[] freqs)
            => CreateFilterByFreqs_Internal(ref filter, freqs, freqs.Length);

        void IDisposable.Dispose()
            => DestroyFilter(ref filter);
    }
}
