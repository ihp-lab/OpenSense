// !!! REFACTOR !!!

// <copyright file="DisplayAudio.cs" company="USC ICT">
// Copyright (c) USC ICT. All rights reserved.
// </copyright>

namespace OpenSense.WPF.Components.Audio.Visualizer {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Psi.Audio;

    /// <summary>
    /// DisplayAudio is a helper class that is used to bind a WPF <image/> to a Psi audio.
    /// </summary>
    public class DisplayAudio : INotifyPropertyChanged {
        /// <summary>
        /// Number of samples captured from Psi audio stream each millisecond.
        /// </summary>
        private const int SamplesPerMillisecond = 1920;

        /// <summary>
        /// Number of bytes in each Psi audio stream sample (32-bit IEEE float).
        /// </summary>
        private const int BytesPerSample = sizeof(float);

        /// <summary>
        /// Number of audio samples represented by each column of pixels in audio bitmap.
        /// </summary>
        private const int SamplesPerColumn = 64;

        /// <summary>
        /// Minimum energy of audio to display (a negative number in dB value, where 0 dB is full scale).
        /// </summary>
        private const int MinEnergy = -50;

        /// <summary>
        /// Width of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        private const int EnergyBitmapWidth = 320;

        /// <summary>
        /// Height of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        private const int EnergyBitmapHeight = 100;

        /// <summary>
        /// Rectangle representing the entire energy bitmap area. Used when drawing background
        /// for energy visualization.
        /// </summary>
        private readonly Int32Rect fullEnergyRect = new Int32Rect(0, 0, EnergyBitmapWidth, EnergyBitmapHeight);

        /// <summary>
        /// Array of background-color pixels corresponding to an area equal to the size of whole energy bitmap.
        /// </summary>
        private readonly byte[] backgroundPixels = new byte[EnergyBitmapWidth * EnergyBitmapHeight];

        // private readonly byte[] audioBuffer = null;

        /// <summary>
        /// Buffer used to store audio stream energy data as we read audio.
        /// We store 25% more energy values than we strictly need for visualization to allow for a smoother
        /// stream animation effect, since rendering happens on a different schedule with respect to audio
        /// capture.
        /// </summary>
        private readonly float[] energy = new float[(uint)(EnergyBitmapWidth * 1.25)];

        /// <summary>
        /// Object for locking energy buffer to synchronize threads.
        /// </summary>
        private readonly object energyLock = new object();

        // private AudioBuffer psiAudio;
        private readonly byte[] psiAudio = null;

        /// <summary>
        /// Array of foreground-color pixels corresponding to a line as long as the energy bitmap is tall.
        /// This gets re-used while constructing the energy visualization.
        /// </summary>
        private readonly byte[] foregroundPixels;

        /// <summary>
        /// Bitmap that contains constructed visualization for audio stream energy, ready to
        /// be displayed. It is a 2-color bitmap with white as background color and black as
        /// foreground color.
        /// </summary>
        private WriteableBitmap audioImage;

        /// <summary>
        /// Sum of squares of audio samples being accumulated to compute the next energy value.
        /// </summary>
        private float accumulatedSquareSum;

        /// <summary>
        /// Number of audio samples accumulated so far to compute the next energy value.
        /// </summary>
        private int accumulatedSampleCount;

        /// <summary>
        /// Index of next element available in audio energy buffer.
        /// </summary>
        private int energyIndex;

        /// <summary>
        /// Number of newly calculated audio stream energy values that have not yet been
        /// displayed.
        /// </summary>
        private int newEnergyAvailable;

        /// <summary>
        /// Error between time slice we wanted to display and time slice that we ended up
        /// displaying, given that we have to display in integer pixels.
        /// </summary>
        private float energyError;

        /// <summary>
        /// Last time energy visualization was rendered to screen.
        /// </summary>
        private DateTime? lastEnergyRefreshTime;

        /// <summary>
        /// Index of first energy element that has never (yet) been displayed to screen.
        /// </summary>
        private int energyRefreshIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAudio"/> class.
        /// </summary>
        public DisplayAudio()
            : base() {
            // System.Windows.Threading.DispatcherTimer dt = new System.Windows.Threading.DispatcherTimer();
            // dt.Interval = new TimeSpan(0, 0, 0, 0, 1);
            // dt.Tick += this.Dt_Tick;
            // dt.Start();

            // Allocate 1024 bytes to hold a single audio sub frame. Duration sub frame
            // is 16 msec, the sample rate is 16khz, which means 256 samples per sub frame.
            // With 4 bytes per sample, that gives us 1024 bytes.
            this.psiAudio = new byte[1920];

            this.audioImage = new WriteableBitmap(EnergyBitmapWidth, EnergyBitmapHeight, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.White, Colors.Black }));

            this.foregroundPixels = new byte[EnergyBitmapHeight];
            for (int i = 0; i < this.foregroundPixels.Length; ++i) {
                this.foregroundPixels[i] = 0xff;
            }

