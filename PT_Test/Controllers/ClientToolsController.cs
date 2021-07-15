using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PT_Test.Models;
using PT_Test.Handlers;

namespace PT_Test.Controllers
{
    [ApiController]
    public class ClientToolsController : ControllerBase
    {
        private readonly PT_Settings pt_config;

        public ClientToolsController(IOptions<PT_Settings> _config)
        {
            pt_config = _config?.Value;
        }

        /// <summary>
        /// Checks the supplied part list for compatibility with other parts in stock.
        /// </summary>
        /// <remarks>
        /// Compatible parts are interchangeable, allowing you to select the cheapest part that is right for the job.
        /// </remarks>
        /// <param name="part_numbers">An array of part numbers.</param>
        /// <example>["1234-asdf"]</example>
        /// <returns>A dictionary with the supplied part numbers as keys and compatible parts as the values.</returns>
        [HttpPost]
        [Route("api/CheckCompatibility")]
        public IActionResult CheckCompatibility([FromBody] string[] part_numbers)
        {
            if (!(part_numbers?.Length > 0)) return BadRequest();  // invalid part data

            /*  Requirement 2 - Check Exclusions List
             *
             *  Valid part numbers should be checked against the provided exclusions list (attached) to determine whether the part should be supplied to PartsTrader or not.
             *  If the given part number is found in the list then the part should not be sent to PartsTrader; in this scenario, the lookup should return an empty collection.
             *  The exclusion file will be updated monthly. 
             */

            var excluded_parts = new List<PartItem>();
            using (StreamReader rdr = new StreamReader(pt_config.Resources.Exclusions))
            {
                // read excluded part list from file
                excluded_parts = JsonConvert.DeserializeObject<List<PartItem>>(rdr.ReadToEnd());
            }
            // parse out only the relevant values, converting to lowercase for easier matching
            var excluded_part_numbers = excluded_parts.Select((x) => x.PartNumber.ToLower());

            /*  Requirement 1 - Validate Part Number
             *  
             *  When given a part number the client tools should validate it to ensure that it conforms to the following specification:
             *  partNumber = partId "-" partCode;
             *  partId     = {4 * digit};
             *  partCode   = {4 * alphanumeric}, {alphanumeric};
             *  
             *  That is a part id comprising of 4 digits, followed by a dash (-), followed by a part code consisting of 4 or more alphanumeric characters.
             *  So, 1234-abcd, 1234-a1b2c3d4 would be valid, a234-abcd, 123-abcd would be invalid. Where an invalid number is found an invalid part exception should be thrown.
             */

            var valid_parts = new List<PartItem>();
            foreach (var cur_part in part_numbers)
            {
                // ensure correct format of input
                var part_format = @"^(\d{4})\-([a-zA-Z0-9]{4,})$";  // \w would match an underscore here
                var part_validate = System.Text.RegularExpressions.Regex.Match(cur_part ?? string.Empty, part_format, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (part_validate.Success)
                {
                    // part is valid
                    if (!excluded_part_numbers.Contains(cur_part.ToLower()))
                    {
                        // part is not present in exclusions list
                        valid_parts.Add(new PartItem()
                        {
                            PartNumber = cur_part,
                            PartId = part_validate.Groups[1].ToString(),
                            PartCode = part_validate.Groups[2].ToString()
                        });
                    }
                }
                else
                {
                    // invalid format
                    return BadRequest(new InvalidPartException($"{cur_part}: format is invalid. "));
                }
            }

            /*  Requirement 3 - Lookup Compatible Parts
             *
             *  If a valid part is supplied that is not on the exclusions list then it should be looked up via the PartsTrader Parts Service in order to retrieve any compatible parts. 
             *  The results of this lookup should be returned.
             */

            // this will presumably make a request to an external API, or call some other function... for testing purposes we're just calling another function
            var compatible_parts = MockDataController.MockLookup(valid_parts);

            return Ok(compatible_parts);
        }
    }
}
