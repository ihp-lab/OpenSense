using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Components.Onnx {
    internal class BitmapObject {
        //[ImageType(224, 224)]
        [ImageType(64,64), ColumnName("image")]
        public Bitmap image { get; set; }

        public BitmapObject() {
            image = null;
        }

        public BitmapObject(Bitmap data) {
            image = data;
        }

        public BitmapObject(string filePath) {
            image = ConvertToBitmap(filePath);
        }
        public IEnumerable<BitmapObject> getBitmapIEnumerable() {
            IEnumerable<BitmapObject> objects = new BitmapObject[] { this };
            return objects;
        }

        //only use for testing
        public static IEnumerable<BitmapObject> ReadFromFile(string imageFile) {
            IEnumerable<BitmapObject> objects = new BitmapObject[] { new BitmapObject(imageFile) };
            return objects;
        }

        public static IEnumerable<BitmapObject> ReadFromFolder(string imageFolder) {
            return Directory
                .GetFiles(imageFolder)
                .Where(filePath => Path.GetExtension(filePath) != ".md")
                .Select(filePath => new BitmapObject(filePath));
        }

        private Bitmap ConvertToBitmap(string fileName) {
            Bitmap bitmap;
            using (Stream bmpStream = File.Open(fileName, FileMode.Open)) {
                System.Drawing.Image image = System.Drawing.Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);

            }
            return bitmap;
        }
    }
}