            // this.waveDisplay.Source = this.energyBitmap;
            CompositionTarget.Rendering += this.UpdateEnergy;
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the WriteableBitmap that we will display in a WPF control.
        /// </summary>
        public WriteableBitmap AudioImage {
            get => this.audioImage;

            set {
                this.audioImage = value;
                this.OnPropertyChanged(nameof(this.AudioImage));
            }
        }

        ///// <summary>
        ///// Handles rendering energy visualization into a bitmap.
        ///// </summary>
        ///// <param name="sender">object sending the event.</param>
        ///// <param name="e">event arguments.</param>
        // public void UpdateAudio(AudioBuffer dispAudio)
        // {
        // lock (this)
        // {
        // dispAudio.Data.CopyTo(psiAudio, 0);

        // // Calculate how many energy samples we need to advance since the last update in order to
        // // have a smooth animation effect
        // DateTime now = DateTime.UtcNow;
        // DateTime? previousRefreshTime = this.lastEnergyRefreshTime;
        // this.lastEnergyRefreshTime = now;
        // //System.Console.WriteLine(psiAudio[0]);

        // // No need to refresh if there is no new energy available to render
        // if (this.newEnergyAvailable <= 0)
        // {
        //      return;
        // }

        // if (previousRefreshTime != null)
        // {
        // float energyToAdvance = this.energyError + (((float)(now - previousRefreshTime.Value).TotalMilliseconds * SamplesPerMillisecond) / SamplesPerColumn);
        // int energySamplesToAdvance = Math.Min(this.newEnergyAvailable, (int)Math.Round(energyToAdvance));
        // this.energyError = energyToAdvance - energySamplesToAdvance;
        // this.energyRefreshIndex = (this.energyRefreshIndex + energySamplesToAdvance) % this.energy.Length;
        // this.newEnergyAvailable -= energySamplesToAdvance;
        // }
        // }
        // }

        /// <summary>
        /// Update audio method.
        /// </summary>
        /// <param name="dispAudio">The audio signal.</param>
        public void Update(AudioBuffer dispAudio) {
            Debug.Assert(dispAudio.Format.FormatTag == WaveFormatTag.WAVE_FORMAT_IEEE_FLOAT);
            for (int i = 0; i < dispAudio.Data.Length; i += BytesPerSample) {
                // Extract the 32-bit IEEE float sample from the byte array
                float audioSample = BitConverter.ToSingle(dispAudio.Data, i);

                this.accumulatedSquareSum += audioSample * audioSample;
                ++this.accumulatedSampleCount;

                if (this.accumulatedSampleCount < SamplesPerColumn) {
                    continue;
                }

                float meanSquare = this.accumulatedSquareSum / SamplesPerColumn;

                if (meanSquare > 1.0f) {
                    // A loud audio source right next to the sensor may result in mean square values
                    // greater than 1.0. Cap it at 1.0f for display purposes.
                    meanSquare = 1.0f;
                }

                // Calculate energy in dB, in the range [MinEnergy, 0], where MinEnergy < 0
                float energy = MinEnergy;

                if (meanSquare > 0) {
                    energy = (float)(10.0 * Math.Log10(meanSquare));
                }

                lock (this.energyLock) {
                    // Normalize values to the range [0, 1] for display
                    this.energy[this.energyIndex] = (MinEnergy - energy) / MinEnergy;
                    this.energyIndex = (this.energyIndex + 1) % this.energy.Length;
                    ++this.newEnergyAvailable;
                }

                this.accumulatedSquareSum = 0;
                this.accumulatedSampleCount = 0;
            }
        }

        /// <summary>
        /// ClearAudio method.
        /// </summary>
        public void Clear() {
            this.audioImage.WritePixels(this.fullEnergyRect, this.backgroundPixels, EnergyBitmapWidth, 0);
            newEnergyAvailable = -1;
        }

        ///// <summary>
        ///// Callback for handling of dispatch timer that will drive our UI update.
        ///// </summary>
        ///// <param name="sender">Timer that triggered this callback.</param>
        ///// <param name="e">Event args for the callback.</param>
        // private void Dt_Tick(object sender, EventArgs e)
        // {
        // //this.audioImage = new WriteableBitmap(EnergyBitmapWidth, EnergyBitmapHeight, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.White, Colors.Black }));
        // for (int i = 0; i < this.psiAudio.Length; i += BytesPerSample)
        // {
        // // Extract the 32-bit IEEE float sample from the byte array
        // float audioSample = BitConverter.ToSingle(psiAudio, i);

        // this.accumulatedSquareSum += audioSample * audioSample;
        // ++this.accumulatedSampleCount;

        // if (this.accumulatedSampleCount < SamplesPerColumn)
        // {
        // continue;
        // }

        // float meanSquare = this.accumulatedSquareSum / SamplesPerColumn;

