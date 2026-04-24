using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StarDataLoader
{
    public class Star
    {
        public float catalog_number;
        public Vector3 position;
        public Color colour;
        public float size;

        // Keep the original points so we can recalculate based on dates.
        protected readonly double right_ascension;
        public readonly double declination;
        protected readonly float ra_proper_motion;
        protected readonly float dec_proper_motion;


        // Constructor
        public Star(float catalog_number, double right_ascension, double declination, byte spectral_type,
                    byte spectral_index, short magnitude, float ra_proper_motion, float dec_proper_motion, StarPositionType positionType)
        {
            this.catalog_number = catalog_number;
            // Save the location parameters
            this.right_ascension = right_ascension;
            this.declination = declination;
            this.ra_proper_motion = ra_proper_motion;
            this.dec_proper_motion = dec_proper_motion;

            bool positionFlag = false;
            bool colorFlag = false;
            bool SizeFlag = false;
            // Set the position
            position = positionType.GetBasePosition(right_ascension, declination, out positionFlag);
            // Set the Colour
            colour = positionType.SetColour(spectral_type, spectral_index, out colorFlag);
            // Set the Size
            size = positionType.SetSize(magnitude, out SizeFlag);

            if(positionFlag)
            {
                Debug.Log($"position flag raised for: HR {catalog_number}");
            }
            if (colorFlag)
            {
                Debug.Log($"color flag raised for: HR {catalog_number}");
            }
            if (SizeFlag)
            {
                Debug.Log($"size flag raised for: HR {catalog_number}");
            }
        }
    }

    public List<Star> LoadData(StarPositionType positiontype)
    {
        List<Star> stars = new();
        // Open the binary file for reading.
        const string filename = "BSC5";
        TextAsset textAsset = Resources.Load(filename) as TextAsset;
        MemoryStream stream = new(textAsset.bytes);
        BinaryReader br = new(stream);
        // Read the header
        int sequence_offset = br.ReadInt32();
        int start_index = br.ReadInt32();
        int num_stars = -br.ReadInt32();
        int star_number_settings = br.ReadInt32();
        int proper_motion_included = br.ReadInt32();
        int num_magnitudes = br.ReadInt32();
        int star_data_size = br.ReadInt32();
        // Read one field at a time.
        for (int i = 0; i < num_stars; i++)
        {
            float catalog_number = br.ReadSingle();
            double right_ascension = br.ReadDouble();
            // Angular distance from celestial equator.
            double declination = br.ReadDouble();
            byte spectral_type = br.ReadByte();
            byte spectral_index = br.ReadByte();
            short magnitude = br.ReadInt16();
            float ra_proper_motion = br.ReadSingle();
            float dec_proper_motion = br.ReadSingle();
            Star star = new(catalog_number, right_ascension, declination, spectral_type, spectral_index, magnitude, ra_proper_motion, dec_proper_motion, positiontype);
            stars.Add(star);
        }

        return stars;
    }

}


