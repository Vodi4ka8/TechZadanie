using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechZadanie
{
    public static class Config
    {
        private const string _connectionStr = @"User ID= postgres;Password=123456;
                                              Host=localhost;Port=5432;Database= postgres; 
                                              Pooling=true;Maximum Pool Size=300;";

        public static string ConnectionStr { get { return _connectionStr; } }
    }
}
