using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchIris
{
    public class IrisCode
    {
        private byte[] bitImage;
        private int sizeImage;
        private byte[] maskImage;
        private uint[] newBitImageRep;
        private uint[] newMaskImageRep;

        public byte[] bit
        {
            get
            {
                return bitImage;
            }
            set
            {
                this.bitImage = value;
            }
        }

        public uint[] newBit
        {
            get
            {
                return newBitImageRep;
            }
            set
            {
                this.newBitImageRep = value;
            }
        }

        public uint[] newMask
        {
            get
            {
                return newMaskImageRep;
            }
            set
            {
                this.newMaskImageRep = value;
            }
        }

        public int size
        {
            get
            {
                return sizeImage;
            }
            set
            {
                this.sizeImage = value;
            }
        }

        public byte[] mask
        {
            get
            {
                return maskImage;
            }
            set
            {
                this.maskImage = value;
            }
        }

    }
}