        // if (meanSquare > 1.0f)
        // {
        // // A loud audio source right next to the sensor may result in mean square values
        // // greater than 1.0. Cap it at 1.0f for display purposes.
        // meanSquare = 1.0f;
        // }

        // // Calculate energy in dB, in the range [MinEnergy, 0], where MinEnergy < 0
        // float energy = MinEnergy;

        // if (meanSquare > 0)
        // {
        // energy = (float)(10.0 * Math.Log10(meanSquare));
        // }

        // lock (this.energyLock)
        // {
        // // Normalize values to the range [0, 1] for display
        // this.energy[this.energyIndex] = (MinEnergy - energy) / MinEnergy;
        // this.energyIndex = (this.energyIndex + 1) % this.energy.Length;
        // ++this.newEnergyAvailable;
        // }

        // this.accumulatedSquareSum = 0;
        // this.accumulatedSampleCount = 0;
        // }

        // this.audioImage = new WriteableBitmap(EnergyBitmapWidth, EnergyBitmapHeight, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.White, Colors.Black }));

        // // clear background of energy visualization area
        // //this.audioImage.WritePixels(this.fullEnergyRect, this.backgroundPixels, EnergyBitmapWidth, 0);

        // // Draw each energy sample as a centered vertical bar, where the length of each bar is
        // // proportional to the amount of energy it represents.
        // // Time advances from left to right, with current time represented by the rightmost bar.
        // int baseIndex = (this.energyRefreshIndex + this.energy.Length - EnergyBitmapWidth) % this.energy.Length;
        // for (int i = 0; i < EnergyBitmapWidth; ++i)
        // {
        // const int HalfImageHeight = EnergyBitmapHeight / 2;

        // // Each bar has a minimum height of 1 (to get a steady signal down the middle) and a maximum height
        // // equal to the bitmap height.
        // int barHeight = (int)Math.Max(1.0, this.energy[(baseIndex + i) % this.energy.Length] * EnergyBitmapHeight);

        // // Center bar vertically on image
        // var barRect = new Int32Rect(i, HalfImageHeight - (barHeight / 2), 1, barHeight);
        // System.Console.WriteLine(barHeight);
        // // Draw bar in foreground color
        // this.audioImage.WritePixels(barRect, this.foregroundPixels, 1, 0);
        // }
        // }

        ///// <summary>
        ///// Helper function for firing an event when the image property changes.
        ///// </summary>
        ///// <param name="propertyName">The name of the property that changed.</param>
        // private void OnPropertyChanged(string propertyName)
        // {
        // this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // }

        ///// <summary>
        ///// UpdateEnergy callback.
        ///// </summary>
        private void UpdateEnergy(object sender, EventArgs e) {
            lock (this.energyLock) {
                // Calculate how many energy samples we need to advance since the last update in order to
                // have a smooth animation effect
                DateTime now = DateTime.UtcNow;
                DateTime? previousRefreshTime = this.lastEnergyRefreshTime;
                this.lastEnergyRefreshTime = now;

                // No need to refresh if there is no new energy available to render
                if (this.newEnergyAvailable <= 0) {
                    return;
                }

                if (previousRefreshTime != null) {
                    float energyToAdvance = this.energyError + (((float)(now - previousRefreshTime.Value).TotalMilliseconds * SamplesPerMillisecond) / SamplesPerColumn);
                    int energySamplesToAdvance = Math.Min(this.newEnergyAvailable, (int)Math.Round(energyToAdvance));
                    this.energyError = energyToAdvance - energySamplesToAdvance;
                    this.energyRefreshIndex = (this.energyRefreshIndex + energySamplesToAdvance) % this.energy.Length;
                    this.newEnergyAvailable -= energySamplesToAdvance;
                }

                // clear background of energy visualization area
                this.audioImage.WritePixels(this.fullEnergyRect, this.backgroundPixels, EnergyBitmapWidth, 0);

                // Draw each energy sample as a centered vertical bar, where the length of each bar is
                // proportional to the amount of energy it represents.
                // Time advances from left to right, with current time represented by the rightmost bar.
                int baseIndex = (this.energyRefreshIndex + this.energy.Length - EnergyBitmapWidth) % this.energy.Length;
                for (int i = 0; i < EnergyBitmapWidth; ++i) {
                    const int HalfImageHeight = EnergyBitmapHeight / 2;

                    // Each bar has a minimum height of 1 (to get a steady signal down the middle) and a maximum height
                    // equal to the bitmap height.
                    int barHeight = (int)Math.Max(1.0, this.energy[(baseIndex + i) % this.energy.Length] * EnergyBitmapHeight);

                    // Center bar vertically on image
                    var barRect = new Int32Rect(i, HalfImageHeight - (barHeight / 2), 1, barHeight);

                    // System.Console.WriteLine(barHeight);
                    // Draw bar in foreground color
                    this.audioImage.WritePixels(barRect, this.foregroundPixels, 1, 0);
                }
            }
        }

        /// <summary>
        /// Helper function for firing an event when the image property changes.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
