using System.Collections.Generic;

namespace Donation.Model
{
    public class Errors : List<Error>
    {
        public bool NoError
        {
            get
            {
                return this.Count == 0;
            }
        }
        public override string ToString()
        {
            var b = new System.Text.StringBuilder();
            foreach (var e in this)
                b.Append(e.ToString()).AppendLine();

            return b.ToString();
        }
    }
}
