using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGame3D
{
    class RandomHelper
    {

        public static int LinearOperator(int o, int n1, int n2)
        {
            if (o == 0)
                return n1 - n2;
            else if (o == 1)
                return n1 + n2;
            else
                return n2 - n1;
        }
    }
}
