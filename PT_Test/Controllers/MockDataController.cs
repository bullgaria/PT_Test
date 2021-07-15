using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PT_Test.Models;

namespace PT_Test.Controllers
{
    public class MockDataController
    {
        /// <summary>
        /// The assumption here is that each part could be matched to multiple other parts based on...something.
        /// </summary>
        /// <param name="part_data">The list of items to be matched.</param>
        /// <returns>A dictionary, using the input part numbers as the keys and a list of matching parts for each.</returns>
        public static Dictionary<string, List<PartItem>> MockLookup(List<PartItem> part_data)
        {
            // this will probably be a database call
            // converting to lookup first since there is not much of a point in showing duplicates
            return part_data.ToLookup(x => x.PartNumber).ToDictionary((x) => x.Key, (x) => MockParts(x.First()));
        }

        /// <summary>
        /// Generates a list of fake parts.
        /// </summary>
        /// <param name="base_part">The part for which comparable parts are being looked up.</param>
        /// <param name="random_count">Set to true if the number of parts should be randomized.</param>
        /// <param name="max_parts">Max parts to be generated, this value will be used if random_count is false.</param>
        /// <param name="min_parts">Min parts to be generated, probably best to be kept at 0.</param>
        /// <returns>The list of mocked parts plus the original, sorted by price with the original on top.</returns>
        private static List<PartItem> MockParts(PartItem base_part, bool random_count = true, int max_parts = 5, int min_parts = 0)
        {
            var _rand = new Random();

            // the part being looked up should always appear on the list
            var output_parts = new List<PartItem>() { base_part };
            output_parts.First().Availability = _rand.Next(0, 10);  // a random quantity, representing the current stock of the selected item
            output_parts.First().Price = Math.Round(Math.Max((decimal)0.01, (decimal)_rand.NextDouble() * _rand.Next(1, 100)), 2);  // a random price (1 cent to 99 dollars)

            // ensure the values are within valid ranges
            if (min_parts < 0) min_parts = 0;
            if (max_parts < min_parts) max_parts = min_parts;

            // the number of parts to be generated
            int num_parts = max_parts;
            if (random_count) num_parts = _rand.Next(min_parts, max_parts + 1);

            // generate the parts list
            var mocked_parts = new List<PartItem>();
            for (int i = 0; i < num_parts; i++)
            {
                var cur_part = new PartItem()
                {
                    Description = "This part is fake, and you know it.",
                    PartId = _rand.Next(1000, 10000).ToString(),
                    PartCode = Path.GetRandomFileName().Replace(".", "").Substring(0, 8),  // cheating a bit here to get the code...
                    Availability = _rand.Next(0, 10),
                    Price = Math.Round(Math.Max((decimal)0.01, (decimal)_rand.NextDouble() * _rand.Next(1, 100)), 2)
                };
                cur_part.PartNumber = cur_part.PartId + "-" + cur_part.PartCode;

                mocked_parts.Add(cur_part);
            }

            // add the mocked parts to the output, sorting by price, but leaving the base part on top
            output_parts.AddRange(mocked_parts.OrderBy((x) => x.Price));

            return output_parts;
        }
    }
}
