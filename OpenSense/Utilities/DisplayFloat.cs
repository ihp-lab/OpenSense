using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Utilities {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Psi.Audio;

    /// <summary>
    /// DisplayAudio is a helper class that is used to bind a WPF <image/> to a Psi audio.
    /// </summary>
    public class DisplayFloat : INotifyPropertyChanged {
        /// <summary>
        /// Number of samples captured from Psi audio stream each millisecond.
        /// </summary>
        private const int SamplesPerMillisecond = 1920;

        private const int BitmapWidth = 320;

        private const int BitmapHeight = 100;

        /// <summary>
        /// Rectangle representing the entire energy bitmap area. Used when drawing background
        /// for energy visualization.
        /// </summary>
        private readonly Int32Rect fullValueRect = new Int32Rect(0, 0, BitmapWidth, BitmapHeight);

        /// <summary>
        /// Array of background-color pixels corresponding to an area equal to the size of whole energy bitmap.
        /// </summary>
        private readonly byte[] backgroundPixels = new byte[BitmapWidth * BitmapHeight];

        // private readonly byte[] audioBuffer = null;

        /// <summary>
        /// Buffer used to store audio stream energy data as we read audio.
        /// We store 25% more energy values than we strictly need for visualization to allow for a smoother
        /// stream animation effect, since rendering happens on a different schedule with respect to audio
        /// capture.
        /// </summary>
        private readonly float[] values = new float[(uint)(BitmapWidth * 1.25)];

        /// <summary>
        /// Object for locking energy buffer to synchronize threads.
        /// </summary>
        private readonly object valueLock = new object();

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
        private WriteableBitmap image;

        private int valueIndex;

        private int newValueAvailable;

        private float valueError;

        private DateTime? lastValueRefreshTime;

        private int valueRefreshIndex;

        private float maxMagnitude = float.MinValue;

        private float minMagnitude = float.MaxValue;

        private float latestValue = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAudio"/> class.
        /// </summary>
        public DisplayFloat() : base() {

            Image = new WriteableBitmap(BitmapWidth, BitmapHeight, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.White, Colors.Black }));

            foregroundPixels = new byte[BitmapHeight];
            for (int i = 0; i < foregroundPixels.Length; ++i) {
                foregroundPixels[i] = 0xff;
            }

            // waveDisplay.Source = energyBitmap;
            CompositionTarget.Rendering += UpdateImage;
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public WriteableBitmap Image {
            get => image;
            private set => SetProperty(ref image, value);
        }

        public float MaxMagnitude {
            get => maxMagnitude;
            private set => SetProperty(ref maxMagnitude, value);
        }

        public float MinMagnitude {
            get => minMagnitude;
            private set => SetProperty(ref minMagnitude, value);
        }

        public float LatestValue {
            get => latestValue;
            set => SetProperty(ref latestValue, value);
        }

        public void Update(float value) {
            lock (valueLock) {
                LatestValue = value;
                values[valueIndex] = value;
                valueIndex = (valueIndex + 1) % values.Length;
                ++newValueAvailable;
            }
        }

        public void Clear() {
            lock (valueLock) {
                image.WritePixels(fullValueRect, backgroundPixels, BitmapWidth, 0);
                newValueAvailable = 0;
                MaxMagnitude = float.MinValue;
                MinMagnitude = float.MaxValue;
                LatestValue = 0;
                for (var i = 0; i < values.Length; i++) {
                    values[i] = 0;
                }
            }

        }

        ///// <summary>
        ///// UpdateEnergy callback.
        ///// </summary>
        private void UpdateImage(object sender, EventArgs e) {
            var tempMin = float.MaxValue;
            var tempMax = float.MinValue;
            lock (valueLock) {
                // Calculate how many energy samples we need to advance since the last update in order to
                // have a smooth animation effect
                DateTime now = DateTime.UtcNow;
                DateTime? previousRefreshTime = lastValueRefreshTime;
                lastValueRefreshTime = now;

                // No need to refresh if there is no new energy available to render
                if (newValueAvailable <= 0) {
                    return;
                }

                valueRefreshIndex = (valueRefreshIndex + newValueAvailable) % values.Length;
                newValueAvailable = 0;

                // clear background of energy visualization area
                image.WritePixels(fullValueRect, backgroundPixels, BitmapWidth, 0);

                // Draw each energy sample as a centered vertical bar, where the length of each bar is
                // proportional to the amount of energy it represents.
                // Time advances from left to right, with current time represented by the rightmost bar.
                int baseIndex = (valueRefreshIndex + values.Length - BitmapWidth) % values.Length;

                for (var i = 0; i < BitmapWidth; i++) {
                    var value = values[(baseIndex + i) % values.Length];
                    if (value > tempMax) {
                        tempMax = value;
                    }
                    if (value < tempMin) {
                        tempMin = value;
                    }
                }
                MinMagnitude = tempMin;
                MaxMagnitude = tempMax;

                for (int i = 0; i < BitmapWidth; ++i) {

                    var value = values[(baseIndex + i) % values.Length];
                    var scaled = Math.Min(1.0, ((double)value - MinMagnitude) / (MaxMagnitude - MinMagnitude));
                    int barHeight = (int)Math.Floor(Math.Abs(scaled) * BitmapHeight);
                    var barRect = new Int32Rect(i, BitmapHeight - barHeight, 1, barHeight);

                    // System.Console.WriteLine(barHeight);
                    // Draw bar in foreground color
                    image.WritePixels(barRect, foregroundPixels, 1, 0);
                }
            }
        }
    }
}
