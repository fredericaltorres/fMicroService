﻿using System.Collections.Generic;
using System.IO;
using System;
using System.JSON;

namespace Donation.Model
{
    public class DonationDTOs : List<DonationDTO>
    {
        public DonationDTOs() // Needed for Deserializer
        {

        }
        public DonationDTOs(IEnumerable<DonationDTO> donations)
        {
            foreach (var d in donations)
                this.Add(d);
        }
        public static DonationDTOs FromJsonFile(string jsonFile)
        {
            if (!File.Exists(jsonFile))
                throw new InvalidDataException($"File not found {jsonFile}");

            return JsonObject.Deserialize<DonationDTOs>(File.ReadAllText(jsonFile));
        }
    }
}
