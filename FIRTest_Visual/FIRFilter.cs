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
            public int filterLength;

            public float* freqs;
            public float* impulse;

            public float* originalSignal;
            public float* outputSignal;
        }

        #region C Methods

        [DllImport("FIR.dll", EntryPoint = "Extern_FIR_CreateImpulse", CallingConvention = CallingConvention.Cdecl)]
        extern static void CreateImpulse_Internal(float[] freqs, [Out] float[] impulse, int count);

        [DllImport("FIR.dll", EntryPoint = "Extern_FIR_CreateFilterByFreqs", CallingConvention = CallingConvention.Cdecl)]
        extern static bool CreateFilterByFreqs_Internal(ref FIR_Filter filter, float[] freqs, int count);

        [DllImport("FIR.dll", EntryPoint = "Extern_FIR_Next", CallingConvention = CallingConvention.Cdecl)]
        extern static float Next_Internal(ref FIR_Filter filter, float input);

        [DllImport("FIR.dll", EntryPoint = "Extern_FIR_UpdateFreqs", CallingConvention = CallingConvention.Cdecl)]
        extern static void UpdateFreqs_Internal(ref FIR_Filter filter, float[] freqs);

        [DllImport("FIR.dll", EntryPoint = "Extern_FIR_DestroyFilter", CallingConvention = CallingConvention.Cdecl)]
        extern static void DestroyFilter(ref FIR_Filter filter);

        #endregion

        FIR_Filter filter = new FIR_Filter();

        public int filterLength
        {
            get => filter.filterLength;
        }

        void CheckIndexRange(int index, int count)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException();
        }

        public float Freqs(int index)
        {
            CheckIndexRange(index, filterLength);
            return filter.freqs[index];
        }

        public float Impulse(int index)
        {
            CheckIndexRange(index, filterLength);
            return filter.impulse[index];
        }

        public float InputSignal(int index)
        {
            CheckIndexRange(index, filterLength);
            return filter.originalSignal[index];
        }

        public float OutputSignal(int index)
        {
            CheckIndexRange(index, filterLength);
            return filter.outputSignal[index];
        }

        public static void CreateImpulse(ref float[] freqs, out float[] impulse)
        {
            int count = freqs.Length;
            impulse = new float[count];
            CreateImpulse_Internal(freqs, impulse, count);
        }

        public float Next(float input)
            => Next_Internal(ref filter, input);

        public FIRFilter(ref float[] freqs)
            => CreateFilterByFreqs_Internal(ref filter, freqs, freqs.Length);

        public void UpdateFreqs(ref float[] freqs)
            => UpdateFreqs_Internal(ref filter, freqs);

        void IDisposable.Dispose()
            => DestroyFilter(ref filter);
    }
}
