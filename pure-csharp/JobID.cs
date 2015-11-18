using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pure_csharp
{
    public class JobID
    {
        public JobID(int id, int par1, string par2)
        {
            Id = id;
            Par1 = par1;
            Par2 = par2;
        }
        public int Id
        {
            get;
            private set;
        }
        public int Par1
        {
            get;
            private set;
        }
        public string Par2
        {
            get;
            private set;
        }
    }
}
