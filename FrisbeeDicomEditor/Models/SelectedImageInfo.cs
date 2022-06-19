using Dicom.Imaging;

namespace FrisbeeDicomEditor.Models
{
    public class SelectedImageInfo
    {
        public PhotometricInterpretation PhotometricInterpretation { get; set; }
        public int BitsAllocated { get; set; }
        public ushort BitsStored { get; set; }
        public ushort SamplesPerPixel { get; set; }
        public ushort HighBit { get; set; }
        public PixelRepresentation PixelRepresentation { get; set; }
        public PlanarConfiguration PlanarConfiguration { get; set; }
        public string ImagePath { get; set; }
    }
}
