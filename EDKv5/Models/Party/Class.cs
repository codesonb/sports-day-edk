using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    public class Class
    {
        //constructor
        public Class(int form, char sign)
        {
            Form = form;
            if (sign >= 'a' && sign <= 'z') sign -= (char)('a' - 'A');
            if (sign < 'A' || sign > 'Z') throw new ArgumentException();
            Sign = sign;
        }

        public int Form { get; private set; }
        public char Sign { get; private set; }

        public string Key { get { return Form.ToString() + Sign; } }

    }
}